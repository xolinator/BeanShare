using BeanShare.Application.Common;
using BeanShare.Application.Abstractions;

namespace BeanShare.Application.Features.Users.Commands.UpdatePreferredCurrency;
public sealed record UpdatePreferredCurrencyCommand(string? CurrencyCode) : ICommand<Result>;
