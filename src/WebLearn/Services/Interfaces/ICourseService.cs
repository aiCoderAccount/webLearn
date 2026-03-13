using WebLearn.Models;
using WebLearn.Models.ViewModels;

namespace WebLearn.Services.Interfaces;

public interface ICourseService
{
    Task<IEnumerable<Course>> GetPublishedCoursesAsync();
    Task<IEnumerable<Course>> GetByInstructorIdAsync(int instructorId);
    Task<CourseDetailViewModel?> GetCourseDetailAsync(int courseId, bool includeUnpublished = false);
    Task<UnitDetailViewModel?> GetUnitDetailAsync(int unitId);
    Task<Course?> GetByIdAsync(int id);
    Task<int> CreateCourseAsync(CourseCreateViewModel vm, int instructorId);
    Task<bool> UpdateCourseAsync(CourseCreateViewModel vm, int instructorId);
    Task<bool> DeleteCourseAsync(int id, int instructorId);
    Task<int> CreateUnitAsync(UnitCreateViewModel vm, int instructorId);
    Task<bool> UpdateUnitAsync(UnitCreateViewModel vm, int instructorId);
    Task<bool> DeleteUnitAsync(int id, int instructorId);
    Task<AssignLessonViewModel?> GetAssignLessonViewModelAsync(int unitId, int instructorId);
    Task SaveLessonAssignmentsAsync(int unitId, List<int> lessonIds, int instructorId);
    Task ReorderLessonAsync(int unitId, int lessonId, int newOrder);
}
