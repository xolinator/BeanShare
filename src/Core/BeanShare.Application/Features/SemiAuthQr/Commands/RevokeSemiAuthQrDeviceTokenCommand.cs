using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Common.Authorization;

namespace BeanShare.Application.Features.SemiAuthQr.Commands;

public sealed record RevokeSemiAuthQrDeviceTokenCommand(string DeviceId, string Token)
    : IAuthorize, ICommand<Result>;
