using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Printing;
using System.Windows.Input;

namespace WPFPrintingService
{
    internal class PrintersViewModel : BaseViewModel
    {
        private List<PrinterFromWindowsSystemModel> _printers = new List<PrinterFromWindowsSystemModel>();

        public List<PrinterFromWindowsSystemModel> Printers
        {
            get { return _printers; }
            set
            {
                _printers = value;
                OnPropertyChanged(nameof(Printers));
            }
        }

        private static PrintersViewModel _instance = new PrintersViewModel();

        private PrintersViewModel() {
            Debug.WriteLine("hahha");
            Printers.Add(new PrinterFromWindowsSystemModel() { Name = "Printer 1" });
            foreach (PrintQueue printer in GetAllSystemPrintersSingleton.GetInstance.Printers)
            {
                //Debug.WriteLine("\tThe shared printer : " + printer.Name);
                //Debug.WriteLine("\tHas Tonner: " + printer.HasToner);
                //Debug.WriteLine("\tIs Busy: " + printer.IsBusy);
                //Debug.WriteLine("\tIs Door Open: " + printer.IsDoorOpened);
                //Debug.WriteLine("\tIs Offline: " + printer.IsOffline);
                //Debug.WriteLine("\tIs Offline: " + printer.IsPrinting);
                Printers.Add(new PrinterFromWindowsSystemModel() { Name = printer.Name });
            }
        }


        public static PrintersViewModel Instance => _instance;

        private ICommand? _refreshPrintersList;
        public ICommand RefreshPrintersListCommand
        {
            get => _refreshPrintersList ?? new RelayCommand(this);
        }
    }

    internal class RefreshPrintersListCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            Debug.WriteLine("Refresh");
        }
    }

    internal class RelayCommand : ICommand
    {
        PrintersViewModel _printerViewModel;
        public RelayCommand(PrintersViewModel printerViewModel)
        {
            this._printerViewModel = printerViewModel;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            Debug.WriteLine("Hello");
            if (_printerViewModel.Printers is null)
                _printerViewModel.Printers = new List<PrinterFromWindowsSystemModel>();

            var temp = _printerViewModel.Printers;
            _printerViewModel.Printers = new List<PrinterFromWindowsSystemModel>();

            temp.Add(new PrinterFromWindowsSystemModel
            {
                Name = "ABC_"
            });
            _printerViewModel.Printers = temp;
        }
    }
}
