using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Services;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Users.Commands.UpdatePreferredCurrency;

public sealed class UpdatePreferredCurrencyHandler : IRequestHandler<UpdatePreferredCurrencyCommand, Result>
{
    private readonly IUserService _userService;
    private readonly IUserContext _userContext;

    public UpdatePreferredCurrencyHandler(
        IUserService userService,
        IUserContext userContext)
    {
        _userService = userService;
        _userContext = userContext;
    }

    public async Task<Result> Handle(UpdatePreferredCurrencyCommand command, CancellationToken cancellationToken)
    {
        var currentUserId = _userContext.CurrentUserId;

        var user = await _userService.GetByIdForUpdateAsync(currentUserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(Error.UserNotFound(currentUserId.Value));
        }

        if (!string.IsNullOrWhiteSpace(command.CurrencyCode))
        {
            try
            {
                _ = Currency.Create(command.CurrencyCode);
            }
            catch (ArgumentException)
            {
                return Result.Failure(Error.InvalidCurrency(command.CurrencyCode));
            }
        }

        user.SetPreferredCurrency(command.CurrencyCode);
        await _userService.UpdateAsync(user, cancellationToken);

        return Result.Success();
    }
}
