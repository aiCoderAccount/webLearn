using Microsoft.AspNetCore.Mvc;
using WebLearn.Models.ViewModels;
using WebLearn.Services.Interfaces;

namespace WebLearn.Controllers;

public class InstructorController : Controller
{
    private readonly ICourseService _courseService;
    private readonly ILessonService _lessonService;

    public InstructorController(ICourseService courseService, ILessonService lessonService)
    {
        _courseService = courseService;
        _lessonService = lessonService;
    }

    private int? GetInstructorId() => HttpContext.Session.GetInt32("InstructorId");

    // Dashboard
    public async Task<IActionResult> Dashboard()
    {
        var id = GetInstructorId();
        if (id == null) return RedirectToAction("Login", "Auth");

        var courses = await _courseService.GetByInstructorIdAsync(id.Value);
        var lessons = await _lessonService.GetByInstructorIdAsync(id.Value);
        ViewData["CourseCount"] = courses.Count();
        ViewData["LessonCount"] = lessons.Count();
        ViewData["InstructorName"] = HttpContext.Session.GetString("InstructorName");
        return View();
    }

    // ── Courses ──────────────────────────────────────────────────
    public async Task<IActionResult> Courses()
    {
        var id = GetInstructorId()!.Value;
        var courses = await _courseService.GetByInstructorIdAsync(id);
        return View(courses);
    }

