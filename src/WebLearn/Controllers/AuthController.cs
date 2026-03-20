using Microsoft.AspNetCore.Mvc;
using WebLearn.Constants;
using WebLearn.Models.ViewModels;
using WebLearn.Services.Interfaces;

namespace WebLearn.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        if (HttpContext.Session.GetInt32(SessionKeys.InstructorId) != null)
            return RedirectToAction("Dashboard", "Instructor");

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var result = await _authService.LoginAsync(vm.Username, vm.Password);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Login failed.");
            return View(vm);
        }

        HttpContext.Session.SetInt32(SessionKeys.InstructorId, result.Instructor!.Id);
        HttpContext.Session.SetString(SessionKeys.InstructorName, result.Instructor.DisplayName);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Dashboard", "Instructor");
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (HttpContext.Session.GetInt32(SessionKeys.InstructorId) != null)
            return RedirectToAction("Dashboard", "Instructor");

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var result = await _authService.RegisterAsync(vm.Username, vm.DisplayName, vm.Email, vm.Password);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Registration failed.");
            return View(vm);
        }

        HttpContext.Session.SetInt32(SessionKeys.InstructorId, result.Instructor!.Id);
        HttpContext.Session.SetString(SessionKeys.InstructorName, result.Instructor.DisplayName);

        TempData["Success"] = "Account created successfully. Welcome!";
        return RedirectToAction("Dashboard", "Instructor");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}
