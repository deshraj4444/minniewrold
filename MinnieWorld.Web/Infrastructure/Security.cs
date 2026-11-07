namespace MinnieWorld.Web.Infrastructure;

public static class RateLimits
{
    public const string Forms = "forms";
    public const string Login = "login";
    public const string Visits = "visits";
}

/// <summary>
/// CSRF defence for the cookie-authenticated admin API: state-changing requests must carry a custom
/// header. Browsers won't let a cross-site form or image set one, and a cross-site fetch() that does
/// is blocked by CORS preflight (no CORS policy is configured).
/// </summary>
public class RequireCsrfHeaderFilter : IEndpointFilter
{
    public const string HeaderName = "X-Requested-With";

    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.HttpContext.Request;
        if (!HttpMethods.IsGet(request.Method) && !HttpMethods.IsHead(request.Method) && !request.Headers.ContainsKey(HeaderName))
        {
            return ValueTask.FromResult<object?>(Results.Problem("Missing X-Requested-With header.", statusCode: StatusCodes.Status400BadRequest));
        }

        return next(context);
    }
}
