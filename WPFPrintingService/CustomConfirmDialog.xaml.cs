using System;
using System.Windows;
using System.Windows.Controls;
using WPFPrintingService.UICallBackDelegates;

namespace WPFPrintingService
{
    public partial class CustomConfirmDialog : UserControl
    {
        public event VoidCallBack? OnConfirmClickCallBack;
        public event VoidCallBack? OnOverlayClicked;
        public event VoidCallBack? OnCancelClicked;

        private string _title;

        public CustomConfirmDialog(string title)
        {
            InitializeComponent();
            this._title = title;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this._closeThisDialog();
            if (this.OnCancelClicked != null)
                this.OnCancelClicked(this, EventArgs.Empty);
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (OnConfirmClickCallBack == null)
            {
                this._closeThisDialog();
                return;
            }
            else
            {
                OnConfirmClickCallBack(this, EventArgs.Empty);
            }
        }

        private void confirmExitDialogOverlay_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this._closeThisDialog();
            if(OnOverlayClicked != null)
                OnOverlayClicked(this, EventArgs.Empty);
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
