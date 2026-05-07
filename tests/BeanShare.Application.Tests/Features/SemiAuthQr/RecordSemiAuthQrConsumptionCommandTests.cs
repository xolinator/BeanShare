using BeanShare.Application.Abstractions;
using BeanShare.Application.Features.SemiAuthQr.Commands;
using BeanShare.Domain.Aggregates.CoffeeStock;
using BeanShare.Domain.Aggregates.Space;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Security.Cryptography;
using System.Text;

namespace BeanShare.Application.Tests.Features.SemiAuthQr;

public sealed class RecordSemiAuthQrConsumptionCommandTests
{
    private readonly IActiveQrCodeRepository _activeQrCodeRepository = Substitute.For<IActiveQrCodeRepository>();
    private readonly ISemiAuthQrDeviceTokenRepository _tokenRepository = Substitute.For<ISemiAuthQrDeviceTokenRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly ISpaceRepository _spaceRepository = Substitute.For<ISpaceRepository>();
    private readonly ICoffeeStockRepository _coffeeStockRepository = Substitute.For<ICoffeeStockRepository>();
    private readonly IConsumptionRepository _consumptionRepository = Substitute.For<IConsumptionRepository>();
    private readonly ISemiAuthQrSettings _settings = Substitute.For<ISemiAuthQrSettings>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly ILogger<RecordSemiAuthQrConsumptionCommandHandler> _logger = Substitute.For<ILogger<RecordSemiAuthQrConsumptionCommandHandler>>();

    private readonly RecordSemiAuthQrConsumptionCommandHandler _handler;

    public RecordSemiAuthQrConsumptionCommandTests()
    {
        _handler = new RecordSemiAuthQrConsumptionCommandHandler(
            _activeQrCodeRepository,
            _tokenRepository,
            _userRepository,
            _spaceRepository,
            _coffeeStockRepository,
            _consumptionRepository,
            _settings,
            _clock,
            _logger);
    }

    [Fact]
    public async Task Handle_WithValidDeviceToken_ShouldRecordConsumption()
    {
        var now = DateTime.UtcNow;
        var userId = UserId.New();
        var spaceId = SpaceId.New();
        const string deviceId = "4f8e8b95-bfd9-4582-8fe6-b890f78d8e36";
        const string rawToken = "token-secret";

        _settings.Enabled.Returns(true);
        _clock.UtcNow.Returns(now);

        var token = SemiAuthQrDeviceToken.Create(
            userId,
            Hash(deviceId),
            Hash(rawToken),
            now.AddHours(1),
            _clock);

        var user = User.CreateWithIdAndPassword(userId.Value, "semiauth@example.com", "Semi Auth", "hash", now.AddDays(-1));
        var product = CoffeeProduct.Create("Test Coffee", "Test Brand", CoffeeType.Espresso);
        var qrCode = ActiveQrCode.Create(spaceId, "Kitchen", product, "Espresso", 10, _clock);

        var space = Space.Create(spaceId, "Test Space", Currency.Create("USD"), userId, new InviteCode("ABCDEF"), _clock);
        var stock = CoffeeStock.Create(spaceId, _clock);
        stock.AddPurchase(product, Weight.FromGrams(100), Money.Create(10, "USD"), "Vendor", userId, now.AddMinutes(-30), _clock);

        _tokenRepository.GetByTokenHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(token);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);
        _activeQrCodeRepository.GetSingleBySpecAsync(Arg.Any<Domain.Specifications.ISpec<ActiveQrCode>>(), Arg.Any<CancellationToken>())
            .Returns(qrCode);
        _spaceRepository.GetByIdAsync(spaceId, Arg.Any<CancellationToken>()).Returns(space);
        _coffeeStockRepository.GetBySpaceIdAsync(spaceId, Arg.Any<CancellationToken>()).Returns(stock);

