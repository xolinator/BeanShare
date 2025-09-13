namespace BeanShare.Domain.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }

    protected DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public sealed class SpaceDomainException : DomainException
{
    public SpaceDomainException(string message) : base(message)
    {
    }

    public SpaceDomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public sealed class InvariantViolationException : DomainException
{
    public InvariantViolationException(string message) : base(message)
    {
    }

    public InvariantViolationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}