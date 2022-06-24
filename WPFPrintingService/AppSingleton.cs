using System.Net;
using System.Net.Sockets;

namespace WPFPrintingService
{
    internal sealed class AppSingleton
    {
        private static AppSingleton instance = new AppSingleton();
        public static AppSingleton GetInstance
        {
            get
            {
                return instance;
            }
        }

        private string? _systemIP;
        public string SystemIP {
            get {
                if(_systemIP == null)
                {
                    using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                    {
                        socket.Connect("8.8.8.8", 65530);
                        IPEndPoint? endPoint = socket.LocalEndPoint as IPEndPoint;
                        _systemIP = endPoint!.Address.ToString();
                    }
                }
                return _systemIP; 
            } 
        }

        private int _port = 1100;
        public int Port { get { return _port; } }
    }
}
