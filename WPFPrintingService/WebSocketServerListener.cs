using System.Diagnostics;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace WPFPrintingService
{
    internal delegate void OnOpenCallBack(string clientId, string clientIp, string clientName);
    internal delegate void OnCloseCallBack(string clientIp);
    internal class WebSocketServerListener : WebSocketBehavior
    {
        private OnOpenCallBack _onOpenCallBack;
        private OnCloseCallBack _onCloseCallBack;

        public WebSocketServerListener(OnOpenCallBack onOpenCallBack, OnCloseCallBack onCloseCallBack)
        {
            this._onOpenCallBack = onOpenCallBack;
            _onCloseCallBack = onCloseCallBack;
        }

        protected override void OnOpen()
        {
            Debug.WriteLine("Open Success");
            base.OnOpen();
            System.Console.WriteLine(Context.QueryString["name"]);
            System.Console.WriteLine(Sessions);
            System.Console.WriteLine(State);
            System.Console.WriteLine(StartTime);
            System.Console.WriteLine(Protocol);
            System.Console.WriteLine(OriginValidator);
            System.Console.WriteLine(ID);
            System.Console.WriteLine(EmitOnPing);
            this._onOpenCallBack(_getClientId() ,_getClientIP(), _getClientName());
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);
        }

        protected override void OnError(ErrorEventArgs e)
        {
            base.OnError(e);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);
            this._onCloseCallBack(_getClientId());
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
