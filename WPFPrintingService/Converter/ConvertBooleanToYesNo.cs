using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
