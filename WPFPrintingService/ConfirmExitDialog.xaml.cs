using System;
using System.Windows;
using System.Windows.Controls;
using WPFPrintingService.UICallBackDelegates;

namespace WPFPrintingService
{
    public partial class ConfirmExitDialog : UserControl
    {
        public event VoidCallBack? OnConfirmExitClickCallBack;

        public ConfirmExitDialog()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this._closeThisDialog();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            if (OnConfirmExitClickCallBack == null) return;

            OnConfirmExitClickCallBack(this, EventArgs.Empty);
        }

        private void confirmExitDialogOverlay_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this._closeThisDialog();
        }

        private void _closeThisDialog()
        {
            ((Panel)this.Parent).Children.Remove(this);
        }
    }
}
