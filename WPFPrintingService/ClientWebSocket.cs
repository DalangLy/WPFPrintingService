namespace WPFPrintingService
{
    internal class ClientWebSocket
    {
        private string _ip;
        private string _name;
        public ClientWebSocket(string ip, string name)
        {
            this._ip = ip;
            this._name = name;
        }

        public string Ip => _ip;
        public string Name => _name;
    }
}
