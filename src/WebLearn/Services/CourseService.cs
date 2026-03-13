using WebLearn.Models;
using WebLearn.Models.ViewModels;
using WebLearn.Repositories.Interfaces;
using WebLearn.Services.Interfaces;

namespace WebLearn.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepo;
    private readonly IUnitRepository _unitRepo;
    private readonly ILessonRepository _lessonRepo;
    private readonly IUnitLessonRepository _unitLessonRepo;

    public CourseService(
        ICourseRepository courseRepo,
        IUnitRepository unitRepo,
        ILessonRepository lessonRepo,
        IUnitLessonRepository unitLessonRepo)
    {
        _courseRepo = courseRepo;
        _unitRepo = unitRepo;
        _lessonRepo = lessonRepo;
        _unitLessonRepo = unitLessonRepo;
    }

    public async Task<IEnumerable<Course>> GetPublishedCoursesAsync() =>
        await _courseRepo.GetAllPublishedAsync();

    public async Task<IEnumerable<Course>> GetByInstructorIdAsync(int instructorId) =>
        await _courseRepo.GetByInstructorIdAsync(instructorId);

    public async Task<Course?> GetByIdAsync(int id) =>
        await _courseRepo.GetByIdAsync(id);

    public async Task<CourseDetailViewModel?> GetCourseDetailAsync(int courseId, bool includeUnpublished = false)
    {
        var course = await _courseRepo.GetByIdAsync(courseId);
        if (course == null) return null;
        if (!includeUnpublished && course.IsPublished == 0) return null;

        var units = await _unitRepo.GetByCourseIdAsync(courseId);
        var unitDetails = new List<UnitDetailViewModel>();
        foreach (var unit in units)
        {
            var unitLessons = await _unitLessonRepo.GetByUnitIdAsync(unit.Id);
            var lessonList = new List<Lesson>();
            foreach (var ul in unitLessons)
            {
                var lesson = await _lessonRepo.GetByIdAsync(ul.LessonId);
                if (lesson != null) lessonList.Add(lesson);
            }
            unitDetails.Add(new UnitDetailViewModel { Unit = unit, Lessons = lessonList });
        }

        return new CourseDetailViewModel { Course = course, Units = unitDetails };
    }

    public async Task<UnitDetailViewModel?> GetUnitDetailAsync(int unitId)
    {
        var unit = await _unitRepo.GetByIdAsync(unitId);
        if (unit == null) return null;

        var unitLessons = await _unitLessonRepo.GetByUnitIdAsync(unitId);
        var lessons = new List<Lesson>();
        foreach (var ul in unitLessons)
        {
            var lesson = await _lessonRepo.GetByIdAsync(ul.LessonId);
            if (lesson != null) lessons.Add(lesson);
        }

        return new UnitDetailViewModel { Unit = unit, Lessons = lessons };
    }

    public async Task<int> CreateCourseAsync(CourseCreateViewModel vm, int instructorId)
    {
        var now = DateTime.UtcNow.ToString("o");
        var course = new Course
        {
            Title = vm.Title,
            Description = vm.Description,
            InstructorId = instructorId,
            IsPublished = vm.IsPublished ? 1 : 0,
            CreatedAt = now,
            UpdatedAt = now
        };
        return await _courseRepo.CreateAsync(course);
    }

    public async Task<bool> UpdateCourseAsync(CourseCreateViewModel vm, int instructorId)
    {
        var existing = await _courseRepo.GetByIdAsync(vm.Id);
        if (existing == null || existing.InstructorId != instructorId) return false;

        var updated = new Course
        {
            Id = existing.Id,
            Title = vm.Title,
            Description = vm.Description,
            InstructorId = existing.InstructorId,
            IsPublished = vm.IsPublished ? 1 : 0,
            CreatedAt = existing.CreatedAt,
            UpdatedAt = DateTime.UtcNow.ToString("o")
        };
        await _courseRepo.UpdateAsync(updated);
        return true;
    }

    public async Task<bool> DeleteCourseAsync(int id, int instructorId)
    {
        var course = await _courseRepo.GetByIdAsync(id);
        if (course == null || course.InstructorId != instructorId) return false;
        await _courseRepo.DeleteAsync(id);
        return true;
    }

    public async Task<int> CreateUnitAsync(UnitCreateViewModel vm, int instructorId)
    {
        var course = await _courseRepo.GetByIdAsync(vm.CourseId);
        if (course == null || course.InstructorId != instructorId) return 0;

        var now = DateTime.UtcNow.ToString("o");
        var unit = new Unit
        {
            CourseId = vm.CourseId,
            Title = vm.Title,
            Description = vm.Description,
            SortOrder = vm.SortOrder,
            CreatedAt = now,
            UpdatedAt = now
        };
        return await _unitRepo.CreateAsync(unit);
    }

    public async Task<bool> UpdateUnitAsync(UnitCreateViewModel vm, int instructorId)
    {
        var existing = await _unitRepo.GetByIdAsync(vm.Id);
        if (existing == null) return false;

        var course = await _courseRepo.GetByIdAsync(existing.CourseId);
        if (course == null || course.InstructorId != instructorId) return false;

        var updated = new Unit
        {
            Id = existing.Id,
            CourseId = existing.CourseId,
            Title = vm.Title,
            Description = vm.Description,
            SortOrder = vm.SortOrder,
            CreatedAt = existing.CreatedAt,
            UpdatedAt = DateTime.UtcNow.ToString("o")
        };
        await _unitRepo.UpdateAsync(updated);
        return true;
    }

    public async Task<bool> DeleteUnitAsync(int id, int instructorId)
    {
        var unit = await _unitRepo.GetByIdAsync(id);
        if (unit == null) return false;

        var course = await _courseRepo.GetByIdAsync(unit.CourseId);
        if (course == null || course.InstructorId != instructorId) return false;

        await _unitRepo.DeleteAsync(id);
        return true;
    }

    public async Task<AssignLessonViewModel?> GetAssignLessonViewModelAsync(int unitId, int instructorId)
    {
        var unit = await _unitRepo.GetByIdAsync(unitId);
        if (unit == null) return null;

        var course = await _courseRepo.GetByIdAsync(unit.CourseId);
        if (course == null || course.InstructorId != instructorId) return null;

        var assigned = (await _unitLessonRepo.GetByUnitIdAsync(unitId)).ToList();
        var allLessons = await _lessonRepo.GetByInstructorIdAsync(instructorId);

        var items = allLessons.Select(l =>
        {
            var ul = assigned.FirstOrDefault(a => a.LessonId == l.Id);
            return new LessonAssignItem
            {
                LessonId = l.Id,
                Title = l.Title,
                IsAssigned = ul != null,
                SortOrder = ul?.SortOrder ?? 0
            };
        }).OrderByDescending(x => x.IsAssigned).ThenBy(x => x.SortOrder).ThenBy(x => x.Title).ToList();

        return new AssignLessonViewModel { Unit = unit, Course = course, AllLessons = items };
    }

    public async Task SaveLessonAssignmentsAsync(int unitId, List<int> lessonIds, int instructorId)
    {
        var unit = await _unitRepo.GetByIdAsync(unitId);
        if (unit == null) return;

        var course = await _courseRepo.GetByIdAsync(unit.CourseId);
        if (course == null || course.InstructorId != instructorId) return;

        var existing = (await _unitLessonRepo.GetByUnitIdAsync(unitId)).ToList();
        var existingIds = existing.Select(u => u.LessonId).ToHashSet();

        // Remove unselected
        foreach (var ul in existing.Where(e => !lessonIds.Contains(e.LessonId)))
            await _unitLessonRepo.UnassignAsync(unitId, ul.LessonId);

        // Add new
        for (int i = 0; i < lessonIds.Count; i++)
        {
            if (!existingIds.Contains(lessonIds[i]))
                await _unitLessonRepo.AssignAsync(unitId, lessonIds[i], i + 1);
        }
    }

    public async Task ReorderLessonAsync(int unitId, int lessonId, int newOrder)
    {
        await _unitLessonRepo.UpdateSortOrderAsync(unitId, lessonId, newOrder);
    }
}
