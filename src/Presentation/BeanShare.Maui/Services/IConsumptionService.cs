using BeanShare.Application.Features.Consumption.Queries;
using BeanShare.Application.Features.Consumption.Dtos;

namespace BeanShare.Maui.Services;

public interface IConsumptionService
{
    Task<GetRecentConsumptionsResult?> GetRecentConsumptionsAsync(Guid spaceId);
    Task<bool> RecordConsumptionAsync(Guid spaceId, int grams, DateTime consumedAt);
    Task<ConsumptionHistoryDto?> GetUserHistoryAsync(
        Guid? spaceId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? billingPeriodId = null,
        int pageNumber = 1,
        int pageSize = 20);
}
