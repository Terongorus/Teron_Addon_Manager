using System.Windows;
using Teron_Addon_Manager.Models;

namespace Teron_Addon_Manager.Helpers
{
    // Restoring/capturing window position only matters when the window isn't maximized — a maximized
    // window's Left/Top is just whatever the OS snapped it to, not something the user actually placed.
    internal static class WindowPlacementHelper
    {
        public static void Apply(Window window, WindowPlacement? placement)
        {
            if (placement is null)
            {
                return;
            }

            if (placement.IsMaximized)
            {
                window.WindowState = WindowState.Maximized;
                return;
            }

            if (IsOnScreen(placement.Left, placement.Top))
            {
                // Overrides whatever WindowStartupLocation the window declares in XAML (e.g. CenterOwner) —
                // a saved position should win once we actually have one.
                window.WindowStartupLocation = WindowStartupLocation.Manual;
                window.Left = placement.Left;
                window.Top = placement.Top;
            }
        }

        public static WindowPlacement Capture(Window window)
        {
            var maximized = window.WindowState == WindowState.Maximized;
            return new WindowPlacement
            {
                IsMaximized = maximized,
                Left = maximized ? 0 : window.Left,
                Top = maximized ? 0 : window.Top
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
