using Teron_Addon_Manager.Models;
using Teron_Addon_Manager.Services;
using Teron_Addon_Manager.Sources;

namespace Teron_Addon_Manager
{
    public partial class Addon_Manager : Form
    {
        private static readonly GameTarget[] TargetsInDisplayOrder = { GameTarget.Live, GameTarget.Ptr };

        private readonly HttpClient _http = new();
        private readonly AddonSourceResolver _resolver = new();
        private readonly AddonInstaller _installer;
        private readonly UpdateChecker _updateChecker;
        private List<InstalledAddon> _addons = new();

        public Addon_Manager()
        {
            InitializeComponent();

            _http.DefaultRequestHeaders.UserAgent.ParseAdd("Teron-Addon-Manager/1.0");
            _installer = new AddonInstaller(_http, _resolver);
            _updateChecker = new UpdateChecker(_http, _resolver);

            foreach (var target in TargetsInDisplayOrder)
            {
                targetListBox.Items.Add(DisplayName(target));
            }
            targetListBox.SelectedIndexChanged += TargetListBox_SelectedIndexChanged;

            addAddonMenuItem.Click += AddAddonButton_Click;
            checkUpdatesMenuItem.Click += CheckUpdatesButton_Click;
            updateSelectedMenuItem.Click += UpdateSelectedButton_Click;
            removeSelectedMenuItem.Click += RemoveSelectedButton_Click;
            openFolderMenuItem.Click += OpenFolderButton_Click;
            Load += Addon_Manager_Load;
        }

        private static string DisplayName(GameTarget target) => target == GameTarget.Ptr ? "ESO PTR" : "ESO Live";

        private GameTarget SelectedTarget =>
            targetListBox.SelectedIndex >= 0 ? TargetsInDisplayOrder[targetListBox.SelectedIndex] : GameTarget.Live;

        private async void Addon_Manager_Load(object? sender, EventArgs e)
        {
            var detected = AddonPaths.DetectInstalledTargets();
            var defaultTarget = detected.Contains(GameTarget.Live) ? GameTarget.Live
                : detected.Count > 0 ? detected[0]
                : GameTarget.Live;
            targetListBox.SelectedIndex = Array.IndexOf(TargetsInDisplayOrder, defaultTarget);

            _addons = AddonLibrary.Load();
            RefreshListView();
            await RunUpdateCheckAsync();
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
            var item = new ListViewItem(addon.Name) { Tag = addon };
            item.SubItems.Add(addon.InstalledVersion);
            item.SubItems.Add(addon.LatestVersion ?? "");
            item.SubItems.Add(StatusText(addon));
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

        private void OpenFolderButton_Click(object? sender, EventArgs e)
        {
            var target = SelectedTarget;
            AddonPaths.EnsureFoldersExist(target);
            System.Diagnostics.Process.Start("explorer.exe", AddonPaths.GetAddOnsFolder(target));
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
            targetListBox.Enabled = !busy;
            if (status is not null)
            {
                SetStatus(status);
            }
        }

        private void SetStatus(string text) => statusLabel.Text = text;
    }
}
