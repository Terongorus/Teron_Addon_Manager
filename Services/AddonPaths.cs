using TeronAddonManager.Models;

namespace TeronAddonManager.Services
{
    public static class AddonPaths
    {
        public static string RootFolder { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Elder Scrolls Online");

        public static string AppDataFolder { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TeronAddonManager");

        public static string LibraryFilePath { get; } = Path.Combine(AppDataFolder, "addons.json");

        public static string UiSettingsFilePath { get; } = Path.Combine(AppDataFolder, "uisettings.json");

        public static string GetGameFolder(GameTarget target) =>
            Path.Combine(RootFolder, target == GameTarget.Live ? "live" : "ptr");

        public static string GetAddOnsFolder(GameTarget target) =>
            Path.Combine(GetGameFolder(target), "AddOns");

        /// <remarks>Checks each target's AddOns folder specifically (not just the live/ptr parent), since that's
        /// what this tool actually needs to manage addons for that install. This tool never creates these
        /// folders itself — only the ESO client does — so a target only shows up once it already has one.</remarks>
        public static IReadOnlyList<GameTarget> DetectInstalledTargets()
        {
            var detected = new List<GameTarget>();
            foreach (var target in new[] { GameTarget.Live, GameTarget.Ptr })
            {
                if (Directory.Exists(GetAddOnsFolder(target)))
                {
                    detected.Add(target);
                }
            }
            return detected;
        }
    }
}
