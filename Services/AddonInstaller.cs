using System.IO.Compression;
using System.Security.Cryptography;
using Teron_Addon_Manager.Models;
using Teron_Addon_Manager.Sources;

namespace Teron_Addon_Manager.Services
{
    public sealed class AddonInstaller
    {
        private readonly HttpClient _http;
        private readonly AddonSourceResolver _resolver;

        public AddonInstaller(HttpClient http, AddonSourceResolver resolver)
        {
            _http = http;
            _resolver = resolver;
        }

        public async Task<InstalledAddon> InstallAsync(Uri url, GameTarget target, IProgress<string>? progress, CancellationToken ct)
        {
            var manifest = await _resolver.ResolveManifestAsync(url, _http, ct).ConfigureAwait(false);
            var (folderNames, hash) = await DownloadAndExtractAsync(manifest, target, progress, ct).ConfigureAwait(false);

            return new InstalledAddon
            {
                Name = manifest.Name,
                Target = target,
                SourceUrl = url.ToString(),
                SourceKind = manifest.SourceKind,
                InstalledVersion = manifest.Version,
                ContentHash = manifest.ContentHash ?? hash,
                FolderNames = folderNames,
                InstalledAt = DateTimeOffset.UtcNow,
                LastChecked = DateTimeOffset.UtcNow,
                LatestVersion = manifest.Version,
                UpdateAvailable = false
            };
        }

        public async Task UpdateAsync(InstalledAddon existing, IProgress<string>? progress, CancellationToken ct)
        {
            var url = new Uri(existing.SourceUrl);
            var manifest = await _resolver.ResolveManifestAsync(url, _http, ct).ConfigureAwait(false);

            // Remove the previously tracked folders first, in case the new archive renames or drops one.
            RemoveFolders(existing.Target, existing.FolderNames);

            var (folderNames, hash) = await DownloadAndExtractAsync(manifest, existing.Target, progress, ct).ConfigureAwait(false);

            existing.Name = manifest.Name;
            existing.InstalledVersion = manifest.Version;
            existing.ContentHash = manifest.ContentHash ?? hash;
            existing.FolderNames = folderNames;
            existing.LastChecked = DateTimeOffset.UtcNow;
            existing.LatestVersion = manifest.Version;
            existing.UpdateAvailable = false;
            existing.LastCheckError = null;
        }

        public void Remove(InstalledAddon addon) => RemoveFolders(addon.Target, addon.FolderNames);

        private static void RemoveFolders(GameTarget target, IEnumerable<string> folderNames)
        {
            var addOnsFolder = AddonPaths.GetAddOnsFolder(target);
            foreach (var folder in folderNames)
            {
                var path = Path.Combine(addOnsFolder, folder);
                if (Directory.Exists(path))
                {
                    ForceDeleteDirectory(path);
                }
            }
        }

        private async Task<(List<string> FolderNames, string Hash)> DownloadAndExtractAsync(
            AddonManifest manifest, GameTarget target, IProgress<string>? progress, CancellationToken ct)
        {
            AddonPaths.EnsureFoldersExist(target);

            var tempZip = Path.Combine(Path.GetTempPath(), $"teron-addon-{Guid.NewGuid():N}.zip");
            var tempExtractDir = Path.Combine(Path.GetTempPath(), $"teron-addon-{Guid.NewGuid():N}");

            try
            {
                progress?.Report($"Downloading {manifest.Name}...");
                var bytes = await _http.GetByteArrayAsync(manifest.DownloadUrl, ct).ConfigureAwait(false);
                await File.WriteAllBytesAsync(tempZip, bytes, ct).ConfigureAwait(false);
                var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

                progress?.Report($"Extracting {manifest.Name}...");
                Directory.CreateDirectory(tempExtractDir);
                try
                {
                    ZipFile.ExtractToDirectory(tempZip, tempExtractDir);
                }
                catch (InvalidDataException ex)
                {
                    throw new InvalidOperationException(
                        $"'{manifest.Name}' did not download as a valid zip archive. The link may not point directly at an archive.", ex);
                }

                var folderNames = CopyExtractedAddonFolders(tempExtractDir, manifest.Name, target);
                return (folderNames, hash);
            }
            finally
            {
                TryDeleteFile(tempZip);
                TryDeleteDirectory(tempExtractDir);
            }
        }

