using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Reflection;
using System.Windows;
using WebSocketSharp;
using WebSocketSharp.Server;
using WPFPrintingService.UICallBackDelegates;
using System.Windows.Controls;
using System.Printing;
using System.Linq;
using WPFPrintingService.Print_Templates;
using System.Diagnostics;
using System.ComponentModel;
using WPFPrintingService.Print_Models;
using System.Windows.Data;
using System.Globalization;

namespace WPFPrintingService
{
    public partial class MainWindow : Window
    {
        private WebSocketServer? _webSocketServer;
        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void _onClientResponseMessage(string clientId, string clientName, string message, OnPrintResponse onPrintResponse, OnSendToServer onSendToServer, OnSendToEveryone onSendToEveryone)
        {
            try
            {
                PrintTemplateModel printTemplateModel = PrintTemplateModel.FromJson(message);
                if (printTemplateModel == null)
                {
                    onPrintResponse(this, EventArgs.Empty, "Wrong Format");
                    return;
                }
                switch (printTemplateModel.Code)
                {
                    case "RequestPrinters":
                        if(_webSocketServer != null && _webSocketServer.IsListening)
                        {
                            //var json = System.Text.Json.JsonSerializer.Serialize(_allPrintersFromWindowsSystem);
                            //_webSocketServer.WebSocketServices["/"].Sessions.SendTo(json, clientId);
                        }
                        break;
                    case "SendToEveryone":
                        onSendToEveryone(this, EventArgs.Empty, $"{clientName} Said : {printTemplateModel.Data}");
                        break;
                    case "SendToServer":

                        //update text status
                        //this._webSocketStatusViewModel.Status += $"\n {clientName} Said : {printTemplateModel.Data}";


                        onSendToServer(this, EventArgs.Empty);
                        break;
                    case "Print":
                        try
                        {
                            if (printTemplateModel?.Data == null)
                            {
                                onPrintResponse(this, EventArgs.Empty, "Wrong Data Format");
                                return;
                            }

                            //find printer
                            //PrinterFromWindowsSystemModel? _foundPrinterModel = _allPrintersFromWindowsSystem.Find(printerModel => printerModel.PrinterName.Equals(printTemplateModel?.Data?.PrinterName));
                            //if (_foundPrinterModel == null)
                            //{
                            //    onPrintResponse(this, EventArgs.Empty, "Can't Find Printer");
                            //    return;
                            //}

                            switch (printTemplateModel?.Data?.PrintMethod)
                            {
                                case "CutOnly":
                                    this._cutOnly(printTemplateModel?.Data.PrinterName, clientId);
                                    break;
                                case "OpenCashDrawer":
                                    this._kickCashDrawer(printTemplateModel?.Data.PrinterName, clientId);
                                    break;
                                case "PrintAndKickCashDrawer":
                                    this._printAndKickCashDrawer(printTemplateModel?.Data.PrinterName, printTemplateModel?.Data?.PrintModel, clientId);
                                    break;
                                default:
                                    this._printAndCut(printTemplateModel?.Data.PrinterName, printTemplateModel?.Data?.PrintModel, clientId);
                                    break;
                            }
                        }
                        catch (Exception)
                        {

                            throw;
                        }

                        break;
                    default:
                        onPrintResponse(this, EventArgs.Empty, "Wrong Code");
                        break;
                }
            }
            catch (Exception ex)
            {
                onPrintResponse(this, EventArgs.Empty, $"Wrong Format : {ex.Message}");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //hide windows to system tray
            Hide();
            myNotifyIcon.Visibility = Visibility.Visible;
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

        private void myNotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            //hide windows to system tray
            Show();
            myNotifyIcon.Visibility = Visibility.Collapsed;
        }

        private void chbRunOnStartUp_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.is_run_at_start_up = true;
            Properties.Settings.Default.Save();

            //set run on start up
            RegistryKey? key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            Assembly curAssembly = Assembly.GetExecutingAssembly();
            //key.SetValue(curAssembly.GetName().Name, curAssembly.Location);
            key!.SetValue("DX Printing Service", curAssembly.Location);
        }

        private void chbRunOnStartUp_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.is_run_at_start_up = false;
            Properties.Settings.Default.Save();

            //remove run on start up
            RegistryKey? key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            Assembly curAssembly = Assembly.GetExecutingAssembly();
            //key.SetValue(curAssembly.GetName().Name, curAssembly.Location);
            key!.DeleteValue("DX Printing Service");
        }

        private void btnSendToEveryClients_Click(object sender, RoutedEventArgs e)
        {
            if (this._webSocketServer == null) return;
            if (!this._webSocketServer.IsListening)
            {
                MessageBox.Show("Start Service First");
                return;
            }
            //if(this._allConnectedWebSocketClients.Count < 1)
            //{
            //    MessageBox.Show("No Client Connected");
            //    return;
            //}


            this._webSocketServer.WebSocketServices["/"].Sessions.Broadcast(txtMessage.Text);
            
            //this.mainGrid.Children.Add(new CustomMessageDialog());
        }

