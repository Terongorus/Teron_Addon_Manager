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
            headerLabel.AutoSize = true;
            headerLabel.Dock = DockStyle.Top;
            headerLabel.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            headerLabel.Location = new Point(0, 0);
            headerLabel.Name = "headerLabel";
            headerLabel.Padding = new Padding(12, 12, 12, 0);
            headerLabel.Size = new Size(154, 37);
            headerLabel.TabIndex = 3;
            headerLabel.Text = "Addon Name";
            // 
            // subtitleLabel
            // 
            subtitleLabel.AutoSize = true;
            subtitleLabel.Dock = DockStyle.Top;
            subtitleLabel.ForeColor = SystemColors.GrayText;
            subtitleLabel.Location = new Point(0, 37);
            subtitleLabel.Name = "subtitleLabel";
            subtitleLabel.Padding = new Padding(12, 2, 12, 10);
            subtitleLabel.Size = new Size(71, 27);
            subtitleLabel.TabIndex = 2;
            subtitleLabel.Text = "Subtitle";
            // 
            // contentPanel
            // 
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.Location = new Point(0, 64);
            contentPanel.Name = "contentPanel";
            contentPanel.Size = new Size(500, 408);
            contentPanel.TabIndex = 0;
            // 
            // buttonPanel
            // 
            buttonPanel.Controls.Add(closeButton);
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Location = new Point(0, 472);
            buttonPanel.Name = "buttonPanel";
            buttonPanel.Padding = new Padding(8);
            buttonPanel.Size = new Size(500, 48);
            buttonPanel.TabIndex = 1;
            // 
            // closeButton
            // 
            closeButton.AutoSize = true;
            closeButton.DialogResult = DialogResult.OK;
            closeButton.Location = new Point(389, 12);
            closeButton.Margin = new Padding(8, 4, 0, 0);
            closeButton.Name = "closeButton";
            closeButton.Size = new Size(75, 27);
            closeButton.TabIndex = 0;
            closeButton.Text = "Close";
            closeButton.UseVisualStyleBackColor = true;
            // 
            // AddonDetailsDialog
            // 
            AcceptButton = closeButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Window;
            CancelButton = closeButton;
            ClientSize = new Size(500, 520);
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
            PerformLayout();
        }
    }
}
