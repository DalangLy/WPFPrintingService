using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFPrintingService.Print_Templates
{
    public partial class CashDrawerTemplate : UserControl
    {
        private string? date;
        private List<string>? files;
        public string DateOutput
        {
            get { return date ?? "No Date"; }
            set { date = value; }
        }

        public List<string> FilesOutput
        {
            get { return files ?? new List<string>(); }
            set { files = value; }
        }
        public CashDrawerTemplate()
        {
            InitializeComponent();
            this.DataContext = this;
        }
    }
}
