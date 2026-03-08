using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.CoffeeStock.Dtos;
using BeanShare.Application.Features.CoffeeStock.Queries;
using BeanShare.Domain.Aggregates.CoffeeStock;
using BeanShare.Domain.Aggregates.Space;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;
using FluentAssertions;
using MapsterMapper;
using NSubstitute;
using Xunit;

namespace BeanShare.Application.Tests.Features;

public sealed class CoffeeStockQueryTests
{
    private readonly ICoffeeStockRepository _coffeeStockRepository;
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUserContext _userContext;
    private readonly IMapper _mapper;
    private readonly IClock _clock;
    private readonly SpaceId _spaceId;
    private readonly UserId _userId;

    public CoffeeStockQueryTests()
    {
        _coffeeStockRepository = Substitute.For<ICoffeeStockRepository>();
        _spaceRepository = Substitute.For<ISpaceRepository>();
        _userContext = Substitute.For<IUserContext>();
        _mapper = Substitute.For<IMapper>();
        _clock = TestClock.Instance;

        _spaceId = new SpaceId(Guid.NewGuid());
        _userId = new UserId(Guid.NewGuid());

        _userContext.CurrentUserId.Returns(_userId);
    }

    [Fact]
    public async Task GetSpaceStock_WithValidSpaceAndStock_ShouldReturnStockData()
    {
        var handler = new GetSpaceStockQueryHandler(_coffeeStockRepository, _spaceRepository, _userContext, _mapper);
        var query = new GetSpaceStockQuery(_spaceId.Value);

        var space = CreateMockSpace();
        var coffeeStock = CreateSampleCoffeeStock();
        var expectedStockLevels = CreateSampleStockLevelDtos();

        _spaceRepository.GetSingleBySpecAsync(Arg.Any<SpaceByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(space);
        _coffeeStockRepository.GetBySpaceIdAsync(_spaceId, Arg.Any<CancellationToken>())
            .Returns(coffeeStock);
        _mapper.Map<List<StockLevelDto>>(Arg.Any<IReadOnlyCollection<StockLevel>>()).Returns(expectedStockLevels);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.SpaceId.Should().Be(_spaceId.Value);
        result.Value.StockLevels.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSpaceStock_WithNoStockForSpace_ShouldReturnEmptyStock()
    {
        var handler = new GetSpaceStockQueryHandler(_coffeeStockRepository, _spaceRepository, _userContext, _mapper);
        var query = new GetSpaceStockQuery(_spaceId.Value);

        var space = CreateMockSpace();

        _spaceRepository.GetSingleBySpecAsync(Arg.Any<SpaceByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(space);
        _coffeeStockRepository.GetBySpaceIdAsync(_spaceId, Arg.Any<CancellationToken>())
            .Returns((CoffeeStock?)null);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.SpaceId.Should().Be(_spaceId.Value);
        result.Value.StockLevels.Should().BeEmpty();
        result.Value.ProductVarietyCount.Should().Be(0);
    }

    [Fact]
    public async Task GetSpaceStock_WithInvalidSpace_ShouldReturnError()
    {
        var handler = new GetSpaceStockQueryHandler(_coffeeStockRepository, _spaceRepository, _userContext, _mapper);
        var query = new GetSpaceStockQuery(_spaceId.Value);

        _spaceRepository.GetSingleBySpecAsync(Arg.Any<SpaceByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns((Space?)null);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code.Contains("SPACE_NOT_FOUND"));
    }

    [Fact]
    public async Task GetSpaceStock_WithNonMember_ShouldReturnError()
    {
        var handler = new GetSpaceStockQueryHandler(_coffeeStockRepository, _spaceRepository, _userContext, _mapper);
        var query = new GetSpaceStockQuery(_spaceId.Value);

        // Create space with different user so current user is not a member
        var differentUserId = new UserId(Guid.NewGuid());
        var spaceId = new SpaceId(Guid.NewGuid());
        var inviteCode = new InviteCode("ABCDEF");
        var space = Space.Create(spaceId, "Test Space", Currency.USD, differentUserId, inviteCode, _clock);

        _spaceRepository.GetSingleBySpecAsync(Arg.Any<SpaceByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(space);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code.Contains("INSUFFICIENT_PRIVILEGES"));
    }

    [Fact]
    public async Task GetLowStockAlerts_WithValidThreshold_ShouldReturnLowStockItems()
    {
        var handler = new GetLowStockAlertsQueryHandler(_coffeeStockRepository, _spaceRepository, _userContext);
        var query = new GetLowStockAlertsQuery(_spaceId.Value, 100m);

        var space = CreateMockSpace();
        var coffeeStock = CreateSampleCoffeeStock();
        var expectedAlerts = CreateSampleLowStockAlerts();

        _spaceRepository.GetSingleBySpecAsync(Arg.Any<SpaceByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(space);
        _coffeeStockRepository.GetSingleBySpecAsync(Arg.Any<LowStockSpecification>(), Arg.Any<CancellationToken>())
            .Returns(coffeeStock);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(1);
        result.Value.First().ProductName.Should().Be("Premium Blend");
    }

    [Fact]
    public async Task GetLowStockAlerts_WithNoLowStock_ShouldReturnEmptyList()
    {
        var handler = new GetLowStockAlertsQueryHandler(_coffeeStockRepository, _spaceRepository, _userContext);
        var query = new GetLowStockAlertsQuery(_spaceId.Value, 10m); // Very low threshold

        var space = CreateMockSpace();
        var coffeeStock = CreateSampleCoffeeStock();

        _spaceRepository.GetSingleBySpecAsync(Arg.Any<SpaceByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(space);
        _coffeeStockRepository.GetBySpaceIdAsync(_spaceId, Arg.Any<CancellationToken>())
            .Returns(coffeeStock);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLowStockAlerts_WithNoStockForSpace_ShouldReturnEmptyList()
    {
        var handler = new GetLowStockAlertsQueryHandler(_coffeeStockRepository, _spaceRepository, _userContext);
        var query = new GetLowStockAlertsQuery(_spaceId.Value, 100m);

        var space = CreateMockSpace();

        _spaceRepository.GetSingleBySpecAsync(Arg.Any<SpaceByIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(space);
        _coffeeStockRepository.GetBySpaceIdAsync(_spaceId, Arg.Any<CancellationToken>())
            .Returns((CoffeeStock?)null);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    private Space CreateMockSpace()
    {
        var inviteCode = new InviteCode("ABCDEF");
        var space = Space.Create(_spaceId, "Test Space", Currency.USD, _userId, inviteCode, _clock);
        return space;
    }

    private CoffeeStock CreateSampleCoffeeStock()
    {
        var coffeeStock = CoffeeStock.Create(_spaceId, _clock);

        var product1 = CoffeeProduct.Create("Premium Blend", "Blue Mountain", CoffeeType.Espresso);
        var product2 = CoffeeProduct.Create("House Roast", "Local Roasters", CoffeeType.Filter);

        coffeeStock.AddPurchase(
            product1,
            Weight.FromGrams(1000),
            Money.Create(25.99m, "USD"),
            "Coffee Supplier Inc",
            _userId,
            _clock.UtcNow.AddDays(-1),
            _clock);

        coffeeStock.AddPurchase(
            product2,
            Weight.FromGrams(500),
            Money.Create(15.50m, "USD"),
            "Local Roasters",
            _userId,
            _clock.UtcNow.AddDays(-2),
            _clock);

        coffeeStock.ConsumeStock(product1, Weight.FromGrams(900), _clock); // Low stock
        coffeeStock.ConsumeStock(product2, Weight.FromGrams(100), _clock); // Good stock

        return coffeeStock;
    }

    private List<StockLevelDto> CreateSampleStockLevelDtos()
    {
        return new List<StockLevelDto>
        {
            new StockLevelDto
            {
                Id = Guid.NewGuid(),
                ProductName = "Premium Blend",
                ProductBrand = "Blue Mountain",
                ProductType = "Espresso",
                ProductDisplayName = "Premium Blend (Blue Mountain)",
                CurrentStockGrams = 100m,
                TotalPurchasedGrams = 1000m,
                TotalConsumedGrams = 900m,
                ConsumptionPercentage = 90m,
                UpdatedAt = _clock.UtcNow
            },
            new StockLevelDto
            {
                Id = Guid.NewGuid(),
                ProductName = "House Roast",
                ProductBrand = "Local Roasters",
                ProductType = "Filter",
                ProductDisplayName = "House Roast (Local Roasters)",
                CurrentStockGrams = 400m,
                TotalPurchasedGrams = 500m,
                TotalConsumedGrams = 100m,
                ConsumptionPercentage = 20m,
                UpdatedAt = _clock.UtcNow
            }
        };
    }

    private IEnumerable<LowStockAlertDto> CreateSampleLowStockAlerts()
    {
        return new List<LowStockAlertDto>
        {
            new LowStockAlertDto
            {
                StockLevelId = Guid.NewGuid(),
                ProductName = "Premium Blend",
                ProductBrand = "Blue Mountain",
                ProductType = "Espresso",
                ProductDisplayName = "Premium Blend (Blue Mountain)",
                CurrentStockGrams = 100m,
                ThresholdGrams = 100m,
                AlertLevel = "Low",
                LastUpdated = _clock.UtcNow
            }
        };
    }
}