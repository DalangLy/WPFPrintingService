using System;
using System.Windows;
using System.Windows.Controls;
using WPFPrintingService.UICallBackDelegates;

namespace WPFPrintingService
{
    public partial class CustomConfirmDialog : UserControl
    {
        public event VoidCallBack? OnConfirmClickCallBack;
        private string _title;

        public CustomConfirmDialog(string title)
        {
            InitializeComponent();
            this._title = title;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this._closeThisDialog();
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (OnConfirmClickCallBack == null) return;

            OnConfirmClickCallBack(this, EventArgs.Empty);
        }

        private void confirmExitDialogOverlay_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this._closeThisDialog();
        }

        private void _closeThisDialog()
        {
            ((Panel)this.Parent).Children.Remove(this);
        }

        private void confirmExitDialog_Loaded(object sender, RoutedEventArgs e)
        {
            lblTitle.Content = _title;
        }
    }
}
