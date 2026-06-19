using System.Text.Json;
using Teron_Addon_Manager.Models;

namespace Teron_Addon_Manager.Services
{
    public static class AddonLibrary
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        public static List<InstalledAddon> Load()
        {
            if (!File.Exists(AddonPaths.LibraryFilePath))
            {
                return new List<InstalledAddon>();
            }

            try
            {
                var json = File.ReadAllText(AddonPaths.LibraryFilePath);
                return JsonSerializer.Deserialize<List<InstalledAddon>>(json, JsonOptions) ?? new List<InstalledAddon>();
            }
            catch (JsonException)
            {
                return new List<InstalledAddon>();
            }
        }

        public static void Save(IEnumerable<InstalledAddon> addons)
        {
            Directory.CreateDirectory(AddonPaths.AppDataFolder);
            var json = JsonSerializer.Serialize(addons, JsonOptions);
            File.WriteAllText(AddonPaths.LibraryFilePath, json);
        }
    }
}
