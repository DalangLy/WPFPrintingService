using System;
using System.Windows.Input;

namespace WPFPrintingService
{
    internal class ToggleAutoRunServiceOnAppLaunchedViewModel : BaseViewModel
    {
        private static ToggleAutoRunServiceOnAppLaunchedViewModel? _instance;
        public static ToggleAutoRunServiceOnAppLaunchedViewModel Instance
        {
            get => _instance ?? (_instance = new ToggleAutoRunServiceOnAppLaunchedViewModel());
        }

        private WebSocketServerViewModel _WebSocketClientViewModel;

        public ICommand ToggleRunWebSocketServerOnAppLaunched { get; set; }

        private bool _isStartServiceOnAppLauch = false;
        public bool IsStartServiceOnAppLauched
        {
            get { return _isStartServiceOnAppLauch; }
            set
            {
                _isStartServiceOnAppLauch = value;
                OnPropertyChanged(nameof(IsStartServiceOnAppLauched));
            }
        }

        public ToggleAutoRunServiceOnAppLaunchedViewModel()
        {
            this._WebSocketClientViewModel = WebSocketServerViewModel.Instance;
            this.ToggleRunWebSocketServerOnAppLaunched = new ToggleAutoRunServiceOnStartUp();

            this.IsStartServiceOnAppLauched = Properties.Settings.Default.RunServiceOnAppLaunched;
            if (this.IsStartServiceOnAppLauched)
            {
                this._WebSocketClientViewModel.StartService();
            }
        }
    }
    internal class ToggleAutoRunServiceOnStartUp : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            if (parameter == null) return;
            bool isChecked = (bool)parameter;
            Properties.Settings.Default.RunServiceOnAppLaunched = isChecked;
            Properties.Settings.Default.Save();
        }
    }
}