        var result = await _handler.Handle(
            new RecordSemiAuthQrConsumptionCommand(qrCode.Id.Value, deviceId, rawToken, 10, now),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(userId.Value);
        await _consumptionRepository.Received(1).AddAsync(Arg.Any<ConsumptionEntry>(), Arg.Any<CancellationToken>());
        await _tokenRepository.Received(1).UpdateAsync(Arg.Any<SemiAuthQrDeviceToken>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithMissingToken_ShouldReturnUnauthenticated()
    {
        _settings.Enabled.Returns(true);
        _clock.UtcNow.Returns(DateTime.UtcNow);
        _tokenRepository.GetByTokenHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((SemiAuthQrDeviceToken?)null);

        var result = await _handler.Handle(
            new RecordSemiAuthQrConsumptionCommand(Guid.NewGuid(), Guid.NewGuid().ToString(), "missing"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "UNAUTHENTICATED");
    }

    [Fact]
    public async Task Handle_WithExpiredToken_ShouldReturnUnauthenticated()
    {
        var now = DateTime.UtcNow;
        var userId = UserId.New();
        _settings.Enabled.Returns(true);
        _clock.UtcNow.Returns(now);

        var token = SemiAuthQrDeviceToken.Create(
            userId,
            Hash("d12ba4b2-337a-413f-9f41-7528f54f14ba"),
            Hash("token"),
            now.AddMinutes(-1),
            _clock);

        _tokenRepository.GetByTokenHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(token);

        var result = await _handler.Handle(
            new RecordSemiAuthQrConsumptionCommand(Guid.NewGuid(), "d12ba4b2-337a-413f-9f41-7528f54f14ba", "token"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "UNAUTHENTICATED");
    }

    [Fact]
    public async Task Handle_WithRevokedToken_ShouldReturnUnauthenticated()
    {
        var now = DateTime.UtcNow;
        var userId = UserId.New();
        _settings.Enabled.Returns(true);
        _clock.UtcNow.Returns(now);

        var token = SemiAuthQrDeviceToken.Create(
            userId,
            Hash("018f9259-5200-4a54-ab7e-cf8108f31be4"),
            Hash("token"),
            now.AddHours(1),
            _clock);
        token.Revoke("manual", _clock);

        _tokenRepository.GetByTokenHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(token);

        var result = await _handler.Handle(
            new RecordSemiAuthQrConsumptionCommand(Guid.NewGuid(), "018f9259-5200-4a54-ab7e-cf8108f31be4", "token"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "UNAUTHENTICATED");
    }

    [Fact]
    public async Task Handle_WithDeviceReplayMismatch_ShouldReturnUnauthenticated()
    {
        var now = DateTime.UtcNow;
        var userId = UserId.New();
        _settings.Enabled.Returns(true);
        _clock.UtcNow.Returns(now);

        var token = SemiAuthQrDeviceToken.Create(
            userId,
            Hash("1ef7f170-4f4b-4b7e-9357-838e95a05af0"),
            Hash("token"),
            now.AddHours(1),
            _clock);

        _tokenRepository.GetByTokenHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(token);

        var result = await _handler.Handle(
            new RecordSemiAuthQrConsumptionCommand(Guid.NewGuid(), "0df2dddf-4f50-4bf8-842f-c0f62230a65f", "token"),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "UNAUTHENTICATED");
    }

    [Fact]
    public async Task Handle_WithRemovedMembership_ShouldReturnNotMember()
    {
        var now = DateTime.UtcNow;
        var userId = UserId.New();
        var otherUserId = UserId.New();
        var spaceId = SpaceId.New();
        var product = CoffeeProduct.Create("Test", "Brand", CoffeeType.Espresso);
        const string deviceId = "c2440e31-5f4f-41fd-b74a-e02f2e5ea329";
        const string tokenRaw = "token";

        _settings.Enabled.Returns(true);
        _clock.UtcNow.Returns(now);

        var token = SemiAuthQrDeviceToken.Create(userId, Hash(deviceId), Hash(tokenRaw), now.AddHours(1), _clock);
        var user = User.CreateWithIdAndPassword(userId.Value, "u@example.com", "U", "hash", now.AddDays(-1));
        var qrCode = ActiveQrCode.Create(spaceId, "Kitchen", product, "Espresso", 10, _clock);
        var space = Space.Create(spaceId, "Space", Currency.Create("USD"), otherUserId, new InviteCode("ABCDEF"), _clock);

        _tokenRepository.GetByTokenHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(token);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);
        _activeQrCodeRepository.GetSingleBySpecAsync(Arg.Any<Domain.Specifications.ISpec<ActiveQrCode>>(), Arg.Any<CancellationToken>())
            .Returns(qrCode);
        _spaceRepository.GetByIdAsync(spaceId, Arg.Any<CancellationToken>()).Returns(space);

        var result = await _handler.Handle(
            new RecordSemiAuthQrConsumptionCommand(qrCode.Id.Value, deviceId, tokenRaw),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "NOT_MEMBER");
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldReturnUnauthenticated()
    {
        var now = DateTime.UtcNow;
        var userId = UserId.New();
        const string deviceId = "37adbf3e-4370-4f06-9126-2911f95b501a";
        const string tokenRaw = "token";

        _settings.Enabled.Returns(true);
        _clock.UtcNow.Returns(now);

        var token = SemiAuthQrDeviceToken.Create(userId, Hash(deviceId), Hash(tokenRaw), now.AddHours(1), _clock);
        var user = User.CreateWithIdAndPassword(userId.Value, "u@example.com", "U", "hash", now.AddDays(-1));
        user.Deactivate(now);

        _tokenRepository.GetByTokenHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(token);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _handler.Handle(
            new RecordSemiAuthQrConsumptionCommand(Guid.NewGuid(), deviceId, tokenRaw),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "UNAUTHENTICATED");
    }

    [Fact]
    public async Task Handle_WithInactiveQr_ShouldReturnNotFound()
    {
        var now = DateTime.UtcNow;
        var userId = UserId.New();
        var spaceId = SpaceId.New();
        var product = CoffeeProduct.Create("Test", "Brand", CoffeeType.Espresso);
        const string deviceId = "18aa2c4c-e1bb-43ce-9775-4d657a124f35";
        const string tokenRaw = "token";

        _settings.Enabled.Returns(true);
        _clock.UtcNow.Returns(now);

        var token = SemiAuthQrDeviceToken.Create(userId, Hash(deviceId), Hash(tokenRaw), now.AddHours(1), _clock);
        var user = User.CreateWithIdAndPassword(userId.Value, "u@example.com", "U", "hash", now.AddDays(-1));
        var qrCode = ActiveQrCode.Create(spaceId, "Kitchen", product, "Espresso", 10, _clock);
        qrCode.Deactivate();

        _tokenRepository.GetByTokenHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(token);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);
        _activeQrCodeRepository.GetSingleBySpecAsync(Arg.Any<Domain.Specifications.ISpec<ActiveQrCode>>(), Arg.Any<CancellationToken>())
            .Returns(qrCode);

        var result = await _handler.Handle(
            new RecordSemiAuthQrConsumptionCommand(qrCode.Id.Value, deviceId, tokenRaw),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Code == "NOT_FOUND");
    }

    private static string Hash(string input)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
