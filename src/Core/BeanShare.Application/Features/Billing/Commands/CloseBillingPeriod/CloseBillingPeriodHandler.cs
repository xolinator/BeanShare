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
        var currentUserId = _userContext.CurrentUserId;
        var billingPeriodId = new BillingPeriodId(command.BillingPeriodId);

        var billingPeriod = await _billingPeriodRepository.GetByIdAsync(billingPeriodId, cancellationToken);
        if (billingPeriod == null)
        {
            return Result.Failure(Error.BillingPeriodNotFound(command.BillingPeriodId));
        }

        var spaceSpec = new SpaceByIdSpecification(billingPeriod.SpaceId);
        var space = await _spaceRepository.GetSingleBySpecAsync(spaceSpec, cancellationToken);

        if (space == null)
        {
            return Result.Failure(Error.SpaceNotFound(billingPeriod.SpaceId.Value));
        }

        if (!space.IsAdmin(currentUserId))
        {
            return Result.Failure(Error.InsufficientSpacePrivileges("close billing periods"));
        }

        try
        {
            var consumptionSpec = new UnassignedConsumptionsInPeriodSpecification(
                billingPeriod.SpaceId,
                billingPeriod.StartDate,
                billingPeriod.EndDate);

            var periodConsumptions = await _consumptionRepository.GetBySpecAsync(consumptionSpec, cancellationToken);

            foreach (var consumption in periodConsumptions)
            {
                consumption.AssignToBillingPeriod(billingPeriodId);
            }

            billingPeriod.Close(currentUserId, _clock);
            await _billingPeriodRepository.UpdateAsync(billingPeriod, cancellationToken);

            return Result.Success();
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(Error.InvalidBillingPeriodState("close", billingPeriod.State.ToString()));
        }
    }
}