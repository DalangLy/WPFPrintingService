using System.Windows.Controls;

namespace WPFPrintingService
{
    internal class PrinterModel
    {
        private string _id;
        private string _ip;
        private string _name;
        private Button _removeButton;
        public PrinterModel(string id, string ip, string name, Button removeButton)
        {
            this._id = id;
            this._ip = ip;
            this._name = name;
            this._removeButton = removeButton;
        }

        public string Id { get { return _id; } }
        public string PrinterIp { get { return _ip; } }
        public string PrinterName { get { return _name; } }
        public Button RemoveButton { get { return _removeButton; } }
    }
}
