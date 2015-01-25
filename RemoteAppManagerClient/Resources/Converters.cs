using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;

namespace RemoteAppManagerClient.Resources
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture) {

                if (value != null && value.GetType() == typeof(Boolean)) {
                    Boolean val = (Boolean)value;

                    if (val) {
                        return Visibility.Visible;
                    }
                    else {
                        return Visibility.Collapsed;
                    }
                }

                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture) {
                return null;
        }
    }
}
