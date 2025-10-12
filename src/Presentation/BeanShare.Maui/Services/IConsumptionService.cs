using BeanShare.Application.Features.Consumption.Queries;

namespace BeanShare.Maui.Services;

public interface IConsumptionService
{
    Task<GetRecentConsumptionsResult?> GetRecentConsumptionsAsync(Guid spaceId);
    Task<bool> RecordConsumptionAsync(Guid spaceId, int grams, DateTime consumedAt);
}
