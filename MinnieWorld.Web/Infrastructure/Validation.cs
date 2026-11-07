using System.ComponentModel.DataAnnotations;

namespace MinnieWorld.Web.Infrastructure;

/// <summary>Runs DataAnnotations on the request body of type <typeparamref name="T"/> (and nested contract objects) and returns a 400 ValidationProblem on failure.</summary>
public class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var model = context.Arguments.OfType<T>().FirstOrDefault();
        if (model is null)
        {
            return Results.BadRequest(new { message = "Request body is required." });
        }

        var errors = new Dictionary<string, List<string>>();
        Collect(model, "", errors);
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors.ToDictionary(e => e.Key, e => e.Value.ToArray()));
        }

        return await next(context);
    }

    private static void Collect(object model, string prefix, Dictionary<string, List<string>> errors)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        foreach (var result in results)
        {
            foreach (var member in result.MemberNames.DefaultIfEmpty(""))
            {
                var key = prefix + JsonCamel(member);
                if (!errors.TryGetValue(key, out var list)) errors[key] = list = [];
                list.Add(result.ErrorMessage ?? "Invalid value.");
            }
        }

        // Validator doesn't descend into nested objects; do it for our own contract types.
        foreach (var prop in model.GetType().GetProperties())
        {
            if (prop.PropertyType.Namespace == typeof(ValidationFilter<>).Namespace!.Replace("Infrastructure", "Contracts")
                && prop.GetValue(model) is { } child)
            {
                Collect(child, $"{prefix}{JsonCamel(prop.Name)}.", errors);
            }
        }
    }

    private static string JsonCamel(string name) => string.IsNullOrEmpty(name) ? name : char.ToLowerInvariant(name[0]) + name[1..];
}

public static class ValidationExtensions
{
    public static RouteHandlerBuilder Validate<T>(this RouteHandlerBuilder builder) where T : class
        => builder.AddEndpointFilter<ValidationFilter<T>>();
}
