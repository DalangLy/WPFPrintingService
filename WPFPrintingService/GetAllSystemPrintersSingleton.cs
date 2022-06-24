using System.Printing;

namespace WPFPrintingService
{
    internal sealed class GetAllSystemPrintersSingleton
    {
        private static GetAllSystemPrintersSingleton instance = new GetAllSystemPrintersSingleton();
        public static GetAllSystemPrintersSingleton GetInstance
        {
            get
            {
                return instance;
            }
        }

        private PrintQueueCollection? _printers;
        public PrintQueueCollection Printers { 
            get { 
                if(_printers == null)
                {
                    LocalPrintServer printServer = new LocalPrintServer();
                    _printers = printServer.GetPrintQueues();
                }
                return _printers; 
            } 
        }

        public PrintQueueCollection RefreshPrinters()
        {
            LocalPrintServer printServer = new LocalPrintServer();
            _printers = printServer.GetPrintQueues();
            return _printers;
        }
    }
}
