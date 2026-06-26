using System.Globalization;
using System.Windows.Data;
using MediaColor = System.Windows.Media.Color;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;
using Brushes = System.Windows.Media.Brushes;

namespace TeronAddonManager.Helpers
{
    // Status badge palette for the addon Status column. Light mode keeps the Excel-style
    // conditional-formatting pastels (pairs well with the default near-black inherited text); dark mode
    // uses deeper saturated fills instead, since those same light pastels read as washed-out/illegible
    // against the dark Fluent theme's near-white inherited text.
    public sealed class StatusColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var dark = ThemeHelper.IsDarkActive();
            return (value as string) switch
            {
                "Check failed" => dark
                    ? new SolidColorBrush(MediaColor.FromRgb(194, 67, 67))
                    : new SolidColorBrush(MediaColor.FromRgb(255, 199, 206)),
                "Update available" => dark
                    ? new SolidColorBrush(MediaColor.FromRgb(199, 138, 28))
                    : new SolidColorBrush(MediaColor.FromRgb(255, 235, 156)),
                "Up to date" => dark
                    ? new SolidColorBrush(MediaColor.FromRgb(46, 160, 90))
                    : new SolidColorBrush(MediaColor.FromRgb(198, 239, 206)),
                _ => Brushes.Transparent
            };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
