using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Consumption.Queries.GetUserConsumptionHistory;
using BeanShare.Application.Services;
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

public sealed class ConsumptionHistoryQueryTests
{
    private readonly IConsumptionRepository _consumptionRepository;
    private readonly ISpaceRepository _spaceRepository;
    private readonly ICostCalculationService _costCalculationService;
    private readonly IUserContext _userContext;
    private readonly IClock _clock;
    private readonly GetUserConsumptionHistoryHandler _handler;

    private readonly UserId _currentUserId;
    private readonly SpaceId _spaceId;

    public ConsumptionHistoryQueryTests()
    {
        _consumptionRepository = Substitute.For<IConsumptionRepository>();
        _spaceRepository = Substitute.For<ISpaceRepository>();
        _costCalculationService = Substitute.For<ICostCalculationService>();
        _userContext = Substitute.For<IUserContext>();
        _clock = Substitute.For<IClock>();

        _currentUserId = new UserId(Guid.NewGuid());
        _spaceId = SpaceId.New();

        _userContext.CurrentUserId.Returns(_currentUserId);
        _clock.UtcNow.Returns(new DateTime(2025, 10, 26, 10, 0, 0, DateTimeKind.Utc));

        _handler = new GetUserConsumptionHistoryHandler(
            _consumptionRepository,
            _spaceRepository,
            _costCalculationService,
            _userContext);
    }

