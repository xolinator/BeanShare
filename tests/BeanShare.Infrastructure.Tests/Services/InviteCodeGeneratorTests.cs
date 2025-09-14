using BeanShare.Infrastructure.Persistence.Services;
using FluentAssertions;
using Xunit;

namespace BeanShare.Infrastructure.Tests.Services;

public sealed class InviteCodeGeneratorTests
{
    [Fact]
    public async Task GenerateAsync_ShouldReturnValidInviteCode()
    {
        var generator = new InviteCodeGenerator();

        var inviteCode = await generator.GenerateAsync();

        inviteCode.Value.Should().HaveLength(6);
        inviteCode.Value.Should().MatchRegex("^[ABCDEFGHJKLMNPQRSTUVWXYZ23456789]{6}$");
    }

    [Fact]
    public async Task GenerateAsync_MultipleCallsShouldProduceDifferentCodes()
    {
        var generator = new InviteCodeGenerator();
        var codes = new HashSet<string>();

        for (int i = 0; i < 100; i++)
        {
            var inviteCode = await generator.GenerateAsync();
            codes.Add(inviteCode.Value);
        }

        codes.Should().HaveCountGreaterThan(90, "codes should be mostly unique with high probability");
    }

    [Fact]
    public async Task GenerateAsync_ShouldOnlyUseSafeCharacters()
    {
        var generator = new InviteCodeGenerator();
        var safeChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToHashSet();

        for (int i = 0; i < 50; i++)
        {
            var inviteCode = await generator.GenerateAsync();
            
            foreach (char c in inviteCode.Value)
            {
                safeChars.Should().Contain(c, $"'{c}' should be a safe character");
            }
        }
    }
}