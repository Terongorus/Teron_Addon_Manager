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
            toolStrip = new ToolStrip();
            addAddonMenuItem = new ToolStripMenuItem();
            checkUpdatesMenuItem = new ToolStripMenuItem();
            updateSelectedMenuItem = new ToolStripMenuItem();
            removeSelectedMenuItem = new ToolStripMenuItem();
            scanLocalMenuItem = new ToolStripMenuItem();
            browseMarketplaceMenuItem = new ToolStripMenuItem();
            openFolderMenuItem = new ToolStripMenuItem();
            targetPanel = new Panel();
            targetHeaderLabel = new Label();
            targetListBox = new ListBox();
            addonListView = new ListView();
            nameColumn = new ColumnHeader();
            installedColumn = new ColumnHeader();
            latestColumn = new ColumnHeader();
            statusColumn = new ColumnHeader();
            sourceColumn = new ColumnHeader();
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            addonContextMenu = new ContextMenuStrip();
            viewDetailsContextItem = new ToolStripMenuItem();
            checkSelectedContextItem = new ToolStripMenuItem();
            updateSelectedContextItem = new ToolStripMenuItem();
            removeSelectedContextItem = new ToolStripMenuItem();
            targetPanel.SuspendLayout();
            statusStrip.SuspendLayout();
            addonContextMenu.SuspendLayout();
            SuspendLayout();
            //
            // toolStrip
            //
            toolStrip.Items.Add(addAddonMenuItem);
            toolStrip.Items.Add(checkUpdatesMenuItem);
            toolStrip.Items.Add(updateSelectedMenuItem);
            toolStrip.Items.Add(removeSelectedMenuItem);
            toolStrip.Items.Add(scanLocalMenuItem);
            toolStrip.Items.Add(browseMarketplaceMenuItem);
            toolStrip.Items.Add(openFolderMenuItem);
            toolStrip.Name = "toolStrip";
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.Size = new Size(800, 25);
            //
            // addAddonMenuItem
            //
            addAddonMenuItem.Name = "addAddonMenuItem";
            addAddonMenuItem.Text = "Add Addon...";
            //
            // checkUpdatesMenuItem
            //
            checkUpdatesMenuItem.Name = "checkUpdatesMenuItem";
            checkUpdatesMenuItem.Text = "Check for Updates";
            //
            // updateSelectedMenuItem
            //
            updateSelectedMenuItem.Name = "updateSelectedMenuItem";
            updateSelectedMenuItem.Text = "Update Selected";
            //
            // removeSelectedMenuItem
            //
            removeSelectedMenuItem.Name = "removeSelectedMenuItem";
            removeSelectedMenuItem.Text = "Remove Selected";
            //
            // scanLocalMenuItem
            //
            scanLocalMenuItem.Name = "scanLocalMenuItem";
            scanLocalMenuItem.Text = "Scan for Local Addons...";
            //
            // browseMarketplaceMenuItem
            //
            browseMarketplaceMenuItem.Name = "browseMarketplaceMenuItem";
            browseMarketplaceMenuItem.Text = "Browse Addons...";
            //
            // openFolderMenuItem
            //
            openFolderMenuItem.Name = "openFolderMenuItem";
            openFolderMenuItem.Text = "Open AddOns Folder";
            //
            // targetPanel
            //
            targetPanel.Controls.Add(targetListBox);
            targetPanel.Controls.Add(targetHeaderLabel);
            targetPanel.Dock = DockStyle.Left;
            targetPanel.Name = "targetPanel";
            targetPanel.Padding = new Padding(4);
            targetPanel.Size = new Size(150, 379);
            //
            // targetHeaderLabel
            //
            targetHeaderLabel.Dock = DockStyle.Top;
            targetHeaderLabel.Padding = new Padding(2, 4, 2, 4);
            targetHeaderLabel.Name = "targetHeaderLabel";
            targetHeaderLabel.Size = new Size(142, 23);
            targetHeaderLabel.Text = "Game Version";
            //
            // targetListBox
            //
            targetListBox.Dock = DockStyle.Fill;
            targetListBox.IntegralHeight = false;
            targetListBox.Name = "targetListBox";
            targetListBox.Size = new Size(142, 348);
            //
            // addonListView
            //
            addonListView.Columns.AddRange(new ColumnHeader[] { nameColumn, installedColumn, latestColumn, statusColumn, sourceColumn });
            addonListView.Dock = DockStyle.Fill;
            addonListView.FullRowSelect = true;
            addonListView.GridLines = true;
            addonListView.MultiSelect = true;
            addonListView.Name = "addonListView";
            addonListView.Size = new Size(650, 379);
            addonListView.UseCompatibleStateImageBehavior = false;
            addonListView.View = View.Details;
            addonListView.ContextMenuStrip = addonContextMenu;
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
            // statusStrip
            //
            statusStrip.Items.Add(statusLabel);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(800, 22);
            //
            // statusLabel
            //
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(785, 17);
            statusLabel.Spring = true;
            statusLabel.Text = "Ready.";
            statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            //
            // addonContextMenu
            //
            addonContextMenu.Items.Add(viewDetailsContextItem);
            addonContextMenu.Items.Add(new ToolStripSeparator());
            addonContextMenu.Items.Add(checkSelectedContextItem);
            addonContextMenu.Items.Add(updateSelectedContextItem);
            addonContextMenu.Items.Add(removeSelectedContextItem);
            addonContextMenu.Name = "addonContextMenu";
            //
            // viewDetailsContextItem
            //
            viewDetailsContextItem.Name = "viewDetailsContextItem";
            viewDetailsContextItem.Text = "View Details...";
            //
            // checkSelectedContextItem
            //
            checkSelectedContextItem.Name = "checkSelectedContextItem";
            checkSelectedContextItem.Text = "Check for Updates";
            //
            // updateSelectedContextItem
            //
            updateSelectedContextItem.Name = "updateSelectedContextItem";
            updateSelectedContextItem.Text = "Update";
            //
            // removeSelectedContextItem
            //
            removeSelectedContextItem.Name = "removeSelectedContextItem";
            removeSelectedContextItem.Text = "Remove";
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
            MinimumSize = new Size(640, 360);
            Name = "Addon_Manager";
            Text = "Teron Addon Manager";
            targetPanel.ResumeLayout(false);
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            addonContextMenu.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
