using WebSocketSharp;
using WebSocketSharp.Server;

namespace WPFPrintingService
{
    internal delegate void OnOpenCallBack();
    internal class WebSocketServerListener : WebSocketBehavior
    {
        private OnOpenCallBack _onOpenCallBack;

        public WebSocketServerListener(OnOpenCallBack onOpenCallBack)
        {
            this._onOpenCallBack = onOpenCallBack;
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            System.Console.WriteLine(Context);
            System.Console.WriteLine(Sessions);
            System.Console.WriteLine(State);
            System.Console.WriteLine(StartTime);
            System.Console.WriteLine(Protocol);
            System.Console.WriteLine(OriginValidator);
            System.Console.WriteLine(ID);
            System.Console.WriteLine(EmitOnPing);
            this._onOpenCallBack();
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
    }
}
