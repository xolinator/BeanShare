using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using BeanShare.Maui.Services;

namespace BeanShare.Tests.Unit.Services;

public sealed class PkceHelperTests
{
    [Fact]
    public void GenerateCodeVerifier_ShouldReturnBase64UrlEncodedString()
    {
        var verifier = PkceHelper.GenerateCodeVerifier();

        verifier.Should().NotBeNullOrEmpty();
        verifier.Should().MatchRegex("^[A-Za-z0-9_-]+$", "should be Base64URL-safe characters only");
    }

    [Fact]
    public void GenerateCodeVerifier_ShouldHaveExpectedLength()
    {
        // 32 bytes -> ~43 Base64 chars (32 * 4/3 = 42.67, trimmed of padding)
        var verifier = PkceHelper.GenerateCodeVerifier();

        verifier.Length.Should().BeGreaterOrEqualTo(42);
        verifier.Length.Should().BeLessOrEqualTo(44);
    }

    [Fact]
    public void GenerateCodeVerifier_ShouldProduceUniqueValues()
    {
        var verifiers = new HashSet<string>();

        for (int i = 0; i < 100; i++)
        {
            verifiers.Add(PkceHelper.GenerateCodeVerifier());
        }

        verifiers.Should().HaveCountGreaterThan(95, "cryptographic random should produce unique values");
    }

    [Fact]
    public void GenerateCodeChallenge_ShouldReturnBase64UrlEncodedHash()
    {
        var verifier = "test_verifier_string";
        var challenge = PkceHelper.GenerateCodeChallenge(verifier);

        challenge.Should().NotBeNullOrEmpty();
        challenge.Should().MatchRegex("^[A-Za-z0-9_-]+$", "should be Base64URL-safe characters only");
    }

    [Fact]
    public void GenerateCodeChallenge_ShouldReturnCorrectSHA256Hash()
    {
        var verifier = "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk";

        // Compute expected SHA256 hash manually
        using var sha256 = SHA256.Create();
        var expectedBytes = sha256.ComputeHash(Encoding.ASCII.GetBytes(verifier));
        var expected = Convert.ToBase64String(expectedBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        var challenge = PkceHelper.GenerateCodeChallenge(verifier);

        challenge.Should().Be(expected);
    }

    [Fact]
    public void GenerateCodeChallenge_ShouldBeDeterministic()
    {
        var verifier = "same_verifier_every_time";

        var challenge1 = PkceHelper.GenerateCodeChallenge(verifier);
        var challenge2 = PkceHelper.GenerateCodeChallenge(verifier);

        challenge1.Should().Be(challenge2, "same verifier should always produce the same challenge");
    }

    [Fact]
    public void GenerateCodeChallenge_DifferentVerifiers_ShouldProduceDifferentChallenges()
    {
        var challenge1 = PkceHelper.GenerateCodeChallenge("verifier_one");
        var challenge2 = PkceHelper.GenerateCodeChallenge("verifier_two");

        challenge1.Should().NotBe(challenge2);
    }

    [Fact]
    public void GenerateCodeChallenge_ShouldHaveExpectedLength()
    {
        // SHA256 produces 32 bytes -> ~43 Base64 chars
        var challenge = PkceHelper.GenerateCodeChallenge("any_verifier");

        challenge.Length.Should().BeGreaterOrEqualTo(42);
        challenge.Length.Should().BeLessOrEqualTo(44);
    }

    [Theory]
    [InlineData(16)]
    [InlineData(32)]
    [InlineData(64)]
    public void GenerateRandomString_ShouldReturnBase64UrlEncodedString(int length)
    {
        var result = PkceHelper.GenerateRandomString(length);

        result.Should().NotBeNullOrEmpty();
        result.Should().MatchRegex("^[A-Za-z0-9_-]+$", "should contain only Base64URL-safe characters");
    }

    [Fact]
    public void GenerateRandomString_ShouldProduceUniqueValues()
    {
        var values = new HashSet<string>();

        for (int i = 0; i < 50; i++)
        {
            values.Add(PkceHelper.GenerateRandomString(32));
        }

        values.Should().HaveCountGreaterThan(45, "random values should be unique");
    }

    [Fact]
    public void GenerateCodeVerifier_ShouldNotContainPadding()
    {
        // Base64URL encoding should strip '=' padding
        for (int i = 0; i < 20; i++)
        {
            var verifier = PkceHelper.GenerateCodeVerifier();
            verifier.Should().NotContain("=", "Base64URL encoding should strip padding");
            verifier.Should().NotContain("+", "Base64URL should use - instead of +");
            verifier.Should().NotContain("/", "Base64URL should use _ instead of /");
        }
    }
}
