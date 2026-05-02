using System;
using System.Globalization;
using System.Windows.Data;

namespace Vibra_DesktopApp.Converters
{
    public class AlternationIndexToOneBasedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int i)
                return (i + 1).ToString(culture);

            return "";
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return default;
        }
    }
}
