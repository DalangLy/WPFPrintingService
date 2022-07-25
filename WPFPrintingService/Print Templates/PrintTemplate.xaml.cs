using System.Windows.Controls;

namespace WPFPrintingService
{
    public partial class PrintTemplate : UserControl
    {
        public PrintTemplateLayoutModel PrintTemplateModel { get; set; } = new PrintTemplateLayoutModel();


        public PrintTemplate(PrintTemplateLayoutModel PrintTemplate)
        {
            DataContext = this;


            PrintTemplateModel = PrintTemplate;

            InitializeComponent();
        }
    }
}