    [Fact]
    public async Task GetUserConsumptionHistory_WhenUserHasNoSpaces_ShouldReturnEmptyResult()
    {
        _spaceRepository.GetBySpecAsync(Arg.Any<ISpec<Space>>(), default).Returns(new List<Space>());

        var query = new GetUserConsumptionHistoryQuery(null, null, null, null, 1, 20);

        var result = await _handler.Handle(query, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.Summary.TotalEntries.Should().Be(0);
    }

    [Fact]
    public async Task GetUserConsumptionHistory_WhenUserHasConsumptions_ShouldReturnPaginatedHistory()
    {
        var space = CreateSpace();
        var consumptions = CreateConsumptions(5);

        _spaceRepository.GetBySpecAsync(Arg.Any<ISpec<Space>>(), default).Returns(new List<Space> { space });
        _consumptionRepository.GetBySpecAsync(Arg.Any<ISpec<ConsumptionEntry>>(), default)
            .Returns(callInfo =>
            {
                var spec = callInfo.Arg<ISpec<ConsumptionEntry>>();
                var predicate = spec.Criteria.Compile();
                return consumptions.Where(predicate).ToList();
            });

        var query = new GetUserConsumptionHistoryQuery(null, null, null, null, 1, 3);

        var result = await _handler.Handle(query, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(5);
        result.Value.Items.Should().HaveCount(3);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(3);
    }

    [Fact]
    public async Task GetUserConsumptionHistory_WithDateRangeFilter_ShouldFilterByDates()
    {
        var space = CreateSpace();
        var consumptions = new List<ConsumptionEntry>
        {
            CreateConsumption(new DateTime(2025, 10, 1, 9, 0, 0, DateTimeKind.Utc)),
            CreateConsumption(new DateTime(2025, 10, 15, 9, 0, 0, DateTimeKind.Utc)),
            CreateConsumption(new DateTime(2025, 10, 25, 9, 0, 0, DateTimeKind.Utc))
        };

        _spaceRepository.GetBySpecAsync(Arg.Any<ISpec<Space>>(), default).Returns(new List<Space> { space });
        _consumptionRepository.GetBySpecAsync(Arg.Any<ISpec<ConsumptionEntry>>(), default)
            .Returns(callInfo =>
            {
                var spec = callInfo.Arg<ISpec<ConsumptionEntry>>();
                var predicate = spec.Criteria.Compile();
                return consumptions.Where(predicate).ToList();
            });

        var query = new GetUserConsumptionHistoryQuery(
            null,
            new DateTime(2025, 10, 10, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 10, 20, 0, 0, 0, DateTimeKind.Utc),
            null,
            1,
            20);

        var result = await _handler.Handle(query, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(1);
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.First().ConsumedAt.Should().Be(new DateTime(2025, 10, 15, 9, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task GetUserConsumptionHistory_WithSpaceFilter_ShouldFilterBySpace()
    {
        var space = CreateSpace();
        var consumptions = CreateConsumptions(3);

        _spaceRepository.GetBySpecAsync(Arg.Any<ISpec<Space>>(), default).Returns(new List<Space> { space });
        _consumptionRepository.GetBySpecAsync(Arg.Any<ISpec<ConsumptionEntry>>(), default)
            .Returns(callInfo =>
            {
                var spec = callInfo.Arg<ISpec<ConsumptionEntry>>();
                var predicate = spec.Criteria.Compile();
                return consumptions.Where(predicate).ToList();
            });

        var query = new GetUserConsumptionHistoryQuery(_spaceId.Value, null, null, null, 1, 20);

        var result = await _handler.Handle(query, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().AllSatisfy(item => item.SpaceId.Should().Be(_spaceId.Value));
    }

    [Fact]
    public async Task GetUserConsumptionHistory_WithBillingPeriodFilter_ShouldFilterByPeriod()
    {
        var space = CreateSpace();
        var billingPeriodId = new BillingPeriodId(Guid.NewGuid());
        var consumptions = new List<ConsumptionEntry>
        {
            CreateConsumption(_clock.UtcNow.AddHours(-1), billingPeriodId),
            CreateConsumption(_clock.UtcNow.AddHours(-2), null),
            CreateConsumption(_clock.UtcNow.AddHours(-3), billingPeriodId)
        };

        _spaceRepository.GetBySpecAsync(Arg.Any<ISpec<Space>>(), default).Returns(new List<Space> { space });
        _consumptionRepository.GetBySpecAsync(Arg.Any<ISpec<ConsumptionEntry>>(), default)
            .Returns(callInfo =>
            {
                var spec = callInfo.Arg<ISpec<ConsumptionEntry>>();
                var predicate = spec.Criteria.Compile();
                return consumptions.Where(predicate).ToList();
            });

        var query = new GetUserConsumptionHistoryQuery(null, null, null, billingPeriodId.Value, 1, 20);

        var result = await _handler.Handle(query, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(2);
        result.Value.Items.Should().AllSatisfy(item =>
            item.BillingPeriodId.Should().Be(billingPeriodId.Value));
    }

    [Fact]
    public async Task GetUserConsumptionHistory_WhenRequestingNonMemberSpace_ShouldReturnError()
    {
        var requestedSpaceId = Guid.NewGuid();
        var query = new GetUserConsumptionHistoryQuery(requestedSpaceId, null, null, null, 1, 20);

        var userSpace = Space.Create(
            _spaceId,
            "My Space",
            Currency.USD,
            _currentUserId,
            new InviteCode("BBBBBB"),
            _clock);

        _spaceRepository.GetBySpecAsync(Arg.Any<ISpec<Space>>(), default).Returns(new List<Space> { userSpace });

        var nonMemberSpace = Space.Create(
            new SpaceId(requestedSpaceId),
            "Other Space",
            Currency.USD,
            new UserId(Guid.NewGuid()),
            new InviteCode("AAAAAA"),
            _clock);

        _spaceRepository.GetSingleBySpecAsync(Arg.Any<ISpec<Space>>(), default).Returns(nonMemberSpace);

        var result = await _handler.Handle(query, default);

        result.IsSuccess.Should().BeFalse();
        result.Errors[0].Code.Should().Be("INSUFFICIENT_PRIVILEGES");
    }

    [Fact]
    public async Task GetUserConsumptionHistory_ShouldCalculateSummaryStatistics()
    {
        var space = CreateSpace();
        var consumptions = new List<ConsumptionEntry>
        {
            CreateConsumption(new DateTime(2025, 10, 1, 9, 0, 0, DateTimeKind.Utc), coffeeGrams: 18m),
            CreateConsumption(new DateTime(2025, 10, 1, 14, 0, 0, DateTimeKind.Utc), coffeeGrams: 18m),
            CreateConsumption(new DateTime(2025, 10, 2, 9, 0, 0, DateTimeKind.Utc), coffeeGrams: 250m, type: CoffeeType.Filter)
        };

        _spaceRepository.GetBySpecAsync(Arg.Any<ISpec<Space>>(), default).Returns(new List<Space> { space });
        _consumptionRepository.GetBySpecAsync(Arg.Any<ISpec<ConsumptionEntry>>(), default)
            .Returns(callInfo =>
            {
                var spec = callInfo.Arg<ISpec<ConsumptionEntry>>();
                var predicate = spec.Criteria.Compile();
                return consumptions.Where(predicate).ToList();
            });

        var query = new GetUserConsumptionHistoryQuery(null, null, null, null, 1, 20);

        var result = await _handler.Handle(query, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Summary.TotalGrams.Should().Be(286m);
        result.Value.Summary.TotalEntries.Should().Be(3);
        result.Value.Summary.UniqueDays.Should().Be(2);
        result.Value.Summary.AverageGramsPerDay.Should().Be(143m);
        result.Value.Summary.ConsumptionByType.Should().ContainKey("Espresso").WhoseValue.Should().Be(2);
        result.Value.Summary.ConsumptionByType.Should().ContainKey("Filter").WhoseValue.Should().Be(1);
    }

    [Fact]
    public async Task GetUserConsumptionHistory_ShouldOrderByConsumedAtDescending()
    {
        var space = CreateSpace();
        var consumptions = new List<ConsumptionEntry>
        {
            CreateConsumption(new DateTime(2025, 10, 1, 9, 0, 0, DateTimeKind.Utc)),
            CreateConsumption(new DateTime(2025, 10, 3, 9, 0, 0, DateTimeKind.Utc)),
            CreateConsumption(new DateTime(2025, 10, 2, 9, 0, 0, DateTimeKind.Utc))
        };

        _spaceRepository.GetBySpecAsync(Arg.Any<ISpec<Space>>(), default).Returns(new List<Space> { space });
        _consumptionRepository.GetBySpecAsync(Arg.Any<ISpec<ConsumptionEntry>>(), default)
            .Returns(callInfo =>
            {
                var spec = callInfo.Arg<ISpec<ConsumptionEntry>>();
                var predicate = spec.Criteria.Compile();
                return consumptions.Where(predicate).ToList();
            });

        var query = new GetUserConsumptionHistoryQuery(null, null, null, null, 1, 20);

        var result = await _handler.Handle(query, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeInDescendingOrder(item => item.ConsumedAt);
        result.Value.Items.First().ConsumedAt.Should().Be(new DateTime(2025, 10, 3, 9, 0, 0, DateTimeKind.Utc));
    }

    private Space CreateSpace()
    {
        var space = Space.Create(_spaceId, "Test Space", Currency.USD, _currentUserId, new InviteCode("TESTAB"), _clock);
        return space;
    }

    private List<ConsumptionEntry> CreateConsumptions(int count)
    {
        var consumptions = new List<ConsumptionEntry>();
        for (int i = 0; i < count; i++)
        {
            consumptions.Add(CreateConsumption(_clock.UtcNow.AddHours(-i - 1)));
        }
        return consumptions;
    }

    private ConsumptionEntry CreateConsumption(
        DateTime consumedAt,
        BillingPeriodId? billingPeriodId = null,
        decimal coffeeGrams = 18m,
        CoffeeType type = CoffeeType.Espresso)
    {
        var product = CoffeeProduct.Create("Test Coffee", "Test Brand", type);
        var consumption = ConsumptionEntry.Create(
            _spaceId,
            _currentUserId,
            product,
            Weight.FromGrams(coffeeGrams),
            consumedAt,
            _clock);

        if (billingPeriodId.HasValue)
        {
            consumption.AssignToBillingPeriod(billingPeriodId.Value);
        }

        return consumption;
    }
}