        private void btnRefreshPrinterList_Click(object sender, RoutedEventArgs e)
        {
            GetAllSystemPrintersSingleton.GetInstance.RefreshPrinters();
        }

        private void chbAutoRunService_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.is_start_server_on_start_up = true;
            Properties.Settings.Default.Save();
        }

        private void chbAutoRunService_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.is_start_server_on_start_up = true;
            Properties.Settings.Default.Save();
        }

        private void btnPrintAndCut_Click(object sender, RoutedEventArgs e)
        {
            PrinterFromWindowsSystemModel? printer = ((FrameworkElement)sender).DataContext as PrinterFromWindowsSystemModel;
            if (printer == null) return;



            //LocalPrintServer printServer = new LocalPrintServer();
            //PrintQueueCollection printQueuesOnLocalServer = printServer.GetPrintQueues();
            //PrintDialog printDialog = new PrintDialog();
            //printDialog.PrintQueue = printQueuesOnLocalServer.FirstOrDefault(x => x.Name == printer.PrinterName);
            //printDialog.PrintVisual(new CashDrawerTemplate(
            //    new List<GG>() { 
            //        new GG(){ Title = "KHR"},
            //        new GG(){ Title = "USD"},
            //    },
            //    DateTime.Now.ToShortDateString()
            //    ), "Cash Drawer");
        }

        private void btnPrintAndKickDrawer_Click(object sender, RoutedEventArgs e)
        {
            PrinterFromWindowsSystemModel? printer = ((FrameworkElement)sender).DataContext as PrinterFromWindowsSystemModel;
            if (printer == null) return;

            PrintDocument printDocument = new PrintDocument();
            printDocument.PrintPage += (o, ev) =>
            {
                if (ev.Graphics == null) return;
                ev.Graphics.DrawString(
                    "Print Test",
                    new Font("Arial", 10),
                    Brushes.Black,
                    ev.MarginBounds.Left,
                    0,
                    new StringFormat()
                );
            };
            printDocument.PrinterSettings.PrinterName = printer.Name;
            printDocument.EndPrint += (o, ev) =>
            {
                //open cash drawer command
                const string ESC1 = "\u001B";
                const string p = "\u0070";
                const string m = "\u0000";
                const string t1 = "\u0025";
                const string t2 = "\u0250";
                const string openTillCommand = ESC1 + p + m + t1 + t2;
                RawPrinterHelper.SendStringToPrinter(printDocument.PrinterSettings.PrinterName, openTillCommand);

                MessageBox.Show("Print Success");
            };
            printDocument.Print();
            printDocument.Dispose();
        }

        private void btnPrintOnly_Click(object sender, RoutedEventArgs e)
        {
            //PrinterFromWindowsSystemModel? printer = ((FrameworkElement)sender).DataContext as PrinterFromWindowsSystemModel;
            //if (printer == null) return;

            MessageBox.Show("Sorry this feature is in progress");
        }

        private void btnCutOnly_Click(object sender, RoutedEventArgs e)
        {
            PrinterFromWindowsSystemModel? printer = ((FrameworkElement)sender).DataContext as PrinterFromWindowsSystemModel;
            if (printer == null) return;

            PrintDocument printDocument = new PrintDocument();
            printDocument.PrinterSettings.PrinterName = printer.Name;

            //cut command
            string GS = Convert.ToString((char)29);
            string ESC = Convert.ToString((char)27);
            string COMMAND = "";
            COMMAND = ESC + "@";
            COMMAND += GS + "V" + (char)1;
            bool _cutted = RawPrinterHelper.SendStringToPrinter(printDocument.PrinterSettings.PrinterName, COMMAND);
            if (_cutted)
            {
                MessageBox.Show("Cut Success");
            }
            printDocument.Dispose();
        }

        private void btnKickDrawer_Click(object sender, RoutedEventArgs e)
        {
            PrinterFromWindowsSystemModel? printer = ((FrameworkElement)sender).DataContext as PrinterFromWindowsSystemModel;
            if (printer == null) return;

            PrintDocument printDocument = new PrintDocument();
            printDocument.PrinterSettings.PrinterName = printer.Name;

            //open cash drawer command
            const string ESC1 = "\u001B";
            const string p = "\u0070";
            const string m = "\u0000";
            const string t1 = "\u0025";
            const string t2 = "\u0250";
            const string openTillCommand = ESC1 + p + m + t1 + t2;
            bool _cashDrawerOpened = RawPrinterHelper.SendStringToPrinter(printDocument.PrinterSettings.PrinterName, openTillCommand);
            if (_cashDrawerOpened)
            {
                MessageBox.Show("Cash Drawer Opened");
            }
            printDocument.Dispose();
        }

        //default windows print functions
        private void _printAndCut(string printerName, PrintModel printModel, string clientId)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    LocalPrintServer printServer = new LocalPrintServer();
                    PrintQueueCollection printQueues = printServer.GetPrintQueues();
                    CashDrawerTemplate c = new CashDrawerTemplate(
                         printModel.Prices.ToList(),
                         printModel.Date
                    );
                    PrintDialog dialog = new PrintDialog();
                    dialog.PrintQueue = printQueues.FirstOrDefault(x => x.Name == "Microsoft Print to PDF");
                    dialog.PrintVisual(c, "Cash Drawer");
                }));
            };
            worker.RunWorkerCompleted += (s, e) =>
            {
                Debug.WriteLine("PrintSuccess");
            };
            worker.RunWorkerAsync();
        }

        private void _printAndKickCashDrawer(string printerName, PrintModel printModel, string clientId)
        {
            //print and kick out cash drawer using default windows print document
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    LocalPrintServer printServer = new LocalPrintServer();
                    PrintQueueCollection printQueues = printServer.GetPrintQueues();
                    CashDrawerTemplate c = new CashDrawerTemplate(
                         printModel.Prices.ToList(),
                         printModel.Date
                    );
                    PrintDialog dialog = new PrintDialog();
                    dialog.PrintQueue = printQueues.FirstOrDefault(x => x.Name == "Microsoft Print to PDF");
                    dialog.PrintVisual(c, "Cash Drawer");
                }));
            };
            worker.RunWorkerCompleted += (s, e) =>
            {
                PrintDocument printDocument = new PrintDocument();
                printDocument.PrinterSettings.PrinterName = printerName;

                //open cash drawer command
                const string ESC1 = "\u001B";
                const string p = "\u0070";
                const string m = "\u0000";
                const string t1 = "\u0025";
                const string t2 = "\u0250";
                const string openTillCommand = ESC1 + p + m + t1 + t2;
                bool _cashDrawerOpened = RawPrinterHelper.SendStringToPrinter(printDocument.PrinterSettings.PrinterName, openTillCommand);
                if (_cashDrawerOpened)
                {
                    if (_webSocketServer != null && _webSocketServer.IsListening)
                        _webSocketServer.WebSocketServices["/"].Sessions.SendTo("Cash Drawer Opened", clientId);
                }
                printDocument.Dispose();
            };
            worker.RunWorkerAsync();
        }

        private void _cutOnly(string printerName, string clientId)
        {
            //cut only using default windows print document

            PrintDocument printDocument = new PrintDocument();
            printDocument.PrinterSettings.PrinterName = printerName;

            //cut command
            string GS = Convert.ToString((char)29);
            string ESC = Convert.ToString((char)27);
            string COMMAND = "";
            COMMAND = ESC + "@";
            COMMAND += GS + "V" + (char)1;
            bool _cutted = RawPrinterHelper.SendStringToPrinter(printDocument.PrinterSettings.PrinterName, COMMAND);
            if (_cutted)
            {
                if (_webSocketServer != null && _webSocketServer.IsListening)
                    _webSocketServer.WebSocketServices["/"].Sessions.SendTo("Cut Success", clientId);
            }
            printDocument.Dispose();
        }

        private void _kickCashDrawer(string printerName, string clientId)
        {
            //kick out cash drawer using default windows print document

            PrintDocument printDocument = new PrintDocument();
            printDocument.PrinterSettings.PrinterName = printerName;

            //open cash drawer command
            const string ESC1 = "\u001B";
            const string p = "\u0070";
            const string m = "\u0000";
            const string t1 = "\u0025";
            const string t2 = "\u0250";
            const string openTillCommand = ESC1 + p + m + t1 + t2;
            bool _cashDrawerOpened = RawPrinterHelper.SendStringToPrinter(printDocument.PrinterSettings.PrinterName, openTillCommand);
            if (_cashDrawerOpened)
            {
                if (_webSocketServer != null && _webSocketServer.IsListening)
                    _webSocketServer.WebSocketServices["/"].Sessions.SendTo("Cash Drawer Opened", clientId);
            }
            printDocument.Dispose();
        }

        private void DialogHost_DialogClosing(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {
            if (eventArgs.Parameter == null) return;
            bool isExit = (bool) eventArgs.Parameter;
            if (isExit)
                _shutdownThisApplication();
        }

        private void ConfirmStopServiceDialogClosing(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {
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

    public class AgeRangeRule : ValidationRule
    {
        public int Min { get; set; }
        public int Max { get; set; }

        public AgeRangeRule()
        {
        }

        public override System.Windows.Controls.ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int age = 0;

            try
            {
                if (((string)value).Length > 0)
                    age = Int32.Parse((String)value);
            }
            catch (Exception e)
            {
                return new System.Windows.Controls.ValidationResult(false, $"Illegal characters or {e.Message}");
            }

            if ((age < Min) || (age > Max))
            {
                return new System.Windows.Controls.ValidationResult(false,
                  $"Please enter an age in the range: {Min}-{Max}.");
            }
            return System.Windows.Controls.ValidationResult.ValidResult;
        }
    }
}
