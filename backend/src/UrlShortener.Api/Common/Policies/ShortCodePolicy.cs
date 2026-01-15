using System.Text.RegularExpressions;

namespace UrlShortener.Api.Common.Policies;

public static partial class ShortCodePolicy
{
    public const int MaxLength = 32;
    public const int DefaultGeneratedLength = 8;
    public const int GenerationMaxAttempts = 5;

    // letters, digits, '_' and '-'
    public static readonly Regex AllowedRegex = AllowedRegexImpl();

    public static readonly HashSet<string> ReservedWords =
        new(new[] { "health", "swagger", "api" }, StringComparer.OrdinalIgnoreCase);

    public static bool IsReserved(string code) => ReservedWords.Contains(code);

    [GeneratedRegex("^[a-zA-Z0-9_-]*$")]
    private static partial Regex AllowedRegexImpl();
}