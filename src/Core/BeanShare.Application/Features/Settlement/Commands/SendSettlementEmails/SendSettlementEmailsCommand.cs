using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Application.Features.Settlement.Commands.SendSettlementEmails;
public sealed record SendSettlementEmailsCommand(
    SettlementId SettlementId,
    bool AttachPdf = true
) : ICommand<Result<int>>; // Returns count of emails sent
