using BeanShare.Domain.Common;

namespace BeanShare.Api.Infrastructure.Mocks;

public sealed class SystemClock : IClock
{
    public static readonly SystemClock Instance = new();

    public SystemClock() { }

    public DateTime UtcNow => DateTime.UtcNow;
}