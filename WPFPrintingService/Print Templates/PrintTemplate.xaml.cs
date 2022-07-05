using System.Collections.Generic;
using System.Windows.Controls;

namespace WPFPrintingService
{
    public partial class PrintTemplate : UserControl
    {
        public List<PrintTemplateModel> PrintTemplateModels { get; set; }

        public PrintTemplate(List<PrintTemplateModel> MyList)
        {
            
            PrintTemplateModels = MyList;

            DataContext = this;

            InitializeComponent();
        }
    }
}
