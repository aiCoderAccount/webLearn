using Microsoft.AspNetCore.Mvc;
using WebLearn.Services.Interfaces;

namespace WebLearn.Controllers;

public class HomeController : Controller
{
    private readonly ICourseService _courseService;

    public HomeController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    public async Task<IActionResult> Index()
    {
        var courses = await _courseService.GetPublishedCoursesAsync();
        return View(courses);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
