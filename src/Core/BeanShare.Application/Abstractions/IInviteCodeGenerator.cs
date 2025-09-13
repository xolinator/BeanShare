using BeanShare.Domain.ValueObjects;

namespace BeanShare.Application.Abstractions;

public interface IInviteCodeGenerator
{
    Task<InviteCode> GenerateAsync(CancellationToken cancellationToken = default);
}