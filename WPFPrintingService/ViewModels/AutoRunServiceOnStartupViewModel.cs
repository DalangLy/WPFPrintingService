using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WPFPrintingService
{
    internal class AutoRunServiceOnStartupViewModel : BaseViewModel
    {
        public static AutoRunServiceOnStartupViewModel Instance => new AutoRunServiceOnStartupViewModel();

        private bool _isChecked;

        public bool IsChecked
        {
            get { return _isChecked; }
            set {  }
        }

        public ICommand EnableRunServiceOnStartUp { get; set; }

        public AutoRunServiceOnStartupViewModel()
        {
            this.EnableRunServiceOnStartUp = new EnableRunServiceOnStartUpCommand(async (p) => await InvokeEnableRunServiceOnStartUp(p));
        }

        private async Task InvokeEnableRunServiceOnStartUp(object param)
        {
            bool isChecked = (bool)param;
            if (isChecked)
            {
                await Task.Delay(1000 * 5);
                Debug.WriteLine("Is Check");
            }
            else
            {
                await Task.Delay(1000 * 5);
                Debug.WriteLine("Is Unchecked");
            }

            this._isChecked ^= true;
            OnPropertyChanged(nameof(IsChecked));
        }
    }

    internal class EnableRunServiceOnStartUpCommand : ICommand
    {
        private Action<object> _onStartUp;

        public EnableRunServiceOnStartUpCommand(Action<object> onStartUp)
        {
            _onStartUp = onStartUp;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            if (parameter == null) return;
            _onStartUp.Invoke(parameter);
        }
    }
}
