namespace WPFPrintingService
{
    internal class PrinterModel
    {
        private string _id;
        private string _ip;
        private string _name;

        public PrinterModel(string id, string ip, string name)
        {
            this._id = id;
            this._ip = ip;
            this._name = name;
        }

        public string Id { get { return _id; } }
        public string PrinterIp { get { return _ip; } }
        public string PrinterName { get { return _name; } }
        public bool IsOnline { get; set; }
    }
}
