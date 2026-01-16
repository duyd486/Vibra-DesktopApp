using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Vibra_DesktopApp.Converters
{
    public class BoolToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isPlaying)
            {
                return isPlaying ? IconChar.Pause : IconChar.Play;
            }

            return IconChar.Play;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
