using System.Windows;
using Teron_Addon_Manager.Models;

namespace Teron_Addon_Manager
{
    public partial class ScanResultsDialog : Window
    {
        private readonly List<CandidateRow> _matchedRows;

        public List<ScannedAddonCandidate> AdoptedCandidates { get; private set; } = new();

        public ScanResultsDialog(List<ScannedAddonCandidate> candidates)
        {
            InitializeComponent();

            _matchedRows = candidates
                .Where(c => c.CatalogEntry is not null)
                .Select(c => new CandidateRow
                {
                    Candidate = c,
                    DisplayText = $"{string.Join(", ", c.FolderNames)}  ->  {c.CatalogEntry!.Title} ({c.CatalogEntry.Author})"
                })
                .ToList();
            var unmatched = candidates.Where(c => c.CatalogEntry is null).ToList();

            matchedListBox.ItemsSource = _matchedRows;
            foreach (var candidate in unmatched)
            {
                unmatchedListBox.Items.Add($"{string.Join(", ", candidate.FolderNames)}  ->  no ESOUI match found");
            }

            matchedGroupBox.Header = $"Found matches ({_matchedRows.Count}) - will be tracked for updates";
            unmatchedGroupBox.Header = $"Found but not matched to ESOUI ({unmatched.Count})";

            adoptButton.Click += AdoptButton_Click;
        }

        private void AdoptButton_Click(object sender, RoutedEventArgs e)
        {
            AdoptedCandidates = _matchedRows.Where(r => r.IsChecked).Select(r => r.Candidate).ToList();
            DialogResult = true;
        }

        private sealed class CandidateRow
        {
            public required ScannedAddonCandidate Candidate { get; init; }
            public required string DisplayText { get; init; }
            public bool IsChecked { get; set; } = true;
        }
    }
}
