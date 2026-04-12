using System;
using System.Globalization;
using System.Windows.Data;

namespace Vibra_DesktopApp.Converters
{
    public sealed class BoolToShuffleOpacityConverter : IValueConverter
    {
        public double EnabledOpacity { get; set; } = 1.0;
        public double DisabledOpacity { get; set; } = 0.45;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return b ? EnabledOpacity : DisabledOpacity;
            }

            return DisabledOpacity;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
