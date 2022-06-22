using System.Collections.Generic;
using System.Windows.Controls;
using WPFPrintingService.Print_Models;

namespace WPFPrintingService.Print_Templates
{
    public partial class CashDrawerTemplate : UserControl
    {
        public CashDrawerTemplate(List<Price> priceList, string date)
        {
            InitializeComponent();

            this.priceList.ItemsSource = priceList;
            this.lblDate.Text = date;
        }
    }
}
