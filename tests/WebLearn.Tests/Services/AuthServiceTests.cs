using FluentAssertions;
using Moq;
using WebLearn.Models;
using WebLearn.Repositories.Interfaces;
using WebLearn.Services;
using Xunit;

namespace WebLearn.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IInstructorRepository> _repoMock = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(_repoMock.Object);
    }

    // ── Login ──────────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccess()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("Password1!");
        _repoMock.Setup(r => r.GetByUsernameAsync("admin"))
            .ReturnsAsync(new Instructor { Id = 1, Username = "admin", PasswordHash = hash, DisplayName = "Admin" });

        var result = await _sut.LoginAsync("admin", "Password1!");

        result.Success.Should().BeTrue();
        result.Instructor.Should().NotBeNull();
        result.Instructor!.Username.Should().Be("admin");
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsFailure()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("correct");
        _repoMock.Setup(r => r.GetByUsernameAsync("admin"))
            .ReturnsAsync(new Instructor { Id = 1, Username = "admin", PasswordHash = hash });

        var result = await _sut.LoginAsync("admin", "wrong");

        result.Success.Should().BeFalse();
        result.Instructor.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_UnknownUser_ReturnsFailure()
    {
        _repoMock.Setup(r => r.GetByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync((Instructor?)null);

        var result = await _sut.LoginAsync("nobody", "pass");

        result.Success.Should().BeFalse();
    }

    [Theory]
    [InlineData("", "pass")]
    [InlineData("user", "")]
    [InlineData("", "")]
    public async Task LoginAsync_EmptyCredentials_ReturnsFailure(string username, string password)
    {
        var result = await _sut.LoginAsync(username, password);
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    // ── Register ───────────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_NewUsername_CreatesInstructorAndReturnsSuccess()
    {
        _repoMock.Setup(r => r.GetByUsernameAsync("newuser"))
            .ReturnsAsync((Instructor?)null);
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Instructor>()))
            .ReturnsAsync(42);

        var result = await _sut.RegisterAsync("newuser", "New User", "new@example.com", "Secure1!");

        result.Success.Should().BeTrue();
        result.Instructor.Should().NotBeNull();
        result.Instructor!.Id.Should().Be(42);
        result.Instructor.Username.Should().Be("newuser");
        result.Instructor.DisplayName.Should().Be("New User");
        result.Instructor.Email.Should().Be("new@example.com");
    }

    [Fact]
    public async Task RegisterAsync_PasswordIsHashed_NotStoredInPlaintext()
    {
        Instructor? captured = null;
        _repoMock.Setup(r => r.GetByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync((Instructor?)null);
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Instructor>()))
            .Callback<Instructor>(i => captured = i)
            .ReturnsAsync(1);

        await _sut.RegisterAsync("user", "User", "u@u.com", "MyPass1!");

        captured.Should().NotBeNull();
        captured!.PasswordHash.Should().NotBe("MyPass1!");
        BCrypt.Net.BCrypt.Verify("MyPass1!", captured.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task RegisterAsync_DuplicateUsername_ReturnsFailure()
    {
        _repoMock.Setup(r => r.GetByUsernameAsync("taken"))
            .ReturnsAsync(new Instructor { Id = 1, Username = "taken" });

        var result = await _sut.RegisterAsync("taken", "Some Name", "a@b.com", "Password1!");

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("taken");
        _repoMock.Verify(r => r.CreateAsync(It.IsAny<Instructor>()), Times.Never);
    }

    [Theory]
    [InlineData("", "Name", "e@e.com", "Password1!")]
    [InlineData("user", "Name", "e@e.com", "")]
    public async Task RegisterAsync_MissingRequiredFields_ReturnsFailure(
        string username, string displayName, string email, string password)
    {
        var result = await _sut.RegisterAsync(username, displayName, email, password);
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RegisterAsync_SetsCreatedAtAndUpdatedAt()
    {
        Instructor? captured = null;
        _repoMock.Setup(r => r.GetByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync((Instructor?)null);
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Instructor>()))
            .Callback<Instructor>(i => captured = i)
            .ReturnsAsync(1);

        await _sut.RegisterAsync("u", "N", "e@e.com", "Pass1!");

        captured!.CreatedAt.Should().NotBeNullOrEmpty();
        captured.UpdatedAt.Should().NotBeNullOrEmpty();
    }
}
