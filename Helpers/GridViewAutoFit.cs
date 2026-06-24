using System.Windows;
using System.Windows.Controls;

namespace Teron_Addon_Manager.Helpers
{
    // Keeps a GridView's columns sized to fill the available width at all times, splitting it equally
    // across every column rather than relying on each column's own fixed Width.
    internal static class GridViewAutoFit
    {
        public static void Attach(Window window, ListView listView)
        {
            if (listView.View is not GridView gridView)
            {
                return;
            }

            void Apply()
            {
                var columnCount = gridView.Columns.Count;
                var available = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth - 4;
                if (columnCount == 0 || available <= 0)
                {
                    return;
                }

                var columnWidth = available / columnCount;
                foreach (var column in gridView.Columns)
                {
                    column.Width = columnWidth;
                }
            }

            // StateChanged can fire a beat before layout catches up with the new bounds, so the actual
            // resize is deferred to let ActualWidth settle first.
            window.StateChanged += (_, _) =>
                window.Dispatcher.BeginInvoke(new Action(Apply), System.Windows.Threading.DispatcherPriority.Loaded);
            listView.SizeChanged += (_, _) => Apply();
        }
    }
}
