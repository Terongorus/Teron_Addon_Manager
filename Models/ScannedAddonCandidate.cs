namespace TeronAddonManager.Models
{
    public sealed record ScannedAddonCandidate(
        List<string> FolderNames,
        EsoUiCatalogEntry? CatalogEntry,
        string? LocalTitle,
        string? LocalVersion);
}
