using TeronAddonManager.Models;

namespace TeronAddonManager.Sources
{
    /// <remarks>Unconditional fallback: handles any URL that no other source claims. Must be registered last.</remarks>
    public sealed class DirectZipAddonSource : IAddonSource
    {
        public AddonSourceKind Kind => AddonSourceKind.DirectZip;

        public bool CanHandle(Uri url) => true;

        public async Task<AddonManifest> ResolveAsync(Uri url, HttpClient http, CancellationToken ct)
        {
            var response = await SendHeaderRequestAsync(url, http, ct).ConfigureAwait(false);
            using (response)
            {
                response.EnsureSuccessStatusCode();
                return BuildManifest(url, response);
            }
        }

        /// <remarks>Some static file hosts reject HEAD; fall back to a header-only GET in that case.</remarks>
        private static async Task<HttpResponseMessage> SendHeaderRequestAsync(Uri url, HttpClient http, CancellationToken ct)
        {
            using var headRequest = new HttpRequestMessage(HttpMethod.Head, url);
            var headResponse = await http.SendAsync(headRequest, ct).ConfigureAwait(false);
            if (headResponse.IsSuccessStatusCode)
            {
                return headResponse;
            }

            headResponse.Dispose();
            using var getRequest = new HttpRequestMessage(HttpMethod.Get, url);
            return await http.SendAsync(getRequest, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        }

        private static AddonManifest BuildManifest(Uri url, HttpResponseMessage response)
        {
            var name = Path.GetFileNameWithoutExtension(url.AbsolutePath.TrimEnd('/'));
            if (string.IsNullOrWhiteSpace(name))
            {
                name = url.Host;
            }

            string? hash = response.Headers.ETag?.Tag?.Trim('"');
            var version = hash
                ?? response.Content.Headers.LastModified?.ToString("O")
                ?? "unknown";

            return new AddonManifest(name, version, url.ToString(), AddonSourceKind.DirectZip, hash,
                response.Content.Headers.LastModified);
        }
    }
}
