namespace BeanShare.Contracts.SemiAuthQr;

public sealed class RecordSemiAuthQrConsumptionRequest
{
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceToken { get; set; } = string.Empty;
    public decimal? QuantityGrams { get; set; }
    public DateTime? ConsumedAt { get; set; }
}
