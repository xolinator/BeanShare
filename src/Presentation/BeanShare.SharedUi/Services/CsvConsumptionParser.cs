using System.Globalization;
using BeanShare.Application.Features.Consumption.Dtos;
using CsvHelper;
using CsvHelper.Configuration;

namespace BeanShare.SharedUi.Services;

public sealed class CsvConsumptionParser
{
    private static readonly string[] ValidCoffeeTypes = ["Espresso", "Filter", "Instant", "Decaf", "Specialty"];
    private static readonly string[] ExpectedFixedHeaders = ["Email", "ProductName", "ProductBrand", "ProductType"];

    public async Task<CsvParseResult> ParseAsync(Stream stream)
    {
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        ms.Position = 0;
        return Parse(ms);
    }

    public CsvParseResult Parse(Stream stream)
    {
        var rows = new List<ImportConsumptionCsvRow>();
        var errors = new List<ImportRowError>();

        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null,
            TrimOptions = TrimOptions.Trim,
            BadDataFound = null,
        });

        if (!csv.Read() || !csv.ReadHeader())
        {
            return new CsvParseResult([], [new ImportRowError(0, "File", "CSV file has no header row")]);
        }

        var headers = csv.HeaderRecord!;

        if (headers.Length < 5)
        {
            errors.Add(new ImportRowError(0, "Header",
                "CSV must have columns: Email, ProductName, ProductBrand, ProductType, followed by at least one date column"));
            return new CsvParseResult([], errors);
        }

        for (var i = 0; i < 4; i++)
        {
            if (!headers[i].Trim().Equals(ExpectedFixedHeaders[i], StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(new ImportRowError(0, "Header",
                    $"Column {i + 1} must be '{ExpectedFixedHeaders[i]}', found '{headers[i].Trim()}'"));
                return new CsvParseResult([], errors);
            }
        }

        var dateColumns = new List<(int Index, string RawHeader, DateTime Date)>();
        for (var i = 4; i < headers.Length; i++)
        {
            var headerText = headers[i].Trim();
            if (string.IsNullOrWhiteSpace(headerText))
                continue;

            if (TryParseDateHeader(headerText, out var date))
            {
                dateColumns.Add((i, headerText, date));
            }
            else
            {
                errors.Add(new ImportRowError(0, headerText,
                    $"Column header '{headerText}' is not a valid date. Use: yyyy-MM-dd or dd/MM/yyyy"));
            }
        }

        if (dateColumns.Count == 0)
        {
            errors.Add(new ImportRowError(0, "Header", "No valid date columns found"));
            return new CsvParseResult([], errors);
        }

        var expandedRowNumber = 0;
        var csvRowNumber = 0;

        while (csv.Read())
        {
            csvRowNumber++;

            string email, productName, productBrand, productType;
            try
            {
                email = csv.GetField(0)?.Trim() ?? "";
                productName = csv.GetField(1)?.Trim() ?? "";
                productBrand = csv.GetField(2)?.Trim() ?? "";
                productType = csv.GetField(3)?.Trim() ?? "";
            }
            catch (Exception ex)
            {
                expandedRowNumber++;
                errors.Add(new ImportRowError(expandedRowNumber, "Row",
                    $"CSV row {csvRowNumber}: Failed to read: {ex.Message}"));
                continue;
            }

            foreach (var (colIndex, rawHeader, date) in dateColumns)
            {
                string cellValue;
                try
                {
                    cellValue = csv.GetField(colIndex)?.Trim() ?? "";
                }
                catch
                {
                    cellValue = "";
                }

                if (string.IsNullOrWhiteSpace(cellValue))
                    continue;

                if (!decimal.TryParse(cellValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var quantity))
                {
                    expandedRowNumber++;
                    errors.Add(new ImportRowError(expandedRowNumber, rawHeader,
                        $"CSV row {csvRowNumber}: '{cellValue}' is not a valid number"));
                    rows.Add(new ImportConsumptionCsvRow(expandedRowNumber, email, productName, productBrand, productType, 0, DateTime.MinValue));
                    continue;
                }

                if (quantity <= 0)
                    continue;

                expandedRowNumber++;
                var consumedAt = date.Date.AddHours(12);

                var row = new ImportConsumptionCsvRow(expandedRowNumber, email, productName, productBrand, productType, quantity, consumedAt);
                rows.Add(row);

                ValidateRow(row, errors);
            }
        }

        return new CsvParseResult(rows, errors);
    }

    private static void ValidateRow(ImportConsumptionCsvRow row, List<ImportRowError> errors)
    {
        if (string.IsNullOrWhiteSpace(row.Email))
            errors.Add(new ImportRowError(row.RowNumber, "Email", "Email is required"));
        else if (!row.Email.Contains('@'))
            errors.Add(new ImportRowError(row.RowNumber, "Email", "Invalid email format"));

        if (string.IsNullOrWhiteSpace(row.ProductName))
            errors.Add(new ImportRowError(row.RowNumber, "ProductName", "Product name is required"));

        if (string.IsNullOrWhiteSpace(row.ProductBrand))
            errors.Add(new ImportRowError(row.RowNumber, "ProductBrand", "Product brand is required"));

        if (!ValidCoffeeTypes.Any(t => t.Equals(row.ProductType, StringComparison.OrdinalIgnoreCase)))
            errors.Add(new ImportRowError(row.RowNumber, "ProductType", $"Must be one of: {string.Join(", ", ValidCoffeeTypes)}"));

        if (row.QuantityGrams <= 0)
            errors.Add(new ImportRowError(row.RowNumber, "QuantityGrams", "Must be greater than 0"));

        if (row.ConsumedAt > DateTime.UtcNow.AddMinutes(5))
            errors.Add(new ImportRowError(row.RowNumber, "ConsumedAt", "Date cannot be in the future"));
    }

    private static bool TryParseDateHeader(string input, out DateTime result)
    {
        var formats = new[] { "yyyy-MM-dd", "dd/MM/yyyy" };
        return DateTime.TryParseExact(input.Trim(), formats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out result);
    }
}

public sealed record CsvParseResult(
    IReadOnlyList<ImportConsumptionCsvRow> Rows,
    IReadOnlyList<ImportRowError> Errors)
{
    public IReadOnlyList<ImportConsumptionCsvRow> ValidRows
    {
        get
        {
            var errorRowNumbers = Errors.Select(e => e.RowNumber).ToHashSet();
            return Rows.Where(r => !errorRowNumbers.Contains(r.RowNumber)).ToList();
        }
    }

    public IReadOnlyList<ImportConsumptionCsvRow> InvalidRows
    {
        get
        {
            var errorRowNumbers = Errors.Select(e => e.RowNumber).ToHashSet();
            return Rows.Where(r => errorRowNumbers.Contains(r.RowNumber)).ToList();
        }
    }
}
