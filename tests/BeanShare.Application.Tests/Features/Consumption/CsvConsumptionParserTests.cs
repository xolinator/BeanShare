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
    public void Parse_ValidCsv_ReturnsAllRowsWithNoErrors()
    {
        var csv = """
            Email,ProductName,ProductBrand,ProductType,QuantityGrams,ConsumedAt
            john@example.com,Super Crema,Lavazza,Espresso,18,2026-01-15 09:30
            jane@example.com,Pike Place,Starbucks,Filter,20,2026-01-15 10:00
            """;

        var result = _parser.Parse(ToStream(csv));

        result.Rows.Should().HaveCount(2);
        result.Errors.Should().BeEmpty();
        result.ValidRows.Should().HaveCount(2);
        result.InvalidRows.Should().BeEmpty();

        result.Rows[0].Email.Should().Be("john@example.com");
        result.Rows[0].ProductName.Should().Be("Super Crema");
        result.Rows[0].ProductBrand.Should().Be("Lavazza");
        result.Rows[0].ProductType.Should().Be("Espresso");
        result.Rows[0].QuantityGrams.Should().Be(18m);
        result.Rows[0].ConsumedAt.Year.Should().Be(2026);
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
        var csv = "Email,ProductName,ProductBrand,ProductType,QuantityGrams,ConsumedAt";

        var result = _parser.Parse(ToStream(csv));

        result.Rows.Should().BeEmpty();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Parse_InvalidQuantity_ReturnsError()
    {
        var csv = """
            Email,ProductName,ProductBrand,ProductType,QuantityGrams,ConsumedAt
            john@example.com,Super Crema,Lavazza,Espresso,not-a-number,2026-01-15 09:30
            """;

        var result = _parser.Parse(ToStream(csv));

        result.Rows.Should().HaveCount(1);
        result.Errors.Should().ContainSingle();
        result.Errors[0].Field.Should().Be("QuantityGrams");
        result.Errors[0].RowNumber.Should().Be(1);
        result.ValidRows.Should().BeEmpty();
        result.InvalidRows.Should().HaveCount(1);
    }

    [Fact]
    public void Parse_InvalidDate_ReturnsError()
    {
        var csv = """
            Email,ProductName,ProductBrand,ProductType,QuantityGrams,ConsumedAt
            john@example.com,Super Crema,Lavazza,Espresso,18,not-a-date
            """;

        var result = _parser.Parse(ToStream(csv));

        result.Rows.Should().HaveCount(1);
        result.Errors.Should().ContainSingle();
        result.Errors[0].Field.Should().Be("ConsumedAt");
    }

    [Fact]
    public void Parse_InvalidCoffeeType_ReturnsError()
    {
        var csv = """
            Email,ProductName,ProductBrand,ProductType,QuantityGrams,ConsumedAt
            john@example.com,Super Crema,Lavazza,Mocha,18,2026-01-15 09:30
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
            Email,ProductName,ProductBrand,ProductType,QuantityGrams,ConsumedAt
            ,Super Crema,Lavazza,Espresso,18,2026-01-15 09:30
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
            Email,ProductName,ProductBrand,ProductType,QuantityGrams,ConsumedAt
            not-an-email,Super Crema,Lavazza,Espresso,18,2026-01-15 09:30
            """;

        var result = _parser.Parse(ToStream(csv));

        result.Errors.Should().ContainSingle();
        result.Errors[0].Field.Should().Be("Email");
        result.Errors[0].Message.Should().Contain("email");
    }

    [Fact]
    public void Parse_ZeroQuantity_ReturnsError()
    {
        var csv = """
            Email,ProductName,ProductBrand,ProductType,QuantityGrams,ConsumedAt
            john@example.com,Super Crema,Lavazza,Espresso,0,2026-01-15 09:30
            """;

        var result = _parser.Parse(ToStream(csv));

        result.Errors.Should().ContainSingle();
        result.Errors[0].Field.Should().Be("QuantityGrams");
        result.Errors[0].Message.Should().Contain("greater than 0");
    }

    [Fact]
    public void Parse_MixedValidAndInvalid_PartitionsCorrectly()
    {
        var csv = """
            Email,ProductName,ProductBrand,ProductType,QuantityGrams,ConsumedAt
            john@example.com,Super Crema,Lavazza,Espresso,18,2026-01-15 09:30
            bad-email,Missing Brand,,Filter,20,2026-01-15 10:00
            jane@example.com,Pike Place,Starbucks,Filter,20,2026-01-15 10:00
            """;

        var result = _parser.Parse(ToStream(csv));

        result.Rows.Should().HaveCount(3);
        result.ValidRows.Should().HaveCount(2);
        result.InvalidRows.Should().HaveCount(1);
        result.InvalidRows[0].RowNumber.Should().Be(2);
    }

    [Fact]
    public void Parse_MultipleDateFormats_AllParsed()
    {
        var csv = """
            Email,ProductName,ProductBrand,ProductType,QuantityGrams,ConsumedAt
            a@b.com,Coffee,Brand,Espresso,18,2026-01-15 09:30
            a@b.com,Coffee,Brand,Espresso,18,2026-01-15 09:30:00
            a@b.com,Coffee,Brand,Espresso,18,2026-01-15T09:30:00
            a@b.com,Coffee,Brand,Espresso,18,2026-01-15T09:30
            a@b.com,Coffee,Brand,Espresso,18,2026-01-15
            a@b.com,Coffee,Brand,Espresso,18,15/01/2026 09:30
            a@b.com,Coffee,Brand,Espresso,18,15/01/2026
            """;

        var result = _parser.Parse(ToStream(csv));

        result.Rows.Should().HaveCount(7);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Parse_TrimsWhitespace_FromFields()
    {
        var csv = """
            Email,ProductName,ProductBrand,ProductType,QuantityGrams,ConsumedAt
              john@example.com  ,  Super Crema  ,  Lavazza  ,  Espresso  ,  18  ,  2026-01-15 09:30
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
            Email,ProductName,ProductBrand,ProductType,QuantityGrams,ConsumedAt
            a@b.com,Coffee,Brand,espresso,18,2026-01-15 09:30
            a@b.com,Coffee,Brand,FILTER,18,2026-01-15 09:30
            a@b.com,Coffee,Brand,Instant,18,2026-01-15 09:30
            a@b.com,Coffee,Brand,decaf,18,2026-01-15 09:30
            a@b.com,Coffee,Brand,Specialty,18,2026-01-15 09:30
            """;

        var result = _parser.Parse(ToStream(csv));

        result.Rows.Should().HaveCount(5);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Parse_DecimalQuantity_ParsedCorrectly()
    {
        var csv = """
            Email,ProductName,ProductBrand,ProductType,QuantityGrams,ConsumedAt
            a@b.com,Coffee,Brand,Espresso,18.5,2026-01-15 09:30
            """;

        var result = _parser.Parse(ToStream(csv));

        result.Rows.Should().HaveCount(1);
        result.Errors.Should().BeEmpty();
        result.Rows[0].QuantityGrams.Should().Be(18.5m);
    }

    [Fact]
    public void Parse_MissingProductName_ReturnsError()
    {
        var csv = """
            Email,ProductName,ProductBrand,ProductType,QuantityGrams,ConsumedAt
            a@b.com,,Brand,Espresso,18,2026-01-15 09:30
            """;

        var result = _parser.Parse(ToStream(csv));

        result.Errors.Should().ContainSingle();
        result.Errors[0].Field.Should().Be("ProductName");
    }

    [Fact]
    public void Parse_RowNumbersAreSequential()
    {
        var csv = """
            Email,ProductName,ProductBrand,ProductType,QuantityGrams,ConsumedAt
            a@b.com,Coffee,Brand,Espresso,18,2026-01-15 09:30
            b@c.com,Coffee,Brand,Filter,20,2026-01-15 10:00
            c@d.com,Coffee,Brand,Instant,15,2026-01-15 11:00
            """;

        var result = _parser.Parse(ToStream(csv));

        result.Rows.Should().HaveCount(3);
        result.Rows[0].RowNumber.Should().Be(1);
        result.Rows[1].RowNumber.Should().Be(2);
        result.Rows[2].RowNumber.Should().Be(3);
    }
}
