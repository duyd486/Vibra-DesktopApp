using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Vibra_DesktopApp.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public bool Inverse { get; set; }
        public bool UseHidden { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool val = value is bool b && b;
            if (Inverse) val = !val;

            if (val)
                return Visibility.Visible;

            return UseHidden ? Visibility.Hidden : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
