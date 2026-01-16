using System.Security.Cryptography;
using UrlShortener.Api.Common.Policies;

namespace UrlShortener.Api.Services;

public sealed class ShortCodeGenerator
{
    private readonly int _length;

    public ShortCodeGenerator(int length = ShortCodePolicy.DefaultGeneratedLength)
    {
        if (length < ShortCodePolicy.MinLength || length > ShortCodePolicy.MaxLength)
            throw new ArgumentOutOfRangeException(nameof(length));

        _length = length;
    }

    public string Generate()
    {
        Span<byte> bytes = stackalloc byte[_length];
        RandomNumberGenerator.Fill(bytes);

        var alphabet = ShortCodePolicy.Alphabet;

        var chars = new char[_length];
        for (int i = 0; i < _length; i++)
            chars[i] = alphabet[bytes[i] % alphabet.Length];

        return new string(chars);
    }
}