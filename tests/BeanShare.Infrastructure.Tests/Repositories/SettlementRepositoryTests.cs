using BeanShare.Domain.Aggregates.Settlement;
using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;
using BeanShare.Infrastructure.Persistence.Repositories;
using BeanShare.Infrastructure.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace BeanShare.Infrastructure.Tests.Repositories;

public sealed class SettlementRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;

    public SettlementRepositoryTests(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
    }

    /// <summary>
    /// Regression test: verifies that <see cref="SettlementRepository.UpdateAsync"/> correctly
    /// persists a payment confirmation change made via <c>ConfirmPayment</c>.
    /// Previously the repository called <c>context.Settlements.Update(settlement)</c> on an
    /// already-tracked aggregate. This test ensures the fix (removing that call) does not
    /// regress payment confirmation persistence.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_WhenPaymentConfirmed_ShouldPersistConfirmation()
    {
        var clock = new TestClock();
        var spaceId = new SpaceId(Guid.NewGuid());
        var billingPeriodId = BillingPeriodId.New();
        var generatorId = new UserId(Guid.NewGuid());
        var memberId = new UserId(Guid.NewGuid());
        SettlementId settlementId;

        // Arrange: create and persist a settlement with one line requiring payment
        await using (var writeCtx = await _databaseFixture.CreateDbContextAsync())
        {
            var settlement = Settlement.Create(spaceId, billingPeriodId, "USD", generatorId, clock);
            settlementId = settlement.Id;

            settlement.AddLine(memberId, totalCoffeeGrams: 100, amountDue: 5.00m, clock);
            settlement.FinalizeGeneration(clock);

            await writeCtx.Settlements.AddAsync(settlement);
            await writeCtx.SaveChangesAsync();
        }

        // Act: load in a fresh context, confirm payment, call UpdateAsync, save
        await using (var updateCtx = await _databaseFixture.CreateDbContextAsync())
        {
            var repo = new SettlementRepository(updateCtx);
            var settlement = await repo.GetByIdAsync(settlementId);
            settlement.Should().NotBeNull();

            settlement!.ConfirmPayment(memberId, generatorId, clock);
            await repo.UpdateAsync(settlement);
            await updateCtx.SaveChangesAsync();
        }

        // Assert: reload in a third context and verify the confirmation is persisted
        await using var readCtx = await _databaseFixture.CreateDbContextAsync();
        var readRepo = new SettlementRepository(readCtx);
        var reloaded = await readRepo.GetByIdAsync(settlementId);

        reloaded.Should().NotBeNull();
        var line = reloaded!.GetLineForUser(memberId);
        line.Should().NotBeNull();
        line!.IsConfirmed.Should().BeTrue("the payment should have been persisted by UpdateAsync + SaveChanges");
    }

    private sealed class TestClock : IClock
    {
        public DateTime UtcNow => new(2025, 9, 14, 12, 0, 0, DateTimeKind.Utc);
    }
}
