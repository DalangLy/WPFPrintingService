using System;
using System.Windows.Controls;
using System.Windows.Input;
using WPFPrintingService.UICallBackDelegates;

namespace WPFPrintingService
{
    public partial class ServerInfoForm : UserControl
    {
        private string _serviceIP;
        private int _servicePort;
        public event VoidCallBack? OnDialogClosed;
        public ServerInfoForm(string serviceIp, int servicePort)
        {
            InitializeComponent();
            this._serviceIP = serviceIp;
            this._servicePort = servicePort;
        }

        private void serverInfoDialogOverlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this._closeThisDialog();
            if(OnDialogClosed != null)
                OnDialogClosed(this, EventArgs.Empty);
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            txtIp.Content = $"ws://{this._serviceIP}:{this._servicePort}";
        }

        private void _closeThisDialog()
        {
            ((Panel)this.Parent).Children.Remove(this);
        }
    }
}
