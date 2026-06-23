using System.Globalization;
using System.Windows.Data;
using MediaColor = System.Windows.Media.Color;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;
using Brushes = System.Windows.Media.Brushes;

namespace Teron_Addon_Manager.Helpers
{
    // Drives ListBoxItem/ListViewItem selection highlighting with a fixed, code-owned color instead of
    // Fluent's DynamicResource accent brushes. The DynamicResource+Trigger combo proved unreliable across
    // repeated runtime ThemeMode switches (still experimental, WPF0001) — newly-created item containers
    // would intermittently fail to resolve the accent brush at all once any switch had happened, leaving
    // the selected row with no visible highlight. A converter re-evaluated on every container creation
    // sidesteps that entirely, the same way StatusColorConverter already does for the Status column.
    public sealed class SelectedHighlightConverter : IValueConverter
    {
        private static readonly SolidColorBrush SelectedBrush = new(MediaColor.FromRgb(0x3B, 0x6E, 0xA5));

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is true ? SelectedBrush : Brushes.Transparent;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }

    // Falling back to "unset" for the non-selected case relies on Fluent's own ToggleButton/RadioButton
    // base style supplying a theme-correct Foreground underneath — it doesn't (confirmed: unselected
    // segments in the theme picker rendered with near-black text in Dark mode). Deciding both states
    // explicitly via ThemeHelper, the same way StatusColorConverter does, avoids depending on that at all.
    public sealed class SelectedForegroundConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is true) return Brushes.White;
            return ThemeHelper.IsDarkActive() ? Brushes.White : Brushes.Black;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
