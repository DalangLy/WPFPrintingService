using System;
using System.Diagnostics;
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

            //LocalPrintServer printServer = new LocalPrintServer();
            //PrintQueueCollection printQueuesOnLocalServer = printServer.GetPrintQueues();
            //PrintDialog printDialog = new PrintDialog();
            //printDialog.PrintQueue = printQueuesOnLocalServer.FirstOrDefault(x => x.Name == printer.PrinterName);
            //printDialog.PrintVisual(new CashDrawerTemplate(
            //    new List<GG>() { 
            //        new GG(){ Title = "KHR"},
            //        new GG(){ Title = "USD"},
            //    },
            //    DateTime.Now.ToShortDateString()
            //    ), "Cash Drawer");
        }
    }
    internal class PrintAndKickOutCashDrawerCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            Debug.WriteLine("Print and Kickout cash drawer");
        }
    }
    internal class CutCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            Debug.WriteLine("Cut");
        }
    }
    internal class KickCashDrawerCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            Debug.WriteLine("Kick Cash Drawer");
        }
    }
}
