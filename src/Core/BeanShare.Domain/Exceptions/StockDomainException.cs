namespace BeanShare.Domain.Exceptions;

public sealed class StockDomainException : DomainException
{
    public StockDomainException(string message) : base(message)
    {
    }

    public StockDomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}