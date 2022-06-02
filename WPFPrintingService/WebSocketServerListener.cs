using WebSocketSharp;
using WebSocketSharp.Server;

namespace WPFPrintingService
{
    internal delegate void OnPrintResponse(string status);
    internal delegate void OnSendToEveryone(string message);
    internal delegate void OnSendToServer();

    internal delegate void OnOpenCallBack(string clientId, string clientIp, string clientName);
    internal delegate void onMessageCallBack(string clientId, string clientName, string message, OnPrintResponse onPrintResponse, OnSendToServer onSendToServer, OnSendToEveryone onSendToEveryone);
    internal delegate void OnCloseCallBack(string clientIp);
    internal class WebSocketServerListener : WebSocketBehavior
    {
        private OnOpenCallBack _onOpenCallBack;
        private onMessageCallBack _onMessageCallBack;
        private OnCloseCallBack _onCloseCallBack;

        public WebSocketServerListener(OnOpenCallBack onOpenCallBack, onMessageCallBack onMessageCallBack, OnCloseCallBack onCloseCallBack)
        {
            this._onOpenCallBack = onOpenCallBack;
            this._onMessageCallBack = onMessageCallBack;
            this._onCloseCallBack = onCloseCallBack;
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            //System.Console.WriteLine(Context.QueryString["name"]);
            //System.Console.WriteLine(Sessions);
            //System.Console.WriteLine(State);
            //System.Console.WriteLine(StartTime);
            //System.Console.WriteLine(Protocol);
            //System.Console.WriteLine(OriginValidator);
            //System.Console.WriteLine(ID);
            //System.Console.WriteLine(EmitOnPing);
            this._onOpenCallBack(_getClientId() ,_getClientIP(), _getClientName());
            Send("Connected");
            Sessions.Broadcast($"{_getClientName()} Has Joined");
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);
            this._onMessageCallBack(_getClientId(), _getClientName(), e.Data,
            (status) =>
            {
                //on print response
                Send(status);
            },
            () =>
            {
                //on send to server
                Send("Sent");
            },
            (message) =>
            {
                //on send to everyone
                Sessions.Broadcast(message);
                Send("Sent");
            }
            );
        }

        protected override void OnError(ErrorEventArgs e)
        {
            base.OnError(e);
            Sessions.Broadcast("Server Error");
        }

        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);
            this._onCloseCallBack(_getClientId());
            Sessions.Broadcast($"{_getClientName()} Has Left");
        }

        private string _getClientId()
        {
            return ID;
        }
        private string _getClientIP()
        {
            return Context.RequestUri.Host;
        }
        private string _getClientName()
        {
            string? name = Context.QueryString["name"];

            return name.IsNullOrEmpty() ? "Unknown" : name!;
        }
    }
}
