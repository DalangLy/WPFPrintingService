using System.Diagnostics;
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
                        try
                        {
                            socket.Connect("8.8.8.8", 65530);
                            IPEndPoint? endPoint = socket.LocalEndPoint as IPEndPoint;
                            _systemIP = endPoint!.Address.ToString();
                        }
                        catch (System.Exception)
                        {
                            Debug.WriteLine("Something went wrong");
                        }
                    }
                }
                return _systemIP; 
            } 
        }

        private int _port = 2713;
        public int Port { get { return _port; } }
    }
}
