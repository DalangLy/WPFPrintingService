using ESCPOS_NET;
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
                NetworkPrinterSettings printerSetting = new NetworkPrinterSettings() { ConnectionString = $"{ip}:{port}"};
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
            int _selectedPrinterIndex = _allConnectedNetworkPrinters.FindIndex(e => e.PrinterName.Equals(printer.PrinterIp));


            int index = dgConnectedPrinters.SelectedIndex;


            //var gg = File.ReadAllBytes();

            //print test
            _allConnectedNetworkPrinters[index].Write(
              ByteSplicer.Combine(
                //_epson.CenterAlign(),
                //_epson.PrintLine($"Selected Printer {printer}"),
                //_epson.PrintLine("B&H PHOTO & VIDEO"),
                _epson.PrintImage(File.ReadAllBytes("C:\\Users\\dalan\\Downloads\\kitten.jpg"), true, true),
                // _epson.PrintLine($"End Printer {printer}"),
                _epson.PartialCutAfterFeed(5)
              )
            );
        }

        private void btnMonitorWebSocketServer_Click(object sender, RoutedEventArgs e)
        {
            mainGrid.Children.Add(new MonitorServerForm());
        }
    }
}
