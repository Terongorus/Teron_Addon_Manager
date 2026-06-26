namespace TeronAddonManager.Models
{
    public sealed record AddonManifest(
        string Name,
        string Version,
        string DownloadUrl,
        AddonSourceKind SourceKind,
        string? ContentHash,
        DateTimeOffset? PublishedAt);
}
