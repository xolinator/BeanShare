using BeanShare.Domain.Common;

namespace BeanShare.Domain.ValueObjects;

/// <summary>
/// Value object representing a payment confirmation for a settlement line.
/// Tracks who confirmed the payment and when.
/// </summary>
public sealed record PaymentConfirmation
{
    public UserId ConfirmedBy { get; private init; }
    public DateTime ConfirmedAt { get; private init; }

    private PaymentConfirmation(UserId confirmedBy, DateTime confirmedAt)
    {
        ConfirmedBy = confirmedBy;
        ConfirmedAt = confirmedAt;
    }

    public static PaymentConfirmation Create(UserId confirmedBy, DateTime confirmedAt)
    {
        ArgumentNullException.ThrowIfNull(confirmedBy);

        return new PaymentConfirmation(confirmedBy, confirmedAt);
    }
}
