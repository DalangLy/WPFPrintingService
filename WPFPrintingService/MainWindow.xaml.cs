using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WebSocketSharp.Server;
using WebSocketSharp.Net;

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
