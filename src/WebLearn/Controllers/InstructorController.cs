using Microsoft.AspNetCore.Mvc;
using WebLearn.Constants;
using WebLearn.Models.ViewModels;
using WebLearn.Services.Interfaces;

namespace WebLearn.Controllers;

public class InstructorController(ICourseService courseService, ILessonService lessonService)
    : InstructorBaseController
{
    // Dashboard
    public async Task<IActionResult> Dashboard()
    {
        var courses = await courseService.GetByInstructorIdAsync(InstructorId);
        var lessons = await lessonService.GetByInstructorIdAsync(InstructorId);
        ViewData["CourseCount"] = courses.Count();
        ViewData["LessonCount"] = lessons.Count();
        ViewData["InstructorName"] = HttpContext.Session.GetString(SessionKeys.InstructorName);
        return View();
    }

    // ── Courses ──────────────────────────────────────────────────
    public async Task<IActionResult> Courses()
    {
        var courses = await courseService.GetByInstructorIdAsync(InstructorId);
        return View(courses);
    }

    [HttpGet]
    public IActionResult CreateCourse() => View(new CourseCreateViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCourse(CourseCreateViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        await courseService.CreateCourseAsync(vm, InstructorId);
        TempData["Success"] = "Course created successfully.";
        return RedirectToAction(nameof(Courses));
    }

    [HttpGet]
    public async Task<IActionResult> EditCourse(int id)
    {
        var course = await courseService.GetByIdAsync(id);
        if (course == null || course.InstructorId != InstructorId) return Forbid();
        var vm = new CourseCreateViewModel
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            IsPublished = course.IsPublished == 1
        };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCourse(CourseCreateViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        if (!await courseService.UpdateCourseAsync(vm, InstructorId)) return Forbid();
        TempData["Success"] = "Course updated.";
        return RedirectToAction(nameof(Courses));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        await courseService.DeleteCourseAsync(id, InstructorId);
        TempData["Success"] = "Course deleted.";
        return RedirectToAction(nameof(Courses));
    }

    // ── Units ─────────────────────────────────────────────────────
    public async Task<IActionResult> Units(int courseId)
    {
        var course = await courseService.GetByIdAsync(courseId);
        if (course == null || course.InstructorId != InstructorId) return Forbid();
        var detail = await courseService.GetCourseDetailAsync(courseId, includeUnpublished: true);
        ViewData["Course"] = course;
        return View(detail?.Units ?? new List<UnitDetailViewModel>());
    }

    [HttpGet]
    public async Task<IActionResult> CreateUnit(int courseId)
    {
        var course = await courseService.GetByIdAsync(courseId);
        if (course == null || course.InstructorId != InstructorId) return Forbid();
        return View(new UnitCreateViewModel { CourseId = courseId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUnit(UnitCreateViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        await courseService.CreateUnitAsync(vm, InstructorId);
        TempData["Success"] = "Unit created.";
        return RedirectToAction(nameof(Units), new { courseId = vm.CourseId });
    }

    [HttpGet]
    public async Task<IActionResult> EditUnit(int id)
    {
        var course = await GetUnitCourseAsync(id);
        if (course == null) return Forbid();
        var unit = (await courseService.GetCourseDetailAsync(course.Id, true))
            ?.Units.FirstOrDefault(u => u.Unit.Id == id)?.Unit;
        if (unit == null) return NotFound();
        var vm = new UnitCreateViewModel
        {
            Id = unit.Id, CourseId = unit.CourseId, Title = unit.Title,
            Description = unit.Description, SortOrder = unit.SortOrder
        };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUnit(UnitCreateViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        if (!await courseService.UpdateUnitAsync(vm, InstructorId)) return Forbid();
        TempData["Success"] = "Unit updated.";
        return RedirectToAction(nameof(Units), new { courseId = vm.CourseId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUnit(int id, int courseId)
    {
        await courseService.DeleteUnitAsync(id, InstructorId);
        TempData["Success"] = "Unit deleted.";
        return RedirectToAction(nameof(Units), new { courseId });
    }

    // ── Lesson Assignments ────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> AssignLessons(int unitId)
    {
        var vm = await courseService.GetAssignLessonViewModelAsync(unitId, InstructorId);
        if (vm == null) return Forbid();
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignLessons(int unitId, List<int> lessonIds)
    {
        await courseService.SaveLessonAssignmentsAsync(unitId, lessonIds ?? [], InstructorId);
        TempData["Success"] = "Lesson assignments saved.";
        var vm = await courseService.GetAssignLessonViewModelAsync(unitId, InstructorId);
        return RedirectToAction(nameof(Units), new { courseId = vm?.Course.Id });
    }

    [HttpPost]
    public async Task<IActionResult> ReorderLesson([FromBody] ReorderRequest req)
    {
        await courseService.ReorderLessonAsync(req.UnitId, req.LessonId, req.NewOrder);
        return Ok();
    }

    // ── Lessons ───────────────────────────────────────────────────
    public async Task<IActionResult> Lessons()
    {
        var lessons = await lessonService.GetByInstructorIdAsync(InstructorId);
        return View(lessons);
    }

    [HttpGet]
    public IActionResult CreateLesson() => View(new LessonEditViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLesson(LessonEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var (success, error, _) = await lessonService.CreateAsync(vm, InstructorId);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to create lesson.");
            return View(vm);
        }
        TempData["Success"] = "Lesson created.";
        return RedirectToAction(nameof(Lessons));
    }

    [HttpGet]
    public async Task<IActionResult> EditLesson(int id)
    {
        var lesson = await lessonService.GetByIdAsync(id);
        if (lesson == null || lesson.InstructorId != InstructorId) return Forbid();
        var vm = new LessonEditViewModel { Id = lesson.Id, Title = lesson.Title, XmlContent = lesson.XmlContent };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditLesson(LessonEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var (success, error) = await lessonService.UpdateAsync(vm, InstructorId);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to update lesson.");
            return View(vm);
        }
        TempData["Success"] = "Lesson saved.";
        return RedirectToAction(nameof(Lessons));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLesson(int id)
    {
        await lessonService.DeleteAsync(id, InstructorId);
        TempData["Success"] = "Lesson deleted.";
        return RedirectToAction(nameof(Lessons));
    }

    [HttpPost]
    public IActionResult PreviewLesson([FromBody] PreviewRequest req)
    {
        var html = lessonService.PreviewXml(req.Xml ?? string.Empty);
        return Content(html, "text/html");
    }

    // ── Helpers ───────────────────────────────────────────────────
    private async Task<WebLearn.Models.Course?> GetUnitCourseAsync(int unitId)
    {
        var courses = await courseService.GetByInstructorIdAsync(InstructorId);
        foreach (var course in courses)
        {
            var detail = await courseService.GetCourseDetailAsync(course.Id, includeUnpublished: true);
            if (detail?.Units.Any(u => u.Unit.Id == unitId) == true)
                return course;
        }
        return null;
    }
}

public record ReorderRequest(int UnitId, int LessonId, int NewOrder);
public record PreviewRequest(string? Xml);
