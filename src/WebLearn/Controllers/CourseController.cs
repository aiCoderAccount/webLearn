using Microsoft.AspNetCore.Mvc;
using WebLearn.Services.Interfaces;

namespace WebLearn.Controllers;

public class CourseController : Controller
{
    private readonly ICourseService _courseService;

    public CourseController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    public async Task<IActionResult> Index()
    {
        var courses = await _courseService.GetPublishedCoursesAsync();
        return View(courses);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var vm = await _courseService.GetCourseDetailAsync(id);
        if (vm == null) return NotFound();
        return View(vm);
    }

    public async Task<IActionResult> UnitDetail(int id)
    {
        var vm = await _courseService.GetUnitDetailAsync(id);
        if (vm == null) return NotFound();
        return View(vm);
    }
}
