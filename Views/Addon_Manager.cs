using Teron_Addon_Manager.Models;
using Teron_Addon_Manager.Services;
using Teron_Addon_Manager.Sources;

namespace Teron_Addon_Manager
{
    public partial class Addon_Manager : Form
    {
        private readonly HttpClient _http = new();
        private readonly AddonSourceResolver _resolver = new();
        private readonly AddonInstaller _installer;
        private readonly UpdateChecker _updateChecker;
        private readonly EsoUiCatalogService _catalogService = new();
        private List<InstalledAddon> _addons = new();

        // Populated from AddonPaths.DetectInstalledTargets() — only targets that already have an AddOns
        // folder on disk are listed, in display order; this tool never creates those folders itself.
        private List<GameTarget> _availableTargets = new();

        public Addon_Manager()
        {
            InitializeComponent();

            _http.DefaultRequestHeaders.UserAgent.ParseAdd("Teron-Addon-Manager/1.0");
            _installer = new AddonInstaller(_http, _resolver);
            _updateChecker = new UpdateChecker(_http, _resolver);

            targetListBox.SelectedIndexChanged += TargetListBox_SelectedIndexChanged;

            addAddonMenuItem.Click += AddAddonButton_Click;
            checkUpdatesMenuItem.Click += CheckUpdatesButton_Click;
            updateSelectedMenuItem.Click += UpdateSelectedButton_Click;
            removeSelectedMenuItem.Click += RemoveSelectedButton_Click;
            scanLocalMenuItem.Click += ScanLocalMenuItem_Click;
            browseMarketplaceMenuItem.Click += BrowseMarketplaceMenuItem_Click;
            openFolderMenuItem.Click += OpenFolderButton_Click;
            Load += Addon_Manager_Load;

            addonListView.MouseDown += AddonListView_MouseDown;
            addonListView.KeyDown += AddonListView_KeyDown;
            viewDetailsContextItem.Click += ViewDetailsContextItem_Click;
            checkSelectedContextItem.Click += CheckSelectedContextItem_Click;
            updateSelectedContextItem.Click += UpdateSelectedButton_Click;
            removeSelectedContextItem.Click += RemoveSelectedButton_Click;
        }

        private void AddonListView_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                foreach (ListViewItem item in addonListView.Items)
                {
                    item.Selected = true;
                }
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void AddonListView_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            var hit = addonListView.HitTest(e.Location);
            if (hit.Item is null || hit.Item.Selected)
            {
                return;
            }

            foreach (ListViewItem selected in addonListView.SelectedItems.Cast<ListViewItem>().ToList())
            {
                selected.Selected = false;
            }
            hit.Item.Selected = true;
            hit.Item.Focused = true;
        }

