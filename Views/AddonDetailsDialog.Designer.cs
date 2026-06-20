namespace Teron_Addon_Manager
{
    partial class AddonDetailsDialog
    {
        private System.ComponentModel.IContainer components = null!;
        private Label headerLabel;
        private Label subtitleLabel;
        private Panel contentPanel;
        private FlowLayoutPanel buttonPanel;
        private Button closeButton;

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
            headerLabel = new Label();
            subtitleLabel = new Label();
            contentPanel = new Panel();
            buttonPanel = new FlowLayoutPanel();
            closeButton = new Button();
            buttonPanel.SuspendLayout();
            SuspendLayout();
            //
            // headerLabel
            //
            headerLabel.Dock = DockStyle.Top;
            headerLabel.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            headerLabel.Padding = new Padding(12, 12, 12, 0);
            headerLabel.AutoSize = true;
            headerLabel.Name = "headerLabel";
            headerLabel.Text = "Addon Name";
            //
            // subtitleLabel
            //
            subtitleLabel.Dock = DockStyle.Top;
            subtitleLabel.ForeColor = SystemColors.GrayText;
            subtitleLabel.Padding = new Padding(12, 2, 12, 10);
            subtitleLabel.AutoSize = true;
            subtitleLabel.Name = "subtitleLabel";
            subtitleLabel.Text = "Subtitle";
            //
            // contentPanel
            //
            contentPanel.AutoScroll = true;
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.Name = "contentPanel";
            //
            // buttonPanel
            //
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Padding = new Padding(8);
            buttonPanel.Height = 48;
            buttonPanel.Controls.Add(closeButton);
            buttonPanel.Name = "buttonPanel";
            //
            // closeButton
            //
            closeButton.AutoSize = true;
            closeButton.DialogResult = DialogResult.OK;
            closeButton.Margin = new Padding(8, 4, 0, 0);
            closeButton.Name = "closeButton";
            closeButton.Size = new Size(75, 27);
            closeButton.Text = "Close";
            closeButton.UseVisualStyleBackColor = true;
            //
            // AddonDetailsDialog
            //
            AcceptButton = closeButton;
            CancelButton = closeButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Window;
            ClientSize = new Size(480, 420);
            Controls.Add(contentPanel);
            Controls.Add(buttonPanel);
            Controls.Add(subtitleLabel);
            Controls.Add(headerLabel);
            MinimumSize = new Size(380, 320);
            Name = "AddonDetailsDialog";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Addon Details";
            buttonPanel.ResumeLayout(false);
            buttonPanel.PerformLayout();
            ResumeLayout(false);
        }
    }
}
