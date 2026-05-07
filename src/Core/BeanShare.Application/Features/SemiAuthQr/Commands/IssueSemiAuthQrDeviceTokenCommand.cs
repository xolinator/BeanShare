using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Common.Authorization;
using BeanShare.Application.Features.SemiAuthQr.Dtos;

namespace BeanShare.Application.Features.SemiAuthQr.Commands;

public sealed record IssueSemiAuthQrDeviceTokenCommand(string DeviceId)
    : IAuthorize, ICommand<Result<DeviceLogTokenDto>>;
