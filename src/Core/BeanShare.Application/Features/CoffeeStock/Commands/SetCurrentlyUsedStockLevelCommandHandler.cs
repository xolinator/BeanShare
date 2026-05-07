using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.Common;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.CoffeeStock.Commands;

public sealed class SetCurrentlyUsedStockLevelCommandHandler : IRequestHandler<SetCurrentlyUsedStockLevelCommand, Result<bool>>
{
    private readonly ICoffeeStockRepository _coffeeStockRepository;
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUserContext _userContext;
    private readonly IClock _clock;

    public SetCurrentlyUsedStockLevelCommandHandler(
        ICoffeeStockRepository coffeeStockRepository,
        ISpaceRepository spaceRepository,
        IUserContext userContext,
        IClock clock)
    {
        _coffeeStockRepository = coffeeStockRepository;
        _spaceRepository = spaceRepository;
        _userContext = userContext;
        _clock = clock;
    }

    public async Task<Result<bool>> Handle(SetCurrentlyUsedStockLevelCommand command, CancellationToken cancellationToken)
    {
        var spaceId = new SpaceId(command.SpaceId);
        var spaceSpec = new SpaceByIdSpecification(spaceId);
        var space = await _spaceRepository.GetSingleBySpecAsync(spaceSpec, cancellationToken);

        if (space == null)
        {
            return Result<bool>.Failure(Error.SpaceNotFound(command.SpaceId));
        }

        var currentUserId = _userContext.CurrentUserId;
        if (!space.IsAdmin(currentUserId))
        {
            return Result<bool>.Failure(Error.InsufficientSpacePrivileges("set currently used coffee"));
        }

        var coffeeStock = await _coffeeStockRepository.GetBySpaceIdAsync(spaceId, cancellationToken);
        if (coffeeStock == null)
        {
            return Result<bool>.Failure(Error.StockNotFound(command.SpaceId));
        }

        try
        {
            if (command.StockLevelId.HasValue)
                coffeeStock.SetCurrentlyUsed(command.StockLevelId.Value, _clock);
            else
                coffeeStock.ClearCurrentlyUsed(_clock);

            await _coffeeStockRepository.UpdateAsync(coffeeStock, cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (ArgumentException)
        {
            return Result<bool>.Failure(Error.StockLevelNotFound(command.StockLevelId ?? Guid.Empty));
        }
        catch (InvalidOperationException ex)
        {
            return Result<bool>.Failure(Error.DomainError(ex.Message));
        }
    }
}
