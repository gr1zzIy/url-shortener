using System.Text.RegularExpressions;

namespace UrlShortener.Api.Common.Policies;

public static partial class ShortCodePolicy
{
    // Довжина коду в URL 
    public const int MinLength = 4;
    public const int MaxLength = 32;

    // Налаштування генератора
    public const int DefaultGeneratedLength = 8;
    public const int GenerationMaxAttempts = 8;

    // Base62 alphabet
    public const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    
    // Зарезервовані сегменти кореневого роуту
    public static readonly HashSet<string> ReservedWords =
        new(["health", "swagger", "api"], StringComparer.OrdinalIgnoreCase);

    // Лише A-Z a-z 0-9 
    private static readonly Regex AllowedRegex = AllowedRegexImpl();

    public static bool IsReserved(string code) => ReservedWords.Contains(code);

    public static bool IsValid(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return false;
        if (code.Length < MinLength || code.Length > MaxLength) return false;
        return AllowedRegex.IsMatch(code);
    }

    // Нормалізація для customCode
    public static string Normalize(string code) => code.Trim();

    [GeneratedRegex("^[A-Za-z0-9]+$")]
    private static partial Regex AllowedRegexImpl();
}