using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using WebSocketSharp;
using WebSocketSharp.Server;
using WPFPrintingService.ImageConversion;
using WPFPrintingService.UICallBackDelegates;
using Newtonsoft.Json;
using System.Windows.Controls;
using System.Diagnostics;

namespace WPFPrintingService
{
    public partial class MainWindow : Window
    {
        private List<ClientWebSocketModel> _allConnectedWebSocketClients = new List<ClientWebSocketModel>();
        private WebSocketServer? _webSocketServer;
        private const int PORT = 1100;
        private List<PrinterFromWindowsSystemModel> _allPrintersFromWindowsSystem = new List<PrinterFromWindowsSystemModel>();
        private bool _isWebSocketSeverRunning = false;
        private bool _isDialogShow = false;

        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //initialize websocket server
            this._setupWebSocketServer();

            //load all printer from windows system
            this._loadAllPrintersFromWindowsSystem();

            //bind list of all connected websocket clients to list view
            this.lvConnectWebSocketClients.ItemsSource = _allConnectedWebSocketClients;

            //check if run on start up button is enable
            this._checkIfRunAtStartUpEnable();

            //check if start service on start up
            this._checkIfStartServiceAtStartUpIsEnable();
        }

        private void _setupWebSocketServer()
        {
            _webSocketServer = new WebSocketServer(IPAddress.Parse(_getLocalIPAddress()), PORT);

            //// Not to remove the inactive sessions periodically.
            //_webSocketServer.KeepClean = true;
        }

        private void _loadAllPrintersFromWindowsSystem()
        {
            ManagementObjectSearcher printerQuery = new ManagementObjectSearcher("SELECT * from Win32_Printer");
            foreach (ManagementBaseObject printer in printerQuery.Get())
            {
                var name = printer.GetPropertyValue("Name");
                var status = printer.GetPropertyValue("Status");
                var isDefault = printer.GetPropertyValue("Default");
                var isNetworkPrinter = printer.GetPropertyValue("Network");

                //Debug.WriteLine("{0} (Status: {1}, Default: {2}, Network: {3}", name, status, isDefault, isNetworkPrinter);
                _allPrintersFromWindowsSystem.Add(new PrinterFromWindowsSystemModel(name.ToString()));
            }

            dgPrinters.ItemsSource = _allPrintersFromWindowsSystem;
        }

        private void _checkIfRunAtStartUpEnable()
        {
            bool _isRunAtStartUp = Properties.Settings.Default.is_run_at_start_up;
            chbRunOnStartUp.IsChecked = _isRunAtStartUp;
        }

        private void _checkIfStartServiceAtStartUpIsEnable()
        {
            bool _isStartServiceAtStartUp = Properties.Settings.Default.is_start_server_on_start_up;
            chbAutoRunService.IsChecked = _isStartServiceAtStartUp;
            if (_isStartServiceAtStartUp)
            {
                _startWebSocketServer();
            }
        }

