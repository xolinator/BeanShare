using BeanShare.Contracts.Billing;

namespace BeanShare.Maui.Services;

public interface IBillingService
{
    Task<List<BillingPeriodSummaryDto>> GetSpaceBillingPeriodsAsync(Guid spaceId);
    Task<BillingPeriodDto?> GetBillingPeriodByIdAsync(Guid billingPeriodId);
    Task<BillingPeriodDto?> CreateBillingPeriodAsync(Guid spaceId, CreateBillingPeriodDto dto);
    Task OpenBillingPeriodAsync(Guid billingPeriodId);
    Task CloseBillingPeriodAsync(Guid billingPeriodId);
}
