using Microsoft.AspNetCore.Mvc;
using WebLearn.Constants;

namespace WebLearn.Controllers;

public abstract class InstructorBaseController : Controller
{
    protected int InstructorId => HttpContext.Session.GetInt32(SessionKeys.InstructorId)!.Value;
}
