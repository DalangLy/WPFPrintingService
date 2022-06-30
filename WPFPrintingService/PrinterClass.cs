using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Printing;
using System.Windows.Controls;

namespace WPFPrintingService
{
    internal class PrinterClass
    {
        public void PrintAndCutPaper(string printerName, UserControl printTemplate)
        {
            
        }

        public void KickDrawer(string printerName)
        {
            
        }

        public void CutPaper(string printerName)
        {
            
        }

        public void PrintAndKickCashDrawer(string printerName, UserControl printTemplate)
        {
            //print and kick out cash drawer using default windows print document
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, e) =>
            {
                LocalPrintServer printServer = new LocalPrintServer();
                PrintQueueCollection printQueues = printServer.GetPrintQueues();
                PrintDialog dialog = new PrintDialog();
                dialog.PrintQueue = printQueues.FirstOrDefault(x => x.Name == "Microsoft Print to PDF");
                dialog.PrintVisual(printTemplate, "Print Tempalte");
            };
            worker.RunWorkerCompleted += (s, e) =>
            {
                PrintDocument printDocument = new PrintDocument();
                printDocument.PrinterSettings.PrinterName = printerName;

                //open cash drawer command
                const string ESC1 = "\u001B";
                const string p = "\u0070";
                const string m = "\u0000";
                const string t1 = "\u0025";
                const string t2 = "\u0250";
                const string openTillCommand = ESC1 + p + m + t1 + t2;
                bool _cashDrawerOpened = RawPrinterHelper.SendStringToPrinter(printDocument.PrinterSettings.PrinterName, openTillCommand);
                if (_cashDrawerOpened)
                {
                    Debug.WriteLine("Success");
                    //if (_webSocketServer != null && _webSocketServer.IsListening)
                    //    _webSocketServer.WebSocketServices["/"].Sessions.SendTo("Cash Drawer Opened", clientId);
                }
                printDocument.Dispose();
            };
            worker.RunWorkerAsync();
        }
    }
}
