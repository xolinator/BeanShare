namespace BeanShare.Contracts.ActiveQrCodes;

public sealed record ReassignActiveQrCodeRequest
{
    public required string ProductName { get; init; }
    public required string ProductBrand { get; init; }
    public required string ProductType { get; init; }
    public required string RecipeName { get; init; }
    public required int DefaultGrams { get; init; }
}
