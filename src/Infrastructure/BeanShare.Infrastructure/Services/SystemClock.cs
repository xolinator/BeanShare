using BeanShare.Domain.Common;

namespace BeanShare.Infrastructure.Services;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}