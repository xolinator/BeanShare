namespace BeanShare.Domain.Exceptions;

public sealed class InsufficientStockException : DomainException
{
    public InsufficientStockException(string productName, string productBrand, decimal requested, decimal available)
        : base($"Cannot consume {requested}g of '{productName}' by '{productBrand}'. Only {available}g available in stock")
    {
        ProductName = productName;
        ProductBrand = productBrand;
        RequestedGrams = requested;
        AvailableGrams = available;
    }

    public string ProductName { get; }
    public string ProductBrand { get; }
    public decimal RequestedGrams { get; }
    public decimal AvailableGrams { get; }
}