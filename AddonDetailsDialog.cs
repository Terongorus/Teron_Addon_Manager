namespace Teron_Addon_Manager
{
    public partial class AddonDetailsDialog : Form
    {
        public AddonDetailsDialog(string title, string? subtitle, IEnumerable<(string Label, string Value)> fields)
        {
            InitializeComponent();

            Text = title;
            headerLabel.Text = title;
            subtitleLabel.Text = subtitle ?? "";
            subtitleLabel.Visible = !string.IsNullOrWhiteSpace(subtitle);

            BuildFieldRows(fields);
        }

        private void BuildFieldRows(IEnumerable<(string Label, string Value)> fields)
        {
            var fieldList = fields.ToList();
            var table = new TableLayoutPanel
            {
                ColumnCount = 2,
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(12, 8, 12, 8),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            for (var i = 0; i < fieldList.Count; i++)
            {
                var (label, value) = fieldList[i];
                table.RowCount++;
                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                var labelControl = new Label
                {
                    Text = label,
                    Font = new Font(Font, FontStyle.Bold),
                    AutoSize = true,
                    Margin = new Padding(0, 4, 8, 4),
                    Anchor = AnchorStyles.Left | AnchorStyles.Top
                };
                table.Controls.Add(labelControl, 0, i);

                Control valueControl = value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                                        value.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                    ? CreateLinkValue(value)
                    : new Label { Text = value, AutoSize = true, Margin = new Padding(0, 4, 0, 4) };

                valueControl.MaximumSize = new Size(300, 0);
                valueControl.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                table.Controls.Add(valueControl, 1, i);
            }

            contentPanel.Controls.Add(table);
        }

        private static LinkLabel CreateLinkValue(string url)
        {
            var link = new LinkLabel { Text = url, AutoSize = true, Margin = new Padding(0, 4, 0, 4) };
            link.LinkClicked += (_, _) =>
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
            return link;
        }
    }
}
