using BeanShare.Application.Abstractions;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Api.Infrastructure.Mocks;

public sealed class MockInviteCodeGenerator : IInviteCodeGenerator
{
    private static readonly Random Random = new();
    private static readonly string SafeCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public Task<InviteCode> GenerateAsync(CancellationToken cancellationToken = default)
    {
        var code = new string(Enumerable.Repeat(SafeCharacters, 6)
            .Select(s => s[Random.Next(s.Length)])
            .ToArray());
            
        return Task.FromResult(new InviteCode(code));
    }
}