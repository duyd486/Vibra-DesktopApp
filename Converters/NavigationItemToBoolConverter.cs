using System;
using System.Globalization;
using System.Windows.Data;
using Vibra_DesktopApp.Models;

namespace Vibra_DesktopApp.Converters
{
    /// <summary>
    /// Converts NavigationItem to bool (true if matches parameter)
    /// Used for button active state binding
    /// </summary>
    public class NavigationItemToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is NavigationItem currentItem && parameter is string paramStr)
            {
                if (Enum.TryParse<NavigationItem>(paramStr, out var targetItem))
                {
                    return currentItem == targetItem;
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}