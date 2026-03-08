using BeanShare.Application.Abstractions;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Infrastructure.Persistence.Services;

public sealed class InviteCodeGenerator : IInviteCodeGenerator
{
    private static readonly char[] SafeCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

    public Task<InviteCode> GenerateAsync(CancellationToken cancellationToken = default)
    {
        const int codeLength = 6;
        var codeChars = new char[codeLength];

        for (int i = 0; i < codeLength; i++)
        {
            codeChars[i] = SafeCharacters[Random.Shared.Next(SafeCharacters.Length)];
        }

        var codeValue = new string(codeChars);
        var inviteCode = new InviteCode(codeValue);

        return Task.FromResult(inviteCode);
    }
}