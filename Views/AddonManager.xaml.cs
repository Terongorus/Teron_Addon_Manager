using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TeronAddonManager.Helpers;
using TeronAddonManager.Models;
using TeronAddonManager.Services;
using TeronAddonManager.Sources;
using Cursors = System.Windows.Input.Cursors;

namespace TeronAddonManager
{
    public partial class AddonManager : Window
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

        // Null direction means "default order" (no sort applied, original enumeration order).
        private GridViewColumn? _sortColumn;
        private ListSortDirection? _sortDirection;

        private bool _busy;

        private readonly UiSettings _uiSettings = UiSettingsStore.Load();

        public AddonManager()
        {
            InitializeComponent();
            Title = AppInfo.DisplayName;

            WindowPlacementHelper.Apply(this, _uiSettings.AddonManagerWindow);
            Closing += AddonManager_Closing;
            StateChanged += (_, _) => UpdateSearchBoxWidth();
            UpdateSearchBoxWidth();

            _http.DefaultRequestHeaders.UserAgent.ParseAdd("TeronAddonManager/1.0");
            _installer = new AddonInstaller(_http, _resolver);
            _updateChecker = new UpdateChecker(_http, _resolver);

            // Fluent's backdrop only fully activates once ThemeMode has been assigned at least once at
            // runtime — a window that has never gone through ThemeRadio_Checked renders measurably (if
            // subtly) different from one that has. Wiring Checked before the initial IsChecked assignment
            // makes this the first real "switch," normalizing first launch to match every later one.
            themeSystemRadio.Checked += ThemeRadio_Checked;
            themeLightRadio.Checked += ThemeRadio_Checked;
            themeDarkRadio.Checked += ThemeRadio_Checked;
            var initialThemeRadio = _uiSettings.ThemeMode switch
            {
                "Light" => themeLightRadio,
                "Dark" => themeDarkRadio,
                _ => themeSystemRadio
            };
            initialThemeRadio.IsChecked = true;

            searchTextBox.TextChanged += (_, _) => RefreshListView();
            searchTextBox.KeyDown += SearchTextBox_KeyDown;
            addAddonButton.Click += AddAddonButton_Click;
            checkUpdatesButton.Click += CheckUpdatesButton_Click;
            updateSelectedButton.Click += UpdateSelectedButton_Click;
            removeSelectedButton.Click += RemoveSelectedButton_Click;
            scanLocalButton.Click += ScanLocalButton_Click;
            browseMarketplaceButton.Click += BrowseMarketplaceButton_Click;
            openFolderButton.Click += OpenFolderButton_Click;
            Loaded += AddonManager_Loaded;

            addonListView.PreviewMouseRightButtonDown += AddonListView_PreviewMouseRightButtonDown;
            addonListView.PreviewMouseLeftButtonDown += AddonListView_PreviewMouseLeftButtonDown;
            addonListView.KeyDown += AddonListView_KeyDown;
            addonListView.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(AddonListView_ColumnHeaderClick));
            addonListView.SelectionChanged += AddonListView_SelectionChanged;
            GridViewAutoFit.Attach(this, addonListView);
            viewDetailsContextItem.Click += ViewDetailsContextItem_Click;
            checkSelectedContextItem.Click += CheckSelectedContextItem_Click;
            updateSelectedContextItem.Click += UpdateSelectedButton_Click;
            removeSelectedContextItem.Click += RemoveSelectedButton_Click;
        }

        private void ThemeRadio_Checked(object sender, RoutedEventArgs e)
        {
            var mode = ThemeMode.System;
            if (sender == themeLightRadio) mode = ThemeMode.Light;
            else if (sender == themeDarkRadio) mode = ThemeMode.Dark;

            // Window-level ThemeMode overrides the Application-level one, so the currently open main
            // window needs its own ThemeMode set directly to actually re-theme live; setting only
            // Application.Current.ThemeMode is still needed so dialogs opened afterward (which have no
            // explicit ThemeMode of their own) pick up the same choice.
            System.Windows.Application.Current.ThemeMode = mode;
            ThemeMode = mode;

            UpdateThemeRadioForeground();

            // The Status column's badge colors depend on the resolved theme; re-running the converter
            // is the only way to pick that up since the underlying Status strings haven't changed.
            RefreshListView();
        }

        // The unselected segments' Foreground can't be left to a Binding driven only by IsChecked: that
        // binding is only re-evaluated when IsChecked itself changes, not when ThemeMode changes a moment
        // later, so a converter reading the *current* theme at that point races the actual theme update
        // and goes stale (confirmed: it broke for whichever segment had just been deselected). Setting it
        // directly here, once per switch, evaluates against the theme that's actually active by then.
        private void UpdateThemeRadioForeground()
        {
            var uncheckedBrush = ThemeHelper.IsDarkActive() ? Brushes.White : Brushes.Black;
            foreach (var radio in new[] { themeSystemRadio, themeLightRadio, themeDarkRadio })
            {
                radio.Foreground = radio.IsChecked == true ? Brushes.White : uncheckedBrush;
            }
        }

        private void AddonListView_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control && e.Key == System.Windows.Input.Key.A)
            {
                addonListView.SelectAll();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Escape)
            {
                addonListView.SelectedItems.Clear();
                e.Handled = true;
            }
        }

        // WPF's default Selector behavior has no way to get back to "nothing selected" once a row is
        // selected: clicking empty space leaves the old selection alone, and re-clicking the same row is a
        // no-op. Both are handled explicitly here so Escape, clicking elsewhere, and re-clicking the
        // selected row all deselect, matching what users expect from a Windows list.
        private void AddonListView_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = FindAncestor<System.Windows.Controls.ListViewItem>(e.OriginalSource as DependencyObject);
            if (item is null)
            {
                addonListView.SelectedItems.Clear();
                return;
            }

            if (item.IsSelected && System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.None)
            {
                item.IsSelected = false;
                e.Handled = true;
            }
        }

        // Cycles a clicked column header through ascending -> descending -> default (no sort) order;
        // clicking a different column always restarts that cycle at ascending.
        private void AddonListView_ColumnHeaderClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is not GridViewColumnHeader { Column: { } column })
            {
                return;
            }

            if (!ReferenceEquals(_sortColumn, column))
            {
                _sortColumn = column;
                _sortDirection = ListSortDirection.Ascending;
            }
            else
            {
                _sortDirection = _sortDirection switch
                {
                    ListSortDirection.Ascending => ListSortDirection.Descending,
                    ListSortDirection.Descending => null,
                    _ => ListSortDirection.Ascending
                };
                if (_sortDirection is null)
                {
                    _sortColumn = null;
                }
            }

            UpdateSortArrows();
            RefreshListView();
        }

        private void UpdateSortArrows()
        {
            SetSortArrow(nameSortArrow, nameColumn);
            SetSortArrow(installedSortArrow, installedColumn);
            SetSortArrow(latestSortArrow, latestColumn);
            SetSortArrow(statusSortArrow, statusColumn);
            SetSortArrow(sourceSortArrow, sourceColumn);
        }

        private void SetSortArrow(TextBlock arrow, GridViewColumn column)
        {
            if (!ReferenceEquals(_sortColumn, column) || _sortDirection is null)
            {
                arrow.Text = "";
                return;
            }
            arrow.Text = _sortDirection == ListSortDirection.Ascending ? "" : "";
        }

        private GridViewColumn? GetColumnByName(string? name) => name switch
        {
            "Name" => nameColumn,
            "Installed" => installedColumn,
            "Latest" => latestColumn,
            "Status" => statusColumn,
            "Source" => sourceColumn,
            _ => null
        };

        private string? GetColumnName(GridViewColumn? column)
        {
            if (ReferenceEquals(column, nameColumn)) return "Name";
            if (ReferenceEquals(column, installedColumn)) return "Installed";
            if (ReferenceEquals(column, latestColumn)) return "Latest";
            if (ReferenceEquals(column, statusColumn)) return "Status";
            if (ReferenceEquals(column, sourceColumn)) return "Source";
            return null;
        }

        private IEnumerable<AddonRow> ApplySort(IEnumerable<AddonRow> rows)
        {
            if (_sortColumn is null || _sortDirection is null)
            {
                return rows;
            }

            Func<AddonRow, string> keySelector = true switch
            {
                _ when ReferenceEquals(_sortColumn, nameColumn) => r => r.Name,
                _ when ReferenceEquals(_sortColumn, installedColumn) => r => r.Installed,
                _ when ReferenceEquals(_sortColumn, latestColumn) => r => r.Latest,
                _ when ReferenceEquals(_sortColumn, statusColumn) => r => r.Status,
                _ when ReferenceEquals(_sortColumn, sourceColumn) => r => r.Source,
                _ => r => r.Name
            };

            return _sortDirection == ListSortDirection.Descending
                ? rows.OrderByDescending(keySelector, NaturalStringComparer.Instance)
                : rows.OrderBy(keySelector, NaturalStringComparer.Instance);
        }

        // Compares version-like strings (e.g. "2.21.0" vs "88") by numeric runs instead of lexically, so
        // the Installed/Latest columns sort the way a user would expect instead of "10" landing before "2".
        private sealed class NaturalStringComparer : IComparer<string>
        {
            public static readonly NaturalStringComparer Instance = new();

            public int Compare(string? a, string? b)
            {
                a ??= "";
                b ??= "";
                int i = 0, j = 0;
                while (i < a.Length && j < b.Length)
                {
                    if (char.IsDigit(a[i]) && char.IsDigit(b[j]))
                    {
                        int startI = i, startJ = j;
                        while (i < a.Length && char.IsDigit(a[i])) i++;
                        while (j < b.Length && char.IsDigit(b[j])) j++;
                        var numA = a[startI..i].TrimStart('0');
                        var numB = b[startJ..j].TrimStart('0');
                        if (numA.Length != numB.Length)
                        {
                            return numA.Length - numB.Length;
                        }
                        var cmp = string.CompareOrdinal(numA, numB);
                        if (cmp != 0)
                        {
                            return cmp;
                        }
                    }
                    else
                    {
                        var cmp = char.ToUpperInvariant(a[i]).CompareTo(char.ToUpperInvariant(b[j]));
                        if (cmp != 0)
                        {
                            return cmp;
                        }
                        i++;
                        j++;
                    }
                }
                return (a.Length - i) - (b.Length - j);
            }
        }

        // Right-clicking an unselected row should select just that row, matching the original
        // WinForms behavior, instead of leaving whatever was previously selected untouched.
        private void AddonListView_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = FindAncestor<System.Windows.Controls.ListViewItem>(e.OriginalSource as DependencyObject);
            if (item is null || item.IsSelected)
            {
                return;
            }
            addonListView.SelectedItems.Clear();
            item.IsSelected = true;
        }

        private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
        {
            while (current is not null)
            {
                if (current is T match)
                {
                    return match;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        private async void ViewDetailsContextItem_Click(object sender, RoutedEventArgs e)
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

            var dialog = new AddonDetailsDialog(addon.Name, $"{addon.SourceKind} addon", fields, description) { Owner = this, ThemeMode = ThemeMode };
            dialog.ShowDialog();
        }

        private async void CheckSelectedContextItem_Click(object sender, RoutedEventArgs e)
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

        private void AddonManager_Closing(object? sender, CancelEventArgs e)
        {
            _uiSettings.ThemeMode = ThemeMode.ToString();
            _uiSettings.AddonSortColumn = GetColumnName(_sortColumn);
            _uiSettings.AddonSortDirection = _sortDirection?.ToString();
            _uiSettings.SelectedGameTarget = _availableTargets.Count > 0 ? SelectedTarget : null;
            _uiSettings.AddonManagerWindow = WindowPlacementHelper.Capture(this);
            UiSettingsStore.Save(_uiSettings);
        }

        private static string DisplayName(GameTarget target) => target == GameTarget.Ptr ? "ESO PTR" : "ESO Live";

        private GameTarget SelectedTarget =>
            targetListBox.SelectedIndex >= 0 && targetListBox.SelectedIndex < _availableTargets.Count
                ? _availableTargets[targetListBox.SelectedIndex]
                : GameTarget.Live;

        private async void AddonManager_Loaded(object sender, RoutedEventArgs e)
        {
            _availableTargets = AddonPaths.DetectInstalledTargets().ToList();
            var rows = _availableTargets.Select(t => new GameTargetRow { DisplayName = DisplayName(t) }).ToList();
            targetListBox.ItemsSource = rows;
            targetListBox.SelectionChanged += TargetListBox_SelectionChanged;
            if (rows.Count > 0)
            {
                var savedTargetIndex = _uiSettings.SelectedGameTarget is { } savedTarget
                    ? _availableTargets.IndexOf(savedTarget)
                    : -1;
                targetListBox.SelectedIndex = savedTargetIndex >= 0 ? savedTargetIndex : 0;
            }

            _addons = AddonLibrary.Load();

            _sortColumn = GetColumnByName(_uiSettings.AddonSortColumn);
            _sortDirection = _sortColumn is not null && Enum.TryParse<ListSortDirection>(_uiSettings.AddonSortDirection, out var savedDirection)
                ? savedDirection
                : null;
            UpdateSortArrows();

            RefreshListView();

            await RunUpdateCheckAsync();
            await ScanForLocalAddonsAsync(isManualTrigger: false);
        }

        // The highlight on the selected row is driven by GameTargetRow.IsCurrent (a plain bound property)
        // instead of the ListBox's native IsSelected, because a runtime ThemeMode switch (still
        // experimental, WPF0001) permanently breaks IsSelected-triggered visuals for Selector controls —
        // confirmed independent of brush/binding/resource choice and even of recreating the control
        // entirely. A regular data binding isn't affected, so selection state is mirrored onto the data
        // objects by hand here instead of relying on WPF to re-evaluate the trigger.
        private void TargetListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var row in e.RemovedItems.Cast<GameTargetRow>())
            {
                row.IsCurrent = false;
            }
            foreach (var row in e.AddedItems.Cast<GameTargetRow>())
            {
                row.IsCurrent = true;
            }
            RefreshListView();
        }

        private sealed class GameTargetRow : INotifyPropertyChanged
        {
            public required string DisplayName { get; init; }

            private bool _isCurrent;
            public bool IsCurrent
            {
                get => _isCurrent;
                set
                {
                    _isCurrent = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCurrent)));
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;
        }

        private IEnumerable<InstalledAddon> AddonsForSelectedTarget() =>
            _addons.Where(a => a.Target == SelectedTarget);

        private void RefreshListView()
        {
            var rows = AddonsForSelectedTarget().Select(CreateRow);
            rows = ApplySearch(rows);
            addonListView.ItemsSource = ApplySort(rows).ToList();
        }

        private IEnumerable<AddonRow> ApplySearch(IEnumerable<AddonRow> rows)
        {
            var search = searchTextBox.Text.Trim();
            return search.Length == 0
                ? rows
                : rows.Where(r => r.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        private void SearchTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                searchTextBox.Clear();
                e.Handled = true;
            }
        }

        private void UpdateSearchBoxWidth()
        {
            searchTextBox.Width = WindowState == WindowState.Maximized ? 420 : 240;
        }

        private static AddonRow CreateRow(InstalledAddon addon) => new()
        {
            Addon = addon,
            Name = addon.Name,
            Installed = addon.InstalledVersion,
            Latest = addon.LatestVersion ?? "",
            Status = StatusText(addon),
            Source = addon.SourceKind.ToString()
        };

        private static string StatusText(InstalledAddon addon)
        {
            if (addon.LastCheckError is not null) return "Check failed";
            if (addon.UpdateAvailable == true) return "Update available";
            if (addon.UpdateAvailable == false) return "Up to date";
            return "Unknown";
        }

        private async void AddAddonButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddAddonDialog { Owner = this, ThemeMode = ThemeMode };
            if (dialog.ShowDialog() != true || dialog.ResultUrl is null)
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
                FluentMessageBox.Show(this, ex.Message, "Could not add addon", icon: FluentMessageBoxIcon.Error);
                SetStatus("Add addon failed.");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void CheckUpdatesButton_Click(object sender, RoutedEventArgs e) => await RunUpdateCheckAsync();

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

        private async void UpdateSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = SelectedAddons().ToList();
            if (selected.Count == 0)
            {
                FluentMessageBox.Show(this, "Select one or more addons to update.", "Update Selected");
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
                        FluentMessageBox.Show(this, $"Failed to update {addon.Name}: {ex.Message}", "Update failed", icon: FluentMessageBoxIcon.Error);
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

        private void RemoveSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = SelectedAddons().ToList();
            if (selected.Count == 0)
            {
                FluentMessageBox.Show(this, "Select one or more addons to remove.", "Remove Selected");
                return;
            }

            var names = string.Join(", ", selected.Select(a => a.Name));
            var confirmed = FluentMessageBox.Show(this, $"Remove {names} and delete their files from the AddOns folder?",
                "Confirm Remove", FluentMessageBoxButtons.YesNo, FluentMessageBoxIcon.Warning);
            if (!confirmed)
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
                    FluentMessageBox.Show(this, $"Failed to remove {addon.Name}: {ex.Message}", "Remove failed", icon: FluentMessageBoxIcon.Error);
                }
            }

            AddonLibrary.Save(_addons);
            RefreshListView();
            if (removed.Count > 0)
            {
                SetStatus($"Removed {string.Join(", ", removed)}.");
            }
        }

        private async void ScanLocalButton_Click(object sender, RoutedEventArgs e) => await ScanForLocalAddonsAsync(isManualTrigger: true);

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
                        FluentMessageBox.Show(this, "No unmanaged addon folders were found.", "Scan for Local Addons");
                    }
                    return;
                }

                var dialog = new ScanResultsDialog(candidates) { Owner = this, ThemeMode = ThemeMode };
                if (dialog.ShowDialog() != true || dialog.AdoptedCandidates.Count == 0)
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
                    FluentMessageBox.Show(this, $"Skipped (already tracked):{Environment.NewLine}{string.Join(Environment.NewLine, skipped)}",
                        "Scan for Local Addons", icon: FluentMessageBoxIcon.Warning);
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

        private void BrowseMarketplaceButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MarketplaceForm(_http, _uiSettings, IsMarketplaceEntryInstalled, InstallMarketplaceEntryAsync, UninstallMarketplaceEntry)
            {
                Owner = this,
                ThemeMode = ThemeMode
            };
            dialog.ShowDialog();
            RefreshListView();
        }

        private InstalledAddon? FindInstalledAddonForEntry(EsoUiCatalogEntry entry)
        {
            var target = SelectedTarget;
            return _addons.FirstOrDefault(a =>
                a.Target == target &&
                (a.SourceUrl == entry.FileInfoUri || a.FolderNames.Intersect(entry.FolderPaths, StringComparer.OrdinalIgnoreCase).Any()));
        }

        private bool IsMarketplaceEntryInstalled(EsoUiCatalogEntry entry) => FindInstalledAddonForEntry(entry) is not null;

        private async Task<(bool Success, string Message)> InstallMarketplaceEntryAsync(EsoUiCatalogEntry entry)
        {
            var target = SelectedTarget;
            try
            {
                var addon = await _installer.InstallAsync(new Uri(entry.FileInfoUri), target, null, CancellationToken.None);

                // The install already wrote addon.FolderNames to disk, possibly overwriting a folder some
                // other tracked addon claims. Re-point tracking to the new source rather than duplicate it.
                var replacedMessage = "";
                if (TryFindFolderConflict(target, addon.FolderNames, out _))
                {
                    var replaced = _addons.Where(a => a.Target == target && a.FolderNames.Intersect(addon.FolderNames, StringComparer.OrdinalIgnoreCase).Any()).ToList();
                    foreach (var old in replaced)
                    {
                        _addons.Remove(old);
                    }
                    replacedMessage = $" (replaced previously tracked {string.Join(", ", replaced.Select(a => a.Name))})";
                }

                _addons.Add(addon);
                AddonLibrary.Save(_addons);
                RefreshListView();
                return (true, $"Installed {addon.Name} {addon.InstalledVersion}.{replacedMessage}");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to install {entry.Title}: {ex.Message}");
            }
        }

        private (bool Success, string Message) UninstallMarketplaceEntry(EsoUiCatalogEntry entry)
        {
            var addon = FindInstalledAddonForEntry(entry);
            if (addon is null)
            {
                return (false, $"{entry.Title} is not currently installed.");
            }

            try
            {
                _installer.Remove(addon);
                _addons.Remove(addon);
                AddonLibrary.Save(_addons);
                RefreshListView();
                return (true, $"Removed {addon.Name}.");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to remove {addon.Name}: {ex.Message}");
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

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", AddonPaths.GetAddOnsFolder(SelectedTarget));
        }

        private IEnumerable<InstalledAddon> SelectedAddons() =>
            addonListView.SelectedItems.Cast<AddonRow>().Select(r => r.Addon);

        private void SetBusy(bool busy, string? status = null)
        {
            _busy = busy;
            Cursor = busy ? Cursors.Wait : Cursors.Arrow;
            addAddonButton.IsEnabled = !busy;
            checkUpdatesButton.IsEnabled = !busy;
            scanLocalButton.IsEnabled = !busy;
            browseMarketplaceButton.IsEnabled = !busy;
            checkSelectedContextItem.IsEnabled = !busy;
            updateSelectedContextItem.IsEnabled = !busy;
            removeSelectedContextItem.IsEnabled = !busy;
            targetListBox.IsEnabled = !busy;
            UpdateSelectionDependentButtons();
            if (status is not null)
            {
                SetStatus(status);
            }
        }

        // Update/Remove only make sense with addons selected; keep them disabled otherwise instead of
        // letting the user click into a "select something first" message box.
        private void UpdateSelectionDependentButtons()
        {
            var enabled = !_busy && addonListView.SelectedItems.Count > 0;
            updateSelectedButton.IsEnabled = enabled;
            removeSelectedButton.IsEnabled = enabled;
        }

        private void AddonListView_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateSelectionDependentButtons();

        private void SetStatus(string text) => statusLabel.Text = text;

        private sealed class AddonRow
        {
            public required InstalledAddon Addon { get; init; }
            public required string Name { get; init; }
            public required string Installed { get; init; }
            public required string Latest { get; init; }
            public required string Status { get; init; }
            public required string Source { get; init; }
        }
    }
}
