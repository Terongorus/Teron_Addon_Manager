using System.Text.Json;
using Teron_Addon_Manager.Models;
using Teron_Addon_Manager.Sources;

namespace Teron_Addon_Manager.Services
{
    public sealed class UpdateChecker
    {
        private readonly HttpClient _http;
        private readonly AddonSourceResolver _resolver;
        private readonly SemaphoreSlim _throttle = new(4);

        public UpdateChecker(HttpClient http, AddonSourceResolver resolver)
        {
            _http = http;
            _resolver = resolver;
        }

        public async Task CheckAsync(InstalledAddon addon, CancellationToken ct)
        {
            try
            {
                var url = new Uri(addon.SourceUrl);
                var manifest = await _resolver.ResolveManifestAsync(url, _http, ct).ConfigureAwait(false);

                addon.LatestVersion = manifest.Version;
                addon.LastChecked = DateTimeOffset.UtcNow;
                addon.LastCheckError = null;
                addon.UpdateAvailable = DetermineUpdateAvailable(addon, manifest);
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                addon.LastCheckError = "Timed out while checking for updates.";
                addon.LastChecked = DateTimeOffset.UtcNow;
                addon.UpdateAvailable = null;
            }
            catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException or JsonException)
            {
                addon.LastCheckError = ex.Message;
                addon.LastChecked = DateTimeOffset.UtcNow;
                addon.UpdateAvailable = null;
            }
        }

        public async Task CheckAllAsync(IEnumerable<InstalledAddon> addons, CancellationToken ct)
        {
            var tasks = addons.Select(async addon =>
            {
                await _throttle.WaitAsync(ct).ConfigureAwait(false);
                try
                {
                    await CheckAsync(addon, ct).ConfigureAwait(false);
                }
                finally
                {
                    _throttle.Release();
                }
            });

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private static bool? DetermineUpdateAvailable(InstalledAddon addon, AddonManifest manifest)
        {
            if (!string.IsNullOrEmpty(manifest.ContentHash) && !string.IsNullOrEmpty(addon.ContentHash))
            {
                return !string.Equals(manifest.ContentHash, addon.ContentHash, StringComparison.OrdinalIgnoreCase);
            }

            if (!string.IsNullOrEmpty(manifest.Version) && manifest.Version != "unknown" && !string.IsNullOrEmpty(addon.InstalledVersion))
            {
                return !string.Equals(manifest.Version, addon.InstalledVersion, StringComparison.OrdinalIgnoreCase);
            }

            return null;
        }
    }
}
