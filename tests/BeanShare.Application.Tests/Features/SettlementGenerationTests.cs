using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Settlement.Commands.GenerateSettlement;
using BeanShare.Application.Services;
using BeanShare.Domain.Aggregates.BillingPeriod;
using BeanShare.Domain.Aggregates.CoffeeStock;
using BeanShare.Domain.Aggregates.Space;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.Enums;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace BeanShare.Application.Tests.Features;

public sealed class SettlementGenerationTests
{
    private readonly ISettlementRepository _settlementRepository;
    private readonly IBillingPeriodRepository _billingPeriodRepository;
    private readonly IConsumptionRepository _consumptionRepository;
    private readonly ICoffeeStockRepository _coffeeStockRepository;
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUserContext _userContext;
    private readonly IUserService _userService;
    private readonly IClock _clock;

    private readonly UserId _adminUserId;
    private readonly UserId _user1Id;
    private readonly UserId _user2Id;
    private readonly SpaceId _spaceId;
    private readonly Space _space;
    private readonly DateTime _fixedNow;

    public SettlementGenerationTests()
    {
        _settlementRepository = Substitute.For<ISettlementRepository>();
        _billingPeriodRepository = Substitute.For<IBillingPeriodRepository>();
        _consumptionRepository = Substitute.For<IConsumptionRepository>();
        _coffeeStockRepository = Substitute.For<ICoffeeStockRepository>();
        _spaceRepository = Substitute.For<ISpaceRepository>();
        _userContext = Substitute.For<IUserContext>();
        _userService = Substitute.For<IUserService>();
        _clock = Substitute.For<IClock>();

        _adminUserId = new UserId(Guid.NewGuid());
        _user1Id = new UserId(Guid.NewGuid());
        _user2Id = new UserId(Guid.NewGuid());
        _spaceId = SpaceId.New();
        _fixedNow = new DateTime(2025, 10, 26, 10, 0, 0, DateTimeKind.Utc);

        _clock.UtcNow.Returns(_fixedNow);
        _userContext.CurrentUserId.Returns(_adminUserId);

        _space = Space.Create(_spaceId, "Test Space", Currency.USD, _adminUserId, new InviteCode("TESTAB"), _clock);
        _space.Join(_user1Id, _clock);
        _space.Join(_user2Id, _clock);
    }

    [Fact]
    public async Task GenerateSettlement_WithClosedPeriod_ShouldSucceed()
    {
        var billingPeriod = CreateClosedBillingPeriod();
        var consumptions = CreateConsumptions(billingPeriod.Id);
        var coffeeStock = CreateCoffeeStock();

        SetupRepositories(billingPeriod, consumptions, coffeeStock);

        var handler = CreateHandler();
        var command = new GenerateSettlementCommand(billingPeriod.Id.Value);

        var result = await handler.Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.BillingPeriodId.Should().Be(billingPeriod.Id.Value);
        result.Value.Lines.Should().HaveCount(2);
        result.Value.TotalAmount.Should().BeGreaterThan(0);
        await _settlementRepository.Received(1).AddAsync(Arg.Any<Domain.Aggregates.Settlement.Settlement>(), default);
        await _billingPeriodRepository.Received(1).UpdateAsync(Arg.Is<BillingPeriod>(bp => bp.State == BillingState.Settled), default);
    }

    [Fact]
    public async Task GenerateSettlement_WhenBillingPeriodNotFound_ShouldFail()
    {
        _billingPeriodRepository.GetByIdAsync(Arg.Any<BillingPeriodId>(), default).Returns((BillingPeriod?)null);

        var handler = CreateHandler();
        var command = new GenerateSettlementCommand(Guid.NewGuid());

        var result = await handler.Handle(command, default);

        result.IsSuccess.Should().BeFalse();
        result.Errors[0].Code.Should().Be("BILLING_PERIOD_NOT_FOUND");
        await _settlementRepository.DidNotReceive().AddAsync(Arg.Any<Domain.Aggregates.Settlement.Settlement>(), default);
    }

    [Fact]
    public async Task GenerateSettlement_WhenPeriodNotClosed_ShouldFail()
    {
        var billingPeriod = BillingPeriod.Create(
            _spaceId,
            "Test Period",
            new DateTime(2025, 10, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 10, 31, 0, 0, 0, DateTimeKind.Utc),
            _adminUserId,
            _clock);

        billingPeriod.Open(_adminUserId, _clock);

        _billingPeriodRepository.GetByIdAsync(Arg.Any<BillingPeriodId>(), default).Returns(billingPeriod);

        var handler = CreateHandler();
        var command = new GenerateSettlementCommand(billingPeriod.Id.Value);

        var result = await handler.Handle(command, default);

        result.IsSuccess.Should().BeFalse();
        result.Errors[0].Code.Should().Be("INVALID_BILLING_PERIOD_STATE");
        await _settlementRepository.DidNotReceive().AddAsync(Arg.Any<Domain.Aggregates.Settlement.Settlement>(), default);
    }

