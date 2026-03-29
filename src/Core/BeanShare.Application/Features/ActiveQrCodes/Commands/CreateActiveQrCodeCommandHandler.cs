using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.ActiveQrCodes.Dtos;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.ActiveQrCodes.Commands;

public sealed class CreateActiveQrCodeCommandHandler : IRequestHandler<CreateActiveQrCodeCommand, Result<ActiveQrCodeDto>>
{
    private readonly IActiveQrCodeRepository _qrCodeRepository;
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUserContext _userContext;
    private readonly IClock _clock;

    public CreateActiveQrCodeCommandHandler(
        IActiveQrCodeRepository qrCodeRepository,
        ISpaceRepository spaceRepository,
        IUserContext userContext,
        IClock clock)
    {
        _qrCodeRepository = qrCodeRepository;
        _spaceRepository = spaceRepository;
        _userContext = userContext;
        _clock = clock;
    }

    public async Task<Result<ActiveQrCodeDto>> Handle(CreateActiveQrCodeCommand command, CancellationToken cancellationToken)
    {
        var spaceId = new SpaceId(command.SpaceId);
        var space = await _spaceRepository.GetByIdAsync(spaceId, cancellationToken);

        if (space is null)
        {
            return Result<ActiveQrCodeDto>.Failure(Error.SpaceNotFound(command.SpaceId));
        }

        if (!space.HasMember(_userContext.CurrentUserId))
        {
            return Result<ActiveQrCodeDto>.Failure(Error.NotMember());
        }

        if (!Enum.TryParse<CoffeeType>(command.ProductType, true, out var coffeeType))
        {
            return Result<ActiveQrCodeDto>.Failure(Error.InvalidCoffeeType());
        }

        try
        {
            var product = CoffeeProduct.Create(command.ProductName, command.ProductBrand, coffeeType);

            var qrCode = ActiveQrCode.Create(
                spaceId,
                command.Label,
                product,
                command.RecipeName,
                command.DefaultGrams,
                _clock);

            await _qrCodeRepository.AddAsync(qrCode, cancellationToken);

            return Result<ActiveQrCodeDto>.Success(ActiveQrCodeMapper.ToDto(qrCode));
        }
        catch (ArgumentException ex)
        {
            return Result<ActiveQrCodeDto>.Failure(Error.ValidationFailure(nameof(CreateActiveQrCodeCommand), ex.Message));
        }
    }
}
