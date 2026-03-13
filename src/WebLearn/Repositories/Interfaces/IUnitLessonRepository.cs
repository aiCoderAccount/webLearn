using WebLearn.Models;

namespace WebLearn.Repositories.Interfaces;

public interface IUnitLessonRepository
{
    Task<IEnumerable<UnitLesson>> GetByUnitIdAsync(int unitId);
    Task AssignAsync(int unitId, int lessonId, int sortOrder);
    Task UnassignAsync(int unitId, int lessonId);
    Task UpdateSortOrderAsync(int unitId, int lessonId, int sortOrder);
}
