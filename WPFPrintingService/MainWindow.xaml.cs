﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Globalization;
using MaterialDesignThemes.Wpf;
using System.IO;
using System.Text.Json;

namespace WPFPrintingService
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
       
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //hide windows to system tray
            Hide();
            printingServiceSystemTrayNotifyIcon.Visibility = Visibility.Visible;
            e.Cancel = true;
        }

        private void btnExitPrintingServiceViaSystemTray_Click(object sender, RoutedEventArgs e)
        {
            this._shutdownThisApplication();
        }

        private void _shutdownThisApplication()
        {
            Application.Current.Shutdown();
        }

        private void printingServiceSystemTrayNotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            //hide windows to system tray
            Show();
            printingServiceSystemTrayNotifyIcon.Visibility = Visibility.Collapsed;
        }

        private void ConfirmExitDialogClosing(object sender, DialogClosingEventArgs eventArgs)
        {
            if (eventArgs.Parameter == null) return;
            bool isExit = (bool) eventArgs.Parameter;
            if (isExit)
                _shutdownThisApplication();
        }

        private void btnDownloadJsonTemplate_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Print Template"; // Default file name
            dlg.DefaultExt = ".json"; // Default file extension
            dlg.Filter = "Json File (.json)|*.json"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                string jsonTemplate = "{ 'printTemplateLayout': { 'paddingTop': 0, 'paddingRight': 0, 'paddingBottom': 0, 'paddingLeft': 0, 'rowGap': 5, 'paperWidth': 500, 'paperBackground': 'transparent', 'fontSize': 12, 'fontFamily':'aril', 'foreground': 'black', 'rows': [ { 'row': { 'rowMarginTop': 0, 'rowMarginRight': 0, 'rowMarginBottom': 0, 'rowMarginLeft': 0, 'rowPaddingTop': 0, 'rowPaddingRight': 0, 'rowPaddingBottom': 0, 'rowPaddingLeft': 0, 'rowBorderTop': 0, 'rowBorderRight': 0, 'rowBorderBottom': 0, 'rowBorderLeft': 0, 'rowBackground': 'blue', 'rowHeight': 0, 'columnVerticalAlign': 'stretch', 'columnHorizontalAlign': 'stretch', 'columns': [ { 'column': { 'content': 'FPTP', 'contentType': 'text', 'qrCodeLogo': '', 'bold': true, 'foreground': 'black', 'fontSize': 22, 'fontFamily': 'Aril', 'contentWidth': 0, 'contentHeight': 0, 'contentHorizontalAlign': 'center', 'contentVerticalAlign': 'center', 'columnBackground': 'gray', 'columnHorizontalAlign': 'stretch', 'columnVerticalAlign': 'stretch', 'columnMarginTop': 0, 'columnMarginRight': 0, 'columnMarginBottom': 0, 'columnMarginLeft': 0, 'columnWidth': 0, 'columnHeight': 0, 'columnPaddingTop': 0, 'columnPaddingRight': 0, 'columnPaddingBottom': 0, 'columnPaddingLeft': 0, 'columnBorderTop': 0, 'columnBorderRight': 0, 'columnBorderBottom': 0, 'columnBorderLeft': 0, 'colSpan': 0, 'rowSpan': 0 } } ] } } ] } }";
                PrintTemplateLayoutModel? layoutModel = PrintTemplateLayoutModel.FromJson(jsonTemplate);
                string json = JsonSerializer.Serialize(layoutModel);
                File.WriteAllText(filename, json);

            }
        }
    }

    public sealed class ParametrizedBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = false;

            if (value is bool)
                flag = (bool)value;
            else
            {
                if (value is bool?)
                {
                    bool? flag2 = (bool?)value;
                    flag = (flag2.HasValue && flag2.Value);
                }
            }

            //If false is passed as a converter parameter then reverse the value of input value
            if (parameter != null)
            {
                bool par = true;
                if ((bool.TryParse(parameter.ToString(), out par)) && (!par)) flag = !flag;
            }

            return flag ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility)
                return (Visibility)value == Visibility.Visible;

            return false;
        }

        public ParametrizedBooleanToVisibilityConverter()
        {
        }
    }


    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    public class NotEmptyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return string.IsNullOrWhiteSpace((value ?? "").ToString())
                ? new ValidationResult(false, "Field is required.")
                : ValidationResult.ValidResult;
        }
    }
}
