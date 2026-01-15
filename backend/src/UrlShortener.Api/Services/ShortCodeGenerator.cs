using System.Security.Cryptography;
using UrlShortener.Api.Common.Policies;

namespace UrlShortener.Api.Services;

public sealed class ShortCodeGenerator
{
    private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private readonly int _length;
    
    public ShortCodeGenerator(int length = ShortCodePolicy.DefaultGeneratedLength)
    {
        _length = length;
    }

    public string Generate()
    {
        Span<byte> bytes = stackalloc byte [_length];
        RandomNumberGenerator.Fill(bytes);
        
        var chars = new char [_length];
        for (int i = 0; i < _length; i++)
        {
            chars[i] = Alphabet[bytes[i] % Alphabet.Length];
        }
        
        return new string(chars);
    }
}