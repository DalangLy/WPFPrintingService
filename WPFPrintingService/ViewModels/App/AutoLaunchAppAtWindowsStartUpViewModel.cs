using Microsoft.Win32;
using System;
using System.Reflection;
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

            this.IsLaunchAppAtWindowsStartUp = Properties.Settings.Default.LaunchAppAtWindowsStartUp;
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
            
            if (isChecked)
            {
                //set run on start up
                RegistryKey? key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                Assembly curAssembly = Assembly.GetExecutingAssembly();
                //key.SetValue(curAssembly.GetName().Name, curAssembly.Location);
                key!.SetValue("DX Printing Service", curAssembly.Location);
            }
            else
            {
                //remove run on start up
                RegistryKey? key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                Assembly curAssembly = Assembly.GetExecutingAssembly();
                //key.SetValue(curAssembly.GetName().Name, curAssembly.Location);
                key!.DeleteValue("DX Printing Service");
            }
            Properties.Settings.Default.LaunchAppAtWindowsStartUp = isChecked;
            Properties.Settings.Default.Save();

            //update ui
            this._autoLaunchAppAtWindowsStartUpViewModel.IsLaunchAppAtWindowsStartUp = isChecked;
        }
    }
}
