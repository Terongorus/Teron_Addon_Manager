namespace TeronAddonManager.Models
{
    public sealed record EsoUiCatalogEntry(
        long Id,
        string Title,
        string Author,
        string Version,
        string FileInfoUri,
        long CategoryId,
        long Downloads,
        long DownloadsMonthly,
        long Favorites,
        DateTimeOffset? LastUpdate,
        bool IsLibrary,
        IReadOnlyList<string> FolderPaths);
}
