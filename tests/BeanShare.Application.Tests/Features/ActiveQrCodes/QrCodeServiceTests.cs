using BeanShare.SharedUi.Services;

namespace BeanShare.Application.Tests.Features.ActiveQrCodes;

public sealed class QrCodeServiceTests
{
    private readonly QrCodeService _service = new("http://localhost:5126");

    [Fact]
    public void GenerateQrCodeBase64ForActiveQr_ReturnsDataUri()
    {
        var qrCodeId = Guid.NewGuid();

        var result = _service.GenerateQrCodeBase64ForActiveQr(qrCodeId);

        result.Should().StartWith("data:image/png;base64,");
        result.Length.Should().BeGreaterThan(100);
    }

    [Fact]
    public void ParseQrPayload_UrlFormat_ExtractsQrCodeId()
    {
        var id = Guid.NewGuid();
        var url = $"http://localhost:5126/scan/{id}";

        var payload = _service.ParseQrPayload(url);

        payload.Should().NotBeNull();
        payload!.Version.Should().Be(2);
        payload.QrCodeId.Should().Be(id);
    }

    [Fact]
    public void ParseQrPayload_UrlWithTrailingSlash_ExtractsQrCodeId()
    {
        var id = Guid.NewGuid();
        var url = $"https://beanshare.app/scan/{id}/";

        var payload = _service.ParseQrPayload(url);

        payload.Should().NotBeNull();
        payload!.QrCodeId.Should().Be(id);
    }

    [Fact]
    public void ParseQrPayload_UrlWithQueryString_ExtractsQrCodeId()
    {
        var id = Guid.NewGuid();
        var url = $"http://example.com/scan/{id}?foo=bar";

        var payload = _service.ParseQrPayload(url);

        payload.Should().NotBeNull();
        payload!.QrCodeId.Should().Be(id);
    }

    [Fact]
    public void ParseQrPayload_LegacyJsonV2_StillWorks()
    {
        var id = Guid.NewGuid();
        var json = $"{{\"v\":2,\"s\":\"00000000-0000-0000-0000-000000000000\",\"n\":\"\",\"b\":\"\",\"t\":\"\",\"g\":0,\"q\":\"{id}\"}}";

        var payload = _service.ParseQrPayload(json);

        payload.Should().NotBeNull();
        payload!.Version.Should().Be(2);
        payload.QrCodeId.Should().Be(id);
    }

    [Fact]
    public void ParseQrPayload_LegacyJsonV1_StillWorks()
    {
        var spaceId = Guid.NewGuid();
        var json = $"{{\"v\":1,\"s\":\"{spaceId}\",\"n\":\"Super Crema\",\"b\":\"Lavazza\",\"t\":\"Espresso\",\"g\":8}}";

        var payload = _service.ParseQrPayload(json);

        payload.Should().NotBeNull();
        payload!.Version.Should().Be(1);
        payload.SpaceId.Should().Be(spaceId);
        payload.ProductName.Should().Be("Super Crema");
    }

    [Fact]
    public void ParseQrPayload_InvalidString_ReturnsNull()
    {
        _service.ParseQrPayload("random garbage text").Should().BeNull();
        _service.ParseQrPayload("").Should().BeNull();
        _service.ParseQrPayload("   ").Should().BeNull();
    }

    [Fact]
    public void ParseQrPayload_UrlWithInvalidGuid_ReturnsNull()
    {
        var url = "http://localhost:5126/scan/not-a-guid";

        _service.ParseQrPayload(url).Should().BeNull();
    }

    [Fact]
    public void GenerateQrCodeBase64_V1Payload_ReturnsDataUri()
    {
        var payload = new QrConsumptionPayload
        {
            Version = 1,
            SpaceId = Guid.NewGuid(),
            ProductName = "Test",
            ProductBrand = "Brand",
            ProductType = "Espresso",
            DefaultGrams = 8
        };

        var result = _service.GenerateQrCodeBase64(payload);

        result.Should().StartWith("data:image/png;base64,");
    }

    [Fact]
    public void GenerateAndParse_V2RoundTrip_PreservesQrCodeId()
    {
        var originalId = Guid.NewGuid();

        var qrImage = _service.GenerateQrCodeBase64ForActiveQr(originalId);
        qrImage.Should().StartWith("data:image/png;base64,");

        // The QR image itself contains a URL, not parseable from the base64 image data.
        // But the URL that would be encoded is predictable:
        var expectedUrl = $"http://localhost:5126/scan/{originalId}";
        var parsed = _service.ParseQrPayload(expectedUrl);

        parsed.Should().NotBeNull();
        parsed!.QrCodeId.Should().Be(originalId);
    }
}
