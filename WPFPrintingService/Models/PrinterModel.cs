namespace WPFPrintingService
{
    internal class PrinterModel
    {
        public string Name { get; set; } = string.Empty;
        public bool IsOnline { get; set; } = false;
        public bool IsPrinting { get; set; } = false;
        public bool HasToner { get; set; } = false;
        public bool IsBusy { get; set; } = false;
        public bool IsDoorOpened { get; set; } = false;
    }
}
