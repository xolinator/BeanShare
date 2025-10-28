using BeanShare.Contracts.Settlement;

namespace BeanShare.Maui.Services;

public interface ISettlementService
{
    Task<List<SettlementSummaryDto>> GetSpaceSettlementsAsync(Guid spaceId);
    Task<SettlementDto> GetSettlementByIdAsync(Guid settlementId);
    Task<SettlementDto> GenerateSettlementAsync(Guid billingPeriodId);
}
