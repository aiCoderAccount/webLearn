using WebLearn.Models;
using WebLearn.Repositories.Interfaces;
using WebLearn.Services.Interfaces;

namespace WebLearn.Services;

public class AuthService : IAuthService
{
    private readonly IInstructorRepository _instructorRepo;

    public AuthService(IInstructorRepository instructorRepo)
    {
        _instructorRepo = instructorRepo;
    }

    public async Task<AuthResult> LoginAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return new AuthResult(false, null, "Username and password are required.");

        var instructor = await _instructorRepo.GetByUsernameAsync(username);
        if (instructor == null)
            return new AuthResult(false, null, "Invalid username or password.");

        bool verified;
        try
        {
            verified = BCrypt.Net.BCrypt.Verify(password, instructor.PasswordHash);
        }
        catch
        {
            return new AuthResult(false, null, "Invalid username or password.");
        }

        if (!verified)
            return new AuthResult(false, null, "Invalid username or password.");

        return new AuthResult(true, instructor, null);
    }

    public async Task<AuthResult> RegisterAsync(string username, string displayName, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return new AuthResult(false, null, "Username and password are required.");

        var existing = await _instructorRepo.GetByUsernameAsync(username);
        if (existing != null)
            return new AuthResult(false, null, "That username is already taken.");

        var now = DateTime.UtcNow.ToString("o");
        var instructor = new Instructor
        {
            Username = username,
            DisplayName = displayName,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            CreatedAt = now,
            UpdatedAt = now
        };

        instructor.Id = await _instructorRepo.CreateAsync(instructor);

        return new AuthResult(true, instructor, null);
    }
}
