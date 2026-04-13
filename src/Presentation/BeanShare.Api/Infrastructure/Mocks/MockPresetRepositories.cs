using System.Security.Claims;
using BeanShare.Application.Abstractions;
using BeanShare.Application.Features.Settlement.Dtos;
using BeanShare.Application.Services;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.ValueObjects;
using BeanShare.Domain.Enums;
using BeanShare.Infrastructure.Services.Documents;

namespace BeanShare.Api.Infrastructure.Mocks;

public sealed class MockGlobalPresetRepository : IGlobalPresetRepository
{
    public Task<GlobalPreset?> GetByIdAsync(GlobalPresetId id, CancellationToken ct = default) => Task.FromResult<GlobalPreset?>(null);
    public Task<IReadOnlyList<GlobalPreset>> GetAllActiveAsync(CancellationToken ct = default) => Task.FromResult<IReadOnlyList<GlobalPreset>>(new List<GlobalPreset>());
    public Task<IReadOnlyList<GlobalPreset>> GetAllAsync(CancellationToken ct = default) => Task.FromResult<IReadOnlyList<GlobalPreset>>(new List<GlobalPreset>());
    public Task AddAsync(GlobalPreset preset, CancellationToken ct = default) => Task.CompletedTask;
    public Task UpdateAsync(GlobalPreset preset, CancellationToken ct = default) => Task.CompletedTask;
    public Task<bool> ExistsAsync(GlobalPresetId id, CancellationToken ct = default) => Task.FromResult(false);
}

