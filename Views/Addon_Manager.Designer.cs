namespace Teron_Addon_Manager
{
    partial class Addon_Manager
    {
        private System.ComponentModel.IContainer components = null!;
        private ToolStrip toolStrip;
        private ToolStripMenuItem addAddonMenuItem;
        private ToolStripMenuItem checkUpdatesMenuItem;
        private ToolStripMenuItem updateSelectedMenuItem;
        private ToolStripMenuItem removeSelectedMenuItem;
        private ToolStripMenuItem scanLocalMenuItem;
        private ToolStripMenuItem browseMarketplaceMenuItem;
        private ToolStripMenuItem openFolderMenuItem;
        private Panel targetPanel;
        private Label targetHeaderLabel;
        private ListBox targetListBox;
        private ListView addonListView;
        private ColumnHeader nameColumn;
        private ColumnHeader installedColumn;
        private ColumnHeader latestColumn;
        private ColumnHeader statusColumn;
        private ColumnHeader sourceColumn;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private ContextMenuStrip addonContextMenu;
        private ToolStripMenuItem viewDetailsContextItem;
        private ToolStripMenuItem checkSelectedContextItem;
        private ToolStripMenuItem updateSelectedContextItem;
        private ToolStripMenuItem removeSelectedContextItem;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Addon_Manager));
            toolStrip = new ToolStrip();
            checkUpdatesMenuItem = new ToolStripMenuItem();
            updateSelectedMenuItem = new ToolStripMenuItem();
            removeSelectedMenuItem = new ToolStripMenuItem();
            addAddonMenuItem = new ToolStripMenuItem();
            scanLocalMenuItem = new ToolStripMenuItem();
            browseMarketplaceMenuItem = new ToolStripMenuItem();
            openFolderMenuItem = new ToolStripMenuItem();
            targetPanel = new Panel();
            targetListBox = new ListBox();
            targetHeaderLabel = new Label();
            addonListView = new ListView();
            nameColumn = new ColumnHeader();
            installedColumn = new ColumnHeader();
            latestColumn = new ColumnHeader();
            statusColumn = new ColumnHeader();
            sourceColumn = new ColumnHeader();
            addonContextMenu = new ContextMenuStrip(components);
            viewDetailsContextItem = new ToolStripMenuItem();
            checkSelectedContextItem = new ToolStripMenuItem();
            updateSelectedContextItem = new ToolStripMenuItem();
            removeSelectedContextItem = new ToolStripMenuItem();
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            toolStrip.SuspendLayout();
            targetPanel.SuspendLayout();
            addonContextMenu.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip
            // 
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.Items.AddRange(new ToolStripItem[] { checkUpdatesMenuItem, updateSelectedMenuItem, removeSelectedMenuItem, addAddonMenuItem, scanLocalMenuItem, browseMarketplaceMenuItem, openFolderMenuItem });
            toolStrip.Location = new Point(0, 0);
            toolStrip.Margin = new Padding(4);
            toolStrip.Name = "toolStrip";
            toolStrip.Size = new Size(800, 25);
            toolStrip.TabIndex = 4;
            // 
            // checkUpdatesMenuItem
            // 
            checkUpdatesMenuItem.Name = "checkUpdatesMenuItem";
            checkUpdatesMenuItem.Size = new Size(116, 25);
            checkUpdatesMenuItem.Text = "Check for Updates";
            // 
            // updateSelectedMenuItem
            // 
            updateSelectedMenuItem.Name = "updateSelectedMenuItem";
            updateSelectedMenuItem.Size = new Size(104, 25);
            updateSelectedMenuItem.Text = "Update Selected";
            // 
            // removeSelectedMenuItem
            // 
            removeSelectedMenuItem.Name = "removeSelectedMenuItem";
            removeSelectedMenuItem.Size = new Size(109, 25);
            removeSelectedMenuItem.Text = "Remove Selected";
            // 
            // addAddonMenuItem
            // 
            addAddonMenuItem.Name = "addAddonMenuItem";
            addAddonMenuItem.Size = new Size(132, 25);
            addAddonMenuItem.Text = "Manual Add Addon...";
            // 
            // scanLocalMenuItem
            // 
            scanLocalMenuItem.Name = "scanLocalMenuItem";
            scanLocalMenuItem.Size = new Size(146, 25);
            scanLocalMenuItem.Text = "Scan for Local Addons...";
            // 
            // browseMarketplaceMenuItem
            // 
            browseMarketplaceMenuItem.Name = "browseMarketplaceMenuItem";
            browseMarketplaceMenuItem.Size = new Size(110, 25);
            browseMarketplaceMenuItem.Text = "Browse Addons...";
            // 
            // openFolderMenuItem
            // 
            openFolderMenuItem.Name = "openFolderMenuItem";
            openFolderMenuItem.Size = new Size(130, 25);
            openFolderMenuItem.Text = "Open AddOns Folder";
            // 
            // targetPanel
            // 
            targetPanel.Controls.Add(targetListBox);
            targetPanel.Controls.Add(targetHeaderLabel);
            targetPanel.Dock = DockStyle.Left;
            targetPanel.Location = new Point(0, 25);
            targetPanel.Name = "targetPanel";
            targetPanel.Padding = new Padding(4);
            targetPanel.Size = new Size(150, 403);
            targetPanel.TabIndex = 2;
            // 
            // targetListBox
            // 
            targetListBox.Dock = DockStyle.Fill;
            targetListBox.IntegralHeight = false;
            targetListBox.Location = new Point(4, 27);
            targetListBox.Name = "targetListBox";
            targetListBox.Size = new Size(142, 372);
            targetListBox.TabIndex = 0;
            // 
            // targetHeaderLabel
            // 
            targetHeaderLabel.Dock = DockStyle.Top;
            targetHeaderLabel.Location = new Point(4, 4);
            targetHeaderLabel.Name = "targetHeaderLabel";
            targetHeaderLabel.Padding = new Padding(2, 4, 2, 4);
            targetHeaderLabel.Size = new Size(142, 23);
            targetHeaderLabel.TabIndex = 1;
            targetHeaderLabel.Text = "Game Version";
            // 
            // addonListView
            // 
            addonListView.Columns.AddRange(new ColumnHeader[] { nameColumn, installedColumn, latestColumn, statusColumn, sourceColumn });
            addonListView.ContextMenuStrip = addonContextMenu;
            addonListView.Dock = DockStyle.Fill;
            addonListView.FullRowSelect = true;
            addonListView.GridLines = true;
            addonListView.Location = new Point(150, 25);
            addonListView.Name = "addonListView";
            addonListView.Size = new Size(650, 403);
            addonListView.TabIndex = 1;
            addonListView.UseCompatibleStateImageBehavior = false;
            addonListView.View = View.Details;
            // 
            // nameColumn
            // 
            nameColumn.Text = "Name";
            nameColumn.Width = 220;
            // 
            // installedColumn
            // 
            installedColumn.Text = "Installed";
            installedColumn.Width = 110;
            // 
            // latestColumn
            // 
            latestColumn.Text = "Latest";
            latestColumn.Width = 110;
            // 
            // statusColumn
            // 
            statusColumn.Text = "Status";
            statusColumn.Width = 120;
            // 
            // sourceColumn
            // 
            sourceColumn.Text = "Source";
            sourceColumn.Width = 100;
            // 
            // addonContextMenu
            // 
            addonContextMenu.Items.AddRange(new ToolStripItem[] { viewDetailsContextItem, checkSelectedContextItem, updateSelectedContextItem, removeSelectedContextItem });
            addonContextMenu.Name = "addonContextMenu";
            addonContextMenu.Size = new Size(172, 92);
            // 
            // viewDetailsContextItem
            // 
            viewDetailsContextItem.Name = "viewDetailsContextItem";
            viewDetailsContextItem.Size = new Size(171, 22);
            viewDetailsContextItem.Text = "View Details...";
            // 
            // checkSelectedContextItem
            // 
            checkSelectedContextItem.Name = "checkSelectedContextItem";
            checkSelectedContextItem.Size = new Size(171, 22);
            checkSelectedContextItem.Text = "Check for Updates";
            // 
            // updateSelectedContextItem
            // 
            updateSelectedContextItem.Name = "updateSelectedContextItem";
            updateSelectedContextItem.Size = new Size(171, 22);
            updateSelectedContextItem.Text = "Update";
            // 
            // removeSelectedContextItem
            // 
            removeSelectedContextItem.Name = "removeSelectedContextItem";
            removeSelectedContextItem.Size = new Size(171, 22);
            removeSelectedContextItem.Text = "Remove";
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
            statusStrip.Location = new Point(0, 428);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(800, 22);
            statusStrip.TabIndex = 3;
            // 
            // statusLabel
            // 
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(785, 17);
            statusLabel.Spring = true;
            statusLabel.Text = "Ready.";
            statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // Addon_Manager
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(addonListView);
            Controls.Add(targetPanel);
            Controls.Add(statusStrip);
            Controls.Add(toolStrip);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(640, 360);
            Name = "Addon_Manager";
            Text = "Teron Addon Manager";
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            targetPanel.ResumeLayout(false);
            addonContextMenu.ResumeLayout(false);
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
