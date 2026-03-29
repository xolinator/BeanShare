using BeanShare.Application.Abstractions;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.Enums;
using BeanShare.Domain.Events;
using MediatR;

namespace BeanShare.Application.Features.Notifications.EventHandlers;

/// <summary>
/// Checks remaining stock after each consumption event and notifies space admins
/// when any product drops below the low-stock threshold (50 grams).
/// </summary>
public sealed class LowStockNotificationHandler(
    INotificationRepository notificationRepository,
    ISpaceRepository spaceRepository,
    IClock clock) : INotificationHandler<StockConsumed>
{
    private const int LowStockThresholdGrams = 50;

    public async Task Handle(StockConsumed notification, CancellationToken cancellationToken)
    {
        if (notification.RemainingStock.Grams > LowStockThresholdGrams)
            return;

        var space = await spaceRepository.GetByIdAsync(notification.SpaceId, cancellationToken);
        if (space is null) return;

        var admins = space.Members.Where(m => m.Role == SpaceRole.Admin);

        foreach (var admin in admins)
        {
            var n = Notification.Create(
                admin.UserId,
                NotificationType.LowStock,
                "Low Stock Alert",
                $"{notification.Product.Name} ({notification.Product.Brand}) is running low in {space.Name} — only {notification.RemainingStock.Grams}g remaining.",
                clock,
                notification.SpaceId);

            await notificationRepository.AddAsync(n, cancellationToken);
        }
    }
}
