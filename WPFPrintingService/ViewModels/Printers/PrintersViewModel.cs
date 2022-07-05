using System;
using System.Collections.ObjectModel;
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

        public ObservableCollection<PrinterModel> Printers { get; set; } = new ObservableCollection<PrinterModel>();

        private bool _isRefreshingPrinters;

        public bool IsRefreshingPrinters
        {
            get { return _isRefreshingPrinters; }
            set { 
                _isRefreshingPrinters = value; 
                OnPropertyChanged();
            }
        }


        public PrintersViewModel() {
            //load all printers
            this._loadAllPrinters();

            //setup command
            this.RefreshPrintersListCommand = new RefreshPrintersListCommandClass(async () => await InvokeRefreshPrinterList());
        }

        private void _loadAllPrinters()
        {
            if(Printers.Count > 0) Printers.Clear();

            foreach (PrintQueue printer in GetAllSystemPrintersSingleton.GetInstance.Printers)
            {
                Printers.Add(new PrinterModel()
                {
                    Name = printer.Name,
                    IsOnline = printer.IsOffline,
                    IsBusy = printer.IsBusy,
                    IsDoorOpened = printer.IsDoorOpened,
                    HasToner = printer.HasToner,
                    IsPrinting = printer.IsPrinting,
                });
            }
        }

        private async Task InvokeRefreshPrinterList()
        {
            this.IsRefreshingPrinters = true;
            await Task.Delay(1000);
            this._loadAllPrinters();
            this.IsRefreshingPrinters = false;
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
