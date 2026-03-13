namespace WebLearn.Middleware;

public class InstructorAuthMiddleware
{
    private readonly RequestDelegate _next;

    public InstructorAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (path.StartsWith("/Instructor", StringComparison.OrdinalIgnoreCase))
        {
            var instructorId = context.Session.GetInt32("InstructorId");
            if (instructorId == null)
            {
                context.Response.Redirect("/Auth/Login?returnUrl=" + Uri.EscapeDataString(path));
                return;
            }
        }

        await _next(context);
    }
}
