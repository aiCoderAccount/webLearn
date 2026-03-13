using WebLearn.Models;

namespace WebLearn.Services.Interfaces;

public record AuthResult(bool Success, Instructor? Instructor, string? ErrorMessage);

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string username, string password);
    Task<AuthResult> RegisterAsync(string username, string displayName, string email, string password);
}
