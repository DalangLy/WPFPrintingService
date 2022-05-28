namespace WPFPrintingService
{
    internal class ClientWebSocket
    {
        private string _id;
        private string _ip;
        private string _name;
        public ClientWebSocket(string id, string ip, string name)
        {
            this._id = id;
            this._ip = ip;
            this._name = name;
        }

        public string Id { get { return _id;} }
        public string Ip { get { return _ip; } }
        public string Name { get { return _name;} }
    }
}
