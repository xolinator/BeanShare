using BeanShare.Application.Common;
using MediatR;

namespace BeanShare.Application.Features.Settlement.Commands.ConfirmPayment;
public sealed record ConfirmPaymentCommand(Guid SettlementId, Guid MemberUserId) : IRequest<Result>;
