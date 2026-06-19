using Teron_Addon_Manager.Models;

namespace Teron_Addon_Manager.Sources
{
    public sealed class AddonSourceResolver
    {
        private readonly IReadOnlyList<IAddonSource> _sources;

        public AddonSourceResolver()
        {
            // DirectZipAddonSource is the unconditional fallback and must stay last.
            _sources = new IAddonSource[]
            {
                new EsoUiAddonSource(),
                new GitHubReleaseAddonSource(),
                new DirectZipAddonSource()
            };
        }

        public IAddonSource Resolve(Uri url) => _sources.First(s => s.CanHandle(url));

        public Task<AddonManifest> ResolveManifestAsync(Uri url, HttpClient http, CancellationToken ct) =>
            Resolve(url).ResolveAsync(url, http, ct);
    }
}
