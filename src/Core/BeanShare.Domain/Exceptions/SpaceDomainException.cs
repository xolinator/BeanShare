namespace BeanShare.Domain.Exceptions;

public sealed class SpaceDomainException : DomainException
{
    public SpaceDomainException(string message) : base(message)
    {
    }

    public SpaceDomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
