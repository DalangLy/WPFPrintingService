using System;
using System.Collections.Generic;
using System.Windows.Controls;

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