    [Fact]
    public async Task GenerateSettlement_WhenSettlementAlreadyExists_ShouldFail()
    {
        var billingPeriod = CreateClosedBillingPeriod();
        var existingSettlement = Domain.Aggregates.Settlement.Settlement.Create(
            _spaceId,
            billingPeriod.Id,
            "USD",
            _adminUserId,
            _clock);

        _billingPeriodRepository.GetByIdAsync(Arg.Any<BillingPeriodId>(), default).Returns(billingPeriod);
        _settlementRepository.GetByBillingPeriodIdAsync(billingPeriod.Id, default).Returns(existingSettlement);

        var handler = CreateHandler();
        var command = new GenerateSettlementCommand(billingPeriod.Id.Value);

        var result = await handler.Handle(command, default);

        result.IsSuccess.Should().BeFalse();
        result.Errors[0].Code.Should().Be("SETTLEMENT_ALREADY_EXISTS");
    }

    [Fact]
    public async Task GenerateSettlement_WhenUserNotAdmin_ShouldFail()
    {
        var billingPeriod = CreateClosedBillingPeriod();
        var consumptions = CreateConsumptions(billingPeriod.Id);
        var coffeeStock = CreateCoffeeStock();

        _userContext.CurrentUserId.Returns(_user1Id);
        SetupRepositories(billingPeriod, consumptions, coffeeStock);

        var handler = CreateHandler();
        var command = new GenerateSettlementCommand(billingPeriod.Id.Value);

        var result = await handler.Handle(command, default);

        result.IsSuccess.Should().BeFalse();
        result.Errors[0].Code.Should().Be("INSUFFICIENT_PRIVILEGES");
        await _settlementRepository.DidNotReceive().AddAsync(Arg.Any<Domain.Aggregates.Settlement.Settlement>(), default);
    }

    [Fact]
    public async Task GenerateSettlement_WhenNoConsumptions_ShouldFail()
    {
        var billingPeriod = CreateClosedBillingPeriod();
        var coffeeStock = CreateCoffeeStock();

        _billingPeriodRepository.GetByIdAsync(Arg.Any<BillingPeriodId>(), default).Returns(billingPeriod);
        _spaceRepository.GetSingleBySpecAsync(Arg.Any<ISpec<Space>>(), default).Returns(_space);
        _consumptionRepository.GetBySpecAsync(Arg.Any<ISpec<ConsumptionEntry>>(), default).Returns(new List<ConsumptionEntry>());
        _coffeeStockRepository.GetBySpaceIdAsync(_spaceId, default).Returns(coffeeStock);
        _settlementRepository.GetByBillingPeriodIdAsync(billingPeriod.Id, default).Returns((Domain.Aggregates.Settlement.Settlement?)null);

        var handler = CreateHandler();
        var command = new GenerateSettlementCommand(billingPeriod.Id.Value);

        var result = await handler.Handle(command, default);

        result.IsSuccess.Should().BeFalse();
        result.Errors[0].Code.Should().Be("NO_CONSUMPTIONS_IN_PERIOD");
        await _settlementRepository.DidNotReceive().AddAsync(Arg.Any<Domain.Aggregates.Settlement.Settlement>(), default);
    }

    [Fact]
    public async Task GenerateSettlement_ShouldCalculateWeightedAverageCost()
    {
        var billingPeriod = CreateClosedBillingPeriod();
        var consumptions = new List<ConsumptionEntry>
        {
            CreateConsumption(_user1Id, billingPeriod.Id, 18m),
            CreateConsumption(_user2Id, billingPeriod.Id, 18m)
        };

        var coffeeStock = CreateCoffeeStockWithPurchases(1000m, 20.00m);

        SetupRepositories(billingPeriod, consumptions, coffeeStock);

        var handler = CreateHandler();
        var command = new GenerateSettlementCommand(billingPeriod.Id.Value);

        var result = await handler.Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        var expectedCostPerGram = 20.00m / 1000m;
        var expectedTotalCost = 36m * expectedCostPerGram;
        result.Value.TotalAmount.Should().BeApproximately(expectedTotalCost, 0.01m);
    }

    [Fact]
    public async Task GenerateSettlement_ShouldDistributeCostProportionally()
    {
        var billingPeriod = CreateClosedBillingPeriod();
        var consumptions = new List<ConsumptionEntry>
        {
            CreateConsumption(_user1Id, billingPeriod.Id, 18m),
            CreateConsumption(_user1Id, billingPeriod.Id, 18m),
            CreateConsumption(_user2Id, billingPeriod.Id, 18m)
        };

        var coffeeStock = CreateCoffeeStock();

        SetupRepositories(billingPeriod, consumptions, coffeeStock);

        var handler = CreateHandler();
        var command = new GenerateSettlementCommand(billingPeriod.Id.Value);

        var result = await handler.Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        var user1Line = result.Value.Lines.First(l => l.UserId == _user1Id.Value);
        var user2Line = result.Value.Lines.First(l => l.UserId == _user2Id.Value);

        user1Line.TotalCoffeeGrams.Should().Be(36m);
        user2Line.TotalCoffeeGrams.Should().Be(18m);
        user1Line.AmountDue.Should().BeApproximately(user2Line.AmountDue * 2, 0.01m);
        user1Line.ConsumptionPercentage.Should().BeApproximately(66.67m, 0.1m);
        user2Line.ConsumptionPercentage.Should().BeApproximately(33.33m, 0.1m);
    }

