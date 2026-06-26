using TeronAddonManager.Models;

namespace TeronAddonManager.Sources
{
    public interface IAddonSource
    {
        AddonSourceKind Kind { get; }

        bool CanHandle(Uri url);

        Task<AddonManifest> ResolveAsync(Uri url, HttpClient http, CancellationToken ct);
    }
}
