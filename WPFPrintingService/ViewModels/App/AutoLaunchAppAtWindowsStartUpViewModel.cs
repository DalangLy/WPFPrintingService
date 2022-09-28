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
                //set app to run on start up
                RunAtStartUp.GetInstance().enable();
            }
            else
            {
                //remove app from runing at start up
                RunAtStartUp.GetInstance().disable();
            }

            saveStatusToApplicationLocalStorage(isChecked);

            updateCheckBoxUI(isChecked);
        }

        private void saveStatusToApplicationLocalStorage(bool isEnableToRunAtStartUp)
        {
            Properties.Settings.Default.LaunchAppAtWindowsStartUp = isEnableToRunAtStartUp;
            Properties.Settings.Default.Save();
        }

        private void updateCheckBoxUI(bool isChecked)
        {
            this._autoLaunchAppAtWindowsStartUpViewModel.IsLaunchAppAtWindowsStartUp = isChecked;
        }
    }

    internal class RunAtStartUp
    {
        private RegistryKey? _key;
        private Assembly _curAssembly;
        private string _registryKeyName = "DX Printing Service";

        private static RunAtStartUp _instance = new RunAtStartUp();


        private RunAtStartUp()
        {
            _key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            _curAssembly = Assembly.GetExecutingAssembly();
        }

        public static RunAtStartUp GetInstance()
        {
            return _instance;
        }

        public RunAtStartUp enable()
        {
            _key!.SetValue(_registryKeyName, _curAssembly.Location);
            return this;
        }

        public RunAtStartUp disable()
        {
            _key!.DeleteValue(_registryKeyName);
            return this;
        }
    }
}
