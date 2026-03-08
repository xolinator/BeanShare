using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Settlement.Dtos;
using BeanShare.Application.Services;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Settlement.Queries.GetSettlementById;

public sealed class GetSettlementByIdHandler : IRequestHandler<GetSettlementByIdQuery, Result<SettlementDto>>
{
    private readonly ISettlementRepository _settlementRepository;
    private readonly IBillingPeriodRepository _billingPeriodRepository;
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUserService _userService;

    public GetSettlementByIdHandler(
        ISettlementRepository settlementRepository,
        IBillingPeriodRepository billingPeriodRepository,
        ISpaceRepository spaceRepository,
        IUserService userService)
    {
        _settlementRepository = settlementRepository;
        _billingPeriodRepository = billingPeriodRepository;
        _spaceRepository = spaceRepository;
        _userService = userService;
    }

    public async Task<Result<SettlementDto>> Handle(GetSettlementByIdQuery request, CancellationToken cancellationToken)
    {
        var settlement = await _settlementRepository.GetByIdAsync(request.Id, cancellationToken);

        if (settlement is null)
        {
            return Result<SettlementDto>.Failure(Error.ValidationFailure("Settlement", $"Settlement with ID {request.Id.Value} not found"));
        }

        var billingPeriod = await _billingPeriodRepository.GetByIdAsync(settlement.BillingPeriodId, cancellationToken);
        if (billingPeriod is null)
        {
            return Result<SettlementDto>.Failure(Error.BillingPeriodNotFound(settlement.BillingPeriodId.Value));
        }

        var space = await _spaceRepository.GetSingleBySpecAsync(
            new SpaceByIdSpec(settlement.SpaceId),
            cancellationToken);

        var userIds = settlement.Lines.Select(l => l.UserId).Distinct().ToList();
        var users = await _userService.GetByIdsAsync(userIds, cancellationToken);
        var userLookup = users.ToDictionary(u => u.Id);

        var lines = new List<SettlementLineDto>();
        foreach (var line in settlement.Lines)
        {
            userLookup.TryGetValue(line.UserId, out var user);
            var userName = user?.Name ?? $"User {line.UserId.Value}";
            var userEmail = user?.Email ?? $"user{line.UserId.Value}@example.com";

            var totalConsumption = settlement.Lines.Sum(l => l.TotalCoffeeGrams);
            var consumptionPercentage = totalConsumption > 0
                ? (line.TotalCoffeeGrams / totalConsumption) * 100
                : 0;

            var lineDto = new SettlementLineDto(
                line.UserId.Value,
                userName,
                userEmail,
                line.TotalCoffeeGrams,
                line.TotalMilkMl ?? 0,
                line.AmountDue.Amount,
                line.AmountDue.Currency,
                consumptionPercentage,
                line.IsConfirmed,
                line.Confirmation?.ConfirmedBy.Value,
                line.Confirmation?.ConfirmedAt
            );

            lines.Add(lineDto);
        }

        var dto = new SettlementDto(
            settlement.Id.Value,
            settlement.SpaceId.Value,
            settlement.BillingPeriodId.Value,
            billingPeriod.Name,
            billingPeriod.StartDate,
            billingPeriod.EndDate,
            settlement.GeneratedAt,
            settlement.GeneratedBy.Value,
            settlement.TotalAmount,
            settlement.Currency,
            settlement.Status.ToString(),
            settlement.CompletedAt,
            settlement.ConfirmedLinesCount,
            settlement.TotalLinesCount,
            lines
        );

        return Result<SettlementDto>.Success(dto);
    }
}

public sealed class SpaceByIdSpec : ISpec<Domain.Aggregates.Space.Space>
{
    private readonly SpaceId _spaceId;

    public SpaceByIdSpec(SpaceId spaceId)
    {
        _spaceId = spaceId;
    }

    public System.Linq.Expressions.Expression<Func<Domain.Aggregates.Space.Space, bool>> Criteria =>
        space => space.Id == _spaceId;

    public string? Reason => $"Space with ID {_spaceId.Value}";
}