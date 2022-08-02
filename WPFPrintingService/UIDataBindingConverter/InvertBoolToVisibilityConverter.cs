using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WPFPrintingService 
{
    public class InvertBoolToVisibilityConverter : BaseValueConverter<InvertBoolToVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;

            return boolValue ? Visibility.Hidden : Visibility.Visible;
        }
    }
}
