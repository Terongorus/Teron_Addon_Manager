using System.Windows;
using Brushes = System.Windows.Media.Brushes;

namespace Teron_Addon_Manager
{
    public enum FluentMessageBoxIcon
    {
        Information,
        Warning,
        Error,
        Question
    }

    public enum FluentMessageBoxButtons
    {
        Ok,
        OkCancel,
        YesNo
    }

    public partial class FluentMessageBoxWindow : Window
    {
        public FluentMessageBoxWindow(string message, string title, FluentMessageBoxButtons buttons, FluentMessageBoxIcon icon)
        {
            InitializeComponent();

            Title = title;
            messageText.Text = message;

            accentBar.Background = icon switch
            {
                FluentMessageBoxIcon.Warning => Brushes.DarkOrange,
                FluentMessageBoxIcon.Error => Brushes.Crimson,
                _ => Brushes.SteelBlue
            };

            switch (buttons)
            {
                case FluentMessageBoxButtons.OkCancel:
                    affirmativeButton.Content = "OK";
                    negativeButton.Visibility = Visibility.Visible;
                    break;
                case FluentMessageBoxButtons.YesNo:
                    affirmativeButton.Content = "Yes";
                    negativeButton.Content = "No";
                    negativeButton.Visibility = Visibility.Visible;
                    break;
                default:
                    affirmativeButton.Content = "OK";
                    break;
            }

            affirmativeButton.Click += (_, _) => { DialogResult = true; };
        }
    }

    internal static class FluentMessageBox
    {
        public static bool Show(Window? owner, string message, string title,
            FluentMessageBoxButtons buttons = FluentMessageBoxButtons.Ok,
            FluentMessageBoxIcon icon = FluentMessageBoxIcon.Information)
        {
            var dialog = new FluentMessageBoxWindow(message, title, buttons, icon);
            if (owner is not null)
            {
                dialog.Owner = owner;
            }
            return dialog.ShowDialog() == true;
        }
    }
}
