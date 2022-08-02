using System;
using System.Windows;
using System.Windows.Data;
using System.Globalization;
using MaterialDesignThemes.Wpf;
using System.IO;

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
                string jsonTemplate = "{ \"requestType\": \"print\", \"printMeta\": { \"printerName\": \"Microsoft Print to PDF\", \"printMethod\": \"printandcut\", \"printTemplateLayout\": { \"PrintTemplateLayout\": { \"PaddingTop\": 0, \"PaddingRight\": 0, \"PaddingBottom\": 0, \"PaddingLeft\": 0, \"RowGap\": 5, \"PaperWidth\": 500, \"PaperBackground\": \"transparent\", \"FontSize\": 12, \"FontFamily\": \"aril\", \"Foreground\": \"black\", \"Rows\": [ { \"Row\": { \"RowMarginTop\": 0, \"RowMarginRight\": 0, \"RowMarginBottom\": 0, \"RowMarginLeft\": 0, \"RowPaddingTop\": 0, \"RowPaddingRight\": 0, \"RowPaddingBottom\": 0, \"RowPaddingLeft\": 0, \"RowBorderTop\": 0, \"RowBorderRight\": 0, \"RowBorderBottom\": 0, \"RowBorderLeft\": 0, \"RowBackground\": \"blue\", \"RowHeight\": 0, \"ColumnVerticalAlign\": \"stretch\", \"ColumnHorizontalAlign\": \"stretch\", \"Columns\": [ { \"Column\": { \"Content\": \"FPTP\", \"ContentType\": \"text\", \"QrCodeLogo\": \"\", \"Bold\": true, \"Foreground\": \"black\", \"FontSize\": 22, \"FontFamily\": \"Aril\", \"ContentWidth\": 0, \"ContentHeight\": 0, \"ContentHorizontalAlign\": \"center\", \"ContentVerticalAlign\": \"center\", \"ColumnBackground\": \"gray\", \"ColumnHorizontalAlign\": \"stretch\", \"ColumnVerticalAlign\": \"stretch\", \"ColumnMarginTop\": 0, \"ColumnMarginRight\": 0, \"ColumnMarginBottom\": 0, \"ColumnMarginLeft\": 0, \"ColumnWidth\": 0, \"ColumnHeight\": 0, \"ColumnPaddingTop\": 0, \"ColumnPaddingRight\": 0, \"ColumnPaddingBottom\": 0, \"ColumnPaddingLeft\": 0, \"ColumnBorderTop\": 0, \"ColumnBorderRight\": 0, \"ColumnBorderBottom\": 0, \"ColumnBorderLeft\": 0, \"ColSpan\": 0, \"RowSpan\": 0 } } ] } } ] } } } }";
                //PrintTemplateLayoutModel? layoutModel = PrintTemplateLayoutModel.FromJson(jsonTemplate);
                //string json = JsonSerializer.Serialize(layoutModel);
                File.WriteAllText(filename, jsonTemplate);
            }



            //string jsonTemplate = "{ 'printTemplateLayout': { 'paddingTop': 0, 'paddingRight': 0, 'paddingBottom': 0, 'paddingLeft': 0, 'rowGap': 5, 'paperWidth': 500, 'paperBackground': 'transparent', 'fontSize': 12, 'fontFamily':'aril', 'foreground': 'black', 'rows': [ { 'row': { 'rowMarginTop': 0, 'rowMarginRight': 0, 'rowMarginBottom': 0, 'rowMarginLeft': 0, 'rowPaddingTop': 0, 'rowPaddingRight': 0, 'rowPaddingBottom': 0, 'rowPaddingLeft': 0, 'rowBorderTop': 0, 'rowBorderRight': 0, 'rowBorderBottom': 0, 'rowBorderLeft': 0, 'rowBackground': 'blue', 'rowHeight': 0, 'columnVerticalAlign': 'stretch', 'columnHorizontalAlign': 'stretch', 'columns': [ { 'column': { 'content': 'FPTP', 'contentType': 'text', 'qrCodeLogo': '', 'bold': true, 'foreground': 'black', 'fontSize': 22, 'fontFamily': 'Aril', 'contentWidth': 0, 'contentHeight': 0, 'contentHorizontalAlign': 'center', 'contentVerticalAlign': 'center', 'columnBackground': 'gray', 'columnHorizontalAlign': 'stretch', 'columnVerticalAlign': 'stretch', 'columnMarginTop': 0, 'columnMarginRight': 0, 'columnMarginBottom': 0, 'columnMarginLeft': 0, 'columnWidth': 0, 'columnHeight': 0, 'columnPaddingTop': 0, 'columnPaddingRight': 0, 'columnPaddingBottom': 0, 'columnPaddingLeft': 0, 'columnBorderTop': 0, 'columnBorderRight': 0, 'columnBorderBottom': 0, 'columnBorderLeft': 0, 'colSpan': 0, 'rowSpan': 0 } } ] } } ] } }";
            //PrintTemplateLayoutModel? layoutModel = PrintTemplateLayoutModel.FromJson(jsonTemplate);
            //string SerializedJsonResult = JsonConvert.SerializeObject(layoutModel);
            //string jsonpath = "C:\\Users\\dalan\\Desktop\\present.json";
            //if (File.Exists(jsonpath))
            //{
            //    File.Delete(jsonpath);
            //    using (var st = new StreamWriter(jsonpath, true))
            //    {
            //        st.WriteLine(SerializedJsonResult.ToString());
            //        st.Close();
            //    }
            //}
        }

        private void btnOnOfflineExit_Click(object sender, RoutedEventArgs e)
        {
            _shutdownThisApplication();
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
}
