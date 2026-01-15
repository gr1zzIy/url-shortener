using FluentValidation;
using UrlShortener.Api.Common.Policies;
using UrlShortener.Api.Contracts.Urls;

namespace UrlShortener.Api.Validators.Urls;

public sealed class CreateShortUrlRequestValidator : AbstractValidator<CreateShortUrlRequest>
{
    public CreateShortUrlRequestValidator()
    {
        RuleFor(x => x.OriginalUrl)
            .NotEmpty()
            .MaximumLength(UrlPolicy.MaxOriginalUrlLength)
            .Must(BeValidAbsoluteUrl)
            .WithMessage("The OriginalUrl must be a valid absolute URL.");

        RuleFor(x => x.CustomCode)
            .MaximumLength(ShortCodePolicy.MaxLength)
            .Must(code => ShortCodePolicy.AllowedRegex.IsMatch(code!))
            .WithMessage("CustomCode can only contain letters, numbers, underscores and hyphens.")
            .When(x => !string.IsNullOrWhiteSpace(x.CustomCode));

        RuleFor(x => x.CustomCode)
            .Must(code => !ShortCodePolicy.IsReserved(code!))
            .WithMessage($"CustomCode cannot be one of the reserved words: {string.Join(", ", ShortCodePolicy.ReservedWords)}.")
            .When(x => !string.IsNullOrWhiteSpace(x.CustomCode));

        RuleFor(x => x.ExpiresAt)
            .Must(x => x > DateTimeOffset.UtcNow)
            .WithMessage("The ExpiresAt must be in the future.")
            .When(x => x.ExpiresAt is not null);
    }

    private static bool BeValidAbsoluteUrl(string url)
        => Uri.TryCreate(url, UriKind.Absolute, out var u)
           && (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps);
}