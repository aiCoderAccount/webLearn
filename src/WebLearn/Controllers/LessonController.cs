using Microsoft.AspNetCore.Mvc;
using WebLearn.Services.Interfaces;

namespace WebLearn.Controllers;

public class LessonController : Controller
{
    private readonly ILessonService _lessonService;

    public LessonController(ILessonService lessonService)
    {
        _lessonService = lessonService;
    }

    public async Task<IActionResult> View(int id, int? unitId)
    {
        var vm = await _lessonService.RenderLessonAsync(id);
        if (vm == null) return NotFound();
        ViewData["UnitId"] = unitId;
        return View(vm);
    }
}
