using System.Text;
using BeanShare.SharedUi.Services;

namespace BeanShare.Application.Tests.Features.Consumption;

public sealed class CsvConsumptionParserTests
{
    private readonly CsvConsumptionParser _parser = new();

    private static Stream ToStream(string csv)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(csv));
    }

    [Fact]
    public void Parse_ValidPivotCsv_ExpandsToCorrectRows()
    {
        var csv = """
            Email,ProductName,ProductBrand,ProductType,2026-01-05,2026-01-06,2026-01-07
            john@example.com,Super Crema,Lavazza,Espresso,28,,14
            john@example.com,Classico,illy,Espresso,,16,
            """;

        var result = _parser.Parse(ToStream(csv));

        result.Rows.Should().HaveCount(3);
        result.Errors.Should().BeEmpty();
        result.ValidRows.Should().HaveCount(3);
        result.InvalidRows.Should().BeEmpty();

        result.Rows[0].Email.Should().Be("john@example.com");
        result.Rows[0].ProductName.Should().Be("Super Crema");
        result.Rows[0].ProductBrand.Should().Be("Lavazza");
        result.Rows[0].ProductType.Should().Be("Espresso");
        result.Rows[0].QuantityGrams.Should().Be(28m);
        result.Rows[0].ConsumedAt.Should().Be(new DateTime(2026, 1, 5, 12, 0, 0, DateTimeKind.Utc));

        result.Rows[1].QuantityGrams.Should().Be(14m);
        result.Rows[1].ConsumedAt.Should().Be(new DateTime(2026, 1, 7, 12, 0, 0, DateTimeKind.Utc));

        result.Rows[2].Email.Should().Be("john@example.com");
        result.Rows[2].ProductName.Should().Be("Classico");
        result.Rows[2].QuantityGrams.Should().Be(16m);
        result.Rows[2].ConsumedAt.Should().Be(new DateTime(2026, 1, 6, 12, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Parse_EmptyCellsAreSkipped()
    {
        var csv = """
            Email,ProductName,ProductBrand,ProductType,2026-01-05,2026-01-06,2026-01-07
            a@b.com,Coffee,Brand,Espresso,,18,
            """;

        var result = _parser.Parse(ToStream(csv));

        result.Rows.Should().HaveCount(1);
        result.Errors.Should().BeEmpty();
        result.Rows[0].QuantityGrams.Should().Be(18m);
        result.Rows[0].ConsumedAt.Should().Be(new DateTime(2026, 1, 6, 12, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Parse_ZeroCellsAreSkipped()
    {
        var csv = """
            Email,ProductName,ProductBrand,ProductType,2026-01-05,2026-01-06
            a@b.com,Coffee,Brand,Espresso,0,-5
            """;

        var result = _parser.Parse(ToStream(csv));

        result.Rows.Should().BeEmpty();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Parse_EmptyFile_ReturnsHeaderError()
    {
        var csv = "";

        var result = _parser.Parse(ToStream(csv));

        result.Rows.Should().BeEmpty();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Message.Should().Contain("header");
    }

    [Fact]
    public void Parse_HeaderOnly_ReturnsNoRowsNoErrors()
    {
        var csv = "Email,ProductName,ProductBrand,ProductType,2026-01-05";

        var result = _parser.Parse(ToStream(csv));

        result.Rows.Should().BeEmpty();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Parse_TooFewColumns_ReturnsError()
    {
        var csv = "Email,ProductName,ProductBrand,ProductType";

        var result = _parser.Parse(ToStream(csv));

        result.Rows.Should().BeEmpty();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Message.Should().Contain("date column");
    }

    [Fact]
    public void Parse_WrongFixedColumnName_ReturnsError()
    {
        var csv = "Name,ProductName,ProductBrand,ProductType,2026-01-05";

        var result = _parser.Parse(ToStream(csv));

        result.Rows.Should().BeEmpty();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Field.Should().Be("Header");
        result.Errors[0].Message.Should().Contain("Email");
    }

    [Fact]
    public void Parse_InvalidDateColumnHeader_ReturnsHeaderError()
    {
        var csv = """
            Email,ProductName,ProductBrand,ProductType,2026-01-05,not-a-date,2026-01-07
            a@b.com,Coffee,Brand,Espresso,18,,14
            """;

        var result = _parser.Parse(ToStream(csv));

        result.Rows.Should().HaveCount(2);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].RowNumber.Should().Be(0);
        result.Errors[0].Field.Should().Be("not-a-date");
    }

    [Fact]
    public void Parse_AllDateColumnsInvalid_ReturnsNoValidDateColumnsError()
    {
        var csv = """
            Email,ProductName,ProductBrand,ProductType,bad-date,also-bad
            a@b.com,Coffee,Brand,Espresso,18,14
            """;

        var result = _parser.Parse(ToStream(csv));

        result.Rows.Should().BeEmpty();
        result.Errors.Should().Contain(e => e.Message.Contains("No valid date columns"));
    }

    [Fact]
    public void Parse_InvalidQuantityInCell_ReturnsError()
    {
        var csv = """
            Email,ProductName,ProductBrand,ProductType,2026-01-05,2026-01-06
            a@b.com,Coffee,Brand,Espresso,abc,18
            """;

        var result = _parser.Parse(ToStream(csv));

        result.Rows.Should().HaveCount(2);
        result.ValidRows.Should().HaveCount(1);
        result.InvalidRows.Should().HaveCount(1);

        var error = result.Errors.First(e => e.RowNumber > 0);
        error.Field.Should().Be("2026-01-05");
        error.Message.Should().Contain("abc");
        error.Message.Should().Contain("not a valid number");
    }

    [Fact]
    public void Parse_InvalidCoffeeType_ReturnsError()
    {
        var csv = """
            Email,ProductName,ProductBrand,ProductType,2026-01-05
            a@b.com,Coffee,Brand,Mocha,18
            """;

        var result = _parser.Parse(ToStream(csv));

        result.Rows.Should().HaveCount(1);
        result.Errors.Should().ContainSingle();
        result.Errors[0].Field.Should().Be("ProductType");
        result.Errors[0].Message.Should().Contain("Espresso");
    }

    [Fact]
    public void Parse_MissingEmail_ReturnsError()
    {
        var csv = """
            Email,ProductName,ProductBrand,ProductType,2026-01-05
            ,Coffee,Brand,Espresso,18
            """;

        var result = _parser.Parse(ToStream(csv));

        result.Rows.Should().HaveCount(1);
        result.Errors.Should().ContainSingle();
        result.Errors[0].Field.Should().Be("Email");
    }

    [Fact]
    public void Parse_InvalidEmailFormat_ReturnsError()
    {
        var csv = """
            Email,ProductName,ProductBrand,ProductType,2026-01-05
            not-an-email,Coffee,Brand,Espresso,18
            """;

        var result = _parser.Parse(ToStream(csv));

        result.Errors.Should().ContainSingle();
        result.Errors[0].Field.Should().Be("Email");
        result.Errors[0].Message.Should().Contain("email");
    }

    [Fact]
    public void Parse_MissingProductName_ReturnsError()
    {
        var csv = """
            Email,ProductName,ProductBrand,ProductType,2026-01-05
            a@b.com,,Brand,Espresso,18
            """;

        var result = _parser.Parse(ToStream(csv));

        result.Errors.Should().ContainSingle();
        result.Errors[0].Field.Should().Be("ProductName");
    }

    [Fact]
    public void Parse_MissingProductBrand_ReturnsError()
    {
        var csv = """
            Email,ProductName,ProductBrand,ProductType,2026-01-05
            a@b.com,Coffee,,Espresso,18
            """;

        var result = _parser.Parse(ToStream(csv));

        result.Errors.Should().ContainSingle();
        result.Errors[0].Field.Should().Be("ProductBrand");
    }

    [Fact]
    public void Parse_SingleDateColumn_Works()
    {
        var csv = """
            Email,ProductName,ProductBrand,ProductType,2026-01-05
            a@b.com,Coffee,Brand,Espresso,18
            b@c.com,Tea,Brand2,Filter,20
            """;

        var result = _parser.Parse(ToStream(csv));

        result.Rows.Should().HaveCount(2);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Parse_MixedDateFormats_InHeaders()
    {
        var csv = """
            Email,ProductName,ProductBrand,ProductType,2026-01-05,06/01/2026
            a@b.com,Coffee,Brand,Espresso,18,20
            """;

        var result = _parser.Parse(ToStream(csv));

        result.Rows.Should().HaveCount(2);
        result.Errors.Should().BeEmpty();
        result.Rows[0].ConsumedAt.Should().Be(new DateTime(2026, 1, 5, 12, 0, 0, DateTimeKind.Utc));
        result.Rows[1].ConsumedAt.Should().Be(new DateTime(2026, 1, 6, 12, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Parse_MixedValidAndInvalid_PartitionsCorrectly()
    {
        var csv = """
            Email,ProductName,ProductBrand,ProductType,2026-01-05,2026-01-06
            john@example.com,Super Crema,Lavazza,Espresso,18,
            bad-email,Missing Brand,,Filter,,20
            jane@example.com,Pike Place,Starbucks,Filter,20,
            """;

        var result = _parser.Parse(ToStream(csv));

        result.Rows.Should().HaveCount(3);
        result.ValidRows.Should().HaveCount(2);
        result.InvalidRows.Should().HaveCount(1);
    }

    [Fact]
    public void Parse_TrimsWhitespace_FromFields()
    {
        var csv = """
            Email,ProductName,ProductBrand,ProductType,2026-01-05
              john@example.com  ,  Super Crema  ,  Lavazza  ,  Espresso  ,  18
            """;

        var result = _parser.Parse(ToStream(csv));

        result.Rows.Should().HaveCount(1);
        result.Errors.Should().BeEmpty();
        result.Rows[0].Email.Should().Be("john@example.com");
        result.Rows[0].ProductName.Should().Be("Super Crema");
        result.Rows[0].ProductBrand.Should().Be("Lavazza");
    }

    [Fact]
    public void Parse_CoffeeTypeCaseInsensitive_Accepted()
    {
        var csv = """
            Email,ProductName,ProductBrand,ProductType,2026-01-05
            a@b.com,Coffee,Brand,espresso,18
            a@b.com,Coffee,Brand,FILTER,18
            a@b.com,Coffee,Brand,Instant,18
            a@b.com,Coffee,Brand,decaf,18
            a@b.com,Coffee,Brand,Specialty,18
            """;

        var result = _parser.Parse(ToStream(csv));

        result.Rows.Should().HaveCount(5);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Parse_DecimalQuantityInCell_ParsedCorrectly()
    {
        var csv = """
            Email,ProductName,ProductBrand,ProductType,2026-01-05
            a@b.com,Coffee,Brand,Espresso,18.5
            """;

        var result = _parser.Parse(ToStream(csv));

        result.Rows.Should().HaveCount(1);
        result.Errors.Should().BeEmpty();
        result.Rows[0].QuantityGrams.Should().Be(18.5m);
    }

    [Fact]
    public void Parse_RowNumbersAreSequentialAfterExpansion()
    {
        var csv = """
            Email,ProductName,ProductBrand,ProductType,2026-01-05,2026-01-06
            a@b.com,Coffee,Brand,Espresso,18,20
            b@c.com,Tea,Brand2,Filter,15,25
            """;

        var result = _parser.Parse(ToStream(csv));

        result.Rows.Should().HaveCount(4);
        result.Rows[0].RowNumber.Should().Be(1);
        result.Rows[1].RowNumber.Should().Be(2);
        result.Rows[2].RowNumber.Should().Be(3);
        result.Rows[3].RowNumber.Should().Be(4);
    }

    [Fact]
    public void Parse_ConsumedAt_IsNoonUtc()
    {
        var csv = """
            Email,ProductName,ProductBrand,ProductType,2026-01-05
            a@b.com,Coffee,Brand,Espresso,18
            """;

        var result = _parser.Parse(ToStream(csv));

        result.Rows.Should().HaveCount(1);
        result.Rows[0].ConsumedAt.Hour.Should().Be(12);
        result.Rows[0].ConsumedAt.Minute.Should().Be(0);
    }
}
