using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Application.Features.Settlement.Commands.SendSettlementEmails;

/// <summary>
/// Command to send settlement notification emails to all space members.
/// </summary>
public sealed record SendSettlementEmailsCommand(
    SettlementId SettlementId,
    bool AttachPdf = true
) : ICommand<Result<int>>; // Returns count of emails sent
