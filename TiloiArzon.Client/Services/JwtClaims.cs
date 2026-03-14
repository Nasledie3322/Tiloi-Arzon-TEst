using System.Security.Claims;
using System.Text.Json;

namespace TiloiArzon.Client.Services;

public static class JwtClaims
{
    public static IEnumerable<Claim> ParseClaims(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes) ?? new();

        foreach (var kvp in keyValuePairs)
        {
            if (kvp.Value is JsonElement element)
            {
                if (element.ValueKind == JsonValueKind.Array)
                {
                    foreach (var v in element.EnumerateArray())
                    {
                        yield return new Claim(kvp.Key, v.ToString());
                    }
                }
                else
                {
                    yield return new Claim(kvp.Key, element.ToString());
                }
            }
            else
            {
                yield return new Claim(kvp.Key, kvp.Value.ToString() ?? string.Empty);
            }
        }
    }

    public static bool IsExpired(string jwt, DateTimeOffset now)
    {
        try
        {
            var exp = ParseClaims(jwt).FirstOrDefault(c => c.Type == "exp")?.Value;
            if (exp == null) return false;
            var seconds = long.Parse(exp);
            var expiry = DateTimeOffset.FromUnixTimeSeconds(seconds);
            return expiry <= now;
        }
        catch
        {
            return false;
        }
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        base64 = base64.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}

