using System.Windows;
using System.Windows.Controls;

namespace WPFPrintingService
{
    public partial class CustomMessageDialog : UserControl
    {
        public CustomMessageDialog()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this._closeThisDialog();
        }
        private void _closeThisDialog()
        {
            ((Panel)this.Parent).Children.Remove(this);
        }

        private void customMessageDialogOverlay_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this._closeThisDialog();
        }
    }
}
