using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using WebSocketSharp.Server;

namespace WPFPrintingService
{
    internal class WebSocketClientViewModel : BaseViewModel
    {

        private bool loadingService;

        public bool LoadingService
        {
            get { return loadingService; }
            set { loadingService = value; OnPropertyChanged(); }
        }


        public ICommand StopCommand { get; set; }

        private List<ClientWebSocketModel>? _webSocketClients;

        public List<ClientWebSocketModel> WebSocketClients
        {
            get { return _webSocketClients ?? new List<ClientWebSocketModel>(); }
            set
            {
                _webSocketClients = value;
                OnPropertyChanged(nameof(WebSocketClients));
            }
        }

        private bool _isStartServiceOnAppLauch = false;
        public bool IsStartServiceOnAppLauch
        {
            get { return _isStartServiceOnAppLauch; }
            set
            {
                _isStartServiceOnAppLauch = value;
                OnPropertyChanged(nameof(IsStartServiceOnAppLauch));
            }
        }

        private bool _isLaunchAppAtStartUp = false;

        public bool IsLaunchAppAtStartUp
        {
            get { return _isLaunchAppAtStartUp; }
            set { 
                _isLaunchAppAtStartUp = value; 
                OnPropertyChanged(nameof(IsLaunchAppAtStartUp));
            }
        }


        private WebSocketServer _webSocketServer;
        private WebSocketClientViewModel()
        {
            //initial websocket server
            this._webSocketServer = new WebSocketServer(IPAddress.Parse(AppSingleton.GetInstance.SystemIP), AppSingleton.GetInstance.Port);


            IsStartServiceOnAppLauch = Properties.Settings.Default.is_start_server_on_start_up;
            IsLaunchAppAtStartUp = Properties.Settings.Default.is_run_at_start_up;

            //if (_isWebSocketSeverRunning)
            //{
            //    CustomConfirmDialog customConfirmDialog = new CustomConfirmDialog("Stop The Service?");
            //    customConfirmDialog.OnConfirmClickCallBack += (sender, e) =>
            //    {
            //        this._stopWebSocketServer();
            //        this.mainGrid.Children.Remove((UserControl)sender);
            //    };
            //    this.mainGrid.Children.Add(customConfirmDialog);
            //}
            //else
            //{
            //    this._startWebSocketServer();
            //}
            this.StopCommand = new PCommand(async (p) => await InvokeStopService(p));
        }

        private async Task InvokeStopService(object p)
        {
            try
            {

                LoadingService = true;
                _webSocketServer.RemoveWebSocketService("/");
                _webSocketServer.Stop();
                LoadingService = false;
            }
            finally
            {
                LoadingService=false;
            }
        }

        private string _serverStatus = string.Empty;

        public string ServerStatus
        {
            get { return _serverStatus; }
            set { 
                _serverStatus = value; 
                OnPropertyChanged(nameof(ServerStatus));
            }
        }

        private bool _isServiceRunning = false;

        public bool IsServiceRunning
        {
            get { return _isServiceRunning; }
            set { 
                _isServiceRunning = value;
                OnPropertyChanged(nameof(IsServiceRunning));
            }
        }


        public void StartService()
        {
            //add web socket server listeners
            this._webSocketServer.AddWebSocketService<WebSocketServerListener>("/", () => new WebSocketServerListener((sender, args, connectedClientId, connectedClientIp, connectedClientName) =>
                {
                    //on client connected
                    if (this.WebSocketClients is null)
                        this.WebSocketClients = new List<ClientWebSocketModel>();

                    var temp = this.WebSocketClients;
                    this.WebSocketClients = new List<ClientWebSocketModel>();

                    temp.Add(new ClientWebSocketModel(connectedClientId, connectedClientIp, connectedClientName));
                    this.WebSocketClients = temp;
                },
                (sender, args, clientId, clientName, message, onPrintResponse, onSendToServer, onSendToEveryone) =>
                {
                    //on message received from client
                    //this._onClientResponseMessage(clientId, clientName, message, onPrintResponse, onSendToServer, onSendToEveryone);
                },
                (sender, args, disconnectedClientId) =>
                {
                    //on client disconnected
                    //this._removeDisconnectedWebSocketClientFromListView(disconnectedClientId);
                }
            ));

            this._webSocketServer.Start();
            if (this._webSocketServer.IsListening)
            {
                this.IsServiceRunning = true;
                this.ServerStatus = $"Service on ws://{AppSingleton.GetInstance.SystemIP}:{AppSingleton.GetInstance.Port}";
            }
        }

        public static WebSocketClientViewModel Instance => new WebSocketClientViewModel();

        private ICommand? _startWebSocketServer;
        public ICommand StartWebSocketServer
        {
            get { return _startWebSocketServer ?? new RelayCommand1(this); }
        }
    }

    internal class RelayCommand1 : ICommand
    {
        private WebSocketClientViewModel _webSocketClientViewModel;
        public RelayCommand1(WebSocketClientViewModel webSocketClientViewModel)
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

    public class PCommand : ICommand
    {
        private Action? mAction;

        private Action<object?>? pAction;

        public PCommand(Action<object> pAction)
        {
            this.pAction = pAction;
        }

        public PCommand(Action mAction)
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
