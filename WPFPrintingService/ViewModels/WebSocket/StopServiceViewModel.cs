using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WPFPrintingService
{
    internal class StopServiceViewModel : BaseViewModel
    {
        private static StopServiceViewModel? _instance;
        public static StopServiceViewModel Instance
        {
            get { return _instance ?? (_instance = new StopServiceViewModel()); }
        }

        public ICommand StopServiceCommand { get; set; }
        public ICommand ConfirmStopServiceCommand { get; set; }

        private bool _isConfirmStopServiceShowUp;

        public bool IsConfirmStopServiceShowUp
        {
            get { return _isConfirmStopServiceShowUp; }
            set
            {
                _isConfirmStopServiceShowUp = value;
                OnPropertyChanged();
            }
        }

        private bool _stopServiceInProgress;

        public bool StopServiceInProgress
        {
            get { return _stopServiceInProgress; }
            set { 
                _stopServiceInProgress = value;
                OnPropertyChanged();
            }
        }


        private WebSocketServerViewModel _server;

        public StopServiceViewModel()
        {
            _server = WebSocketServerViewModel.Instance;
            this.StopServiceCommand = new StopServiceCommand(async (p) => await InvokeStopService(p));
            this.ConfirmStopServiceCommand = new ConfirmStopServiceCommand(() => InvokeConfirmStopService());
        }

        private void InvokeConfirmStopService()
        {
            this.IsConfirmStopServiceShowUp = true;
        }

        private async Task InvokeStopService(object? p)
        {
            this.StopServiceInProgress = true;
            await Task.Delay(1000);

            this._server.stopService();
            this.IsConfirmStopServiceShowUp = false;
            this.StopServiceInProgress = false;
        }
    }

    internal class ConfirmStopServiceCommand : ICommand
    {
        private Action _execute;

        public ConfirmStopServiceCommand(Action execute)
        {
            _execute = execute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            this._execute();
        }
    }

    internal class StopServiceCommand : ICommand
    {
        private Action<object?> _execute;

        public StopServiceCommand(Action<object?> execute)
        {
            _execute = execute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            this._execute(parameter);
        }
    }
}
