using WebLearn.Models;

namespace WebLearn.Repositories.Interfaces;

public interface IUnitRepository
{
    Task<IEnumerable<Unit>> GetByCourseIdAsync(int courseId);
    Task<Unit?> GetByIdAsync(int id);
    Task<int> CreateAsync(Unit unit);
    Task UpdateAsync(Unit unit);
    Task DeleteAsync(int id);
}
