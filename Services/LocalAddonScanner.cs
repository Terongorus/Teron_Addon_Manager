using TeronAddonManager.Models;

namespace TeronAddonManager.Services
{
    public static class LocalAddonScanner
    {
        public static List<ScannedAddonCandidate> Scan(
            GameTarget target,
            IReadOnlyCollection<string> alreadyTrackedFolders,
            Dictionary<string, EsoUiCatalogEntry> folderIndex)
        {
            var addOnsFolder = AddonPaths.GetAddOnsFolder(target);
            if (!Directory.Exists(addOnsFolder))
            {
                return new List<ScannedAddonCandidate>();
            }

            var tracked = new HashSet<string>(alreadyTrackedFolders, StringComparer.OrdinalIgnoreCase);
            var localFolders = Directory.GetDirectories(addOnsFolder)
                .Select(Path.GetFileName)
                .Where(name => name is not null && !tracked.Contains(name))
                .Select(name => name!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var candidates = new List<ScannedAddonCandidate>();
            var consumed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var folder in localFolders)
            {
                if (consumed.Contains(folder))
                {
                    continue;
                }

                var manifest = AddonManifestParser.TryParse(Path.Combine(addOnsFolder, folder));

                if (folderIndex.TryGetValue(folder, out var entry))
                {
                    // Group every other folder this same catalog entry declares, if it's also present and untracked.
                    var groupFolders = entry.FolderPaths
                        .Where(p => localFolders.Contains(p) && !consumed.Contains(p))
                        .ToList();

                    foreach (var groupFolder in groupFolders)
                    {
                        consumed.Add(groupFolder);
                    }

                    candidates.Add(new ScannedAddonCandidate(groupFolders, entry, manifest?.Title, manifest?.Version));
                }
                else
                {
                    consumed.Add(folder);
                    candidates.Add(new ScannedAddonCandidate(new List<string> { folder }, null, manifest?.Title, manifest?.Version));
                }
            }

            return candidates;
        }
    }
}
