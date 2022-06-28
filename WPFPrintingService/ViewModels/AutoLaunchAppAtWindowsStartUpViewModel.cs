using System;
using System.Windows.Input;

namespace WPFPrintingService
{
    internal class AutoLaunchAppAtWindowsStartUpViewModel : BaseViewModel
    {
        public static AutoLaunchAppAtWindowsStartUpViewModel Instance => new AutoLaunchAppAtWindowsStartUpViewModel();

        private bool _isLaunchAppAtWindowsStartUp;

        public bool IsLaunchAppAtWindowsStartUp
        {
            get { return _isLaunchAppAtWindowsStartUp; }
            set { 
                _isLaunchAppAtWindowsStartUp = value;
                OnPropertyChanged();
            }
        }

        public ICommand ToggleAutoLaunchAppCheckBox { get; set; }

        public AutoLaunchAppAtWindowsStartUpViewModel()
        {
            this.ToggleAutoLaunchAppCheckBox = new ToggleAutoLaunchAppAtWindowsStartUpCommand(this);

            this.IsLaunchAppAtWindowsStartUp = Properties.Settings.Default.is_start_server_on_start_up;
        }
    }

    internal class ToggleAutoLaunchAppAtWindowsStartUpCommand : ICommand
    {
        private AutoLaunchAppAtWindowsStartUpViewModel _autoLaunchAppAtWindowsStartUpViewModel;

        public ToggleAutoLaunchAppAtWindowsStartUpCommand(AutoLaunchAppAtWindowsStartUpViewModel autoLaunchAppAtWindowsStartUpViewModel)
        {
            _autoLaunchAppAtWindowsStartUpViewModel = autoLaunchAppAtWindowsStartUpViewModel;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            if (parameter == null) return;
            bool isChecked = (bool)parameter;
            Properties.Settings.Default.is_run_at_start_up = isChecked;
            this._autoLaunchAppAtWindowsStartUpViewModel.IsLaunchAppAtWindowsStartUp = isChecked;
        }
    }
}
