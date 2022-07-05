using System;
using System.Globalization;

namespace WPFPrintingService
{
    public class ConvertBooleanToYesNo : BaseValueConverter<ConvertBooleanToYesNo>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;

            return boolValue?"Yes":"No";
        }
    }
}
