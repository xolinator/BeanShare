using BeanShare.Application.Common;
using MediatR;

namespace BeanShare.Application.Features.Users.Commands.UpdatePreferredCurrency;

/// <summary>
/// Command to update the user's preferred currency for cost conversion.
/// Null currency code clears the preference (uses first space's currency).
/// </summary>
/// <param name="CurrencyCode">The 3-letter ISO currency code, or null to clear</param>
public sealed record UpdatePreferredCurrencyCommand(string? CurrencyCode) : IRequest<Result>;
