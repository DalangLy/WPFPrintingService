﻿using WPFPrintingService.Print_Models;

namespace WPFPrintingService
{
    internal class RequestModel
    {
        public string Code { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
    }

    internal class PrintDataModel
    {
        public string PrinterName { get; set; } = string.Empty;
        public string PrintMethod { get; set; } = "PrintAndCut";
        public string TemplateName { get; set; } = "CashDrawer";
        public IPrintModel? PrintModel { get; set; }
    }
}
