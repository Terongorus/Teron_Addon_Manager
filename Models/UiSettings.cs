namespace Teron_Addon_Manager.Models
{
    public sealed class UiSettings
    {
        public string? ThemeMode { get; set; }
        public string? AddonSortColumn { get; set; }
        public string? AddonSortDirection { get; set; }
        public GameTarget? SelectedGameTarget { get; set; }
        public WindowPlacement? AddonManagerWindow { get; set; }
        public WindowPlacement? MarketplaceWindow { get; set; }
    }

    public sealed class WindowPlacement
    {
        public bool IsMaximized { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
    }
}
