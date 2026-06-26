using System.Windows;

namespace TeronAddonManager
{
    public partial class AddAddonDialog : Window
    {
        public Uri? ResultUrl { get; private set; }

        public AddAddonDialog()
        {
            InitializeComponent();
            okButton.Click += OkButton_Click;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            var text = urlTextBox.Text.Trim();
            if (!Uri.TryCreate(text, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                FluentMessageBox.Show(this, "Enter a valid http(s) URL to the addon's page or download archive.",
                    "Invalid URL", icon: FluentMessageBoxIcon.Warning);
                return;
            }

            ResultUrl = uri;
            DialogResult = true;
        }
    }
}
