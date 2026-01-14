using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Vibra_DesktopApp.Views.Components
{
    /// <summary>
    /// Interaction logic for Header.xaml
    /// </summary>
    public partial class Header : UserControl
    {
        public Header()
        {
            InitializeComponent();
            MouseLeftButtonDown += (_, e) =>
            {
                if (e.ClickCount == 2)
                    Window.GetWindow(this)?.WindowState =
                        Window.GetWindow(this)?.WindowState == WindowState.Maximized
                            ? WindowState.Normal
                            : WindowState.Maximized;
                else
                    Window.GetWindow(this)?.DragMove();
            };
        }
    }
}
