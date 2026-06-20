using Teron_Addon_Manager.Models;
using Teron_Addon_Manager.Services;
using Teron_Addon_Manager.Sources;

namespace Teron_Addon_Manager
{
    public partial class MarketplaceForm : Form
    {
        private readonly HttpClient _http;
        private readonly EsoUiCatalogService _catalogService = new();
        private List<EsoUiCatalogEntry> _allEntries = new();
        private Dictionary<long, string> _categoryTitles = new();

        public List<EsoUiCatalogEntry> SelectedAddons { get; private set; } = new();

        public MarketplaceForm(HttpClient http)
        {
            InitializeComponent();
            _http = http;

            sortComboBox.Items.AddRange(new object[] { "Name (A-Z)", "Last Updated (Newest)", "Most Downloaded" });
            sortComboBox.SelectedIndex = 0;

            searchTextBox.TextChanged += (_, _) => ApplyFilters();
            categoryComboBox.SelectedIndexChanged += (_, _) => ApplyFilters();
            sortComboBox.SelectedIndexChanged += (_, _) => ApplyFilters();
            refreshMenuItem.Click += async (_, _) => await LoadCatalogAsync(forceRefresh: true);
            installButton.Click += InstallButton_Click;
            installContextItem.Click += InstallButton_Click;
            viewDetailsContextItem.Click += ViewDetailsContextItem_Click;
            resultsListView.MouseDown += ResultsListView_MouseDown;
            resultsListView.KeyDown += ResultsListView_KeyDown;
            Load += async (_, _) => await LoadCatalogAsync(forceRefresh: false);
        }

        private void ResultsListView_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                foreach (ListViewItem item in resultsListView.Items)
                {
                    item.Selected = true;
                }
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void ResultsListView_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            var hit = resultsListView.HitTest(e.Location);
            if (hit.Item is null || hit.Item.Selected)
            {
                return;
            }

            foreach (ListViewItem selected in resultsListView.SelectedItems.Cast<ListViewItem>().ToList())
            {
                selected.Selected = false;
            }
            hit.Item.Selected = true;
            hit.Item.Focused = true;
        }

        private async void ViewDetailsContextItem_Click(object? sender, EventArgs e)
        {
            var entry = resultsListView.SelectedItems.Cast<ListViewItem>().Select(i => (EsoUiCatalogEntry)i.Tag!).FirstOrDefault();
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
                statusLabel.Text = $"Could not load addon description: {ex.Message}";
            }
            finally
            {
                SetBusy(false);
            }

            using var dialog = new AddonDetailsDialog(entry.Title, "ESOUI marketplace listing", fields, description);
            dialog.ShowDialog(this);
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
                statusLabel.Text = $"Failed to load catalog: {ex.Message}";
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
                1 => query.OrderByDescending(e => e.LastUpdate ?? DateTimeOffset.MinValue),
                2 => query.OrderByDescending(e => e.Downloads),
                _ => query.OrderBy(e => e.Title, StringComparer.OrdinalIgnoreCase)
            };

            resultsListView.BeginUpdate();
            resultsListView.Items.Clear();
            foreach (var entry in query)
            {
                var item = new ListViewItem(entry.Title) { Tag = entry };
                item.SubItems.Add(entry.Author);
                item.SubItems.Add(_categoryTitles.TryGetValue(entry.CategoryId, out var title) ? title : "");
                item.SubItems.Add(entry.Downloads.ToString("N0"));
                item.SubItems.Add(entry.LastUpdate?.ToString("yyyy-MM-dd") ?? "");
                resultsListView.Items.Add(item);
            }
            resultsListView.EndUpdate();

            statusLabel.Text = $"Showing {resultsListView.Items.Count} of {_allEntries.Count} addons.";
        }

        private void InstallButton_Click(object? sender, EventArgs e)
        {
            var selected = resultsListView.SelectedItems.Cast<ListViewItem>().Select(i => (EsoUiCatalogEntry)i.Tag!).ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show(this, "Select one or more addons to install.", "Install", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SelectedAddons = selected;
            DialogResult = DialogResult.OK;
        }

        private void SetBusy(bool busy, string? status = null)
        {
            UseWaitCursor = busy;
            installButton.Enabled = !busy;
            installContextItem.Enabled = !busy;
            refreshMenuItem.Enabled = !busy;
            if (status is not null)
            {
                statusLabel.Text = status;
            }
        }
    }
}
