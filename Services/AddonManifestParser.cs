using TeronAddonManager.Models;

namespace TeronAddonManager.Services
{
    public static class AddonManifestParser
    {
        public static AddonManifestInfo? TryParse(string folderPath)
        {
            var folderName = Path.GetFileName(folderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            var manifestPath = Path.Combine(folderPath, $"{folderName}.txt");
            if (!File.Exists(manifestPath))
            {
                return null;
            }

            string? title = null;
            string? version = null;
            var isLibrary = false;

            foreach (var line in File.ReadLines(manifestPath))
            {
                var trimmed = line.TrimStart();
                if (!trimmed.StartsWith("##"))
                {
                    continue;
                }

                var withoutMarker = trimmed[2..].TrimStart();
                var separatorIndex = withoutMarker.IndexOf(':');
                if (separatorIndex < 0)
                {
                    continue;
                }

                var directive = withoutMarker[..separatorIndex].Trim();
                var value = withoutMarker[(separatorIndex + 1)..].Trim();

                switch (directive.ToLowerInvariant())
                {
                    case "title":
                        title = value;
                        break;
                    case "version":
                        version = value;
                        break;
                    case "islibrary":
                        bool.TryParse(value, out isLibrary);
                        break;
                }
            }

            // Deliberately not falling back to "## AddOnVersion" here: it's an internal, monotonically
            // incrementing build counter the game client uses, not a human/catalog-comparable version string.
            // Using it as a stand-in causes false "update available" flags right after adopting a local addon.
            return new AddonManifestInfo(title, version, isLibrary);
        }
    }
}
