using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Controls;
using System.Windows.Input;

namespace WPFPrintingService
{
    public partial class ServerInfoForm : UserControl
    {
        private OnPopUpFormClickCallBack _onPopUpFormClickCallBack;
        public ServerInfoForm(OnPopUpFormClickCallBack onPopUpFormClickCallBack)
        {
            InitializeComponent();
            this._onPopUpFormClickCallBack = onPopUpFormClickCallBack;
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this._onPopUpFormClickCallBack(this);
        }

        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            txtIp.Content = $"ws://{GetLocalIPAddress()}:8000";
        }
    }
}
