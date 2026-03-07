using System;
using System.Globalization;
using System.Windows.Data;
using Vibra_DesktopApp.Models;

namespace Vibra_DesktopApp.Converters
{
    /// <summary>
    /// Converts NavigationItem comparison to "Active" tag string
    /// Used for button style triggers
    /// </summary>
    public class NavigationItemToTagConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is NavigationItem currentItem && parameter is string paramStr)
            {
                if (Enum.TryParse<NavigationItem>(paramStr, out var targetItem))
                {
                    return currentItem == targetItem ? "Active" : null;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}