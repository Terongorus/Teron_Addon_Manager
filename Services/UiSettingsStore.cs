using System.Text.Json;
using System.Text.Json.Serialization;
using Teron_Addon_Manager.Models;

namespace Teron_Addon_Manager.Services
{
    public static class UiSettingsStore
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public static UiSettings Load()
        {
            if (!File.Exists(AddonPaths.UiSettingsFilePath))
            {
                return new UiSettings();
            }

            try
            {
                var json = File.ReadAllText(AddonPaths.UiSettingsFilePath);
                return JsonSerializer.Deserialize<UiSettings>(json, JsonOptions) ?? new UiSettings();
            }
            catch (JsonException)
            {
                return new UiSettings();
            }
        }

        public static void Save(UiSettings settings)
        {
            Directory.CreateDirectory(AddonPaths.AppDataFolder);
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(AddonPaths.UiSettingsFilePath, json);
        }
    }
}
