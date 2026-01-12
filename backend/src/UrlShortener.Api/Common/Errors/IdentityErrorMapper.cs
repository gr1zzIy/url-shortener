using Microsoft.AspNetCore.Identity;

namespace UrlShortener.Api.Common.Errors;

public static class IdentityErrorMapper
{
    public static (int status, string code, IDictionary<string, string[]> errors) ToProblem(IdentityResult result)
    {
        // групуємо помилки як field -> messages
        // для Identity немає строгих "field", тому нормалізуємо:
        // - DuplicateUserName/DuplicateEmail -> "email"
        // - Password* -> "password"
        // - інше -> "general"

        var dict = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var e in result.Errors)
        {
            var key = MapField(e.Code);
            if (!dict.TryGetValue(key, out var list))
                dict[key] = list = new List<string>();

            list.Add(e.Description);
        }

        var errors = dict.ToDictionary(k => k.Key, v => v.Value.Distinct().ToArray(), StringComparer.OrdinalIgnoreCase);

        // Конфлікт: email/username зайняті
        var isConflict = result.Errors.Any(e =>
            e.Code.Contains("Duplicate", StringComparison.OrdinalIgnoreCase));

        return isConflict
            ? (StatusCodes.Status409Conflict, ApiErrorCodes.Conflict, errors)
            : (StatusCodes.Status400BadRequest, ApiErrorCodes.ValidationFailed, errors);
    }

    private static string MapField(string code)
    {
        if (code.Contains("Email", StringComparison.OrdinalIgnoreCase) || code.Contains("UserName", StringComparison.OrdinalIgnoreCase))
            return "email";
        if (code.Contains("Password", StringComparison.OrdinalIgnoreCase))
            return "password";
        return "general";
    }
}