public sealed class MockPresetRecipeRepository : IPresetRecipeRepository
{
    public Task<PresetRecipe?> GetByIdAsync(PresetRecipeId id, CancellationToken ct = default) => Task.FromResult<PresetRecipe?>(null);
    public Task<IReadOnlyList<PresetRecipe>> GetByUserIdAsync(UserId userId, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<PresetRecipe>>(new List<PresetRecipe>());
    public Task<IReadOnlyList<PresetRecipe>> GetBySpaceIdAsync(SpaceId spaceId, bool sharedOnly = false, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<PresetRecipe>>(new List<PresetRecipe>());
    public Task<IReadOnlyList<PresetRecipe>> GetUserPresetsForSpaceAsync(UserId userId, SpaceId spaceId, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<PresetRecipe>>(new List<PresetRecipe>());
    public Task AddAsync(PresetRecipe preset, CancellationToken ct = default) => Task.CompletedTask;
    public Task UpdateAsync(PresetRecipe preset, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(PresetRecipeId id, CancellationToken ct = default) => Task.CompletedTask;
    public Task<bool> ExistsAsync(PresetRecipeId id, CancellationToken ct = default) => Task.FromResult(false);
}

public sealed class MockSpaceGlobalPresetConfigRepository : ISpaceGlobalPresetConfigRepository
{
    public Task<SpaceGlobalPresetConfig?> GetByIdAsync(SpaceGlobalPresetConfigId id, CancellationToken ct = default) => Task.FromResult<SpaceGlobalPresetConfig?>(null);
    public Task<SpaceGlobalPresetConfig?> GetBySpaceAndPresetAsync(SpaceId spaceId, GlobalPresetId globalPresetId, CancellationToken ct = default) => Task.FromResult<SpaceGlobalPresetConfig?>(null);
    public Task<IReadOnlyList<SpaceGlobalPresetConfig>> GetBySpaceIdAsync(SpaceId spaceId, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<SpaceGlobalPresetConfig>>(new List<SpaceGlobalPresetConfig>());
    public Task<IReadOnlyList<GlobalPresetId>> GetDisabledPresetIdsForSpaceAsync(SpaceId spaceId, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<GlobalPresetId>>(new List<GlobalPresetId>());
    public Task AddAsync(SpaceGlobalPresetConfig config, CancellationToken ct = default) => Task.CompletedTask;
    public Task UpdateAsync(SpaceGlobalPresetConfig config, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(SpaceGlobalPresetConfigId id, CancellationToken ct = default) => Task.CompletedTask;
}

public sealed class MockUserPresetFavoriteRepository : IUserPresetFavoriteRepository
{
    public Task<UserPresetFavorite?> GetByIdAsync(UserPresetFavoriteId id, CancellationToken ct = default) => Task.FromResult<UserPresetFavorite?>(null);
    public Task<IReadOnlyList<UserPresetFavorite>> GetByUserAndSpaceAsync(UserId userId, SpaceId spaceId, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<UserPresetFavorite>>(new List<UserPresetFavorite>());
    public Task<UserPresetFavorite?> GetByUserSpaceAndGlobalPresetAsync(UserId userId, SpaceId spaceId, GlobalPresetId globalPresetId, CancellationToken ct = default) => Task.FromResult<UserPresetFavorite?>(null);
    public Task<UserPresetFavorite?> GetByUserSpaceAndSpacePresetAsync(UserId userId, SpaceId spaceId, PresetRecipeId presetRecipeId, CancellationToken ct = default) => Task.FromResult<UserPresetFavorite?>(null);
    public Task<int> GetMaxDisplayOrderAsync(UserId userId, SpaceId spaceId, CancellationToken ct = default) => Task.FromResult(0);
    public Task AddAsync(UserPresetFavorite favorite, CancellationToken ct = default) => Task.CompletedTask;
    public Task UpdateAsync(UserPresetFavorite favorite, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(UserPresetFavoriteId id, CancellationToken ct = default) => Task.CompletedTask;
}

public sealed class MockNotificationRepository : INotificationRepository
{
    public Task<Notification?> GetByIdAsync(NotificationId id, CancellationToken cancellationToken = default) => Task.FromResult<Notification?>(null);
    public Task<IReadOnlyList<Notification>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Notification>>(new List<Notification>());
    public Task<IReadOnlyList<Notification>> GetUnreadByUserIdAsync(UserId userId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Notification>>(new List<Notification>());
    public Task<int> GetUnreadCountByUserIdAsync(UserId userId, CancellationToken cancellationToken = default) => Task.FromResult(0);
    public Task AddAsync(Notification notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task DeleteAsync(Notification notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task DeleteAllByUserIdAsync(UserId userId, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task MarkAllAsReadByUserIdAsync(UserId userId, IClock clock, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

public sealed class MockExchangeRateRepository : IExchangeRateRepository
{
    public Task<ExchangeRate?> GetRateAsync(string baseCurrency, string targetCurrency, CancellationToken ct = default) => Task.FromResult<ExchangeRate?>(null);
    public Task<IReadOnlyList<ExchangeRate>> GetAllRatesAsync(CancellationToken ct = default) => Task.FromResult<IReadOnlyList<ExchangeRate>>(new List<ExchangeRate>());
    public Task<DateTime?> GetLastFetchTimeAsync(CancellationToken ct = default) => Task.FromResult<DateTime?>(null);
    public Task UpsertRateAsync(ExchangeRate rate, CancellationToken ct = default) => Task.CompletedTask;
    public Task UpsertRatesAsync(IEnumerable<ExchangeRate> rates, CancellationToken ct = default) => Task.CompletedTask;
}

public sealed class MockUserRepository : IUserRepository
{
    public Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default) => Task.FromResult<User?>(null);
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) => Task.FromResult<User?>(null);
    public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default) => Task.FromResult<IReadOnlyList<User>>(new List<User>());
    public Task<IReadOnlyList<User>> GetAllActiveAsync(CancellationToken ct = default) => Task.FromResult<IReadOnlyList<User>>(new List<User>());
    public Task<IReadOnlyList<User>> GetBySystemRoleAsync(SystemRole role, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<User>>(new List<User>());
    public Task<(IReadOnlyList<User> Users, int TotalCount)> GetPagedAsync(int page, int pageSize, string? searchTerm = null, SystemRole? roleFilter = null, bool? activeFilter = null, CancellationToken ct = default) => Task.FromResult<(IReadOnlyList<User>, int)>((new List<User>(), 0));
    public Task AddAsync(User user, CancellationToken ct = default) => Task.CompletedTask;
    public Task UpdateAsync(User user, CancellationToken ct = default) => Task.CompletedTask;
    public Task<bool> ExistsAsync(UserId id, CancellationToken ct = default) => Task.FromResult(false);
    public Task<int> CountAsync(CancellationToken ct = default) => Task.FromResult(0);
    public Task<int> CountActiveAsync(CancellationToken ct = default) => Task.FromResult(0);
    public Task<int> CountByRoleAsync(SystemRole role, CancellationToken ct = default) => Task.FromResult(0);
}

public sealed class MockEmailService : IEmailService
{
    public Task SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task SendEmailWithAttachmentAsync(EmailMessage message, EmailAttachment attachment, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task SendEmailWithAttachmentsAsync(EmailMessage message, IEnumerable<EmailAttachment> attachments, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<IReadOnlyList<EmailSendResult>> SendBatchAsync(IReadOnlyList<EmailMessage> messages, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<EmailSendResult>>(messages.Select(m => new EmailSendResult(m.To, true)).ToList());
    public Task<IReadOnlyList<EmailSendResult>> SendBatchAsync(IReadOnlyList<EmailMessage> messages, IReadOnlyList<EmailAttachment> sharedAttachments, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<EmailSendResult>>(messages.Select(m => new EmailSendResult(m.To, true)).ToList());
}

public sealed class MockCurrencyConversionService : ICurrencyConversionService
{
    public Task<Money?> ConvertAsync(Money amount, Currency targetCurrency, CancellationToken ct = default)
    {
        if (amount.Currency == targetCurrency)
            return Task.FromResult<Money?>(amount);
        return Task.FromResult<Money?>(Money.Create(amount.Amount, targetCurrency));
    }

    public Task<MultiCurrencyConversionResult> ConvertAllAsync(IEnumerable<Money> amounts, Currency targetCurrency, CancellationToken ct = default)
    {
        var breakdown = amounts.Select(a => new CurrencyBreakdown(
            a.Currency,
            a,
            Money.Create(a.Amount, targetCurrency),
            true)).ToList();
        var total = Money.Create(breakdown.Sum(b => b.ConvertedAmount!.Amount), targetCurrency);
        return Task.FromResult(new MultiCurrencyConversionResult(true, total, breakdown));
    }

    public Task<bool> AreRatesAvailableAsync(CancellationToken ct = default) => Task.FromResult(true);
}

public sealed class MockSettlementReportGenerator : ISettlementReportGenerator
{
    public byte[] GeneratePdf(SettlementReportData data) => Array.Empty<byte>();
    public byte[] GenerateExcel(SettlementReportData data) => Array.Empty<byte>();
}

public sealed class MockExchangeRateProvider : IExchangeRateProvider
{
    public Task<ExchangeRatesResult> GetCurrentRatesAsync(CancellationToken ct = default)
        => Task.FromResult(new ExchangeRatesResult(true, "USD", new Dictionary<string, decimal> { { "EUR", 0.85m }, { "CZK", 22.5m }, { "GBP", 0.73m } }, DateTime.UtcNow));
}

public sealed class MockUserSynchronizationService : IUserSynchronizationService
{
    public Task<User> SyncFromClaimsAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var user = User.CreateFromOidc(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "testuser@beanshare.com",
            "Test User",
            BeanShare.Domain.Enums.AuthenticationProvider.Oidc,
            DateTime.UtcNow);
        return Task.FromResult(user);
    }
}

public sealed class MockSettlementEmailTemplateService : ISettlementEmailTemplateService
{
    public (string HtmlBody, string PlainTextBody) GenerateSettlementEmail(
        string userName, string spaceName, string billingPeriodName,
        DateTime periodStart, DateTime periodEnd, decimal amountDue,
        string currency, decimal consumptionPercentage, string? appUrl = null)
    {
        return ($"<p>Settlement for {userName} in {spaceName}: {amountDue} {currency}</p>",
                $"Settlement for {userName} in {spaceName}: {amountDue} {currency}");
    }
}