        private string _getLocalIPAddress()
        {
            string localIP = string.Empty;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint? endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint!.Address.ToString();
            }
            //Console.WriteLine("IP Address = " + localIP);
            return localIP;
        }

        
        private void btnStartStopServer_Click(object sender, RoutedEventArgs e)
        {
            if (_isWebSocketSeverRunning)
            {
                CustomConfirmDialog customConfirmDialog = new CustomConfirmDialog("Stop The Service?");
                customConfirmDialog.OnConfirmClickCallBack += (sender, e) =>
                {
                    _stopWebSocketServer();
                    mainGrid.Children.Remove((UserControl)sender);
                };
                mainGrid.Children.Add(customConfirmDialog);
            }
            else
            {
                _startWebSocketServer();
            }
        }

        private void _startWebSocketServer()
        {
            if (_webSocketServer == null) return;

            //add web socket server listeners
            _webSocketServer.AddWebSocketService<WebSocketServerListener>("/", () => new WebSocketServerListener((sender, args, connectedClientId, connectedClientIp, connectedClientName) =>
            {
                //on open or on client connected
                _addConnectedWebSocketClientToListView(connectedClientId, connectedClientIp, connectedClientName);

            },
                (sender, args, clientId, clientName, message, onPrintResponse, onSendToServer, onSendToEveryone) =>
                {
                    //onMessageCallBack
                    _onClientResponseMessage(clientId, clientName, message, onPrintResponse, onSendToServer, onSendToEveryone);

                },
                (sender, args, disconnectedClientId) =>
                {
                    //on close or on client disconnected
                    _removeDisconnectedWebSocketClientFromListView(disconnectedClientId);
                }
            ));

            _webSocketServer.Start();

            btnStartStopServer.Content = "Stop";
            txtServerStatus.Text = $"Service on ws://{_getLocalIPAddress()}:{PORT}";
            _isWebSocketSeverRunning = true;
        }

        private void _stopWebSocketServer()
        {
            if (_webSocketServer == null) return;

            _webSocketServer.RemoveWebSocketService("/");

            _webSocketServer.Stop(CloseStatusCode.Away, "Server Stop");

            btnStartStopServer.Content = "Start";
            txtServerStatus.Text = "Server Stopped";
            _isWebSocketSeverRunning = false;
        }

        private void _addConnectedWebSocketClientToListView(string id, string ip, string name)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _allConnectedWebSocketClients.Add(new ClientWebSocketModel(id, ip, name));
                lvConnectWebSocketClients.ItemsSource = _allConnectedWebSocketClients;
                lvConnectWebSocketClients.Items.Refresh();

                //update text status
                txtServerStatus.Text += $"\n{name} has joined";
            }), DispatcherPriority.Background);
        }

        private void _removeDisconnectedWebSocketClientFromListView(string disconnectedClientId)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                //find disconnected client from list
                ClientWebSocketModel? _disconnectedWebSocketClient = _allConnectedWebSocketClients.Find(webSocketClient => webSocketClient.Id == disconnectedClientId);
                if (_disconnectedWebSocketClient == null) return;

                //remove disconnected client from list and listview
                _allConnectedWebSocketClients.Remove(_disconnectedWebSocketClient);
                lvConnectWebSocketClients.ItemsSource = _allConnectedWebSocketClients;
                lvConnectWebSocketClients.Items.Refresh();

                //update text status
                txtServerStatus.Text += $"\n{_disconnectedWebSocketClient.Name} has Left";
            }), DispatcherPriority.Background);
        }

        private void _onClientResponseMessage(string clientId, string clientName, string message, OnPrintResponse onPrintResponse, OnSendToServer onSendToServer, OnSendToEveryone onSendToEveryone)
        {
            try
            {
                RequestModel? requestModel = JsonConvert.DeserializeObject<RequestModel>(message);
                if (requestModel == null)
                {
                    onPrintResponse(this, EventArgs.Empty, "Wrong Format");
                    return;
                }
                switch (requestModel.Code)
                {
                    case "RequestPrinters":
                        if(_webSocketServer != null && _webSocketServer.IsListening)
                        {
                            var json = System.Text.Json.JsonSerializer.Serialize(_allPrintersFromWindowsSystem);
                            _webSocketServer.WebSocketServices["/"].Sessions.SendTo(json, clientId);
                        }
                        break;
                    case "SendToEveryOne":
                        onSendToEveryone(this, EventArgs.Empty, $"{clientName} Said : {requestModel.Data}");
                        break;
                    case "SendToServer":

                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            //update text status
                            txtServerStatus.Text += $"\n {clientName} Said : {requestModel.Data}";

                        }), DispatcherPriority.Background);

                        onSendToServer(this, EventArgs.Empty);
                        break;
                    case "Print":

                        //serialize print data model
                        try
                        {
                            PrintDataModel? printDataModel = JsonConvert.DeserializeObject<PrintDataModel>(requestModel.Data);
                            if (printDataModel == null)
                            {
                                onPrintResponse(this, EventArgs.Empty, "Wrong Data Format");
                                return;
                            }
                            //find printer

                            PrinterFromWindowsSystemModel? _foundPrinterModel = _allPrintersFromWindowsSystem.Find(printerModel => printerModel.PrinterName.Equals(printDataModel.PrinterName));
                            if (_foundPrinterModel == null)
                            {
                                onPrintResponse(this, EventArgs.Empty, "Can't Find Printer");
                                return;
                            }

                            //if (!_foundPrinterModel.IsOnline)
                            //{
                            //    onPrintResponse("Select Printer is currently Offline");
                            //    return;
                            //}

                            if (printDataModel.Base64Image == "")
                            {
                                onPrintResponse(this, EventArgs.Empty, "Base64 Image Null");
                                return;
                            }

                            //validate base64 image
                            IAttachmentType attachmentType = GetMimeType(printDataModel.Base64Image);
                            if (attachmentType != AttachmentType.Photo)
                            {
                                return;
                            }

                            string _path = AppDomain.CurrentDomain.BaseDirectory;
                            String savedImage = _path + "\\temp_print." + attachmentType.Extension;
                            File.WriteAllBytes(savedImage, Convert.FromBase64String(printDataModel.Base64Image));

                            switch (printDataModel.PrintMethod)
                            {
                                case "PrintOnly":
                                    //print test
                                    
                                    break;
                                case "CutOnly":
                                    //print test
                                    
                                    break;
                                case "OpenCashDrawer":
                                    //print test
                                    
                                    break;
                                default:
                                    //print test
                                    _printAndCut(printDataModel.PrinterName);
                                    break;
                            }

                            onPrintResponse(this, EventArgs.Empty, "Print Success!");
                        }
                        catch (Exception ex)
                        {
                            onPrintResponse(this, EventArgs.Empty, $"Wrong Print Format {ex.Message}");
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

        
        private void btnServerInfo_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(_webSocketServer.IsListening);
            if (this._isDialogShow) return;
            ServerInfoForm serverInfoForm = new ServerInfoForm(_getLocalIPAddress(), PORT);
            serverInfoForm.OnDialogClosed += (s, e) =>
            {
                this._isDialogShow = false;
            };
            mainGrid.Children.Add(serverInfoForm);
            this._isDialogShow = true;
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
            _shutdownThisApplication();
        }

        private void btnQuitApplication_Click(object sender, RoutedEventArgs e)
        {
            if(this._isDialogShow) return;
            CustomConfirmDialog confirmExitDialog = new CustomConfirmDialog("Exit?");
            confirmExitDialog.OnConfirmClickCallBack += (s, ev) =>
            {
                _shutdownThisApplication();
            };
            confirmExitDialog.OnDialogClosed += (s, e) =>
            {
                this._isDialogShow = false;
            };
            mainGrid.Children.Add(confirmExitDialog);
            this._isDialogShow = true;
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
            if (_webSocketServer == null) return;
            if (!_webSocketServer.IsListening)
            {
                MessageBox.Show("Start Service First");
                return;
            }


            _webSocketServer.WebSocketServices["/"].Sessions.Broadcast(txtMessage.Text);
            mainGrid.Children.Add(new CustomMessageDialog());
        }

        private void btnRefreshPrinterList_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Not Implement");
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

            _printAndCut(printer.PrinterName);
        }

        private void btnPrintAndKickDrawer_Click(object sender, RoutedEventArgs e)
        {
            PrinterFromWindowsSystemModel? printer = ((FrameworkElement)sender).DataContext as PrinterFromWindowsSystemModel;
            if (printer == null) return;

            _printAndKickCashDrawer(printer.PrinterName);
        }

        private void btnPrintOnly_Click(object sender, RoutedEventArgs e)
        {
            //PrinterFromWindowsSystemModel? printer = ((FrameworkElement)sender).DataContext as PrinterFromWindowsSystemModel;
            //if (printer == null) return;

            //_printOnly(printer.PrinterName);

            MessageBox.Show("Sorry this feature is in progress");
        }

        private void btnCutOnly_Click(object sender, RoutedEventArgs e)
        {
            PrinterFromWindowsSystemModel? printer = ((FrameworkElement)sender).DataContext as PrinterFromWindowsSystemModel;
            if (printer == null) return;

            _cutOnly(printer.PrinterName);
        }

        private void btnKickDrawer_Click(object sender, RoutedEventArgs e)
        {
            PrinterFromWindowsSystemModel? printer = ((FrameworkElement)sender).DataContext as PrinterFromWindowsSystemModel;
            if (printer == null) return;

            _kickCashDrawer(printer.PrinterName);
        }

        //default windows print functions
        private void _printAndCut(string printerName)
        {
            //print and cut using default windows print document
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
            printDocument.PrinterSettings.PrinterName = printerName;
            printDocument.EndPrint += (o, ev) =>
            {
                if (_webSocketServer != null && _webSocketServer.IsListening)
                    _webSocketServer.WebSocketServices["/"].Sessions.Broadcast("Print Success hahha");
            };
            printDocument.Print();
            printDocument.Dispose();
        }

        private void _printOnly(string printerName)
        {
            //print only using default windows print document

            MessageBox.Show("Sorry, This Feature In Progress");
            //PrintDocument printDocument = new PrintDocument();
            //printDocument.PrintPage += (o, ev) =>
            //{
            //    if (ev.Graphics == null) return;
            //    ev.Graphics.DrawString(
            //        "",
            //        new Font("Arial", 10),
            //        Brushes.Black,
            //        ev.MarginBounds.Left,
            //        0,
            //        new StringFormat()
            //    );
            //    //ev.HasMorePages = true;
            //    //printDocument.Dispose();
            //};
            //printDocument.PrinterSettings.PrinterName = printerName;
            //printDocument.EndPrint += (o, ev) =>
            //{
            //    PrintEventArgs printEventArgs = (PrintEventArgs)ev;
            //    Debug.WriteLine("Print Success");
            //    Debug.WriteLine(printEventArgs.PrintAction);
            //    //if(printEventArgs.PrintAction == PrintAction.PrintToPrinter)
            //    //{
            //    //    printEventArgs.Cancel = true;
            //    //}
            //    printDocument.Dispose();
            //};
            //printDocument.BeginPrint += (o, ev) =>
            //{
            //    Debug.WriteLine("Print Start");
            //};
            //printDocument.QueryPageSettings += (o, ev) =>
            //{
            //    Debug.WriteLine("");
            //};
            //printDocument.Print();
            //printDocument.Dispose();
        }

        private void _printAndKickCashDrawer(string printerName)
        {
            //print and kick out cash drawer using default windows print document

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
            printDocument.PrinterSettings.PrinterName = printerName;
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
            };
            printDocument.Print();
            printDocument.Dispose();
        }

        private void _cutOnly(string printerName)
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
                MessageBox.Show("Cut Success");
            }
            printDocument.Dispose();
        }

        private void _kickCashDrawer(string printerName)
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
                MessageBox.Show("Cash Drawer Opened");
            }
            printDocument.Dispose();
        }

        //Image Conversion
        private IAttachmentType GetMimeType(string value)
        {
            IAttachmentType result;

            return string.IsNullOrEmpty(value)
                ? AttachmentType.UnknownMime
                : (mimeMap.TryGetValue(value.Substring(0, 5), out result) ? result : AttachmentType.Unknown);
        }
        private static readonly IDictionary<string, IAttachmentType> mimeMap =
        new Dictionary<string, IAttachmentType>(StringComparer.OrdinalIgnoreCase)
        {
            { "IVBOR", AttachmentType.Photo },
            { "/9J/4", AttachmentType.Photo },
            { "AAAAF", AttachmentType.Video },
            { "JVBER", AttachmentType.Document }
        };
    }
}
