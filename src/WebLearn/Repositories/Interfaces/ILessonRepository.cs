using WebLearn.Models;

namespace WebLearn.Repositories.Interfaces;

public interface ILessonRepository
{
    Task<IEnumerable<Lesson>> GetByInstructorIdAsync(int instructorId);
    Task<Lesson?> GetByIdAsync(int id);
    Task<int> CreateAsync(Lesson lesson);
    Task UpdateAsync(Lesson lesson);
    Task DeleteAsync(int id);
}
