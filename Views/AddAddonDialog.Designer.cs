namespace Teron_Addon_Manager
{
    partial class AddAddonDialog
    {
        private System.ComponentModel.IContainer components = null!;
        private Label promptLabel;
        private TextBox urlTextBox;
        private Button okButton;
        private Button cancelButton;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddAddonDialog));
            promptLabel = new Label();
            urlTextBox = new TextBox();
            okButton = new Button();
            cancelButton = new Button();
            SuspendLayout();
            // 
            // promptLabel
            // 
            promptLabel.AutoSize = true;
            promptLabel.Location = new Point(12, 15);
            promptLabel.Name = "promptLabel";
            promptLabel.Size = new Size(169, 15);
            promptLabel.TabIndex = 0;
            promptLabel.Text = "Addon page or download URL:";
            // 
            // urlTextBox
            // 
            urlTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            urlTextBox.Location = new Point(12, 35);
            urlTextBox.Name = "urlTextBox";
            urlTextBox.Size = new Size(396, 23);
            urlTextBox.TabIndex = 1;
            // 
            // okButton
            // 
            okButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            okButton.Location = new Point(252, 75);
            okButton.Name = "okButton";
            okButton.Size = new Size(75, 27);
            okButton.TabIndex = 2;
            okButton.Text = "OK";
            okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(333, 75);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(75, 27);
            cancelButton.TabIndex = 3;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            // 
            // AddAddonDialog
            // 
            AcceptButton = okButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = cancelButton;
            ClientSize = new Size(420, 114);
            Controls.Add(promptLabel);
            Controls.Add(urlTextBox);
            Controls.Add(okButton);
            Controls.Add(cancelButton);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AddAddonDialog";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Add Addon";
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
