using WebLearn.Models;

namespace WebLearn.Repositories.Interfaces;

public interface ICourseRepository
{
    Task<IEnumerable<Course>> GetAllPublishedAsync();
    Task<IEnumerable<Course>> GetByInstructorIdAsync(int instructorId);
    Task<Course?> GetByIdAsync(int id);
    Task<int> CreateAsync(Course course);
    Task UpdateAsync(Course course);
    Task DeleteAsync(int id);
}
