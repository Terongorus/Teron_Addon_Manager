using Teron_Addon_Manager.Models;

namespace Teron_Addon_Manager
{
    public partial class ScanResultsDialog : Form
    {
        private readonly List<ScannedAddonCandidate> _matched;

        public List<ScannedAddonCandidate> AdoptedCandidates { get; private set; } = new();

        public ScanResultsDialog(List<ScannedAddonCandidate> candidates)
        {
            InitializeComponent();

            _matched = candidates.Where(c => c.CatalogEntry is not null).ToList();
            var unmatched = candidates.Where(c => c.CatalogEntry is null).ToList();

            foreach (var candidate in _matched)
            {
                var entry = candidate.CatalogEntry!;
                matchedCheckedListBox.Items.Add($"{string.Join(", ", candidate.FolderNames)}  ->  {entry.Title} ({entry.Author})", true);
            }

            foreach (var candidate in unmatched)
            {
                unmatchedListBox.Items.Add($"{string.Join(", ", candidate.FolderNames)}  ->  no ESOUI match found");
            }

            matchedGroupBox.Text = $"Found matches ({_matched.Count}) - will be tracked for updates";
            unmatchedGroupBox.Text = $"Found but not matched to ESOUI ({unmatched.Count})";

            adoptButton.Click += AdoptButton_Click;
        }

        private void AdoptButton_Click(object? sender, EventArgs e)
        {
            AdoptedCandidates = _matched.Where((_, index) => matchedCheckedListBox.GetItemChecked(index)).ToList();
            DialogResult = DialogResult.OK;
        }
    }
}
