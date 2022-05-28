using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using WebSocketSharp.Server;

namespace WPFPrintingService
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private WebSocketServer? _webSocketServer;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _initializeWebSocketServer();
        }

        private void _initializeWebSocketServer()
        {
            _webSocketServer = new WebSocketServer("ws://127.0.0.1:8000");

            //add listener
            _webSocketServer.AddWebSocketService<WebSocketServerListener>("/", () => new WebSocketServerListener(
                (clientIp, clientName) => {
                    _addConnectedWebSocketClientToListView(clientIp, clientName);
                    Debug.WriteLine("Client IP : " + clientIp + ", Client Name : " + clientName);
                }
            ));
        }

        
        private void _addConnectedWebSocketClientToListView(string ip, string name)
        {
            //to update ui
            Dispatcher.BeginInvoke(new Action(() =>
            {
                lvConnectWebSocketClients.Items.Add(new ClientWebSocket(ip, name));
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
    }
}
