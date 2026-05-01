using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Vibra_DesktopApp.Converters
{
    public sealed class PaymentStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Note: boxed Nullable<int> becomes boxed int when HasValue == true
            int? status = value is int i ? i : null;

            // Match Vue:
            // - success (2): green
            // - fail (1): yellow
            return status switch
            {
                2 => new SolidColorBrush(Color.FromArgb(0x66, 0x10, 0xB9, 0x81)), // green w/ alpha
                1 => new SolidColorBrush(Color.FromArgb(0x66, 0xF5, 0x9E, 0x0B)), // yellow w/ alpha
                _ => new SolidColorBrush(Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF)),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }

    public sealed class PaymentStatusToDotBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int? status = value is int i ? i : null;
            return status switch
            {
                2 => new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81)),
                1 => new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B)),
                _ => new SolidColorBrush(Color.FromRgb(0x9C, 0xA3, 0xAF)),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }

    public sealed class PaymentStatusToTextBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int? status = value is int i ? i : null;
            return status switch
            {
                2 => new SolidColorBrush(Color.FromRgb(0x34, 0xD3, 0x99)),
                1 => new SolidColorBrush(Color.FromRgb(0xFB, 0xBF, 0x24)),
                _ => new SolidColorBrush(Color.FromRgb(0xD1, 0xD5, 0xDB)),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }

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
