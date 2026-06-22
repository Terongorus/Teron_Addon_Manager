namespace Teron_Addon_Manager
{
    partial class ScanResultsDialog
    {
        private System.ComponentModel.IContainer components = null!;
        private GroupBox matchedGroupBox;
        private CheckedListBox matchedCheckedListBox;
        private GroupBox unmatchedGroupBox;
        private ListBox unmatchedListBox;
        private FlowLayoutPanel buttonPanel;
        private Button adoptButton;
        private Button skipButton;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScanResultsDialog));
            matchedGroupBox = new GroupBox();
            matchedCheckedListBox = new CheckedListBox();
            unmatchedGroupBox = new GroupBox();
            unmatchedListBox = new ListBox();
            buttonPanel = new FlowLayoutPanel();
            skipButton = new Button();
            adoptButton = new Button();
            matchedGroupBox.SuspendLayout();
            unmatchedGroupBox.SuspendLayout();
            buttonPanel.SuspendLayout();
            SuspendLayout();
            // 
            // matchedGroupBox
            // 
            matchedGroupBox.Controls.Add(matchedCheckedListBox);
            matchedGroupBox.Dock = DockStyle.Top;
            matchedGroupBox.Location = new Point(0, 0);
            matchedGroupBox.Name = "matchedGroupBox";
            matchedGroupBox.Padding = new Padding(6);
            matchedGroupBox.Size = new Size(520, 220);
            matchedGroupBox.TabIndex = 1;
            matchedGroupBox.TabStop = false;
            matchedGroupBox.Text = "Found matches";
            // 
            // matchedCheckedListBox
            // 
            matchedCheckedListBox.CheckOnClick = true;
            matchedCheckedListBox.Dock = DockStyle.Fill;
            matchedCheckedListBox.IntegralHeight = false;
            matchedCheckedListBox.Location = new Point(6, 22);
            matchedCheckedListBox.Name = "matchedCheckedListBox";
            matchedCheckedListBox.Size = new Size(508, 192);
            matchedCheckedListBox.TabIndex = 0;
            // 
            // unmatchedGroupBox
            // 
            unmatchedGroupBox.Controls.Add(unmatchedListBox);
            unmatchedGroupBox.Dock = DockStyle.Fill;
            unmatchedGroupBox.Location = new Point(0, 220);
            unmatchedGroupBox.Name = "unmatchedGroupBox";
            unmatchedGroupBox.Padding = new Padding(6);
            unmatchedGroupBox.Size = new Size(520, 152);
            unmatchedGroupBox.TabIndex = 0;
            unmatchedGroupBox.TabStop = false;
            unmatchedGroupBox.Text = "Found but not matched to ESOUI";
            // 
            // unmatchedListBox
            // 
            unmatchedListBox.Dock = DockStyle.Fill;
            unmatchedListBox.IntegralHeight = false;
            unmatchedListBox.Location = new Point(6, 22);
            unmatchedListBox.Name = "unmatchedListBox";
            unmatchedListBox.Size = new Size(508, 124);
            unmatchedListBox.TabIndex = 0;
            // 
            // buttonPanel
            // 
            buttonPanel.Controls.Add(skipButton);
            buttonPanel.Controls.Add(adoptButton);
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Location = new Point(0, 372);
            buttonPanel.Name = "buttonPanel";
            buttonPanel.Padding = new Padding(8);
            buttonPanel.Size = new Size(520, 48);
            buttonPanel.TabIndex = 2;
            // 
            // skipButton
            // 
            skipButton.AutoSize = true;
            skipButton.DialogResult = DialogResult.Cancel;
            skipButton.Location = new Point(429, 12);
            skipButton.Margin = new Padding(8, 4, 0, 0);
            skipButton.Name = "skipButton";
            skipButton.Size = new Size(75, 27);
            skipButton.TabIndex = 0;
            skipButton.Text = "Skip";
            skipButton.UseVisualStyleBackColor = true;
            // 
            // adoptButton
            // 
            adoptButton.AutoSize = true;
            adoptButton.Location = new Point(301, 12);
            adoptButton.Margin = new Padding(8, 4, 0, 0);
            adoptButton.Name = "adoptButton";
            adoptButton.Size = new Size(120, 27);
            adoptButton.TabIndex = 1;
            adoptButton.Text = "Adopt Selected";
            adoptButton.UseVisualStyleBackColor = true;
            // 
            // ScanResultsDialog
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = skipButton;
            ClientSize = new Size(520, 420);
            Controls.Add(unmatchedGroupBox);
            Controls.Add(matchedGroupBox);
            Controls.Add(buttonPanel);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(420, 320);
            Name = "ScanResultsDialog";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Local Addons Found";
            matchedGroupBox.ResumeLayout(false);
            unmatchedGroupBox.ResumeLayout(false);
            buttonPanel.ResumeLayout(false);
            buttonPanel.PerformLayout();
            ResumeLayout(false);
        }
    }
}
