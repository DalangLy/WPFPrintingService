namespace WPFPrintingService.ViewModels
{
    internal class StartServiceViewModel : BaseViewModel
    {
        private bool _isStartOnStartup;

        public bool IsStartOnStartUp
        {
            get { return _isStartOnStartup; }
            set
            {
                _isStartOnStartup = value;
                OnPropertyChanged(nameof(IsStartOnStartUp));
            }
        }
    }
}
