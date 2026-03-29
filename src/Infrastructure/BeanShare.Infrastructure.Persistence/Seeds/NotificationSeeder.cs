using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.Enums;
using BeanShare.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BeanShare.Infrastructure.Persistence.Seeds;

public class NotificationSeeder : IDataSeeder
{
    public int Order => 70;
    public bool IsEssential => false;

    public async Task SeedAsync(BeanShareDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Notifications.AnyAsync(cancellationToken))
            return;

        var clock = new SeedClock();
        var johnId = new UserId(new Guid("11111111-1111-1111-1111-111111111111"));
        var sarahId = new UserId(new Guid("22222222-2222-2222-2222-222222222222"));
        var mikeId = new UserId(new Guid("33333333-3333-3333-3333-333333333333"));
        var engineeringSpaceId = new SpaceId(new Guid("aaaa1111-aaaa-1111-aaaa-111111111111"));

        var notifications = new List<Notification>
        {
            Notification.Create(johnId, NotificationType.MemberJoined, "New Member",
                "Emily Chen joined Engineering Team.", clock, engineeringSpaceId),

            Notification.Create(johnId, NotificationType.LowStock, "Low Stock Alert",
                "Super Crema (Lavazza) is running low in Engineering Team — only 35g remaining.", clock, engineeringSpaceId),

            Notification.Create(sarahId, NotificationType.SettlementReady, "Settlement Ready",
                "A new settlement has been generated for Engineering Team. Please review your share.", clock, engineeringSpaceId),

            Notification.Create(sarahId, NotificationType.BillingPeriodClosed, "Billing Period Closed",
                "A billing period in Engineering Team has been closed. A settlement will follow.", clock, engineeringSpaceId),

            Notification.Create(mikeId, NotificationType.MemberJoined, "New Member",
                "Emily Chen joined Engineering Team.", clock, engineeringSpaceId),
        };

        await context.Notifications.AddRangeAsync(notifications, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private sealed class SeedClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow.AddHours(-2);
    }
}
