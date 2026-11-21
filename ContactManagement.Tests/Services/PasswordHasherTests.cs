using ContactManagement.Core.Interfaces.Services;
using ContactManagement.Services.Services;

namespace ContactManagement.Tests.Services;

public sealed class PasswordHasherTests
{
    private readonly IPasswordHasher _hasher = new PasswordHasher();

    [Fact]
    public void Hash_DoesNotReturnPlainTextPassword()
    {
        var hash = _hasher.Hash("Str0ng!Password");

        Assert.NotEqual("Str0ng!Password", hash);
        Assert.Contains(".", hash);
    }

    [Fact]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        var hash = _hasher.Hash("Str0ng!Password");

        Assert.True(_hasher.Verify("Str0ng!Password", hash));
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash("Str0ng!Password");

        Assert.False(_hasher.Verify("Wrong!Password", hash));
    }

    [Fact]
    public void Hash_WithSamePassword_ProducesDifferentHashes()
    {
        Assert.NotEqual(_hasher.Hash("Str0ng!Password"), _hasher.Hash("Str0ng!Password"));
    }
}
