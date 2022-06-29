using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    internal class WebSocketClientViewModel : BaseViewModel
    {
        public static WebSocketClientViewModel Instance => new WebSocketClientViewModel();

        public ICommand StartWebSocketServerCommand { get; set; }
        public ICommand StopWebSocketServerCommand { get; set; }
        public ICommand SendMessageToAllClientsCommand { get; set; }
        public ICommand CloseSentMessageDialogCommand { get; set; }
        public ICommand ToggleRunWebSocketServerOnAppLaunched { get; set; }
        public ICommand ShowConfirmStopServiceCommand { get; set; }

        public ObservableCollection<ClientWebSocketModel> ConnectedWebSocketClients { get; set; } = new ObservableCollection<ClientWebSocketModel>();

        private bool _isStartServiceOnAppLauch = false;
        public bool IsStartServiceOnAppLauched
        {
            get { return _isStartServiceOnAppLauch; }
            set
            {
                _isStartServiceOnAppLauch = value;
                OnPropertyChanged(nameof(IsStartServiceOnAppLauched));
            }
        }

        private bool _isConfirmStopServiceShowUp;

        public bool IsConfirmStopServiceShowUp
        {
            get { return _isConfirmStopServiceShowUp; }
            set { 
                _isConfirmStopServiceShowUp = value; 
                OnPropertyChanged();
            }
        }

        private bool _isSendingMessageToAllClients;

        public bool IsSendingMessageToAllClients
        {
            get { return _isSendingMessageToAllClients; }
            set { 
                _isSendingMessageToAllClients = value;
                OnPropertyChanged();
            }
        }

        private bool _isSentMessageToAllClientsSuccess;

        public bool IsSentMessageToAllClientsSuccess
        {
            get { return _isSentMessageToAllClientsSuccess; }
            set { 
                _isSentMessageToAllClientsSuccess = value;
                OnPropertyChanged();
            }
        }

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

        private bool _isServiceRunning = false;

        public bool IsServiceRunning
        {
            get { return _isServiceRunning; }
            set
            {
                _isServiceRunning = value;
                OnPropertyChanged(nameof(IsServiceRunning));
            }
        }


        private WebSocketServer _webSocketServer;

        private WebSocketClientViewModel()
        {
            //initial websocket server
            this._webSocketServer = new WebSocketServer(IPAddress.Parse(AppSingleton.GetInstance.SystemIP), AppSingleton.GetInstance.Port);


            this.IsStartServiceOnAppLauched = Properties.Settings.Default.RunServiceOnAppLaunched;
            if (this.IsStartServiceOnAppLauched)
            {
                this.StartService();
            }

            //set up command
            this.StartWebSocketServerCommand = new StartServiceCommand(this);
            this.StopWebSocketServerCommand = new StopServiceCommand(async (p) => await InvokeStopService(p));
            this.SendMessageToAllClientsCommand = new SendMessageToAllClientCommand(async (p) => await InvokeSendMessageToAllClient(p));
            this.CloseSentMessageDialogCommand = new CloseSentMessageDialogCommand(this);
            this.ToggleRunWebSocketServerOnAppLaunched = new ToggleAutoRunServiceOnStartUp();
            this.ShowConfirmStopServiceCommand = new ShowCofirmStopServiceCommand(this);
        }

        private async Task InvokeSendMessageToAllClient(object p)
        {
            if (p == null) return;
            string message = (string)p;
            this.IsSendingMessageToAllClients = true; //show sending progress dialog
            await Task.Delay(1000*5);//delay 5 seconds
            this._webSocketServer.WebSocketServices["/"].Sessions.Broadcast(message);
            this.IsSendingMessageToAllClients = false; //remove sending progress dialog
            this.IsSentMessageToAllClientsSuccess = true;//show sent dialog
        }

        private async Task InvokeStopService(object p)
        {
            try
            {
                if (!this._webSocketServer.IsListening) return;

                this._webSocketServer.RemoveWebSocketService("/");

                this._webSocketServer.Stop(CloseStatusCode.Away, "Server Stop");

                this.ServerStatus = "Server Stopped";

                IsServiceRunning = false;
                this.IsConfirmStopServiceShowUp = false;
            }
            finally
            {
                IsServiceRunning = false;
                this.IsConfirmStopServiceShowUp = false;
            }
        }

        

      
        public void StartService()
        {
            //add web socket server listeners
            this._webSocketServer.AddWebSocketService<WebSocketServerListener>("/", () => new WebSocketServerListener((sender, args, connectedClientId, connectedClientIp, connectedClientName) =>
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

            this._webSocketServer.Start();
            if (this._webSocketServer.IsListening)
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
                        if (_webSocketServer != null && _webSocketServer.IsListening)
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

    internal class CloseSentMessageDialogCommand : ICommand
    {
        private WebSocketClientViewModel _webSocketClientViewModel;

        public CloseSentMessageDialogCommand(WebSocketClientViewModel webSocketClientViewModel)
        {
            _webSocketClientViewModel = webSocketClientViewModel;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            this._webSocketClientViewModel.IsSentMessageToAllClientsSuccess = false;
        }
    }

    internal class ShowCofirmStopServiceCommand : ICommand
    {
        private WebSocketClientViewModel webSocketClientViewModel;

        public ShowCofirmStopServiceCommand(WebSocketClientViewModel webSocketClientViewModel)
        {
            this.webSocketClientViewModel = webSocketClientViewModel;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            this.webSocketClientViewModel.IsConfirmStopServiceShowUp = true;
        }
    }

    internal class ToggleAutoRunServiceOnStartUp : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            if (parameter == null) return;
            bool isChecked = (bool)parameter;
            Properties.Settings.Default.RunServiceOnAppLaunched = isChecked;
            Properties.Settings.Default.Save();
        }
    }

    internal class SendMessageToAllClientCommand : ICommand
    {
        private Action? mAction;

        private Action<object?>? pAction;

        public SendMessageToAllClientCommand(Action<object> pAction)
        {
            this.pAction = pAction;
        }

        public SendMessageToAllClientCommand(Action mAction)
        {
            this.mAction = mAction;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            mAction?.Invoke();
            pAction?.Invoke(parameter);
        }
    }

    internal class StartServiceCommand : ICommand
    {
        private WebSocketClientViewModel _webSocketClientViewModel;
        public StartServiceCommand(WebSocketClientViewModel webSocketClientViewModel)
        {
            this._webSocketClientViewModel = webSocketClientViewModel;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            this._webSocketClientViewModel.StartService();
        }
    }

    public class StopServiceCommand : ICommand
    {
        private Action? mAction;

        private Action<object?>? pAction;

        public StopServiceCommand(Action<object> pAction)
        {
            this.pAction = pAction;
        }

        public StopServiceCommand(Action mAction)
        {
            this.mAction = mAction;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            mAction?.Invoke();
            pAction?.Invoke(parameter);
        }
    }
}
