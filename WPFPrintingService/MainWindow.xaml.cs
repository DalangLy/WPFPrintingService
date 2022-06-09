using ESCPOS_NET;
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace WPFPrintingService
{
    public partial class MainWindow : Window
    {
        private List<ClientWebSocketModel> _allConnectedWebSocketClients = new List<ClientWebSocketModel>();
        private WebSocketServer? _webSocketServer;
        private List<NetworkPrinter> _allConnectedNetworkPrinters = new List<NetworkPrinter>();
        private List<PrinterModel> _allConnectedPrintersToDisplayOnDataGridView = new List<PrinterModel>();
        private BaseCommandEmitter _epson = new EPSON();
        private const int PORT = 1100;

        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //initialize websocket server
            _initializeWebSocketServer();


            //load all printer from windows system
            _loadAllPrintersFromWindowsSystem();

            //bind list of all connected websocket clients to list view
            lvConnectWebSocketClients.ItemsSource = _allConnectedWebSocketClients;

            //bind list of all connected printers to list view
            dgConnectedPrinters.ItemsSource = _allConnectedNetworkPrinters;


            //check if run on start up button is enable
            bool _isRunAtStartUp = Properties.Settings.Default.is_run_at_start_up;
            chbRunOnStartUp.IsChecked = _isRunAtStartUp;
        }

        private List<PrinterFromWindowsSystemModel> _allPrintersFromWindowsSystem = new List<PrinterFromWindowsSystemModel>();
        private void _loadAllPrintersFromWindowsSystem()
        {
            var printerQuery = new ManagementObjectSearcher("SELECT * from Win32_Printer");
            foreach (var printer in printerQuery.Get())
            {
                var name = printer.GetPropertyValue("Name");
                var status = printer.GetPropertyValue("Status");
                var isDefault = printer.GetPropertyValue("Default");
                var isNetworkPrinter = printer.GetPropertyValue("Network");

                Debug.WriteLine("{0} (Status: {1}, Default: {2}, Network: {3}",
                            name, status, isDefault, isNetworkPrinter);
                _allPrintersFromWindowsSystem.Add(new PrinterFromWindowsSystemModel(name.ToString()));
            }

            dgPrinters.ItemsSource = _allPrintersFromWindowsSystem;
        }

        private string GetLocalIPAddress()
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

        private void _onClientResponseMessage(string clientId, string clientName, string message, OnPrintResponse onPrintResponse, OnSendToServer onSendToServer, OnSendToEveryone onSendToEveryone)
        {
            try
            {
                RequestModel? requestModel = JsonConvert.DeserializeObject<RequestModel>(message);
                if (requestModel == null)
                {
                    onPrintResponse("Wrong Format");
                    return;
                }
                switch (requestModel.Code)
                {
                    case "SendToEveryOne":
                        onSendToEveryone($"{clientName} Said : {requestModel.Data}");
                        break;
                    case "SendToServer":

                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            //update text status
                            txtServerStatus.Text += $"\n {clientName} Said : {requestModel.Data}";

                        }), DispatcherPriority.Background);

                        onSendToServer();
                        break;
                    case "Print":

                        //serialize print data model
                        try
                        {
                            PrintDataModel? printDataModel = JsonConvert.DeserializeObject<PrintDataModel>(requestModel.Data);
                            if (printDataModel == null)
                            {
                                onPrintResponse("Wrong Data Format");
                                return;
                            }
                            //find printer

                            PrinterModel? _foundPrinterModel = _allConnectedPrintersToDisplayOnDataGridView.Find(printerModel => printerModel.PrinterName.Equals(printDataModel.PrinterName));
                            if (_foundPrinterModel == null)
                            {
                                onPrintResponse("Can't Find Printer");
                                return;
                            }

                            if (!_foundPrinterModel.IsOnline)
                            {
                                onPrintResponse("Select Printer is currently Offline");
                                return;
                            }

                            if (printDataModel.Base64Image == "")
                            {
                                onPrintResponse("Base64 Image Null");
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


                            int _selectedPrinterIndex = _allConnectedPrintersToDisplayOnDataGridView.IndexOf(_foundPrinterModel);

                            switch (printDataModel.PrintMethod)
                            {
                                case "PrintOnly":
                                    //print test
                                    _allConnectedNetworkPrinters[_selectedPrinterIndex].Write(
                                      ByteSplicer.Combine(
                                        _epson.PrintImage(File.ReadAllBytes(savedImage), true, true)
                                      )
                                    );
                                    break;
                                case "CutOnly":
                                    //print test
                                    _allConnectedNetworkPrinters[_selectedPrinterIndex].Write(
                                      ByteSplicer.Combine(
                                        _epson.PartialCutAfterFeed(5)
                                      )
                                    );
                                    break;
                                case "OpenCashDrawer":
                                    //print test
                                    _allConnectedNetworkPrinters[_selectedPrinterIndex].Write(
                                      ByteSplicer.Combine(
                                        _epson.CashDrawerOpenPin2()
                                      )
                                    );
                                    break;
                                default:
                                    //print test
                                    _allConnectedNetworkPrinters[_selectedPrinterIndex].Write(
                                      ByteSplicer.Combine(
                                        _epson.PrintImage(File.ReadAllBytes(savedImage), true, true),
                                        _epson.PartialCutAfterFeed(5)
                                      )
                                    );
                                    break;
                            }

                            onPrintResponse("Print Success!");
                        }
                        catch (Exception ex)
                        {
                            onPrintResponse($"Wrong Print Format {ex.Message}");
                        }
                        break;
                    default:
                        onPrintResponse("Wrong Code");
                        break;
                }
            }
            catch (Exception ex)
            {
                onPrintResponse($"Wrong Format : {ex.Message}");
            }
        }

        private void _initializeWebSocketServer()
        {
            _webSocketServer = new WebSocketServer(IPAddress.Parse(GetLocalIPAddress()), PORT);
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

        private void _startWebSocketServer()
        {
            if (_webSocketServer == null) return;

            //add web socket server listeners
            _webSocketServer.AddWebSocketService<WebSocketServerListener>("/", () => new WebSocketServerListener(
                (connectedClientId, connectedClientIp, connectedClientName) => {
                    //on open or on client connected
                    _addConnectedWebSocketClientToListView(connectedClientId, connectedClientIp, connectedClientName);
                },
                (clientId, clientName, message, onPrintResponse, onSendToServer, onSendToEveryone) =>
                {
                    //onMessageCallBack
                    _onClientResponseMessage(clientId, clientName, message, onPrintResponse, onSendToServer, onSendToEveryone);
                },
                (disconnectedClientId) =>
                {
                    //on close or on client disconnected
                    _removeDisconnectedWebSocketClientFromListView(disconnectedClientId);
                }
            ));

            _webSocketServer.Start();
        }

        private void _stopWebSocketServer()
        {
            if (_webSocketServer == null) return;

            _webSocketServer.RemoveWebSocketService("/");

            _webSocketServer.Stop(CloseStatusCode.Away, "Server Stop");
        }

        private bool _isWebSocketSeverRunning = false;
        private void btnStartStopServer_Click(object sender, RoutedEventArgs e)
        {
            if (_isWebSocketSeverRunning)
            {
                _stopWebSocketServer();
            }
            else
            {
                _startWebSocketServer();
            }
            _isWebSocketSeverRunning = !_isWebSocketSeverRunning;
            btnStartStopServer.Content = _isWebSocketSeverRunning ? "Stop" : "Start";
            txtServerStatus.Text = _isWebSocketSeverRunning ? $"Service on ws://{GetLocalIPAddress()}:{PORT}" : "Server Stopped";
        }

        private void btnAddPrinter_Click(object sender, RoutedEventArgs e)
        {
            mainGrid.Children.Add(new AddPrinterForm(
                (ip, port, name, childForm) =>
                {
                    this._addPrinter(ip, port, name);
                    mainGrid.Children.Remove(childForm);
                },
                (childForm) =>
                {
                    mainGrid.Children.Remove(childForm);
                }
            ));
        }

        private void _addPrinter(string ip, string port, string name)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                //test connect to printer
                NetworkPrinterSettings printerSetting = new NetworkPrinterSettings() { ConnectionString = $"{ip}:{port}", PrinterName = name};
                NetworkPrinter printer = new NetworkPrinter(printerSetting);
                printer.StatusChanged += _StatusChanged;
                printer.Connected += _printerConnectedListener;
                printer.Disconnected += _printerDisconnectedListener;

                printer.Write(_epson.Initialize());
                printer.Write(_epson.Enable());
                printer.Write(_epson.EnableAutomaticStatusBack());


                //add connected printer to list
                _allConnectedNetworkPrinters.Add(printer);

                _allConnectedPrintersToDisplayOnDataGridView.Add(new PrinterModel("", ip, printer.PrinterName));

                dgConnectedPrinters.ItemsSource = _allConnectedPrintersToDisplayOnDataGridView;
                dgConnectedPrinters.Items.Refresh();
            }), DispatcherPriority.Background);
        }

        private void _printerDisconnectedListener(object sender, EventArgs ps)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                NetworkPrinter printer = (NetworkPrinter)sender;
                var status = (ConnectionEventArgs)ps;
                PrinterModel printerModel = this._allConnectedPrintersToDisplayOnDataGridView.Find(e => e.PrinterName == printer.PrinterName)!;
                printerModel.IsOnline = status.IsConnected;
                dgConnectedPrinters.Items.Refresh();
                Debug.WriteLine($"On Printer Disconnect Event: {status.IsConnected}");
            }), DispatcherPriority.Background);
        }

        private void _printerConnectedListener(object sender, EventArgs ps)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                NetworkPrinter printer = (NetworkPrinter)sender;
                var status = (ConnectionEventArgs)ps;
                PrinterModel printerModel = this._allConnectedPrintersToDisplayOnDataGridView.Find(e => e.PrinterName == printer.PrinterName)!;
                printerModel.IsOnline = status.IsConnected;
                dgConnectedPrinters.Items.Refresh();
                Debug.WriteLine($"On Printer Connect Event: {status.IsConnected}");
            }), DispatcherPriority.Background);
        }

        private void _StatusChanged(object sender, EventArgs ps)
        {
            var status = (PrinterStatusEventArgs)ps;
            Debug.WriteLine($"On Status Change: {status.IsPrinterOnline}");
            Debug.WriteLine($"Has Paper? {status.IsPaperOut}");
            Debug.WriteLine($"Paper Running Low? {status.IsPaperLow}");
            Debug.WriteLine($"Cash Drawer Open? {status.IsCashDrawerOpen}");
            Debug.WriteLine($"Cover Open? {status.IsCoverOpen}");
            Debug.WriteLine($"Feeding? {status.IsPaperCurrentlyFeeding}");
            Debug.WriteLine($"Error State? {status.IsInErrorState}");
            Debug.WriteLine($"Waiting? {status.IsWaitingForOnlineRecovery}");
            
        }

        private void Remove_Printer_Button_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Printer REmove");
        }

        private void Print_Test_Button_Click(object sender, RoutedEventArgs e)
        {
            PrinterModel? printer = ((FrameworkElement)sender).DataContext as PrinterModel;
            if (printer == null) return;
            //find printer
            int _selectedPrinterIndex = _allConnectedNetworkPrinters.FindIndex(e => e.PrinterName.Equals(printer.PrinterName));


            int index = dgConnectedPrinters.SelectedIndex;


            //var gg = File.ReadAllBytes();

            //print test
            _allConnectedNetworkPrinters[index].Write(
              ByteSplicer.Combine(
                _epson.CenterAlign(),
                _epson.PrintLine($"Selected Printer {printer.GetHashCode}"),
                _epson.PrintLine("B&H PHOTO & VIDEO"),
                //_epson.PrintImage(File.ReadAllBytes("C:\\Users\\dalan\\Downloads\\kitten.jpg"), true, true),
                _epson.PrintLine($"End Printer {printer.PrinterName}"),
                _epson.PartialCutAfterFeed(5)
              )
            );
        }

        private void btnMonitorWebSocketServer_Click(object sender, RoutedEventArgs e)
        {
            mainGrid.Children.Add(new MonitorServerForm((childForm) =>
            {
                mainGrid.Children.Remove(childForm);
            }));
        }

        private void btnServerInfo_Click(object sender, RoutedEventArgs e)
        {
            mainGrid.Children.Add(new ServerInfoForm((childForm) =>
            {
                mainGrid.Children.Remove(childForm);
            }));
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
            _shutdownThisApplication();
        }

        private void _shutdownThisApplication()
        {
            Application.Current.Shutdown();
        }

        private void btnAddPrinter_Copy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var scope = new ManagementScope(@"\root\cimv2");
                scope.Connect();

                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer");
                var results = searcher.Get();
                Console.WriteLine("Network printers list:");
                foreach (var printer in results)
                {
                    var portName = printer.Properties["PortName"].Value;

                    var searcher2 = new ManagementObjectSearcher("SELECT * FROM Win32_TCPIPPrinterPort where Name LIKE '" + portName + "'");
                    var results2 = searcher2.Get();
                    foreach (var printer2 in results2)
                    {
                        Debug.WriteLine("Name:" + printer.Properties["Name"].Value);
                        //Console.WriteLine("PortName:" + portName);
                        Debug.WriteLine("PortNumber:" + printer2.Properties["PortNumber"].Value);
                        Debug.WriteLine("HostAddress:" + printer2.Properties["HostAddress"].Value);


                        NetworkPrinterSettings printerSetting = new NetworkPrinterSettings() { ConnectionString = $"{printer2.Properties["HostAddress"].Value}:{printer2.Properties["PortNumber"].Value}", PrinterName = printer.Properties["Name"].Value.ToString() };
                        NetworkPrinter printer1 = new NetworkPrinter(printerSetting);

                        //print test
                        printer1.Write(
                          ByteSplicer.Combine(
                            _epson.CenterAlign(),
                            _epson.PrintLine($"Selected Printer 1"),
                            _epson.PrintLine("B&H PHOTO & VIDEO"),
                             //_epson.PrintImage(File.ReadAllBytes("C:\\Users\\dalan\\Downloads\\kitten.jpg"), true, true),
                             _epson.PrintLine($"End Printer"),
                            _epson.Print("Test")
                          )
                        );


                        NetworkPrinterSettings printerSetting1 = new NetworkPrinterSettings() { ConnectionString = $"{printer2.Properties["HostAddress"].Value}:{printer2.Properties["PortNumber"].Value}", PrinterName = printer.Properties["Name"].Value.ToString() };
                        NetworkPrinter printer3 = new NetworkPrinter(printerSetting1);

                        //print test
                        printer3.Write(
                          ByteSplicer.Combine(
                            _epson.CenterAlign(),
                            _epson.PrintLine($"Selected Printer 2"),
                            _epson.PrintLine("B&H PHOTO & VIDEO"),
                             //_epson.PrintImage(File.ReadAllBytes("C:\\Users\\dalan\\Downloads\\kitten.jpg"), true, true),
                             _epson.PrintLine($"End Printer"),
                            _epson.Print("Test")
                          )
                        );

                    }
                    Console.WriteLine();
                }
                Console.ReadLine();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "");
                throw;
            }
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

        private void btnSendToEveryClients_Click(object sender, RoutedEventArgs e)
        {
            if (_webSocketServer == null) return;
            _webSocketServer.WebSocketServices["/"].Sessions.Broadcast(txtMessage.Text);
        }

        private void btnPrintAndCut_Click(object sender, RoutedEventArgs e)
        {
            PrinterFromWindowsSystemModel? printer = ((FrameworkElement)sender).DataContext as PrinterFromWindowsSystemModel;
            if (printer == null) return;

            _printAndCut(printer.PrinterName);
        }

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
            printDocument.Print();
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
            RawPrinterHelper.SendStringToPrinter(printDocument.PrinterSettings.PrinterName, COMMAND);

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
            RawPrinterHelper.SendStringToPrinter(printDocument.PrinterSettings.PrinterName, openTillCommand);

            printDocument.Dispose();
        }
    }
}
