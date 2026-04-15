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

    public sealed class NullableIntEqualsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values is null || values.Length < 2)
                return false;

            var a = TryGetNullableInt(values[0]);
            var b = TryGetNullableInt(values[1]);
            return a.HasValue && b.HasValue && a.Value == b.Value;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();

        private static int? TryGetNullableInt(object value)
        {
            if (value is null || value == System.Windows.DependencyProperty.UnsetValue)
                return null;

            if (value is int i)
                return i;

            if (int.TryParse(value.ToString(), out var parsed))
                return parsed;

            return null;
        }
    }
}