    [HttpGet]
    public IActionResult CreateCourse() => View(new CourseCreateViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCourse(CourseCreateViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var id = GetInstructorId()!.Value;
        await _courseService.CreateCourseAsync(vm, id);
        TempData["Success"] = "Course created successfully.";
        return RedirectToAction(nameof(Courses));
    }

    [HttpGet]
    public async Task<IActionResult> EditCourse(int id)
    {
        var course = await _courseService.GetByIdAsync(id);
        if (course == null || course.InstructorId != GetInstructorId()) return Forbid();
        var vm = new CourseCreateViewModel
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            IsPublished = course.IsPublished == 1
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCourse(CourseCreateViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var id = GetInstructorId()!.Value;
        var success = await _courseService.UpdateCourseAsync(vm, id);
        if (!success) return Forbid();
        TempData["Success"] = "Course updated.";
        return RedirectToAction(nameof(Courses));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        var instructorId = GetInstructorId()!.Value;
        await _courseService.DeleteCourseAsync(id, instructorId);
        TempData["Success"] = "Course deleted.";
        return RedirectToAction(nameof(Courses));
    }

    // ── Units ─────────────────────────────────────────────────────
    public async Task<IActionResult> Units(int courseId)
    {
        var course = await _courseService.GetByIdAsync(courseId);
        if (course == null || course.InstructorId != GetInstructorId()) return Forbid();
        var detail = await _courseService.GetCourseDetailAsync(courseId, includeUnpublished: true);
        ViewData["Course"] = course;
        return View(detail?.Units ?? new List<UnitDetailViewModel>());
    }

    [HttpGet]
    public async Task<IActionResult> CreateUnit(int courseId)
    {
        var course = await _courseService.GetByIdAsync(courseId);
        if (course == null || course.InstructorId != GetInstructorId()) return Forbid();
        return View(new UnitCreateViewModel { CourseId = courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUnit(UnitCreateViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var id = GetInstructorId()!.Value;
        await _courseService.CreateUnitAsync(vm, id);
        TempData["Success"] = "Unit created.";
        return RedirectToAction(nameof(Units), new { courseId = vm.CourseId });
    }

    [HttpGet]
    public async Task<IActionResult> EditUnit(int id)
    {
        var course = await GetUnitCourseAsync(id);
        if (course == null) return Forbid();
        var unit = (await _courseService.GetCourseDetailAsync(course.Id, true))?.Units.FirstOrDefault(u => u.Unit.Id == id)?.Unit;
        if (unit == null) return NotFound();
        var vm = new UnitCreateViewModel { Id = unit.Id, CourseId = unit.CourseId, Title = unit.Title, Description = unit.Description, SortOrder = unit.SortOrder };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUnit(UnitCreateViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var id = GetInstructorId()!.Value;
        var success = await _courseService.UpdateUnitAsync(vm, id);
        if (!success) return Forbid();
        TempData["Success"] = "Unit updated.";
        return RedirectToAction(nameof(Units), new { courseId = vm.CourseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUnit(int id, int courseId)
    {
        var instructorId = GetInstructorId()!.Value;
        await _courseService.DeleteUnitAsync(id, instructorId);
        TempData["Success"] = "Unit deleted.";
        return RedirectToAction(nameof(Units), new { courseId });
    }

    // ── Lesson Assignments ────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> AssignLessons(int unitId)
    {
        var id = GetInstructorId()!.Value;
        var vm = await _courseService.GetAssignLessonViewModelAsync(unitId, id);
        if (vm == null) return Forbid();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignLessons(int unitId, List<int> lessonIds)
    {
        var id = GetInstructorId()!.Value;
        await _courseService.SaveLessonAssignmentsAsync(unitId, lessonIds ?? new List<int>(), id);
        TempData["Success"] = "Lesson assignments saved.";
        var vm = await _courseService.GetAssignLessonViewModelAsync(unitId, id);
        return RedirectToAction(nameof(Units), new { courseId = vm?.Course.Id });
    }

    [HttpPost]
    public async Task<IActionResult> ReorderLesson([FromBody] ReorderRequest req)
    {
        await _courseService.ReorderLessonAsync(req.UnitId, req.LessonId, req.NewOrder);
        return Ok();
    }

    // ── Lessons ───────────────────────────────────────────────────
    public async Task<IActionResult> Lessons()
    {
        var id = GetInstructorId()!.Value;
        var lessons = await _lessonService.GetByInstructorIdAsync(id);
        return View(lessons);
    }

    [HttpGet]
    public IActionResult CreateLesson() => View(new LessonEditViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLesson(LessonEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var id = GetInstructorId()!.Value;
        var (success, error, newId) = await _lessonService.CreateAsync(vm, id);
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
        var lesson = await _lessonService.GetByIdAsync(id);
        if (lesson == null || lesson.InstructorId != GetInstructorId()) return Forbid();
        var vm = new LessonEditViewModel { Id = lesson.Id, Title = lesson.Title, XmlContent = lesson.XmlContent };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditLesson(LessonEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var id = GetInstructorId()!.Value;
        var (success, error) = await _lessonService.UpdateAsync(vm, id);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to update lesson.");
            return View(vm);
        }
        TempData["Success"] = "Lesson saved.";
        return RedirectToAction(nameof(Lessons));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLesson(int id)
    {
        var instructorId = GetInstructorId()!.Value;
        await _lessonService.DeleteAsync(id, instructorId);
        TempData["Success"] = "Lesson deleted.";
        return RedirectToAction(nameof(Lessons));
    }

    [HttpPost]
    public IActionResult PreviewLesson([FromBody] PreviewRequest req)
    {
        var html = _lessonService.PreviewXml(req.Xml ?? string.Empty);
        return Content(html, "text/html");
    }

    // ── Helpers ───────────────────────────────────────────────────
    private async Task<WebLearn.Models.Course?> GetUnitCourseAsync(int unitId)
    {
        // We need to find which course this unit belongs to, and verify ownership
        var vm = await _courseService.GetByInstructorIdAsync(GetInstructorId()!.Value);
        foreach (var course in vm)
        {
            var detail = await _courseService.GetCourseDetailAsync(course.Id, includeUnpublished: true);
            if (detail?.Units.Any(u => u.Unit.Id == unitId) == true)
                return course;
        }
        return null;
    }
}

public record ReorderRequest(int UnitId, int LessonId, int NewOrder);
public record PreviewRequest(string? Xml);
