using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.Settlement.Commands.ConfirmPayment;
public sealed record ConfirmPaymentCommand(Guid SettlementId, Guid MemberUserId) : ICommand<Result>;