        private async void ViewDetailsContextItem_Click(object? sender, EventArgs e)
        {
            var addon = SelectedAddons().FirstOrDefault();
            if (addon is null)
            {
                return;
            }

            var fields = new List<(string Label, string Value)>
            {
                ("Game Version", DisplayName(addon.Target)),
                ("Source", addon.SourceKind.ToString()),
                ("Source URL", addon.SourceUrl),
                ("Installed Version", addon.InstalledVersion),
                ("Latest Version", addon.LatestVersion ?? "(not checked)"),
                ("Status", StatusText(addon)),
                ("Content Hash", addon.ContentHash ?? "(none)"),
                ("Installed At", addon.InstalledAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm")),
                ("Last Checked", addon.LastChecked is { } lastChecked ? lastChecked.ToLocalTime().ToString("yyyy-MM-dd HH:mm") : "(never)"),
                ("Last Check Error", addon.LastCheckError ?? "(none)"),
                ("Folders", string.Join(", ", addon.FolderNames))
            };

            string? description = null;
            if (addon.SourceKind == AddonSourceKind.EsoUi && EsoUiAddonSource.ExtractId(new Uri(addon.SourceUrl)) is { } id)
            {
                SetBusy(true, "Loading addon description...");
                try
                {
                    description = await EsoUiAddonSource.FetchDescriptionAsync(id, _http, CancellationToken.None).ConfigureAwait(true);
                }
                catch (Exception ex)
                {
                    SetStatus($"Could not load addon description: {ex.Message}");
                }
                finally
                {
                    SetBusy(false);
                }
            }

            using var dialog = new AddonDetailsDialog(addon.Name, $"{addon.SourceKind} addon", fields, description);
            dialog.ShowDialog(this);
        }

        private async void CheckSelectedContextItem_Click(object? sender, EventArgs e)
        {
            var selected = SelectedAddons().ToList();
            if (selected.Count == 0)
            {
                return;
            }

            SetBusy(true, "Checking selected addons for updates...");
            try
            {
                await _updateChecker.CheckAllAsync(selected, CancellationToken.None);
                AddonLibrary.Save(_addons);
                RefreshListView();
                SetStatus($"Checked {selected.Count} addon(s) for updates.");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private static string DisplayName(GameTarget target) => target == GameTarget.Ptr ? "ESO PTR" : "ESO Live";

        private GameTarget SelectedTarget =>
            targetListBox.SelectedIndex >= 0 && targetListBox.SelectedIndex < _availableTargets.Count
                ? _availableTargets[targetListBox.SelectedIndex]
                : GameTarget.Live;

        private async void Addon_Manager_Load(object? sender, EventArgs e)
        {
            _availableTargets = AddonPaths.DetectInstalledTargets().ToList();
            targetListBox.Items.Clear();
            foreach (var target in _availableTargets)
            {
                targetListBox.Items.Add(DisplayName(target));
            }
            if (_availableTargets.Count > 0)
            {
                targetListBox.SelectedIndex = 0;
            }

            _addons = AddonLibrary.Load();
            RefreshListView();
            await RunUpdateCheckAsync();
            await ScanForLocalAddonsAsync(isManualTrigger: false);
        }

        private void TargetListBox_SelectedIndexChanged(object? sender, EventArgs e) => RefreshListView();

        private IEnumerable<InstalledAddon> AddonsForSelectedTarget() =>
            _addons.Where(a => a.Target == SelectedTarget);

        private void RefreshListView()
        {
            addonListView.BeginUpdate();
            addonListView.Items.Clear();
            foreach (var addon in AddonsForSelectedTarget())
            {
                addonListView.Items.Add(CreateListViewItem(addon));
            }
            addonListView.EndUpdate();
        }

        private static ListViewItem CreateListViewItem(InstalledAddon addon)
        {
            var item = new ListViewItem(addon.Name) { Tag = addon, UseItemStyleForSubItems = false };
            item.SubItems.Add(addon.InstalledVersion);
            item.SubItems.Add(addon.LatestVersion ?? "");
            item.SubItems.Add(StatusText(addon)).BackColor = StatusColor(addon);
            item.SubItems.Add(addon.SourceKind.ToString());
            return item;
        }

        private static string StatusText(InstalledAddon addon)
        {
            if (addon.LastCheckError is not null) return "Check failed";
            if (addon.UpdateAvailable == true) return "Update available";
            if (addon.UpdateAvailable == false) return "Up to date";
            return "Unknown";
        }

        private static Color StatusColor(InstalledAddon addon)
        {
            if (addon.LastCheckError is not null) return Color.FromArgb(255, 199, 206);
            if (addon.UpdateAvailable == true) return Color.FromArgb(255, 235, 156);
            if (addon.UpdateAvailable == false) return Color.FromArgb(198, 239, 206);
            return SystemColors.Window;
        }

        private async void AddAddonButton_Click(object? sender, EventArgs e)
        {
            using var dialog = new AddAddonDialog();
            if (dialog.ShowDialog(this) != DialogResult.OK || dialog.ResultUrl is null)
            {
                return;
            }

            var target = SelectedTarget;
            SetBusy(true, $"Adding addon for {DisplayName(target)}...");
            try
            {
                var addon = await _installer.InstallAsync(dialog.ResultUrl, target, new Progress<string>(SetStatus), CancellationToken.None);
                _addons.Add(addon);
                AddonLibrary.Save(_addons);
                RefreshListView();
                SetStatus($"Installed {addon.Name} {addon.InstalledVersion}.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Could not add addon", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus("Add addon failed.");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void CheckUpdatesButton_Click(object? sender, EventArgs e) => await RunUpdateCheckAsync();

        private async Task RunUpdateCheckAsync()
        {
            if (_addons.Count == 0)
            {
                return;
            }

            SetBusy(true, "Checking for updates...");
            try
            {
                await _updateChecker.CheckAllAsync(_addons, CancellationToken.None);
                AddonLibrary.Save(_addons);
                RefreshListView();
                SetStatus("Update check complete.");
            }
            catch (Exception ex)
            {
                SetStatus($"Update check failed: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void UpdateSelectedButton_Click(object? sender, EventArgs e)
        {
            var selected = SelectedAddons().ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show(this, "Select one or more addons to update.", "Update Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SetBusy(true, "Updating selected addons...");
            try
            {
                foreach (var addon in selected)
                {
                    SetStatus($"Updating {addon.Name}...");
                    try
                    {
                        await _installer.UpdateAsync(addon, new Progress<string>(SetStatus), CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, $"Failed to update {addon.Name}: {ex.Message}", "Update failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                AddonLibrary.Save(_addons);
                RefreshListView();
                SetStatus("Update complete.");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void RemoveSelectedButton_Click(object? sender, EventArgs e)
        {
            var selected = SelectedAddons().ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show(this, "Select one or more addons to remove.", "Remove Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var names = string.Join(", ", selected.Select(a => a.Name));
            var confirm = MessageBox.Show(this, $"Remove {names} and delete their files from the AddOns folder?",
                "Confirm Remove", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes)
            {
                return;
            }

            var removed = new List<string>();
            foreach (var addon in selected)
            {
                try
                {
                    _installer.Remove(addon);
                    _addons.Remove(addon);
                    removed.Add(addon.Name);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"Failed to remove {addon.Name}: {ex.Message}", "Remove failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            AddonLibrary.Save(_addons);
            RefreshListView();
            if (removed.Count > 0)
            {
                SetStatus($"Removed {string.Join(", ", removed)}.");
            }
        }

        private async void ScanLocalMenuItem_Click(object? sender, EventArgs e) => await ScanForLocalAddonsAsync(isManualTrigger: true);

        private async Task ScanForLocalAddonsAsync(bool isManualTrigger)
        {
            var target = SelectedTarget;
            SetBusy(true, "Scanning for local addons...");
            try
            {
                var catalog = await _catalogService.GetCatalogAsync(_http, CancellationToken.None).ConfigureAwait(true);
                var folderIndex = EsoUiCatalogService.BuildFolderIndex(catalog);
                var trackedFolders = _addons.Where(a => a.Target == target).SelectMany(a => a.FolderNames).ToList();

                var candidates = LocalAddonScanner.Scan(target, trackedFolders, folderIndex);
                if (candidates.Count == 0)
                {
                    SetStatus("No new local addons found.");
                    if (isManualTrigger)
                    {
                        MessageBox.Show(this, "No unmanaged addon folders were found.", "Scan for Local Addons", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return;
                }

                using var dialog = new ScanResultsDialog(candidates);
                if (dialog.ShowDialog(this) != DialogResult.OK || dialog.AdoptedCandidates.Count == 0)
                {
                    SetStatus($"Found {candidates.Count} local addon(s); none adopted.");
                    return;
                }

                var adopted = new List<string>();
                var skipped = new List<string>();
                foreach (var candidate in dialog.AdoptedCandidates)
                {
                    var entry = candidate.CatalogEntry!;
                    if (TryFindFolderConflict(target, candidate.FolderNames, out var conflictFolder))
                    {
                        skipped.Add($"{entry.Title} (folder '{conflictFolder}' is already tracked by another addon)");
                        continue;
                    }

                    _addons.Add(new InstalledAddon
                    {
                        Name = entry.Title,
                        Target = target,
                        SourceUrl = entry.FileInfoUri,
                        SourceKind = AddonSourceKind.EsoUi,
                        InstalledVersion = candidate.LocalVersion ?? entry.Version,
                        FolderNames = candidate.FolderNames,
                        InstalledAt = DateTimeOffset.UtcNow
                    });
                    adopted.Add(entry.Title);
                }

                AddonLibrary.Save(_addons);
                RefreshListView();
                SetStatus(adopted.Count > 0 ? $"Adopted {adopted.Count} local addon(s)." : "No addons adopted.");
                if (skipped.Count > 0)
                {
                    MessageBox.Show(this, $"Skipped (already tracked):{Environment.NewLine}{string.Join(Environment.NewLine, skipped)}",
                        "Scan for Local Addons", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                if (adopted.Count > 0)
                {
                    await RunUpdateCheckAsync();
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Local addon scan failed: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void BrowseMarketplaceMenuItem_Click(object? sender, EventArgs e)
        {
            using var dialog = new MarketplaceForm(_http);
            if (dialog.ShowDialog(this) != DialogResult.OK || dialog.SelectedAddons.Count == 0)
            {
                return;
            }

            var target = SelectedTarget;
            SetBusy(true, "Installing from marketplace...");
            try
            {
                foreach (var entry in dialog.SelectedAddons)
                {
                    SetStatus($"Installing {entry.Title}...");
                    try
                    {
                        var addon = await _installer.InstallAsync(new Uri(entry.FileInfoUri), target, new Progress<string>(SetStatus), CancellationToken.None);

                        // The install already wrote addon.FolderNames to disk, possibly overwriting a folder some
                        // other tracked addon claims. Re-point tracking to the new source rather than duplicate it.
                        if (TryFindFolderConflict(target, addon.FolderNames, out _, excluding: null))
                        {
                            var replaced = _addons.Where(a => a.Target == target && a.FolderNames.Intersect(addon.FolderNames, StringComparer.OrdinalIgnoreCase).Any()).ToList();
                            foreach (var old in replaced)
                            {
                                _addons.Remove(old);
                            }
                            MessageBox.Show(this,
                                $"'{entry.Title}' shares a folder with {string.Join(", ", replaced.Select(a => a.Name))}, which was already tracked. Replaced it with the marketplace install.",
                                "Install", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }

                        _addons.Add(addon);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, $"Failed to install {entry.Title}: {ex.Message}", "Install failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                AddonLibrary.Save(_addons);
                RefreshListView();
                SetStatus($"Installed {dialog.SelectedAddons.Count} addon(s) from the marketplace.");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private bool TryFindFolderConflict(GameTarget target, IEnumerable<string> folderNames, out string? conflictFolder, InstalledAddon? excluding = null)
        {
            var tracked = new HashSet<string>(
                _addons.Where(a => a.Target == target && a != excluding).SelectMany(a => a.FolderNames),
                StringComparer.OrdinalIgnoreCase);
            conflictFolder = folderNames.FirstOrDefault(tracked.Contains);
            return conflictFolder is not null;
        }

        private void OpenFolderButton_Click(object? sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", AddonPaths.GetAddOnsFolder(SelectedTarget));
        }

        private IEnumerable<InstalledAddon> SelectedAddons() =>
            addonListView.SelectedItems.Cast<ListViewItem>().Select(i => (InstalledAddon)i.Tag!);

        private void SetBusy(bool busy, string? status = null)
        {
            UseWaitCursor = busy;
            addAddonMenuItem.Enabled = !busy;
            checkUpdatesMenuItem.Enabled = !busy;
            updateSelectedMenuItem.Enabled = !busy;
            removeSelectedMenuItem.Enabled = !busy;
            scanLocalMenuItem.Enabled = !busy;
            browseMarketplaceMenuItem.Enabled = !busy;
            checkSelectedContextItem.Enabled = !busy;
            updateSelectedContextItem.Enabled = !busy;
            removeSelectedContextItem.Enabled = !busy;
            targetListBox.Enabled = !busy;
            if (status is not null)
            {
                SetStatus(status);
            }
        }

        private void SetStatus(string text) => statusLabel.Text = text;
    }
}