    [Fact]
    public async Task GenerateSettlement_ShouldMarkBillingPeriodAsSettled()
    {
        var billingPeriod = CreateClosedBillingPeriod();
        var consumptions = CreateConsumptions(billingPeriod.Id);
        var coffeeStock = CreateCoffeeStock();

        SetupRepositories(billingPeriod, consumptions, coffeeStock);

        var handler = CreateHandler();
        var command = new GenerateSettlementCommand(billingPeriod.Id.Value);

        var result = await handler.Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        billingPeriod.State.Should().Be(BillingState.Settled);
        billingPeriod.SettledBy.Should().Be(_adminUserId);
        billingPeriod.SettledAt.Should().Be(_fixedNow);
    }

    [Fact]
    public async Task GenerateSettlement_WithNoPurchasesInPeriod_ShouldUseAllPurchases()
    {
        var billingPeriod = BillingPeriod.Create(
            _spaceId,
            "October 2025",
            new DateTime(2025, 10, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 10, 31, 0, 0, 0, DateTimeKind.Utc),
            _adminUserId,
            _clock);

        billingPeriod.Open(_adminUserId, _clock);
        billingPeriod.Close(_adminUserId, _clock);

        var consumptions = CreateConsumptions(billingPeriod.Id);
        var coffeeStock = CreateCoffeeStockWithPurchases(1000m, 20.00m, new DateTime(2025, 9, 1, 0, 0, 0, DateTimeKind.Utc));

        SetupRepositories(billingPeriod, consumptions, coffeeStock);

        var handler = CreateHandler();
        var command = new GenerateSettlementCommand(billingPeriod.Id.Value);

        var result = await handler.Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalAmount.Should().BeGreaterThan(0);
    }

    private BillingPeriod CreateClosedBillingPeriod()
    {
        var period = BillingPeriod.Create(
            _spaceId,
            "Test Period",
            new DateTime(2025, 10, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 10, 31, 0, 0, 0, DateTimeKind.Utc),
            _adminUserId,
            _clock);

        period.Open(_adminUserId, _clock);
        period.Close(_adminUserId, _clock);

        return period;
    }

    private List<ConsumptionEntry> CreateConsumptions(BillingPeriodId billingPeriodId)
    {
        return new List<ConsumptionEntry>
        {
            CreateConsumption(_user1Id, billingPeriodId, 18m),
            CreateConsumption(_user2Id, billingPeriodId, 18m)
        };
    }

    private ConsumptionEntry CreateConsumption(UserId userId, BillingPeriodId billingPeriodId, decimal grams)
    {
        var product = CoffeeProduct.Create("Test Coffee", "Test Brand", CoffeeType.Espresso);
        var consumption = ConsumptionEntry.Create(
            _spaceId,
            userId,
            product,
            Weight.FromGrams(grams),
            new DateTime(2025, 10, 15, 9, 0, 0, DateTimeKind.Utc),
            _clock);

        consumption.AssignToBillingPeriod(billingPeriodId);

        return consumption;
    }

    private CoffeeStock CreateCoffeeStock()
    {
        return CreateCoffeeStockWithPurchases(1000m, 20.00m);
    }

    private CoffeeStock CreateCoffeeStockWithPurchases(decimal grams, decimal cost, DateTime? purchaseDate = null)
    {
        var stock = CoffeeStock.Create(_spaceId, _clock);
        var product = CoffeeProduct.Create("Test Coffee", "Test Brand", CoffeeType.Espresso);
        stock.AddPurchase(
            product,
            Weight.FromGrams(grams),
            Money.Create(cost, "USD"),
            "Test Vendor",
            _adminUserId,
            purchaseDate ?? new DateTime(2025, 10, 15, 0, 0, 0, DateTimeKind.Utc),
            _clock);

        return stock;
    }

    private void SetupRepositories(BillingPeriod billingPeriod, List<ConsumptionEntry> consumptions, CoffeeStock coffeeStock)
    {
        _billingPeriodRepository.GetByIdAsync(Arg.Any<BillingPeriodId>(), default).Returns(billingPeriod);
        _spaceRepository.GetSingleBySpecAsync(Arg.Any<ISpec<Space>>(), default).Returns(_space);
        _consumptionRepository.GetBySpecAsync(Arg.Any<ISpec<ConsumptionEntry>>(), default).Returns(consumptions);
        _coffeeStockRepository.GetBySpaceIdAsync(_spaceId, default).Returns(coffeeStock);
        _settlementRepository.GetByBillingPeriodIdAsync(billingPeriod.Id, default).Returns((Domain.Aggregates.Settlement.Settlement?)null);
    }

    private GenerateSettlementHandler CreateHandler()
    {
        return new GenerateSettlementHandler(
            _settlementRepository,
            _billingPeriodRepository,
            _consumptionRepository,
            _coffeeStockRepository,
            _spaceRepository,
            _userContext,
            _userService,
            _clock);
    }
}
