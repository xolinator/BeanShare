using System.Globalization;
using BeanShare.Application.Features.Consumption.Dtos;
using CsvHelper;
using CsvHelper.Configuration;

namespace BeanShare.SharedUi.Services;

public sealed class CsvConsumptionParser
{
    private static readonly string[] ValidCoffeeTypes = ["Espresso", "Filter", "Instant", "Decaf", "Specialty"];

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

        var rowNumber = 0;
        while (csv.Read())
        {
            rowNumber++;

            string email, productName, productBrand, productType, quantityStr, dateStr;
            try
            {
                email = csv.GetField("Email")?.Trim() ?? "";
                productName = csv.GetField("ProductName")?.Trim() ?? "";
                productBrand = csv.GetField("ProductBrand")?.Trim() ?? "";
                productType = csv.GetField("ProductType")?.Trim() ?? "";
                quantityStr = csv.GetField("QuantityGrams")?.Trim() ?? "";
                dateStr = csv.GetField("ConsumedAt")?.Trim() ?? "";
            }
            catch (Exception ex)
            {
                errors.Add(new ImportRowError(rowNumber, "Row", $"Failed to read row: {ex.Message}"));
                continue;
            }

            if (!decimal.TryParse(quantityStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var quantity))
            {
                errors.Add(new ImportRowError(rowNumber, "QuantityGrams", $"'{quantityStr}' is not a valid number"));
                rows.Add(new ImportConsumptionCsvRow(rowNumber, email, productName, productBrand, productType, 0, DateTime.MinValue));
                continue;
            }

            if (!TryParseDate(dateStr, out var consumedAt))
            {
                errors.Add(new ImportRowError(rowNumber, "ConsumedAt", $"'{dateStr}' is not a valid date. Use: yyyy-MM-dd HH:mm"));
                rows.Add(new ImportConsumptionCsvRow(rowNumber, email, productName, productBrand, productType, quantity, DateTime.MinValue));
                continue;
            }

            var row = new ImportConsumptionCsvRow(rowNumber, email, productName, productBrand, productType, quantity, consumedAt);
            rows.Add(row);

            ValidateRow(row, errors);
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

    private static bool TryParseDate(string input, out DateTime result)
    {
        var formats = new[]
        {
            "yyyy-MM-dd HH:mm",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm",
            "yyyy-MM-dd",
            "dd/MM/yyyy HH:mm",
            "dd/MM/yyyy",
        };

        return DateTime.TryParseExact(input, formats,
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
