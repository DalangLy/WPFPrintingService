using System.Collections.Generic;
using System.Windows.Controls;

namespace WPFPrintingService
{
    public partial class PrintTemplate : UserControl
    {
        public List<PrintDatum> PrintTemplateModels { get; set; }

        public PrintTemplate(List<PrintDatum> MyList)
        {
            
            PrintTemplateModels = MyList;

            DataContext = this;

            InitializeComponent();
        }
    }
}
