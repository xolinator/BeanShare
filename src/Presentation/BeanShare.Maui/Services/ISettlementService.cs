using BeanShare.Contracts.Settlement;

namespace BeanShare.Maui.Services;

public interface ISettlementService
{
    Task<List<SettlementSummaryDto>> GetSpaceSettlementsAsync(Guid spaceId);
    Task<SettlementDto> GetSettlementByIdAsync(Guid settlementId);
    Task<SettlementDto> GenerateSettlementAsync(Guid billingPeriodId);
    Task<bool> ConfirmPaymentAsync(Guid settlementId, Guid memberUserId);
    Task<byte[]?> ExportPdfAsync(Guid settlementId);
    Task<byte[]?> ExportExcelAsync(Guid settlementId);
    Task<int> SendEmailsAsync(Guid settlementId, bool attachPdf = true);
}
