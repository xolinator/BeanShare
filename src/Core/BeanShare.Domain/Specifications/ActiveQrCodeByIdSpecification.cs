using System.Linq.Expressions;
using BeanShare.Domain.Entities;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Specifications;

public sealed class ActiveQrCodeByIdSpecification : Spec<ActiveQrCode>
{
    private readonly ActiveQrCodeId _id;

    public ActiveQrCodeByIdSpecification(ActiveQrCodeId id)
    {
        _id = id;
    }

    public override Expression<Func<ActiveQrCode, bool>> Criteria => qr => qr.Id == _id;
    public override string? Reason => $"Active QR code with ID {_id}";
}
