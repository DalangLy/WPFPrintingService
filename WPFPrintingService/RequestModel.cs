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
        public string Base64Image { get; set; } = string.Empty;
    }
}
