using System;
using System.Collections.Concurrent;
using System.Management;
using System.Printing;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WPFPrintingService
{
    internal class PrintersViewModel : BaseViewModel
    {
        private static PrintersViewModel? _instance;
        public static PrintersViewModel Instance
        {
            get { return _instance ?? (_instance = new PrintersViewModel()); }
        }

        public ConcurrentBag<PrinterModel> Printers { get; set; } = new ConcurrentBag<PrinterModel>();

        private bool _isShowRefreshingPrintersIndicator;

        public bool IsShowRefreshingPrintersIndicator
        {
            get { return _isShowRefreshingPrintersIndicator; }
            set { 
                _isShowRefreshingPrintersIndicator = value; 
                OnPropertyChanged();
            }
        }


        public PrintersViewModel() {
            this._loadAllPrinters();

            //setup command
            this.RefreshPrintersListCommand = new RefreshPrintersListCommandClass(async () => await InvokeRefreshPrinterList());
        }

        private void _loadAllPrinters()
        {
            if(Printers.Count > 0) Printers.Clear();

            foreach (PrintQueue printer in new LocalPrintServer().GetPrintQueues())
            {
                bool IsOffline = false;
                ManagementObjectSearcher searcher = new
                ManagementObjectSearcher("SELECT * FROM Win32_Printer where Name='" + printer.Name + "'");
                foreach (ManagementObject foundPrinter in searcher.Get())
                {
                    IsOffline = (bool)foundPrinter["WorkOffline"];
                }
                
                Printers.Add(new PrinterModel()
                {
                    Name = printer.Name,
                    IsOnline = !IsOffline,
                    IsBusy = printer.IsBusy,
                    IsDoorOpened = printer.IsDoorOpened,
                    HasToner = printer.HasToner,
                    IsPrinting = printer.IsPrinting,
                });
            }
        }

        private async Task InvokeRefreshPrinterList()
        {
            this.IsShowRefreshingPrintersIndicator = true;

            //reload all printer in other thread to avoid ui loading indicator lagging
            await Task.Run(() =>
            {
                this._loadAllPrinters();
            });
                
            this.IsShowRefreshingPrintersIndicator = false;
        }

        public ICommand RefreshPrintersListCommand { get; set; }
    }

    internal class RefreshPrintersListCommandClass : ICommand
    {
        private Action _onRefreshingCommandExecuted;

        public RefreshPrintersListCommandClass(Action OnRefreshingCommandExecuted)
        {
            this._onRefreshingCommandExecuted = OnRefreshingCommandExecuted;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            this._onRefreshingCommandExecuted.Invoke();
        }
    }
}
