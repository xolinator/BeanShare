namespace BeanShare.Domain.Common;

/// <summary>
/// Abstracts the system clock to enable deterministic time in tests.
/// </summary>
public interface IClock
{
    DateTime UtcNow { get; }
}
