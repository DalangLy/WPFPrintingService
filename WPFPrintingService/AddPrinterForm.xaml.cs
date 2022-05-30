using System.Windows;
using System.Windows.Controls;

namespace WPFPrintingService
{
    public delegate void OnConnectPrinterCallBack(string ip, string port, string name, UserControl thisForm);
    public delegate void OnPopUpFormClickCallBack(UserControl thisForm);
    public partial class AddPrinterForm : UserControl
    {
        private OnConnectPrinterCallBack _onConnectPrinterCallBack;
        private OnPopUpFormClickCallBack _onPopUpFormClickCallBack;
        public AddPrinterForm(OnConnectPrinterCallBack onConnectPrinterCallBack, OnPopUpFormClickCallBack onPopUpFormClickCallBack)
        {
            InitializeComponent();
            this._onConnectPrinterCallBack = onConnectPrinterCallBack;
            this._onPopUpFormClickCallBack = onPopUpFormClickCallBack;
        }

        private void btnConnectPrinter_Click(object sender, RoutedEventArgs e)
        {
            if (txtIp.Text == "" || txtPort.Text == "") return;
            this._onConnectPrinterCallBack(txtIp.Text, txtPort.Text, txtPrinterName.Text, this);
        }

        private void UserControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this._onPopUpFormClickCallBack(this);
        }
    }
}
