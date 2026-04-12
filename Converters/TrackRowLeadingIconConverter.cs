using FontAwesome.Sharp;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Vibra_DesktopApp.Converters
{
    public sealed class TrackRowLeadingIconConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var isMouseOver = values.Length > 0 && values[0] is bool b0 && b0;
            var isCurrentTrack = values.Length > 1 && values[1] is bool b1 && b1;
            var isPlaying = values.Length > 2 && values[2] is bool b2 && b2;

            if (isMouseOver)
            {
                return (isCurrentTrack && isPlaying) ? IconChar.Pause : IconChar.Play;
            }

            return IconChar.Add;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
