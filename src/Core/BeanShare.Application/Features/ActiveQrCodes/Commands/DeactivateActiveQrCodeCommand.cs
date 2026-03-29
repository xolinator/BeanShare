using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.ActiveQrCodes.Commands;

public sealed record DeactivateActiveQrCodeCommand(
    Guid QrCodeId
) : ICommand<Result<bool>>;
