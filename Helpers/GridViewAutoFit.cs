using System.Windows;
using System.Windows.Controls;

namespace Teron_Addon_Manager.Helpers
{
    // Stretches a GridView's columns to fill the available width while the window is maximized (there's
    // room to spare), and snaps them back to their original fixed widths as soon as it's restored.
    internal static class GridViewAutoFit
    {
        public static void Attach(Window window, ListView listView)
        {
            if (listView.View is not GridView gridView)
            {
                return;
            }

            var originalWidths = gridView.Columns.Select(c => c.Width).ToArray();

            void Apply()
            {
                if (gridView.Columns.Count != originalWidths.Length)
                {
                    return;
                }

                if (window.WindowState != WindowState.Maximized)
                {
                    for (var i = 0; i < gridView.Columns.Count; i++)
                    {
                        gridView.Columns[i].Width = originalWidths[i];
                    }
                    return;
                }

                var totalOriginal = originalWidths.Sum();
                var available = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth - 4;
                if (totalOriginal <= 0 || available <= totalOriginal)
                {
                    return;
                }

                var scale = available / totalOriginal;
                for (var i = 0; i < gridView.Columns.Count; i++)
                {
                    gridView.Columns[i].Width = originalWidths[i] * scale;
                }
            }

            // StateChanged can fire a beat before layout catches up with the new bounds, so the actual
            // stretch is deferred to let ActualWidth settle first.
            window.StateChanged += (_, _) =>
                window.Dispatcher.BeginInvoke(new Action(Apply), System.Windows.Threading.DispatcherPriority.Loaded);
            listView.SizeChanged += (_, _) =>
            {
                if (window.WindowState == WindowState.Maximized)
                {
                    Apply();
                }
            };
        }
    }
}
