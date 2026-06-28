using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TeronAddonManager.Services;

namespace TeronAddonManager
{
    public partial class App : Application
    {
        private Mutex? _singleInstanceMutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += (_, args) => LogException(args.Exception);
            TaskScheduler.UnobservedTaskException += (_, args) =>
            {
                LogException(args.Exception);
                args.SetObserved();
            };

            _singleInstanceMutex = new Mutex(true, "TeronAddonManager.SingleInstance", out bool createdNew);
            if (!createdNew)
            {
                MessageBox.Show(
                    $"{AppInfo.DisplayName} is already running.",
                    AppInfo.DisplayName,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                Shutdown();
                return;
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _singleInstanceMutex?.ReleaseMutex();
            base.OnExit(e);
        }

        private static void LogException(Exception ex)
        {
            try
            {
                string logPath = Path.Combine(AddonPaths.AppDataFolder, "error.log");

                Directory.CreateDirectory(AddonPaths.AppDataFolder);
                File.AppendAllText(logPath, $"{DateTime.Now:O}{Environment.NewLine}{ex}{Environment.NewLine}{Environment.NewLine}");
            }
            catch
            {
                // Logging is best-effort; nothing else we can do if it fails.
            }
        }
    }
}
