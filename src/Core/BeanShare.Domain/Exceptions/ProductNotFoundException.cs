namespace BeanShare.Domain.Exceptions;

public sealed class ProductNotFoundException : DomainException
{
    public ProductNotFoundException(string productName, string productBrand)
        : base($"Product '{productName}' by '{productBrand}' not found in stock")
    {
        ProductName = productName;
        ProductBrand = productBrand;
    }

    public string ProductName { get; }
    public string ProductBrand { get; }
}