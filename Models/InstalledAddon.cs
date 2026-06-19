namespace Teron_Addon_Manager.Models
{
    public sealed class InstalledAddon
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public GameTarget Target { get; set; } = GameTarget.Live;
        public string SourceUrl { get; set; } = string.Empty;
        public AddonSourceKind SourceKind { get; set; }
        public string InstalledVersion { get; set; } = string.Empty;
        public string? ContentHash { get; set; }
        public List<string> FolderNames { get; set; } = new();
        public DateTimeOffset InstalledAt { get; set; }
        public DateTimeOffset? LastChecked { get; set; }
        public string? LatestVersion { get; set; }

        /// <remarks>null means the last check couldn't determine a definite answer (no comparable version/hash signal).</remarks>
        public bool? UpdateAvailable { get; set; }
        public string? LastCheckError { get; set; }
    }
}
