using WebLearn.Models;

namespace WebLearn.Repositories.Interfaces;

public interface IInstructorRepository
{
    Task<Instructor?> GetByIdAsync(int id);
    Task<Instructor?> GetByUsernameAsync(string username);
    Task<int> CreateAsync(Instructor instructor);
    Task UpdateAsync(Instructor instructor);
}
