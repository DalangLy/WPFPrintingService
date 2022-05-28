using System.Diagnostics;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace WPFPrintingService
{
    internal delegate void OnOpenCallBack(string clientIp, string clientName);
    internal class WebSocketServerListener : WebSocketBehavior
    {
        private OnOpenCallBack _onOpenCallBack;

        public WebSocketServerListener(OnOpenCallBack onOpenCallBack)
        {
            this._onOpenCallBack = onOpenCallBack;
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
            this._onOpenCallBack(_getClientIP(), _getClientName());
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
