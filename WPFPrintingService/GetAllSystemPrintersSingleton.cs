using System.Printing;

namespace WPFPrintingService
{
    internal sealed class GetAllSystemPrintersSingleton
    {
        private static GetAllSystemPrintersSingleton? instance = null;
        public static GetAllSystemPrintersSingleton GetInstance
        {
            get
            {
                if (instance == null)
                    instance = new GetAllSystemPrintersSingleton();
                return instance;
            }
        }

        private static PrintQueueCollection? _printers;
        public PrintQueueCollection? Printers
        {
            get { return _printers; }
        }

        public void GetAllPrinters()
        {
            LocalPrintServer printServer = new LocalPrintServer();
            _printers = printServer.GetPrintQueues();
        }
    }
}
