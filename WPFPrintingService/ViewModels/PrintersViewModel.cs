using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Printing;
using System.Windows.Input;
using WPFPrintingService.Models;

namespace WPFPrintingService
{
    internal class PrintersViewModel : BaseViewModel
    {
        private ObservableCollection<PrinterModel> _printers = new ObservableCollection<PrinterModel>();

        public ObservableCollection<PrinterModel> Printers
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
            foreach (PrintQueue printer in GetAllSystemPrintersSingleton.GetInstance.Printers)
            {
                Printers.Add(new PrinterModel() { 
                    Name = printer.Name, 
                    IsOnline = printer.IsOffline,
                    IsBusy = printer.IsBusy,
                    IsDoorOpened = printer.IsDoorOpened,
                    HasToner = printer.HasToner,
                    IsPrinting = printer.IsPrinting,
                });
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
            _printerViewModel.Printers.Add(new PrinterModel
            {
                Name = "ABC_",
                IsOnline = true
            });
        }
    }
}
