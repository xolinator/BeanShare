using System.Text.Json.Serialization;

namespace BeanShare.SharedUi.Services;

public sealed class QrConsumptionPayload
{
    [JsonPropertyName("v")]
    public int Version { get; set; } = 1;

    [JsonPropertyName("s")]
    public Guid SpaceId { get; set; }

    [JsonPropertyName("n")]
    public string ProductName { get; set; } = "";

    [JsonPropertyName("b")]
    public string ProductBrand { get; set; } = "";

    [JsonPropertyName("t")]
    public string ProductType { get; set; } = "";

    [JsonPropertyName("g")]
    public int DefaultGrams { get; set; }

    [JsonPropertyName("q")]
    public Guid? QrCodeId { get; set; }

    [JsonIgnore]
    public string? QrLabel { get; set; }
}
