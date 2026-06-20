namespace Teron_Addon_Manager
{
    public partial class AddAddonDialog : Form
    {
        public Uri? ResultUrl { get; private set; }

        public AddAddonDialog()
        {
            InitializeComponent();
            okButton.Click += OkButton_Click;
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            var text = urlTextBox.Text.Trim();
            if (!Uri.TryCreate(text, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                MessageBox.Show(this, "Enter a valid http(s) URL to the addon's page or download archive.",
                    "Invalid URL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            ResultUrl = uri;
            DialogResult = DialogResult.OK;
        }
    }
}
