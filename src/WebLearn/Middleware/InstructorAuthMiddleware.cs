using WebLearn.Constants;

namespace WebLearn.Middleware;

public class InstructorAuthMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (path.StartsWith(Routes.InstructorPrefix, StringComparison.OrdinalIgnoreCase)
            && context.Session.GetInt32(SessionKeys.InstructorId) == null)
        {
            context.Response.Redirect("/Auth/Login?returnUrl=" + Uri.EscapeDataString(path));
            return;
        }

        await next(context);
    }
}
