using ESCPOS_NET;
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WebSocketSharp.Server;

namespace WPFPrintingService
{
    public partial class MainWindow : Window
    {
        private List<ClientWebSocketModel> _allConnectedWebSocketClients = new List<ClientWebSocketModel>();
        private WebSocketServer? _webSocketServer;
        private List<NetworkPrinter> _allConnectedNetworkPrinters = new List<NetworkPrinter>();
        private List<PrinterModel> _allConnectedPrinters = new List<PrinterModel>();
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
            lvConnectedPrinters.ItemsSource = _allConnectedNetworkPrinters;
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
                    JToken k = json.First;
                    string printerName = (string)k.Last;
                    JToken l = json.Last;
                    string data = (string)l.Last;

                    //find printer
                    int _selectedPrinterIndex = _allConnectedNetworkPrinters.FindIndex(e => e.PrinterName.Equals(printerName));


                    //print test
                    var epson = new EPSON();
                    _allConnectedNetworkPrinters[_selectedPrinterIndex].Write(
                      ByteSplicer.Combine(
                        epson.CenterAlign(),
                        epson.PrintLine($"Selected Printer {printerName}"),
                        epson.PrintLine("B&H PHOTO & VIDEO"),
                        epson.PrintLine("End Printer 1"),
                        epson.PartialCutAfterFeed(5)
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
        }

        private void btnAddPrinter_Click(object sender, RoutedEventArgs e)
        {
            mainGrid.Children.Add(new AddPrinterForm(
                (ip, port, childForm) =>
                {
                    this._addPrinter(ip, port);
                    mainGrid.Children.Remove(childForm);
                },
                (childForm) =>
                {
                    mainGrid.Children.Remove(childForm);
                }
            ));
        }

        private void _addPrinter(string ip, string port)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                //test connect to printer
                NetworkPrinterSettings printerSetting = new NetworkPrinterSettings() { ConnectionString = $"{ip}:{port}", PrinterName = "POS80" };
                NetworkPrinter printer = new NetworkPrinter(printerSetting);

                //add connected printer to list
                _allConnectedNetworkPrinters.Add(printer);

                Button _removeButton = new Button();
                _removeButton.Content = "Remove";
                _allConnectedPrinters.Add(new PrinterModel("", printerSetting.PrinterName, "IP", _removeButton));

                lvConnectedPrinters.ItemsSource = _allConnectedPrinters;
                lvConnectedPrinters.Items.Refresh();
            }), DispatcherPriority.Background);
        }

        private void Remove_Printer_Button_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Printer REmove");
        }

        private void lvConnectedPrinters_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Debug.WriteLine("Hello");
            var item = sender as ListViewItem;
            if (item != null && item.IsSelected)
            {
                Debug.WriteLine(item);
            }
        }

        private void StackPanel_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Debug.WriteLine("hello World");
        }
    }
}
