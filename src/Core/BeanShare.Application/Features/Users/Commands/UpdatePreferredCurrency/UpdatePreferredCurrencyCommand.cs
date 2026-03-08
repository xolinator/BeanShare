using BeanShare.Application.Common;
using MediatR;

namespace BeanShare.Application.Features.Users.Commands.UpdatePreferredCurrency;
public sealed record UpdatePreferredCurrencyCommand(string? CurrencyCode) : IRequest<Result>;