        private static List<string> CopyExtractedAddonFolders(string extractDir, string fallbackName, GameTarget target)
        {
            var entries = Directory.GetFileSystemEntries(extractDir)
                .Where(e => !IsJunk(Path.GetFileName(e)))
                .ToList();

            var topLevelDirs = entries.Where(Directory.Exists).ToList();
            var topLevelFiles = entries.Where(File.Exists).ToList();

            var sourceFolders = new List<string>();
            if (topLevelDirs.Count == 0 && topLevelFiles.Count > 0)
            {
                // Some archives place files directly at the root instead of inside a named folder; wrap them so
                // they install as a single addon folder rather than scattering loose files into AddOns\.
                // fallbackName comes from the source's reported addon name (e.g. ESOUI/GitHub API response),
                // so it must be sanitized before use as a path segment.
                var wrapped = Path.Combine(extractDir, SanitizeFolderName(fallbackName));
                Directory.CreateDirectory(wrapped);
                foreach (var file in topLevelFiles)
                {
                    File.Move(file, Path.Combine(wrapped, Path.GetFileName(file)));
                }
                sourceFolders.Add(wrapped);
            }
            else
            {
                sourceFolders.AddRange(topLevelDirs);
            }

            var addOnsFolder = AddonPaths.GetAddOnsFolder(target);
            var folderNames = new List<string>();
            foreach (var sourceFolder in sourceFolders)
            {
                var folderName = Path.GetFileName(sourceFolder);
                var destination = Path.Combine(addOnsFolder, folderName);
                if (Directory.Exists(destination))
                {
                    ForceDeleteDirectory(destination);
                }
                CopyDirectory(sourceFolder, destination);
                folderNames.Add(folderName);
            }

            return folderNames;
        }

        private static bool IsJunk(string name) =>
            name.Equals("__MACOSX", StringComparison.OrdinalIgnoreCase) || name == ".DS_Store";

        private static string SanitizeFolderName(string name)
        {
            var baseName = Path.GetFileName(name.Trim());
            var invalid = Path.GetInvalidFileNameChars();
            var sanitized = new string(baseName.Where(c => !invalid.Contains(c)).ToArray()).Trim();
            return string.IsNullOrWhiteSpace(sanitized) ? "Addon" : sanitized;
        }

        private static void CopyDirectory(string sourceDir, string destinationDir)
        {
            Directory.CreateDirectory(destinationDir);
            foreach (var dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourceDir, destinationDir));
            }
            foreach (var filePath in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                var destinationFile = filePath.Replace(sourceDir, destinationDir);
                File.Copy(filePath, destinationFile, overwrite: true);

                // Archives sometimes carry a read-only flag on entries; File.Copy preserves it, which would
                // later block us from deleting/overwriting these files on update or removal.
                ClearReadOnly(destinationFile);
            }
        }

        /// <remarks>Directory.Delete throws on read-only entries, which some addon archives carry; clear the
        /// attribute before deleting so update/remove don't fail on addons that happen to be marked read-only.</remarks>
        private static void ForceDeleteDirectory(string path)
        {
            foreach (var entry in Directory.EnumerateFileSystemEntries(path, "*", SearchOption.AllDirectories))
            {
                ClearReadOnly(entry);
            }
            ClearReadOnly(path);
            Directory.Delete(path, recursive: true);
        }

        private static void ClearReadOnly(string path)
        {
            var attributes = File.GetAttributes(path);
            if ((attributes & FileAttributes.ReadOnly) != 0)
            {
                File.SetAttributes(path, attributes & ~FileAttributes.ReadOnly);
            }
        }

        private static void TryDeleteFile(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        private static void TryDeleteDirectory(string path)
        {
            try { if (Directory.Exists(path)) ForceDeleteDirectory(path); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}
