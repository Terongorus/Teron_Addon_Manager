using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Teron_Addon_Manager.Models;

namespace Teron_Addon_Manager.Sources
{
    public sealed class EsoUiAddonSource : IAddonSource
    {
        private static readonly Regex InfoIdPattern = new(@"/downloads/info(\d+)-", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public AddonSourceKind Kind => AddonSourceKind.EsoUi;

        public bool CanHandle(Uri url)
        {
            return url.Host.Contains("esoui.com", StringComparison.OrdinalIgnoreCase)
                && InfoIdPattern.IsMatch(url.AbsolutePath);
        }

        public static string? ExtractId(Uri url)
        {
            var match = InfoIdPattern.Match(url.AbsolutePath);
            return match.Success ? match.Groups[1].Value : null;
        }

        public static async Task<string?> FetchDescriptionAsync(string id, HttpClient http, CancellationToken ct)
        {
            var element = await FetchFileDetailsAsync(id, http, ct).ConfigureAwait(false);
            return element.TryGetProperty("UIDescription", out var descriptionProp) ? descriptionProp.GetString() : null;
        }

        private static async Task<JsonElement> FetchFileDetailsAsync(string id, HttpClient http, CancellationToken ct)
        {
            var apiUrl = $"https://api.mmoui.com/v3/game/ESO/filedetails/{id}.json";

            using var response = await http.GetAsync(apiUrl, ct).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct).ConfigureAwait(false);

            var element = document.RootElement;
            if (element.ValueKind == JsonValueKind.Array)
            {
                if (element.GetArrayLength() == 0)
                {
                    throw new InvalidOperationException($"ESOUI returned no file details for addon id {id}.");
                }
                element = element[0];
            }

            return element.Clone();
        }

        public async Task<AddonManifest> ResolveAsync(Uri url, HttpClient http, CancellationToken ct)
        {
            var id = ExtractId(url) ?? throw new InvalidOperationException($"Could not extract an ESOUI addon id from '{url}'.");
            var element = await FetchFileDetailsAsync(id, http, ct).ConfigureAwait(false);

            var name = element.GetProperty("UIName").GetString() ?? $"ESOUI Addon {id}";
            var version = element.TryGetProperty("UIVersion", out var versionProp) ? versionProp.GetString() ?? "" : "";
            var downloadUrl = element.GetProperty("UIDownload").GetString()
                ?? throw new InvalidOperationException($"ESOUI addon {id} did not include a download URL.");
            var hash = element.TryGetProperty("UIMD5", out var hashProp) ? hashProp.GetString() : null;

            DateTimeOffset? publishedAt = null;
            if (element.TryGetProperty("UIDate", out var dateProp))
            {
                long ms;
                if (dateProp.ValueKind == JsonValueKind.Number && dateProp.TryGetInt64(out ms))
                {
                    publishedAt = DateTimeOffset.FromUnixTimeMilliseconds(ms);
                }
                else if (dateProp.ValueKind == JsonValueKind.String
                    && long.TryParse(dateProp.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out ms))
                {
                    publishedAt = DateTimeOffset.FromUnixTimeMilliseconds(ms);
                }
            }

            return new AddonManifest(name, version, downloadUrl, AddonSourceKind.EsoUi, hash, publishedAt);
        }
    }
}
