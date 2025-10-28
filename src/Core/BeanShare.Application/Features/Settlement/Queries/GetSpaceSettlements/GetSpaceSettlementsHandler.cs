using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Settlement.Dtos;
using BeanShare.Domain.Entities;
using MediatR;

namespace BeanShare.Application.Features.Settlement.Queries.GetSpaceSettlements;

public sealed class GetSpaceSettlementsHandler : IRequestHandler<GetSpaceSettlementsQuery, Result<List<SettlementSummaryDto>>>
{
    private readonly ISettlementRepository _settlementRepository;
    private readonly IBillingPeriodRepository _billingPeriodRepository;
    private readonly IUserContext _userContext;

    public GetSpaceSettlementsHandler(
        ISettlementRepository settlementRepository,
        IBillingPeriodRepository billingPeriodRepository,
        IUserContext userContext)
    {
        _settlementRepository = settlementRepository;
        _billingPeriodRepository = billingPeriodRepository;
        _userContext = userContext;
    }

    public async Task<Result<List<SettlementSummaryDto>>> Handle(GetSpaceSettlementsQuery request, CancellationToken cancellationToken)
    {
        var settlements = await _settlementRepository.GetBySpaceIdAsync(request.SpaceId, cancellationToken);

        var summaries = new List<SettlementSummaryDto>();

        foreach (var settlement in settlements)
        {
            var billingPeriod = await _billingPeriodRepository.GetByIdAsync(settlement.BillingPeriodId, cancellationToken);
            var billingPeriodName = billingPeriod?.Name ?? "Unknown Period";

            var currentUserId = _userContext.CurrentUserId;
            var userLine = settlement.Lines.FirstOrDefault(l => l.UserId == currentUserId);
            var yourShare = userLine?.AmountDue.Amount ?? 0;

            var summary = new SettlementSummaryDto(
                settlement.Id.Value,
                billingPeriodName,
                settlement.GeneratedAt,
                settlement.TotalAmount,
                settlement.Currency,
                settlement.Lines.Count,
                yourShare
            );

            summaries.Add(summary);
        }

        return Result<List<SettlementSummaryDto>>.Success(summaries);
    }
}