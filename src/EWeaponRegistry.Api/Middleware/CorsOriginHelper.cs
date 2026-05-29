namespace EWeaponRegistry.Api.Middleware;

internal static class CorsOriginHelper
{
    public static bool IsAllowed(string? origin, IConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(origin))
        {
            return false;
        }

        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (IsFigmaMakeOrigin(uri.Host) || IsLocalDevelopmentOrigin(uri.Host))
        {
            return true;
        }

        return configuration.GetValue("Cors:AllowAnyOrigin", true);
    }

    public static void ApplyHeaders(HttpContext context, IConfiguration configuration)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        var origin = context.Request.Headers.Origin.ToString();
        if (!IsAllowed(origin, configuration))
        {
            return;
        }

        if (!context.Response.Headers.ContainsKey("Access-Control-Allow-Origin"))
        {
            context.Response.Headers["Access-Control-Allow-Origin"] = origin;
        }

        if (!context.Response.Headers.ContainsKey("Access-Control-Allow-Headers"))
        {
            context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization";
        }

        if (!context.Response.Headers.ContainsKey("Access-Control-Allow-Methods"))
        {
            context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, PATCH, DELETE, OPTIONS";
        }

        ApplyPrivateNetworkAccessHeader(context);
    }

    public static void ApplyPrivateNetworkAccessHeader(HttpContext context)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        var requestsPrivateNetwork = context.Request.Headers.ContainsKey("Access-Control-Request-Private-Network");
        if (!requestsPrivateNetwork && !IsLoopbackHost(context.Request.Host.Host))
        {
            return;
        }

        context.Response.Headers["Access-Control-Allow-Private-Network"] = "true";
    }

    private static bool IsFigmaMakeOrigin(string host) =>
        host.EndsWith(".figma.site", StringComparison.OrdinalIgnoreCase)
        || host.Equals("figma.site", StringComparison.OrdinalIgnoreCase);

    private static bool IsLoopbackHost(string host) =>
        host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
        || host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)
        || host.Equals("[::1]", StringComparison.OrdinalIgnoreCase);

    private static bool IsLocalDevelopmentOrigin(string host) =>
        host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
        || host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase);
}
