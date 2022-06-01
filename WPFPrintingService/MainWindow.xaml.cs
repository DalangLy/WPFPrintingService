using ESCPOS_NET;
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Windows;
using System.Windows.Threading;
using WebSocketSharp.Server;

namespace WPFPrintingService
{
    public partial class MainWindow : Window
    {
        private List<ClientWebSocketModel> _allConnectedWebSocketClients = new List<ClientWebSocketModel>();
        private WebSocketServer? _webSocketServer;
        private List<NetworkPrinter> _allConnectedNetworkPrinters = new List<NetworkPrinter>();
        private List<PrinterModel> _allConnectedPrintersToDisplayOnDataGridView = new List<PrinterModel>();
        private BaseCommandEmitter _epson = new EPSON();

        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //initialize websocket server
            _initializeWebSocketServer();


            //bind list of all connected websocket clients to list view
            lvConnectWebSocketClients.ItemsSource = _allConnectedWebSocketClients;

            //bind list of all connected printers to list view
            dgConnectedPrinters.ItemsSource = _allConnectedNetworkPrinters;
        }

        private void _initializeWebSocketServer()
        {
            _webSocketServer = new WebSocketServer("ws://127.0.0.1:8000");

            //add web socket server listeners
            _webSocketServer.AddWebSocketService<WebSocketServerListener>("/", () => new WebSocketServerListener(
                (connectedClientId, connectedClientIp, connectedClientName) => {
                    //on open or on client connected
                    _addConnectedWebSocketClientToListView(connectedClientId, connectedClientIp, connectedClientName);
                },
                (clientId, clientName, message) =>
                {
                    //Debug.WriteLine($"ID : {clientId}, Name : {clientName}, Message : {message}");
                    JObject json = JObject.Parse(message);
                    JToken? k = json.First;
                    string printerName = (string)k!.Last!;
                    JToken? l = json.Last;
                    string data = (string)l!.Last!;

                    //find printer
                    int _selectedPrinterIndex = _allConnectedNetworkPrinters.FindIndex(e => e.PrinterName.Equals(printerName));


                    //print test
                    _allConnectedNetworkPrinters[_selectedPrinterIndex].Write(
                      ByteSplicer.Combine(
                        _epson.CenterAlign(),
                        _epson.PrintLine($"Selected Printer {printerName}"),
                        _epson.PrintLine("B&H PHOTO & VIDEO"),
                        _epson.PrintLine("End Printer 1"),
                        _epson.PartialCutAfterFeed(5)
                      )
                    );

                    return "Success";
                },
                (disconnectedClientId) =>
                {
                    //on close or on client disconnected
                    _removeDisconnectedWebSocketClientFromListView(disconnectedClientId);
                }
            ));
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
            }), DispatcherPriority.Background);
        }

        private void _startWebSocketServer()
        {
            if (_webSocketServer == null) return;
            _webSocketServer.Start();
        }

        private void _stopWebSocketServer()
        {
            if (_webSocketServer == null) return;
            _webSocketServer.Stop();
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
            txtServerStatus.Text = _isWebSocketSeverRunning ? "Service on 127.0.0.1:8000" : "Server Stopped";
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
                printer.StatusChanged += StatusChanged;
                printer.Connected += _printerConnectedListener;
                printer.Disconnected += _printerDisconnectedListener;

                BaseCommandEmitter _ff = new EPSON();
                printer.Write(_ff.Initialize());
                printer.Write(_ff.Enable());
                printer.Write(_ff.EnableAutomaticStatusBack());


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

        private void StatusChanged(object sender, EventArgs ps)
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
            PrinterModel printer = ((FrameworkElement)sender).DataContext as PrinterModel;
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
            mainGrid.Children.Add(new MonitorServerForm());
        }

        private void btnServerInfo_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Hello World");
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

                        _epson = new EPSON();
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

                        _epson = new EPSON();
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
    }
}
