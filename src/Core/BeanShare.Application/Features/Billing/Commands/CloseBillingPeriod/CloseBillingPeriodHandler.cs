using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.Common;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Billing.Commands.CloseBillingPeriod;
public sealed class CloseBillingPeriodHandler : IRequestHandler<CloseBillingPeriodCommand, Result>
{
    private readonly IBillingPeriodRepository _billingPeriodRepository;
    private readonly ISpaceRepository _spaceRepository;
    private readonly IConsumptionRepository _consumptionRepository;
    private readonly IUserContext _userContext;
    private readonly IClock _clock;

    public CloseBillingPeriodHandler(
        IBillingPeriodRepository billingPeriodRepository,
        ISpaceRepository spaceRepository,
        IConsumptionRepository consumptionRepository,
        IUserContext userContext,
        IClock clock)
    {
        _billingPeriodRepository = billingPeriodRepository;
        _spaceRepository = spaceRepository;
        _consumptionRepository = consumptionRepository;
        _userContext = userContext;
        _clock = clock;
    }

    public async Task<Result> Handle(CloseBillingPeriodCommand command, CancellationToken cancellationToken)
    {
        var billingPeriodId = new BillingPeriodId(command.BillingPeriodId);

        var billingPeriod = await _billingPeriodRepository.GetByIdAsync(billingPeriodId, cancellationToken);
        if (billingPeriod == null)
            return Result.Failure(Error.BillingPeriodNotFound(command.BillingPeriodId));

        var authResult = await EnsureAdminAccess(billingPeriod.SpaceId, cancellationToken);
        if (!authResult.IsSuccess)
            return authResult;

        try
        {
            await AssignUnbilledConsumptions(billingPeriod, billingPeriodId, cancellationToken);

            billingPeriod.Close(_userContext.CurrentUserId, _clock);
            await _billingPeriodRepository.UpdateAsync(billingPeriod, cancellationToken);

            return Result.Success();
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(Error.InvalidBillingPeriodState("close", billingPeriod.State.ToString()));
        }
    }

    private async Task<Result> EnsureAdminAccess(SpaceId spaceId, CancellationToken ct)
    {
        var space = await _spaceRepository.GetSingleBySpecAsync(
            new SpaceByIdSpecification(spaceId), ct);

        if (space == null)
            return Result.Failure(Error.SpaceNotFound(spaceId.Value));
        if (!space.IsAdmin(_userContext.CurrentUserId))
            return Result.Failure(Error.InsufficientSpacePrivileges("close billing periods"));

        return Result.Success();
    }

    private async Task AssignUnbilledConsumptions(
        Domain.Aggregates.BillingPeriod.BillingPeriod billingPeriod,
        BillingPeriodId billingPeriodId,
        CancellationToken ct)
    {
        var spec = new UnassignedConsumptionsInPeriodSpecification(
            billingPeriod.SpaceId, billingPeriod.StartDate, billingPeriod.EndDate);

        var consumptions = await _consumptionRepository.GetBySpecAsync(spec, ct);
        foreach (var entry in consumptions)
            entry.AssignToBillingPeriod(billingPeriodId);
    }
}