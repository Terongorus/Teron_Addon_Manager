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
        private ContextMenuStrip resultsContextMenu;
        private ToolStripMenuItem viewDetailsContextItem;
        private ToolStripMenuItem installContextItem;

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
            components = new System.ComponentModel.Container();
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
            resultsContextMenu = new ContextMenuStrip(components);
            viewDetailsContextItem = new ToolStripMenuItem();
            installContextItem = new ToolStripMenuItem();
            buttonPanel = new FlowLayoutPanel();
            closeButton = new Button();
            installButton = new Button();
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            toolStrip.SuspendLayout();
            resultsContextMenu.SuspendLayout();
            buttonPanel.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip
            // 
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.Items.AddRange(new ToolStripItem[] { searchLabel, searchTextBox, categoryLabel, categoryComboBox, sortLabel, sortComboBox, refreshMenuItem });
            toolStrip.Location = new Point(0, 0);
            toolStrip.Margin = new Padding(4);
            toolStrip.Name = "toolStrip";
            toolStrip.Size = new Size(820, 25);
            toolStrip.TabIndex = 4;
            // 
            // searchLabel
            // 
            searchLabel.Name = "searchLabel";
            searchLabel.Size = new Size(45, 22);
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
            categoryLabel.Size = new Size(58, 22);
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
            sortLabel.Size = new Size(47, 22);
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
            refreshMenuItem.Size = new Size(102, 25);
            refreshMenuItem.Text = "Refresh Catalog";
            // 
            // resultsListView
            // 
            resultsListView.Columns.AddRange(new ColumnHeader[] { titleColumn, authorColumn, categoryColumn, downloadsColumn, lastUpdatedColumn });
            resultsListView.ContextMenuStrip = resultsContextMenu;
            resultsListView.Dock = DockStyle.Fill;
            resultsListView.FullRowSelect = true;
            resultsListView.GridLines = true;
            resultsListView.Location = new Point(0, 25);
            resultsListView.Name = "resultsListView";
            resultsListView.Size = new Size(820, 425);
            resultsListView.TabIndex = 1;
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
            // resultsContextMenu
            // 
            resultsContextMenu.Items.AddRange(new ToolStripItem[] { viewDetailsContextItem, installContextItem });
            resultsContextMenu.Name = "resultsContextMenu";
            resultsContextMenu.Size = new Size(153, 54);
            // 
            // viewDetailsContextItem
            // 
            viewDetailsContextItem.Name = "viewDetailsContextItem";
            viewDetailsContextItem.Size = new Size(152, 22);
            viewDetailsContextItem.Text = "View Details...";
            // 
            // installContextItem
            // 
            installContextItem.Name = "installContextItem";
            installContextItem.Size = new Size(152, 22);
            installContextItem.Text = "Install Selected";
            // 
            // buttonPanel
            // 
            buttonPanel.Controls.Add(closeButton);
            buttonPanel.Controls.Add(installButton);
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Location = new Point(0, 450);
            buttonPanel.Name = "buttonPanel";
            buttonPanel.Padding = new Padding(8);
            buttonPanel.Size = new Size(820, 48);
            buttonPanel.TabIndex = 2;
            // 
            // closeButton
            // 
            closeButton.AutoSize = true;
            closeButton.DialogResult = DialogResult.Cancel;
            closeButton.Location = new Point(729, 12);
            closeButton.Margin = new Padding(8, 4, 0, 0);
            closeButton.Name = "closeButton";
            closeButton.Size = new Size(75, 27);
            closeButton.TabIndex = 0;
            closeButton.Text = "Close";
            closeButton.UseVisualStyleBackColor = true;
            // 
            // installButton
            // 
            installButton.AutoSize = true;
            installButton.Location = new Point(601, 12);
            installButton.Margin = new Padding(8, 4, 0, 0);
            installButton.Name = "installButton";
            installButton.Size = new Size(120, 27);
            installButton.TabIndex = 1;
            installButton.Text = "Install Selected";
            installButton.UseVisualStyleBackColor = true;
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
            statusStrip.Location = new Point(0, 498);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(820, 22);
            statusStrip.TabIndex = 3;
            // 
            // statusLabel
            // 
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(805, 17);
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
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            resultsContextMenu.ResumeLayout(false);
            buttonPanel.ResumeLayout(false);
            buttonPanel.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
