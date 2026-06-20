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
            matchedGroupBox = new GroupBox();
            matchedCheckedListBox = new CheckedListBox();
            unmatchedGroupBox = new GroupBox();
            unmatchedListBox = new ListBox();
            buttonPanel = new FlowLayoutPanel();
            adoptButton = new Button();
            skipButton = new Button();
            matchedGroupBox.SuspendLayout();
            unmatchedGroupBox.SuspendLayout();
            buttonPanel.SuspendLayout();
            SuspendLayout();
            //
            // matchedGroupBox
            //
            matchedGroupBox.Controls.Add(matchedCheckedListBox);
            matchedGroupBox.Dock = DockStyle.Top;
            matchedGroupBox.Height = 220;
            matchedGroupBox.Name = "matchedGroupBox";
            matchedGroupBox.Padding = new Padding(6);
            matchedGroupBox.Text = "Found matches";
            //
            // matchedCheckedListBox
            //
            matchedCheckedListBox.CheckOnClick = true;
            matchedCheckedListBox.Dock = DockStyle.Fill;
            matchedCheckedListBox.IntegralHeight = false;
            matchedCheckedListBox.Name = "matchedCheckedListBox";
            //
            // unmatchedGroupBox
            //
            unmatchedGroupBox.Controls.Add(unmatchedListBox);
            unmatchedGroupBox.Dock = DockStyle.Fill;
            unmatchedGroupBox.Name = "unmatchedGroupBox";
            unmatchedGroupBox.Padding = new Padding(6);
            unmatchedGroupBox.Text = "Found but not matched to ESOUI";
            //
            // unmatchedListBox
            //
            unmatchedListBox.Dock = DockStyle.Fill;
            unmatchedListBox.IntegralHeight = false;
            unmatchedListBox.Name = "unmatchedListBox";
            //
            // buttonPanel
            //
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Padding = new Padding(8);
            buttonPanel.Height = 48;
            buttonPanel.Controls.Add(skipButton);
            buttonPanel.Controls.Add(adoptButton);
            buttonPanel.Name = "buttonPanel";
            //
            // adoptButton
            //
            adoptButton.AutoSize = true;
            adoptButton.Margin = new Padding(8, 4, 0, 0);
            adoptButton.Name = "adoptButton";
            adoptButton.Size = new Size(120, 27);
            adoptButton.Text = "Adopt Selected";
            adoptButton.UseVisualStyleBackColor = true;
            //
            // skipButton
            //
            skipButton.AutoSize = true;
            skipButton.DialogResult = DialogResult.Cancel;
            skipButton.Margin = new Padding(8, 4, 0, 0);
            skipButton.Name = "skipButton";
            skipButton.Size = new Size(75, 27);
            skipButton.Text = "Skip";
            skipButton.UseVisualStyleBackColor = true;
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
