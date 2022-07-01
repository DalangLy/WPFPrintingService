using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace WPFPrintingService
{
    public abstract class BaseValueConverter<TCHILD> : MarkupExtension, IValueConverter
        where TCHILD : BaseValueConverter<TCHILD>,new()
    {
        private TCHILD? value;

        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return value??(value = new TCHILD());
        }
    }
}
