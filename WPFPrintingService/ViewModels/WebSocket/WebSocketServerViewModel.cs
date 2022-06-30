using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using WebSocketSharp;
using WebSocketSharp.Server;
using WPFPrintingService.Print_Models;
using WPFPrintingService.UICallBackDelegates;

namespace WPFPrintingService
{
    internal class WebSocketServerViewModel : BaseViewModel
    {
        private static WebSocketServerViewModel? _instance;
        public static WebSocketServerViewModel Instance
        {
            get { return _instance ?? (_instance = new WebSocketServerViewModel()); }
        }

        public ObservableCollection<ClientWebSocketModel> ConnectedWebSocketClients { get; set; } = new ObservableCollection<ClientWebSocketModel>();

        private string _serverStatus = string.Empty;

        public string ServerStatus
        {
            get { return _serverStatus; }
            set
            {
                _serverStatus = value;
                OnPropertyChanged(nameof(ServerStatus));
            }
        }

        private bool _isServiceRunning = true;

        public bool IsServiceRunning
        {
            get { return true; }
            set
            {
                _isServiceRunning = value;
                OnPropertyChanged(nameof(IsServiceRunning));
            }
        }


        public WebSocketServer WebSocketServer;

        private WebSocketServerViewModel()
        {
            //initial websocket server
            this.WebSocketServer = new WebSocketServer(IPAddress.Parse(AppSingleton.GetInstance.SystemIP), AppSingleton.GetInstance.Port);
        }

        

        public void stopService()
        {
            this.WebSocketServer.RemoveWebSocketService("/");
            this.WebSocketServer.Stop();
            if (!this.WebSocketServer.IsListening)
            {
                this.IsServiceRunning = false;
                this.ServerStatus = "Service Stopped";
            }
        }

      
        public void StartService()
        {
            //add web socket server listeners
            this.WebSocketServer.AddWebSocketService<WebSocketServerListener>("/", () => new WebSocketServerListener((sender, args, connectedClientId, connectedClientIp, connectedClientName) =>
                {
                    //on client connected
                    App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
                    {
                        this.ConnectedWebSocketClients.Add(new ClientWebSocketModel() { Name = connectedClientName, ID = connectedClientId, IP = connectedClientIp });
                    });
                },
                (sender, args, clientId, clientName, message, onPrintResponse, onSendToServer, onSendToEveryone) =>
                {
                    //on message received from client
                    this._onClientResponseMessage(clientId, clientName, message, onPrintResponse, onSendToServer, onSendToEveryone);
                },
                (sender, args, disconnectedClientId) =>
                {
                    //on client disconnected
                    ClientWebSocketModel foundClient = this.ConnectedWebSocketClients.Where(x => x.ID == disconnectedClientId).First();
                    App.Current.Dispatcher?.Invoke((Action)delegate
                    {
                        this.ConnectedWebSocketClients.Remove(foundClient);
                    });
                }
            ));

            this.WebSocketServer.Start();
            if (this.WebSocketServer.IsListening)
            {
                this.IsServiceRunning = true;
                this.ServerStatus = $"Service on ws://{AppSingleton.GetInstance.SystemIP}:{AppSingleton.GetInstance.Port}";
            }
        }

        private void _onClientResponseMessage(string clientId, string clientName, string message, OnPrintResponse onPrintResponse, OnSendToServer onSendToServer, OnSendToEveryone onSendToEveryone)
        {
            try
            {
                PrintTemplateModel printTemplateModel = PrintTemplateModel.FromJson(message);
                if (printTemplateModel == null)
                {
                    onPrintResponse(this, EventArgs.Empty, "Wrong Format");
                    return;
                }
                switch (printTemplateModel.Code)
                {
                    case "RequestPrinters":
                        if (WebSocketServer != null && WebSocketServer.IsListening)
                        {
                            //var json = System.Text.Json.JsonSerializer.Serialize();
                            //_webSocketServer.WebSocketServices["/"].Sessions.SendTo(json, clientId);
                        }
                        break;
                    case "SendToEveryone":
                        onSendToEveryone(this, EventArgs.Empty, $"{clientName} Said : {printTemplateModel.Data}");
                        break;
                    case "SendToServer":

                        //update text status
                        this.ServerStatus += $"\n {clientName} Said : {printTemplateModel.Data}";


                        onSendToServer(this, EventArgs.Empty);
                        break;
                    case "Print":
                        try
                        {
                            if (printTemplateModel?.Data == null)
                            {
                                onPrintResponse(this, EventArgs.Empty, "Wrong Data Format");
                                return;
                            }

                            //find printer
                            //PrinterFromWindowsSystemModel? _foundPrinterModel = _allPrintersFromWindowsSystem.Find(printerModel => printerModel.PrinterName.Equals(printTemplateModel?.Data?.PrinterName));
                            //if (_foundPrinterModel == null)
                            //{
                            //    onPrintResponse(this, EventArgs.Empty, "Can't Find Printer");
                            //    return;
                            //}

                            switch (printTemplateModel?.Data?.PrintMethod)
                            {
                                case "CutOnly":
                                    PrinterClass printerClass1 = new PrinterClass();
                                    printerClass1.CutPaper("Hello");
                                    break;
                                case "OpenCashDrawer":
                                    PrinterClass printerClass = new PrinterClass();
                                    printerClass.KickDrawer("Hello");
                                    break;
                                case "PrintAndKickCashDrawer":
                                    App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
                                    {
                                        PrinterClass printerClass = new PrinterClass();
                                        TestPrintTemplate testPrintTemplate = new TestPrintTemplate();
                                        printerClass.PrintAndKickCashDrawer("Hello", testPrintTemplate);
                                    });
                                    break;
                                default:
                                    App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
                                    {
                                        PrinterClass printerClass = new PrinterClass();
                                        TestPrintTemplate testPrintTemplate = new TestPrintTemplate();
                                        printerClass.PrintAndCutPaper("Hello", testPrintTemplate);
                                    });
                                    break;
                            }
                        }
                        catch (Exception)
                        {

                            throw;
                        }

                        break;
                    default:
                        onPrintResponse(this, EventArgs.Empty, "Wrong Code");
                        break;
                }
            }
            catch (Exception ex)
            {
                onPrintResponse(this, EventArgs.Empty, $"Wrong Format : {ex.Message}");
            }
        }

        
    }
}
