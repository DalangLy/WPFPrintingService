namespace WPFPrintingService
{
    internal class PrinterFromWindowsSystemModel
    {
        private string _printerName;
        public PrinterFromWindowsSystemModel(string printerName)
        {
            this._printerName = printerName;
        }

        public string PrinterName { get { return _printerName; } }
    }
}
