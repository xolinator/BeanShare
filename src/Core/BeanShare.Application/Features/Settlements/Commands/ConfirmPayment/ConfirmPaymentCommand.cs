using BeanShare.Application.Common;
using MediatR;

namespace BeanShare.Application.Features.Settlements.Commands.ConfirmPayment;

/// <summary>
/// Command to confirm payment for a settlement line.
/// Can be executed by the member themselves or by an administrator.
/// </summary>
/// <param name="SettlementId">The settlement containing the line to confirm</param>
/// <param name="MemberUserId">The user whose payment is being confirmed</param>
public sealed record ConfirmPaymentCommand(Guid SettlementId, Guid MemberUserId) : IRequest<Result>;
