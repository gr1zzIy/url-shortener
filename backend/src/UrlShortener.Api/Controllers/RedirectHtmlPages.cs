namespace UrlShortener.Api.Controllers;

internal static class RedirectHtmlPages
{
    public static string NotFound(string shortCode, string message)
        => Render(
            title: "Link not found",
            headline: "This short link doesn't exist",
            body: $"Code <b>/{Escape(shortCode)}</b> is not available. {Escape(message)}",
            badge: "404");

    public static string Gone(string shortCode, string message, DateTimeOffset? expiresAt)
    {
        var expiredAtHtml = expiresAt.HasValue
            ? $"<div class=\"muted\">Expired at: {expiresAt:yyyy-MM-dd HH:mm} UTC</div>"
            : string.Empty;

        return Render(
            title: "Link expired",
            headline: "This short link has expired",
            body: $"Code <b>/{Escape(shortCode)}</b> is no longer active. {Escape(message)}{expiredAtHtml}",
            badge: "410");
    }

    private static string Render(string title, string headline, string body, string badge)
    {
        var lines = new[]
        {
            "<!doctype html>",
            "<html lang=\"en\">",
            "<head>",
            "  <meta charset=\"utf-8\" />",
            "  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />",
            $"  <title>{Escape(title)}</title>",
            "  <style>",
            "    :root {",
            "      color-scheme: dark;",
            "      --bg1:#070A12;",
            "      --bg2:#0B1022;",
            "      --card: rgba(255,255,255,0.07);",
            "      --ring: rgba(255,255,255,0.12);",
            "      --text: rgba(255,255,255,0.92);",
            "      --muted: rgba(255,255,255,0.60);",
            "    }",
            "    body {",
            "      margin:0;",
            "      min-height:100vh;",
            "      display:grid;",
            "      place-items:center;",
            "      background: radial-gradient(1200px 800px at 20% 10%, rgba(255,255,255,0.10), transparent 55%),",
            "                  radial-gradient(900px 700px at 80% 30%, rgba(255,255,255,0.06), transparent 55%),",
            "                  linear-gradient(180deg, var(--bg1), var(--bg2));",
            "      font-family: ui-sans-serif, system-ui, -apple-system, Segoe UI, Roboto, Helvetica, Arial, Apple Color Emoji, Segoe UI Emoji;",
            "      color: var(--text);",
            "    }",
            "    .card {",
            "      width:min(720px, calc(100% - 32px));",
            "      border-radius: 24px;",
            "      background: var(--card);",
            "      border: 1px solid var(--ring);",
            "      backdrop-filter: blur(18px);",
            "      box-shadow: 0 30px 90px rgba(0,0,0,0.45);",
            "      padding: 28px;",
            "      position: relative;",
            "      overflow: hidden;",
            "    }",
            "    .shine {",
            "      position:absolute; inset:-2px;",
            "      background: radial-gradient(900px 300px at 20% 0%, rgba(255,255,255,0.14), transparent 55%);",
            "      pointer-events:none;",
            "    }",
            "    .top {",
            "      display:flex; align-items:center; justify-content:space-between;",
            "      gap: 16px;",
            "    }",
            "    .badge {",
            "      font-size: 12px;",
            "      padding: 6px 10px;",
            "      border-radius: 999px;",
            "      border: 1px solid var(--ring);",
            "      color: var(--muted);",
            "      background: rgba(0,0,0,0.22);",
            "    }",
            "    h1 {",
            "      margin: 14px 0 0;",
            "      font-size: 26px;",
            "      line-height: 1.2;",
            "    }",
            "    p {",
            "      margin: 12px 0 0;",
            "      color: var(--muted);",
            "      line-height: 1.6;",
            "      font-size: 15px;",
            "    }",
            "    .muted {",
            "      margin-top: 10px;",
            "      color: var(--muted);",
            "      font-size: 12px;",
            "    }",
            "    .actions {",
            "      margin-top: 20px;",
            "      display:flex;",
            "      flex-wrap: wrap;",
            "      gap: 10px;",
            "    }",
            "    a.btn {",
            "      display:inline-flex;",
            "      align-items:center;",
            "      justify-content:center;",
            "      padding: 10px 14px;",
            "      border-radius: 16px;",
            "      text-decoration:none;",
            "      border: 1px solid var(--ring);",
            "      background: rgba(255,255,255,0.08);",
            "      color: var(--text);",
            "    }",
            "    a.btn.primary {",
            "      background: rgba(255,255,255,0.90);",
            "      color: #0b0f1a;",
            "      border-color: rgba(255,255,255,0.9);",
            "    }",
            "    a.btn:hover {",
            "      filter: brightness(1.05);",
            "    }",
            "  </style>",
            "</head>",
            "<body>",
            "  <div class=\"card\">",
            "    <div class=\"shine\"></div>",
            "    <div class=\"top\">",
            "      <div style=\"font-weight:600; letter-spacing:-0.2px;\">GlassLink</div>",
            $"      <div class=\"badge\">{Escape(badge)}</div>",
            "    </div>",
            string.Empty,
            $"    <h1>{Escape(headline)}</h1>",
            $"    <p>{body}</p>",
            string.Empty,
            "    <div class=\"actions\">",
            "      <a class=\"btn primary\" href=\"/health\">API health</a>",
            "      <a class=\"btn\" href=\"javascript:history.back()\">Go back</a>",
            "    </div>",
            string.Empty,
            "    <div class=\"muted\">If you believe this is a mistake, contact the owner of this link.</div>",
            "  </div>",
            "</body>",
            "</html>"
        };

        return string.Join(Environment.NewLine, lines);
    }

    private static string Escape(string value)
        => System.Net.WebUtility.HtmlEncode(value);
}
