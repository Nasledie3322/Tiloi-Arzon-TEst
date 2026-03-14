namespace TiloiArzon.Client.Services;

public sealed record AppUrls(Uri ApiBaseUri)
{
    public string ResolveApiRelative(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return string.Empty;
        if (Uri.TryCreate(url, UriKind.Absolute, out var absolute)) return absolute.ToString();
        if (url.StartsWith('/')) return new Uri(ApiBaseUri, url).ToString();
        return new Uri(ApiBaseUri, "/" + url).ToString();
    }
}

