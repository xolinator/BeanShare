using BeanShare.Domain.Common;

namespace BeanShare.Api.Infrastructure.Mocks;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}