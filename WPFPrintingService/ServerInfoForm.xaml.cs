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
    }
}
