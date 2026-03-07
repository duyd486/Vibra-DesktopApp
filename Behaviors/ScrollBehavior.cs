using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Vibra_DesktopApp.Behaviors
{
    /// <summary>
    /// Attached behavior to control ScrollViewer scroll direction.
    /// Prevents horizontal ScrollViewers from capturing vertical mouse wheel events.
    /// Enables Shift+MouseWheel for explicit horizontal scrolling.
    /// </summary>
    public static class ScrollBehavior
    {
        #region HorizontalScrollOnly Attached Property

        public static readonly DependencyProperty HorizontalScrollOnlyProperty =
            DependencyProperty.RegisterAttached(
                "HorizontalScrollOnly",
                typeof(bool),
                typeof(ScrollBehavior),
                new PropertyMetadata(false, OnHorizontalScrollOnlyChanged));

        public static bool GetHorizontalScrollOnly(DependencyObject obj)
        {
            return (bool)obj.GetValue(HorizontalScrollOnlyProperty);
        }

        public static void SetHorizontalScrollOnly(DependencyObject obj, bool value)
        {
            obj.SetValue(HorizontalScrollOnlyProperty, value);
        }

        private static void OnHorizontalScrollOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer)
            {
                if ((bool)e.NewValue)
                {
                    scrollViewer.PreviewMouseWheel += ScrollViewer_PreviewMouseWheel;
                }
                else
                {
                    scrollViewer.PreviewMouseWheel -= ScrollViewer_PreviewMouseWheel;
                }
            }
        }

        private static void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer == null) return;

            // Check if Shift is pressed for horizontal scrolling
            bool shiftPressed = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);

            if (shiftPressed)
            {
                // Horizontal scroll with Shift+Wheel
                double offset = scrollViewer.HorizontalOffset - (e.Delta / 3.0);
                scrollViewer.ScrollToHorizontalOffset(offset);
                e.Handled = true;
            }
            else
            {
                // Vertical scroll: bubble up to parent ScrollViewer
                // Mark as handled = false so parent can handle it
                e.Handled = false;
                
                // Find parent ScrollViewer and scroll it
                var parentScrollViewer = FindParentScrollViewer(scrollViewer);
                if (parentScrollViewer != null)
                {
                    double offset = parentScrollViewer.VerticalOffset - (e.Delta / 3.0);
                    parentScrollViewer.ScrollToVerticalOffset(offset);
                    e.Handled = true;
                }
            }
        }

        private static ScrollViewer? FindParentScrollViewer(DependencyObject child)
        {
            var parent = System.Windows.Media.VisualTreeHelper.GetParent(child);
            while (parent != null)
            {
                if (parent is ScrollViewer sv && sv.VerticalScrollBarVisibility != ScrollBarVisibility.Disabled)
                {
                    return sv;
                }
                parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        #endregion
    }
}