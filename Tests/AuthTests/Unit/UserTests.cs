using FluentAssertions;
using Auth.Repositories.Models;

namespace AuthTests.Unit;

public sealed class UserTests
{
    [Fact]
    public void Create_SetsIdCorrectly()
    {
        var id = Guid.NewGuid();

        var user = User.Create(id, "Alice", "alice@example.com", "hash");

        user.Id.Should().Be(id);
    }

    [Fact]
    public void Create_SetsEmailCorrectly()
    {
        const string email = "bob@example.com";

        var user = User.Create(Guid.NewGuid(), "Bob", email, "hash");

        user.Email.Should().Be(email);
    }

    [Fact]
    public void Create_SetsNameAndPasswordHashCorrectly()
    {
        const string name = "Charlie";
        const string hash = "bcrypt-hash-value";

        var user = User.Create(Guid.NewGuid(), name, "charlie@example.com", hash);

        user.Name.Should().Be(name);
        user.PasswordHash.Should().Be(hash);
    }
}
