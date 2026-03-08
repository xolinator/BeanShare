using BeanShare.Application.Common;
using MediatR;

namespace BeanShare.Application.Features.Settlements.Commands.ConfirmPayment;
public sealed record ConfirmPaymentCommand(Guid SettlementId, Guid MemberUserId) : IRequest<Result>;
