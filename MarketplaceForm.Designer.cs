namespace Teron_Addon_Manager
{
    partial class MarketplaceForm
    {
        private System.ComponentModel.IContainer components = null!;
        private ToolStrip toolStrip;
        private ToolStripLabel searchLabel;
        private ToolStripTextBox searchTextBox;
        private ToolStripLabel categoryLabel;
        private ToolStripComboBox categoryComboBox;
        private ToolStripLabel sortLabel;
        private ToolStripComboBox sortComboBox;
        private ToolStripMenuItem refreshMenuItem;
        private ListView resultsListView;
        private ColumnHeader titleColumn;
        private ColumnHeader authorColumn;
        private ColumnHeader categoryColumn;
        private ColumnHeader downloadsColumn;
        private ColumnHeader lastUpdatedColumn;
        private FlowLayoutPanel buttonPanel;
        private Button installButton;
        private Button closeButton;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            toolStrip = new ToolStrip();
            searchLabel = new ToolStripLabel();
            searchTextBox = new ToolStripTextBox();
            categoryLabel = new ToolStripLabel();
            categoryComboBox = new ToolStripComboBox();
            sortLabel = new ToolStripLabel();
            sortComboBox = new ToolStripComboBox();
            refreshMenuItem = new ToolStripMenuItem();
            resultsListView = new ListView();
            titleColumn = new ColumnHeader();
            authorColumn = new ColumnHeader();
            categoryColumn = new ColumnHeader();
            downloadsColumn = new ColumnHeader();
            lastUpdatedColumn = new ColumnHeader();
            buttonPanel = new FlowLayoutPanel();
            installButton = new Button();
            closeButton = new Button();
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            buttonPanel.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // toolStrip
            //
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.Items.Add(searchLabel);
            toolStrip.Items.Add(searchTextBox);
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(categoryLabel);
            toolStrip.Items.Add(categoryComboBox);
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(sortLabel);
            toolStrip.Items.Add(sortComboBox);
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(refreshMenuItem);
            toolStrip.Name = "toolStrip";
            toolStrip.Size = new Size(820, 25);
            //
            // searchLabel
            //
            searchLabel.Name = "searchLabel";
            searchLabel.Text = "Search:";
            //
            // searchTextBox
            //
            searchTextBox.Name = "searchTextBox";
            searchTextBox.Size = new Size(160, 25);
            //
            // categoryLabel
            //
            categoryLabel.Name = "categoryLabel";
            categoryLabel.Text = "Category:";
            //
            // categoryComboBox
            //
            categoryComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            categoryComboBox.Name = "categoryComboBox";
            categoryComboBox.Size = new Size(170, 25);
            //
            // sortLabel
            //
            sortLabel.Name = "sortLabel";
            sortLabel.Text = "Sort by:";
            //
            // sortComboBox
            //
            sortComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            sortComboBox.Name = "sortComboBox";
            sortComboBox.Size = new Size(150, 25);
            //
            // refreshMenuItem
            //
            refreshMenuItem.Name = "refreshMenuItem";
            refreshMenuItem.Text = "Refresh Catalog";
            //
            // resultsListView
            //
            resultsListView.Columns.AddRange(new ColumnHeader[] { titleColumn, authorColumn, categoryColumn, downloadsColumn, lastUpdatedColumn });
            resultsListView.Dock = DockStyle.Fill;
            resultsListView.FullRowSelect = true;
            resultsListView.GridLines = true;
            resultsListView.MultiSelect = true;
            resultsListView.Name = "resultsListView";
            resultsListView.UseCompatibleStateImageBehavior = false;
            resultsListView.View = View.Details;
            //
            // titleColumn
            //
            titleColumn.Text = "Title";
            titleColumn.Width = 220;
            //
            // authorColumn
            //
            authorColumn.Text = "Author";
            authorColumn.Width = 140;
            //
            // categoryColumn
            //
            categoryColumn.Text = "Category";
            categoryColumn.Width = 150;
            //
            // downloadsColumn
            //
            downloadsColumn.Text = "Downloads";
            downloadsColumn.Width = 90;
            //
            // lastUpdatedColumn
            //
            lastUpdatedColumn.Text = "Last Updated";
            lastUpdatedColumn.Width = 100;
            //
            // buttonPanel
            //
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Padding = new Padding(8);
            buttonPanel.Height = 48;
            buttonPanel.Controls.Add(closeButton);
            buttonPanel.Controls.Add(installButton);
            buttonPanel.Name = "buttonPanel";
            //
            // installButton
            //
            installButton.AutoSize = true;
            installButton.Margin = new Padding(8, 4, 0, 0);
            installButton.Name = "installButton";
            installButton.Size = new Size(120, 27);
            installButton.Text = "Install Selected";
            installButton.UseVisualStyleBackColor = true;
            //
            // closeButton
            //
            closeButton.AutoSize = true;
            closeButton.DialogResult = DialogResult.Cancel;
            closeButton.Margin = new Padding(8, 4, 0, 0);
            closeButton.Name = "closeButton";
            closeButton.Size = new Size(75, 27);
            closeButton.Text = "Close";
            closeButton.UseVisualStyleBackColor = true;
            //
            // statusStrip
            //
            statusStrip.Items.Add(statusLabel);
            statusStrip.Name = "statusStrip";
            //
            // statusLabel
            //
            statusLabel.Name = "statusLabel";
            statusLabel.Spring = true;
            statusLabel.Text = "Loading...";
            statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            //
            // MarketplaceForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = closeButton;
            ClientSize = new Size(820, 520);
            Controls.Add(resultsListView);
            Controls.Add(buttonPanel);
            Controls.Add(statusStrip);
            Controls.Add(toolStrip);
            MinimumSize = new Size(640, 360);
            Name = "MarketplaceForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Browse ESOUI Addons";
            buttonPanel.ResumeLayout(false);
            buttonPanel.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
        }
    }
}
