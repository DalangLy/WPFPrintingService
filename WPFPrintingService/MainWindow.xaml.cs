using System.Windows;
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
            if(_webSocketServer == null)
            {
                _initializeWebSocketServer();
            }
        }

        private void _initializeWebSocketServer()
        {
            _webSocketServer = new WebSocketServer("ws://172.30.160.1:8000");

            //add listener
            _webSocketServer.AddWebSocketService<WebSocketServerListener>("/", () => new WebSocketServerListener(
                () => {

                }
            ));
        }

        private void _startWebSocketServer()
        {
            if (_webSocketServer == null) return;

            //check if server is not start

            _webSocketServer.Start();
        }

        private void _stopWebSocketServer()
        {
            if (_webSocketServer == null) return;

            //check if server is running

            //stop the server
            _webSocketServer.Stop();
        }
    }
}
