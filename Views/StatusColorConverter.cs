using System.Globalization;
using System.Windows.Data;
using MediaColor = System.Windows.Media.Color;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;
using Brushes = System.Windows.Media.Brushes;

namespace Teron_Addon_Manager
{
    // Excel-style conditional-formatting palette for the addon Status column, ported directly from the
    // WinForms version's StatusColor method.
    public sealed class StatusColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (value as string) switch
            {
                "Check failed" => new SolidColorBrush(MediaColor.FromRgb(255, 199, 206)),
                "Update available" => new SolidColorBrush(MediaColor.FromRgb(255, 235, 156)),
                "Up to date" => new SolidColorBrush(MediaColor.FromRgb(198, 239, 206)),
                _ => Brushes.Transparent
            };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
