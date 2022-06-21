﻿using System.Collections.Generic;
using System.Windows.Controls;

namespace WPFPrintingService.Print_Templates
{
    public partial class CashDrawerTemplate : UserControl
    {
        public CashDrawerTemplate(List<GG> priceList, string date)
        {
            InitializeComponent();

            this.priceList.ItemsSource = priceList;
            this.lblDate.Text = date;
        }
    }

    public class GG
    {
        public string Title { get; set; } = "Hello";
    }
}
