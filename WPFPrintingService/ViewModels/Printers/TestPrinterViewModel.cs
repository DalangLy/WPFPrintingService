using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Linq;
using System.Printing;
using System.Windows.Controls;
using System.Windows.Input;
using WPFPrintingService.Models;

namespace WPFPrintingService
{
    internal class TestPrinterViewModel : BaseViewModel
    {
        public static TestPrinterViewModel Instance => new TestPrinterViewModel();
        public ICommand PrintAndCutCommand { get; set; }
        public ICommand PrintAndKickOutCashDrawerCommand { get; set; }
        public ICommand CutCommand { get; set; }
        public ICommand KickCashDrawerCommand { get; set; }

        public TestPrinterViewModel()
        {
            this.PrintAndCutCommand = new PrintAndCutCommand();
            this.PrintAndKickOutCashDrawerCommand = new PrintAndKickOutCashDrawerCommand();
            this.CutCommand = new CutCommand();
            this.KickCashDrawerCommand = new KickCashDrawerCommand();
        }
    }

    internal class PrintAndCutCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            if (parameter == null) return;
            PrinterModel printer = (PrinterModel) parameter;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, e) =>
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    LocalPrintServer printServer = new LocalPrintServer();
                    PrintQueueCollection printQueues = printServer.GetPrintQueues();
                    PrintDialog dialog = new PrintDialog();
                    dialog.PrintQueue = printQueues.FirstOrDefault(x => x.Name == printer.Name);
                    //get print instance
                    UserControl printTemplate = new TestPrintTemplate();
                    dialog.PrintVisual(printTemplate, "Test Print Template");
                });
            };
            worker.RunWorkerCompleted += (s, e) =>
            {
                Debug.WriteLine("PrintSuccess");
            };
            worker.RunWorkerAsync();
        }
    }
    internal class PrintAndKickOutCashDrawerCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            if (parameter == null) return;
            PrinterModel printer = (PrinterModel)parameter;

            //print and kick out cash drawer using default windows print document
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, e) =>
            {
                App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
                {
                    LocalPrintServer printServer = new LocalPrintServer();
                    PrintQueueCollection printQueues = printServer.GetPrintQueues();
                    PrintDialog dialog = new PrintDialog();
                    dialog.PrintQueue = printQueues.FirstOrDefault(x => x.Name == printer.Name);
                    UserControl printTemplate = new TestPrintTemplate();
                    dialog.PrintVisual(printTemplate, "Print Tempalte");
                });
            };
            worker.RunWorkerCompleted += (s, e) =>
            {
                PrintDocument printDocument = new PrintDocument();
                printDocument.PrinterSettings.PrinterName = printer.Name;

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
                }
                printDocument.Dispose();
            };
            worker.RunWorkerAsync();
        }
    }
    internal class CutCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            if (parameter == null) return;
            PrinterModel printer = (PrinterModel)parameter;

            PrintDocument printDocument = new PrintDocument();
            printDocument.PrinterSettings.PrinterName = printer.Name;

            //cut command
            string GS = Convert.ToString((char)29);
            string ESC = Convert.ToString((char)27);
            string COMMAND = "";
            COMMAND = ESC + "@";
            COMMAND += GS + "V" + (char)1;
            bool _cutted = RawPrinterHelper.SendStringToPrinter(printDocument.PrinterSettings.PrinterName, COMMAND);
            if (_cutted)
            {
                Debug.WriteLine("Cut Success");
            }
            printDocument.Dispose();
        }
    }
    internal class KickCashDrawerCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            if (parameter == null) return;
            PrinterModel printer = (PrinterModel)parameter;

            PrintDocument printDocument = new PrintDocument();
            printDocument.PrinterSettings.PrinterName = printer.Name;

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
                Debug.WriteLine("Cash Drawer Opened");
            }
            printDocument.Dispose();
        }
    }
}
