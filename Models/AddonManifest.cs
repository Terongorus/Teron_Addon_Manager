namespace Teron_Addon_Manager.Models
{
    public sealed record AddonManifest(
        string Name,
        string Version,
        string DownloadUrl,
        AddonSourceKind SourceKind,
        string? ContentHash,
        DateTimeOffset? PublishedAt);
}
