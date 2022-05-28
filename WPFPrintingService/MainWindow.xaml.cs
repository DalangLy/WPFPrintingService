using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using WebSocketSharp.Server;

namespace WPFPrintingService
{
    public partial class MainWindow : Window
    {
        private List<ClientWebSocket> _allConnectedWebSocketClients = new List<ClientWebSocket>();
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private WebSocketServer? _webSocketServer;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _initializeWebSocketServer();


            //set list of all connected websocket clients to list view
            lvConnectWebSocketClients.ItemsSource = _allConnectedWebSocketClients;
        }

        private void _initializeWebSocketServer()
        {
            _webSocketServer = new WebSocketServer("ws://127.0.0.1:8000");

            //add listener
            _webSocketServer.AddWebSocketService<WebSocketServerListener>("/", () => new WebSocketServerListener(
                (clientId, clientIp, clientName) => {
                    _addConnectedWebSocketClientToListView(clientId, clientIp, clientName);
                },
                (disconnectedClientId) =>
                {
                    //_removeDisconnectedWebSocketClientFromListView(disconnectedClientId);
                }
            ));
        }

        
        private void _addConnectedWebSocketClientToListView(string id, string ip, string name)
        {
            _allConnectedWebSocketClients.Add(new ClientWebSocket(id, ip, name));
            lvConnectWebSocketClients.ItemsSource = _allConnectedWebSocketClients;

            _updateListViewOfAllConnectedWebSocketClients();
        }

        private void _updateListViewOfAllConnectedWebSocketClients()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                lvConnectWebSocketClients.Items.Refresh();
            }), DispatcherPriority.Background);
        }

        private void _removeDisconnectedWebSocketClientFromListView(string disconnectedClientId)
        {
            ClientWebSocket? _disconnectedWebSocketClient = _allConnectedWebSocketClients.Find(x => x.Id == disconnectedClientId);
            if (_disconnectedWebSocketClient == null) return;
            _allConnectedWebSocketClients.Remove(_disconnectedWebSocketClient);
            lvConnectWebSocketClients.ItemsSource = _allConnectedWebSocketClients;

            //to update ui
            _updateListViewOfAllConnectedWebSocketClients();
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
    }
}
