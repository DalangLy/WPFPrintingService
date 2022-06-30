using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WPFPrintingService
{
    internal class StartServiceViewModel : BaseViewModel
    {
        private static StartServiceViewModel? _instance;
        public static StartServiceViewModel Instance
        {
            get { return _instance ?? (_instance = new StartServiceViewModel()); }
        }

        private WebSocketServerViewModel _server;

        public StartServiceViewModel()
        {
            this._server = WebSocketServerViewModel.Instance;

            this.StartServiceCommand = new StartServiceCommand(async (p) => await InvokeStartService(p));
        }

        private async Task InvokeStartService(object? p)
        {
            //show progress
            this.StartServiceInProgress = true;
            await Task.Delay(1000);
            this._server.StartService();
            this.StartServiceInProgress = false;
        }

        private bool _startServiceInProgress;

        public bool StartServiceInProgress
        {
            get { return _startServiceInProgress; }
            set { 
                _startServiceInProgress = value;
                OnPropertyChanged();
            }
        }


        public ICommand StartServiceCommand { get; set; }
    }

    internal class StartServiceCommand : ICommand
    {
        private Action<object?> _execute;

        public StartServiceCommand(Action<object?> execute)
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
