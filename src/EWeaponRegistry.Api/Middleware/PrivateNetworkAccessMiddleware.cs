namespace EWeaponRegistry.Api.Middleware;

/// <summary>
/// Chrome Private Network Access: public origins (e.g. Figma iframe preview) need
/// Access-Control-Allow-Private-Network when calling loopback addresses like localhost.
/// </summary>
public class PrivateNetworkAccessMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public PrivateNetworkAccessMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var origin = context.Request.Headers.Origin.ToString();
        if (CorsOriginHelper.IsAllowed(origin, _configuration))
        {
            context.Response.OnStarting(() =>
            {
                CorsOriginHelper.ApplyPrivateNetworkAccessHeader(context);
                return Task.CompletedTask;
            });
        }

        await _next(context);
    }
}

public static class PrivateNetworkAccessMiddlewareExtensions
{
    public static IApplicationBuilder UsePrivateNetworkAccess(this IApplicationBuilder app)
    {
        return app.UseMiddleware<PrivateNetworkAccessMiddleware>();
    }
}
