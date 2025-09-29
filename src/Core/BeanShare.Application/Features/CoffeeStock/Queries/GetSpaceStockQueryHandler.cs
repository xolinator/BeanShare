using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.CoffeeStock.Dtos;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;
using MapsterMapper;
using MediatR;

namespace BeanShare.Application.Features.CoffeeStock.Queries;

public sealed class GetSpaceStockQueryHandler : IRequestHandler<GetSpaceStockQuery, Result<CoffeeStockDto>>
{
    private readonly ICoffeeStockRepository _coffeeStockRepository;
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUserContext _userContext;
    private readonly IMapper _mapper;

    public GetSpaceStockQueryHandler(
        ICoffeeStockRepository coffeeStockRepository,
        ISpaceRepository spaceRepository,
        IUserContext userContext,
        IMapper mapper)
    {
        _coffeeStockRepository = coffeeStockRepository;
        _spaceRepository = spaceRepository;
        _userContext = userContext;
        _mapper = mapper;
    }

    public async Task<Result<CoffeeStockDto>> Handle(GetSpaceStockQuery query, CancellationToken cancellationToken)
    {
        var spaceId = new SpaceId(query.SpaceId);
        var spaceSpec = new SpaceByIdSpecification(spaceId);
        var space = await _spaceRepository.GetSingleBySpecAsync(spaceSpec, cancellationToken);

        if (space == null)
        {
            return Result<CoffeeStockDto>.Failure(Error.SpaceNotFound(query.SpaceId));
        }

        var currentUserId = _userContext.CurrentUserId;
        if (!space.HasMember(currentUserId))
        {
            return Result<CoffeeStockDto>.Failure(Error.InsufficientSpacePrivileges("view coffee stock"));
        }

        var coffeeStock = await _coffeeStockRepository.GetBySpaceIdAsync(spaceId, cancellationToken);

        if (coffeeStock == null)
        {
            return Result<CoffeeStockDto>.Success(new CoffeeStockDto
            {
                Id = Guid.Empty,
                SpaceId = query.SpaceId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PurchaseCount = 0,
                ProductVarietyCount = 0,
                TotalCurrentStockGrams = 0,
                TotalInvestmentAmount = 0,
                TotalInvestmentCurrency = "USD",
                StockLevels = [],
                RecentPurchases = []
            });
        }

        var dto = _mapper.Map<CoffeeStockDto>(coffeeStock);
        return Result<CoffeeStockDto>.Success(dto);
    }
}