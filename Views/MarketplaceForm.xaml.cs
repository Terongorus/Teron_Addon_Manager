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
    public partial class MarketplaceForm : Window
    {
        private readonly HttpClient _http;
        private readonly UiSettings _uiSettings;
        private readonly Func<EsoUiCatalogEntry, bool> _isInstalled;
        private readonly Func<EsoUiCatalogEntry, Task<(bool Success, string Message)>> _installAsync;
        private readonly Func<EsoUiCatalogEntry, (bool Success, string Message)> _uninstall;
        private readonly EsoUiCatalogService _catalogService = new();
        private List<EsoUiCatalogEntry> _allEntries = new();
        private Dictionary<long, string> _categoryTitles = new();

        public MarketplaceForm(
            HttpClient http,
            UiSettings uiSettings,
            Func<EsoUiCatalogEntry, bool> isInstalled,
            Func<EsoUiCatalogEntry, Task<(bool Success, string Message)>> installAsync,
            Func<EsoUiCatalogEntry, (bool Success, string Message)> uninstall)
        {
            InitializeComponent();
            _http = http;
            _uiSettings = uiSettings;
            _isInstalled = isInstalled;
            _installAsync = installAsync;
            _uninstall = uninstall;

            WindowPlacementHelper.Apply(this, _uiSettings.MarketplaceWindow);
            Closing += MarketplaceForm_Closing;
            StateChanged += (_, _) => UpdateToolbarWidths();
            UpdateToolbarWidths();

            sortComboBox.Items.Add("Name (A-Z)");
            sortComboBox.Items.Add("Name (Z-A)");
            sortComboBox.Items.Add("Last Updated (Newest)");
            sortComboBox.Items.Add("Most Downloaded");
            sortComboBox.SelectedIndex = 0;

            searchTextBox.TextChanged += (_, _) => ApplyFilters();
            searchTextBox.KeyDown += SearchTextBox_KeyDown;
            categoryComboBox.SelectionChanged += (_, _) => ApplyFilters();
            sortComboBox.SelectionChanged += (_, _) => ApplyFilters();
            refreshButton.Click += async (_, _) => await LoadCatalogAsync(forceRefresh: true);
            viewDetailsContextItem.Click += ViewDetailsContextItem_Click;
            resultsListView.PreviewMouseRightButtonDown += ResultsListView_PreviewMouseRightButtonDown;
            resultsListView.PreviewMouseLeftButtonDown += ResultsListView_PreviewMouseLeftButtonDown;
            resultsListView.KeyDown += ResultsListView_KeyDown;
            GridViewAutoFit.Attach(this, resultsListView);
            Loaded += async (_, _) => await LoadCatalogAsync(forceRefresh: false);
        }

        private void MarketplaceForm_Closing(object? sender, CancelEventArgs e)
        {
            _uiSettings.MarketplaceWindow = WindowPlacementHelper.Capture(this);
            UiSettingsStore.Save(_uiSettings);
        }

        private void UpdateToolbarWidths()
        {
            var maximized = WindowState == WindowState.Maximized;
            searchTextBox.Width = maximized ? 320 : 180;
            categoryComboBox.Width = maximized ? 280 : 160;
            sortComboBox.Width = maximized ? 240 : 140;
        }

        private void SearchTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                searchTextBox.Clear();
                e.Handled = true;
            }
        }

        private void ResultsListView_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control && e.Key == System.Windows.Input.Key.A)
            {
                resultsListView.SelectAll();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Escape)
            {
                resultsListView.SelectedItems.Clear();
                e.Handled = true;
            }
        }

        // Right-clicking an unselected row should select just that row, matching the original
        // WinForms behavior, instead of leaving whatever was previously selected untouched.
        private void ResultsListView_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = FindAncestor<System.Windows.Controls.ListViewItem>(e.OriginalSource as DependencyObject);
            if (item is null || item.IsSelected)
            {
                return;
            }
            resultsListView.SelectedItems.Clear();
            item.IsSelected = true;
        }

        // WPF's default Selector behavior has no way to get back to "nothing selected" once a row is
        // selected: clicking empty space leaves the old selection alone, and re-clicking the same row is a
        // no-op. Both are handled explicitly here, matching AddonManager's addonListView.
        private void ResultsListView_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = FindAncestor<System.Windows.Controls.ListViewItem>(e.OriginalSource as DependencyObject);
            if (item is null)
            {
                resultsListView.SelectedItems.Clear();
                return;
            }

            if (item.IsSelected && System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.None)
            {
                item.IsSelected = false;
                e.Handled = true;
            }
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
            var entry = resultsListView.SelectedItems.Cast<MarketplaceRow>().FirstOrDefault()?.Entry;
            if (entry is null)
            {
                return;
            }

            var fields = new List<(string Label, string Value)>
            {
                ("Author", entry.Author),
                ("Category", _categoryTitles.TryGetValue(entry.CategoryId, out var title) ? title : "(unknown)"),
                ("Version", entry.Version),
                ("Downloads", $"{entry.Downloads:N0} ({entry.DownloadsMonthly:N0} this month)"),
                ("Favorites", entry.Favorites.ToString("N0")),
                ("Last Updated", entry.LastUpdate is { } lastUpdate ? lastUpdate.ToLocalTime().ToString("yyyy-MM-dd") : "(unknown)"),
                ("Library", entry.IsLibrary ? "Yes" : "No"),
                ("Installs folders", entry.FolderPaths.Count > 0 ? string.Join(", ", entry.FolderPaths) : "(unknown)"),
                ("Page", entry.FileInfoUri)
            };

            string? description = null;
            SetBusy(true, "Loading addon description...");
            try
            {
                description = await EsoUiAddonSource.FetchDescriptionAsync(entry.Id.ToString(), _http, CancellationToken.None).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                statusText.Text = $"Could not load addon description: {ex.Message}";
            }
            finally
            {
                SetBusy(false);
            }

            var dialog = new AddonDetailsDialog(entry.Title, "ESOUI marketplace listing", fields, description) { Owner = this, ThemeMode = ThemeMode };
            dialog.ShowDialog();
        }

        private async Task LoadCatalogAsync(bool forceRefresh)
        {
            SetBusy(true, "Loading ESOUI catalog...");
            try
            {
                _allEntries = await _catalogService.GetCatalogAsync(_http, CancellationToken.None, forceRefresh).ConfigureAwait(true);
                var categories = await _catalogService.GetCategoriesAsync(_http, CancellationToken.None, forceRefresh).ConfigureAwait(true);
                _categoryTitles = categories.ToDictionary(c => c.Id, c => c.Title);

                var previousCategory = categoryComboBox.SelectedItem as EsoUiCategory;
                categoryComboBox.Items.Clear();
                categoryComboBox.Items.Add("All Categories");
                foreach (var category in categories.OrderBy(c => c.Title, StringComparer.OrdinalIgnoreCase))
                {
                    categoryComboBox.Items.Add(category);
                }
                categoryComboBox.SelectedIndex = previousCategory is null
                    ? 0
                    : Math.Max(0, categoryComboBox.Items.IndexOf(previousCategory));

                ApplyFilters();
            }
            catch (Exception ex)
            {
                statusText.Text = $"Failed to load catalog: {ex.Message}";
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void ApplyFilters()
        {
            IEnumerable<EsoUiCatalogEntry> query = _allEntries;

            if (categoryComboBox.SelectedItem is EsoUiCategory category)
            {
                query = query.Where(e => e.CategoryId == category.Id);
            }

            var search = searchTextBox.Text.Trim();
            if (search.Length > 0)
            {
                query = query.Where(e =>
                    e.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    e.Author.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            query = sortComboBox.SelectedIndex switch
            {
                1 => query.OrderByDescending(e => e.Title, StringComparer.OrdinalIgnoreCase),
                2 => query.OrderByDescending(e => e.LastUpdate ?? DateTimeOffset.MinValue),
                3 => query.OrderByDescending(e => e.Downloads),
                _ => query.OrderBy(e => e.Title, StringComparer.OrdinalIgnoreCase)
            };

            var rows = query.Select(entry => new MarketplaceRow
            {
                Entry = entry,
                Title = entry.Title,
                Author = entry.Author,
                CategoryTitle = _categoryTitles.TryGetValue(entry.CategoryId, out var title) ? title : "",
                DownloadsText = entry.Downloads.ToString("N0"),
                LastUpdatedText = entry.LastUpdate?.ToString("yyyy-MM-dd") ?? "",
                IsInstalled = _isInstalled(entry)
            }).ToList();

            resultsListView.ItemsSource = rows;
            statusText.Text = $"Showing {rows.Count} of {_allEntries.Count} addons.";
        }

        private async void InstallRowButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var row = (MarketplaceRow)button.DataContext;

            if (row.IsInstalled)
            {
                var confirmed = FluentMessageBox.Show(this, $"Remove {row.Title} and delete its files from the AddOns folder?",
                    "Confirm Remove", FluentMessageBoxButtons.YesNo, FluentMessageBoxIcon.Warning);
                if (!confirmed)
                {
                    return;
                }

                button.IsEnabled = false;
                try
                {
                    var (success, message) = _uninstall(row.Entry);
                    statusText.Text = message;
                    if (success)
                    {
                        row.IsInstalled = false;
                    }
                    else
                    {
                        FluentMessageBox.Show(this, message, "Remove failed", icon: FluentMessageBoxIcon.Error);
                    }
                }
                finally
                {
                    button.IsEnabled = true;
                }
                return;
            }

            button.IsEnabled = false;
            try
            {
                var (success, message) = await _installAsync(row.Entry);
                statusText.Text = message;
                if (success)
                {
                    row.IsInstalled = true;
                }
                else
                {
                    FluentMessageBox.Show(this, message, "Install failed", icon: FluentMessageBoxIcon.Error);
                }
            }
            finally
            {
                button.IsEnabled = true;
            }
        }

        private void SetBusy(bool busy, string? status = null)
        {
            Cursor = busy ? Cursors.Wait : Cursors.Arrow;
            refreshButton.IsEnabled = !busy;
            if (status is not null)
            {
                statusText.Text = status;
            }
        }

        private sealed class MarketplaceRow : INotifyPropertyChanged
        {
            public required EsoUiCatalogEntry Entry { get; init; }
            public required string Title { get; init; }
            public required string Author { get; init; }
            public required string CategoryTitle { get; init; }
            public required string DownloadsText { get; init; }
            public required string LastUpdatedText { get; init; }

            private bool _isInstalled;
            public bool IsInstalled
            {
                get => _isInstalled;
                set
                {
                    if (_isInstalled == value)
                    {
                        return;
                    }
                    _isInstalled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsInstalled)));
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;
        }
    }
}
