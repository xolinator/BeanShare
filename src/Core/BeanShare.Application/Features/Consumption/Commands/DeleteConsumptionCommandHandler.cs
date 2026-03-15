using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.Enums;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Consumption.Commands;

public sealed class DeleteConsumptionCommandHandler
    : IRequestHandler<DeleteConsumptionCommand, Result<bool>>
{
    private readonly IConsumptionRepository _consumptionRepository;
    private readonly IBillingPeriodRepository _billingPeriodRepository;

    public DeleteConsumptionCommandHandler(
        IConsumptionRepository consumptionRepository,
        IBillingPeriodRepository billingPeriodRepository)
    {
        _consumptionRepository = consumptionRepository;
        _billingPeriodRepository = billingPeriodRepository;
    }

    public async Task<Result<bool>> Handle(
        DeleteConsumptionCommand request,
        CancellationToken cancellationToken)
    {
        var entryId = new ConsumptionEntryId(request.Id);
        var entry = await _consumptionRepository.GetByIdAsync(entryId, cancellationToken);

        if (entry == null)
            return Result<bool>.Failure(
                Error.NotFound("ConsumptionEntry", $"Consumption entry {request.Id} not found"));

        if (entry.SpaceId.Value != request.SpaceId)
            return Result<bool>.Failure(Error.InsufficientSpacePrivileges("delete this consumption"));

        if (entry.BillingPeriodId != null)
        {
            var billingPeriod = await _billingPeriodRepository.GetByIdAsync(entry.BillingPeriodId.Value, cancellationToken);
            if (billingPeriod != null && billingPeriod.State != BillingState.Draft && billingPeriod.State != BillingState.Open)
                return Result<bool>.Failure(
                    Error.InvalidBillingPeriodState("delete consumption in", billingPeriod.State.ToString()));
        }

        await _consumptionRepository.RemoveAsync(entry, cancellationToken);

        return Result<bool>.Success(true);
    }
}
