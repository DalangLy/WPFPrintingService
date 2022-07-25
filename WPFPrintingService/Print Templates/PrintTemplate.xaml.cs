using System.Windows.Controls;

namespace WPFPrintingService
{
    public partial class PrintTemplate : UserControl
    {
        public PrintTemplateModel PrintTemplateModel { get; set; } = new PrintTemplateModel();


        public PrintTemplate(PrintTemplateModel PrintTemplate)
        {
            DataContext = this;


            PrintTemplateModel = PrintTemplate;

            InitializeComponent();
        }
    }
}
