using System.Net.Http.Headers;
using System.Text.Json;
using TeronAddonManager.Models;

namespace TeronAddonManager.Sources
{
    public sealed class GitHubReleaseAddonSource : IAddonSource
    {
        public AddonSourceKind Kind => AddonSourceKind.GitHubRelease;

        public bool CanHandle(Uri url)
        {
            if (!url.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var segments = url.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            return segments.Length >= 2;
        }

        public async Task<AddonManifest> ResolveAsync(Uri url, HttpClient http, CancellationToken ct)
        {
            var segments = url.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            var owner = segments[0];
            var repo = segments[1];

            var apiUrl = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";
            using var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("TeronAddonManager", "1.0"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

            using var response = await http.SendAsync(request, ct).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct).ConfigureAwait(false);
            var root = document.RootElement;

            var version = root.TryGetProperty("tag_name", out var tagProp) ? tagProp.GetString() ?? "" : "";

            string? downloadUrl = null;
            string? hash = null;
            if (root.TryGetProperty("assets", out var assetsProp) && assetsProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var asset in assetsProp.EnumerateArray())
                {
                    var assetName = asset.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;
                    if (assetName is not null && assetName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    {
                        downloadUrl = asset.TryGetProperty("browser_download_url", out var urlProp) ? urlProp.GetString() : null;
                        if (asset.TryGetProperty("digest", out var digestProp) && digestProp.ValueKind == JsonValueKind.String)
                        {
                            var digest = digestProp.GetString();
                            hash = digest?.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase) == true
                                ? digest["sha256:".Length..]
                                : digest;
                        }
                        break;
                    }
                }
            }

            downloadUrl ??= root.TryGetProperty("zipball_url", out var zipballProp) ? zipballProp.GetString() : null;
            if (downloadUrl is null)
            {
                throw new InvalidOperationException($"GitHub repo '{owner}/{repo}' has no downloadable release asset.");
            }

            var name = root.TryGetProperty("name", out var nameElProp) && nameElProp.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(nameElProp.GetString())
                ? nameElProp.GetString()!
                : repo;

            DateTimeOffset? publishedAt = null;
            if (root.TryGetProperty("published_at", out var publishedProp)
                && publishedProp.ValueKind == JsonValueKind.String
                && DateTimeOffset.TryParse(publishedProp.GetString(), out var parsed))
            {
                publishedAt = parsed;
            }

            return new AddonManifest(name, version, downloadUrl, AddonSourceKind.GitHubRelease, hash, publishedAt);
        }
    }
}
