using Teron_Addon_Manager.Models;

namespace Teron_Addon_Manager.Sources
{
    public interface IAddonSource
    {
        AddonSourceKind Kind { get; }

        bool CanHandle(Uri url);

        Task<AddonManifest> ResolveAsync(Uri url, HttpClient http, CancellationToken ct);
    }
}
