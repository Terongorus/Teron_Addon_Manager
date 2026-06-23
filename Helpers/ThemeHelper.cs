using Microsoft.Win32;

namespace Teron_Addon_Manager.Helpers
{
    // WPF's ThemeMode has no public API to resolve "System" to the actual active Light/Dark theme,
    // so System mode falls back to reading the same registry value Windows itself uses for apps.
    internal static class ThemeHelper
    {
        public static bool IsDarkActive()
        {
            var mode = System.Windows.Application.Current.ThemeMode;
            if (mode == System.Windows.ThemeMode.Dark) return true;
            if (mode == System.Windows.ThemeMode.Light) return false;
            return IsSystemAppsThemeDark();
        }

        private static bool IsSystemAppsThemeDark()
        {
            try
            {
                var value = Registry.GetValue(
                    @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                    "AppsUseLightTheme", 1);
                return value is int useLightTheme && useLightTheme == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
