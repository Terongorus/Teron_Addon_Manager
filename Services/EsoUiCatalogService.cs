using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Teron_Addon_Manager.Models;

namespace Teron_Addon_Manager.Services
{
    public sealed class EsoUiCatalogService
    {
        private const string FileListUrl = "https://api.mmoui.com/v4/game/ESO/filelist.json";
        private const string CategoryListUrl = "https://api.mmoui.com/v4/game/ESO/categorylist.json";
        private static readonly TimeSpan CacheLifetime = TimeSpan.FromHours(24);
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        private static string CatalogCacheFilePath => Path.Combine(AddonPaths.AppDataFolder, "esoui_catalog.json");
        private static string CategoryCacheFilePath => Path.Combine(AddonPaths.AppDataFolder, "esoui_categories.json");

        public async Task<List<EsoUiCatalogEntry>> GetCatalogAsync(HttpClient http, CancellationToken ct, bool forceRefresh = false)
        {
            if (!forceRefresh)
            {
                var cached = TryLoadCache<EsoUiCatalogEntry>(CatalogCacheFilePath);
                if (cached is not null)
                {
                    return cached;
                }
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var raw = await http.GetFromJsonAsync<List<RawEntry>>(FileListUrl, options, ct).ConfigureAwait(false)
                ?? new List<RawEntry>();
            var entries = raw.Select(MapEntry).ToList();
            SaveCache(CatalogCacheFilePath, entries);
            return entries;
        }

        public async Task<List<EsoUiCategory>> GetCategoriesAsync(HttpClient http, CancellationToken ct, bool forceRefresh = false)
        {
            if (!forceRefresh)
            {
                var cached = TryLoadCache<EsoUiCategory>(CategoryCacheFilePath);
                if (cached is not null)
                {
                    return cached;
                }
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var raw = await http.GetFromJsonAsync<List<RawCategory>>(CategoryListUrl, options, ct).ConfigureAwait(false)
                ?? new List<RawCategory>();
            var categories = raw.Select(c => new EsoUiCategory(c.Id, c.Title ?? "", c.FileCount)).ToList();
            SaveCache(CategoryCacheFilePath, categories);
            return categories;
        }

        public static Dictionary<string, EsoUiCatalogEntry> BuildFolderIndex(IEnumerable<EsoUiCatalogEntry> entries)
        {
            var index = new Dictionary<string, EsoUiCatalogEntry>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in entries)
            {
                foreach (var folder in entry.FolderPaths)
                {
                    index.TryAdd(folder, entry);
                }
            }
            return index;
        }

        private static EsoUiCatalogEntry MapEntry(RawEntry raw)
        {
            DateTimeOffset? lastUpdate = raw.LastUpdate is long ms ? DateTimeOffset.FromUnixTimeMilliseconds(ms) : null;
            var folderPaths = raw.Addons?.Select(a => a.Path).Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p!).ToList()
                ?? new List<string>();

            return new EsoUiCatalogEntry(
                raw.Id,
                raw.Title ?? "",
                raw.Author ?? "",
                raw.Version ?? "",
                raw.FileInfoUri ?? "",
                raw.CategoryId,
                raw.Downloads,
                raw.DownloadsMonthly,
                raw.Favorites,
                lastUpdate,
                raw.Library ?? false,
                folderPaths);
        }

        private static List<T>? TryLoadCache<T>(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                var json = File.ReadAllText(path);
                var cache = JsonSerializer.Deserialize<Cache<T>>(json, JsonOptions);
                if (cache is null || DateTimeOffset.UtcNow - cache.FetchedAt > CacheLifetime)
                {
                    return null;
                }
                return cache.Entries;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static void SaveCache<T>(string path, List<T> entries)
        {
            Directory.CreateDirectory(AddonPaths.AppDataFolder);
            var cache = new Cache<T> { FetchedAt = DateTimeOffset.UtcNow, Entries = entries };
            File.WriteAllText(path, JsonSerializer.Serialize(cache, JsonOptions));
        }

        private sealed class Cache<T>
        {
            public DateTimeOffset FetchedAt { get; set; }
            public List<T> Entries { get; set; } = new();
        }

        private sealed class RawEntry
        {
            public long Id { get; set; }
            public string? Title { get; set; }
            public string? Author { get; set; }
            public string? Version { get; set; }
            public string? FileInfoUri { get; set; }

            [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
            public long CategoryId { get; set; }

            public long Downloads { get; set; }
            public long DownloadsMonthly { get; set; }
            public long Favorites { get; set; }
            public long? LastUpdate { get; set; }
            public bool? Library { get; set; }
            public List<RawAddonPath>? Addons { get; set; }
        }

        private sealed class RawAddonPath
        {
            public string? Path { get; set; }
        }

        private sealed class RawCategory
        {
            [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
            public long Id { get; set; }

            public string? Title { get; set; }

            [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
            public long FileCount { get; set; }
        }
    }
}
