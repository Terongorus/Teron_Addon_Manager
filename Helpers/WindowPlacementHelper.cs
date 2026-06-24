using System.Windows;
using Teron_Addon_Manager.Models;

namespace Teron_Addon_Manager.Helpers
{
    internal static class WindowPlacementHelper
    {
        public static void Apply(Window window, WindowPlacement? placement)
        {
            if (placement is null)
            {
                return;
            }

            if (placement.Width > 0 && placement.Height > 0)
            {
                window.Width = Math.Min(placement.Width, SystemParameters.VirtualScreenWidth);
                window.Height = Math.Min(placement.Height, SystemParameters.VirtualScreenHeight);
            }

            if (IsOnScreen(placement.Left, placement.Top))
            {
                // Overrides whatever WindowStartupLocation the window declares in XAML (e.g. CenterOwner) —
                // a saved position should win once we actually have one.
                window.WindowStartupLocation = WindowStartupLocation.Manual;
                window.Left = placement.Left;
                window.Top = placement.Top;
            }

            // Applied last, after the bounds above, so that un-maximizing later restores to the saved
            // size/position instead of whatever WPF would default to.
            if (placement.IsMaximized)
            {
                window.WindowState = WindowState.Maximized;
            }
        }

        public static WindowPlacement Capture(Window window)
        {
            var maximized = window.WindowState == WindowState.Maximized;

            // RestoreBounds holds the size/position the window would return to when un-maximized; Left/Top/
            // Width/Height while actually maximized just reflect whatever the OS snapped it to.
            var bounds = maximized
                ? window.RestoreBounds
                : new Rect(window.Left, window.Top, window.Width, window.Height);

            return new WindowPlacement
            {
                IsMaximized = maximized,
                Left = bounds.Left,
                Top = bounds.Top,
                Width = bounds.Width,
                Height = bounds.Height
            };
        }

        // Guards against a saved position from a monitor configuration that's no longer there (e.g. a
        // second monitor got unplugged), which would otherwise put the window somewhere unreachable.
        private static bool IsOnScreen(double left, double top) =>
            left >= SystemParameters.VirtualScreenLeft &&
            left <= SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - 100 &&
            top >= SystemParameters.VirtualScreenTop &&
            top <= SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight - 100;
    }
}
