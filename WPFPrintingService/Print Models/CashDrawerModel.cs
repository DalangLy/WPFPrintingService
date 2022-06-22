using System.Collections.Generic;

namespace WPFPrintingService.Print_Models
{
    internal class CashDrawerModel : IPrintModel
    {
        public string Date { get; set; } = string.Empty;
        public List<CashDrawerPriceModel> Prices { get; set; } = new List<CashDrawerPriceModel>();
    }

    internal class CashDrawerPriceModel
    {
        public string Currency { get; set; } = string.Empty;
        public double Price { get; set; }
    }
}
