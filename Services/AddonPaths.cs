using Teron_Addon_Manager.Models;

namespace Teron_Addon_Manager.Services
{
    public static class AddonPaths
    {
        public static string RootFolder { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Elder Scrolls Online");

        public static string AppDataFolder { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Teron_Addon_Manager");

        public static string LibraryFilePath { get; } = Path.Combine(AppDataFolder, "addons.json");

        public static string GetGameFolder(GameTarget target) =>
            Path.Combine(RootFolder, target == GameTarget.Live ? "live" : "ptr");

        public static string GetAddOnsFolder(GameTarget target) =>
            Path.Combine(GetGameFolder(target), "AddOns");

        /// <remarks>The ESO client creates "live"/"ptr" the first time that version is launched, so their
        /// presence on disk tells us which installs actually exist on this machine.</remarks>
        public static IReadOnlyList<GameTarget> DetectInstalledTargets()
        {
            var detected = new List<GameTarget>();
            foreach (var target in new[] { GameTarget.Live, GameTarget.Ptr })
            {
                if (Directory.Exists(GetGameFolder(target)))
                {
                    detected.Add(target);
                }
            }
            return detected;
        }

        public static void EnsureFoldersExist(GameTarget target)
        {
            Directory.CreateDirectory(GetAddOnsFolder(target));
            Directory.CreateDirectory(AppDataFolder);
        }
    }
}
