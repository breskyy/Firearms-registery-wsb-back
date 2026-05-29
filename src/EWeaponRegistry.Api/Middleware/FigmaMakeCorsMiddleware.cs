namespace EWeaponRegistry.Api.Middleware;

/// <summary>
/// Figma Make preview (public HTTPS) → localhost requires PNA headers and early OPTIONS handling.
/// </summary>
public class FigmaMakeCorsMiddleware
{
    private readonly RequestDelegate _next;

    public FigmaMakeCorsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var origin = context.Request.Headers.Origin.ToString();
        context.Response.Headers["Access-Control-Allow-Origin"] = string.IsNullOrEmpty(origin) ? "*" : origin;
        context.Response.Headers["Access-Control-Allow-Methods"] = "*";
        context.Response.Headers["Access-Control-Allow-Headers"] = "*";
        context.Response.Headers["Access-Control-Allow-Private-Network"] = "true";

        if (HttpMethods.IsOptions(context.Request.Method))
        {
            context.Response.StatusCode = StatusCodes.Status204NoContent;
            return;
        }

        await _next(context);
    }
}

public static class FigmaMakeCorsMiddlewareExtensions
{
    public static IApplicationBuilder UseFigmaMakeCors(this IApplicationBuilder app)
    {
        return app.UseMiddleware<FigmaMakeCorsMiddleware>();
    }
}
