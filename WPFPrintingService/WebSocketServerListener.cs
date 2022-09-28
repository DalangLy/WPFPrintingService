using System;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace WPFPrintingService
{
    internal class WebSocketServerListener : WebSocketBehavior
    {
        private OnOpenCallBack _onClientConnected;
        private onMessageCallBack _onMessageCallBack;
        public event OnCloseCallBack _onClientDisconnected;

        public WebSocketServerListener(OnOpenCallBack onClientConnected, onMessageCallBack onMessageCallBack, OnCloseCallBack onClientDisconnected)
        {
            this._onClientConnected = onClientConnected;
            this._onMessageCallBack = onMessageCallBack;
            this._onClientDisconnected = onClientDisconnected;
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            //Debug.WriteLine(Context.QueryString["name"]);
            //Debug.WriteLine(Sessions);
            //Debug.WriteLine(State);
            //Debug.WriteLine(StartTime);
            //Debug.WriteLine(Protocol);
            //Debug.WriteLine(OriginValidator);
            //Debug.WriteLine(ID);
            //Debug.WriteLine(EmitOnPing);


            this._onClientConnected(this, EventArgs.Empty, _getClientId() ,_getClientIP(), _getClientName());

            //notify back to client
            Send("Connected");
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);

            this._onMessageCallBack(
                this, 
                EventArgs.Empty,
                _getClientId(),
                _getClientName(), 
                e.Data
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
            
            this._onClientDisconnected(this, e, _getClientId());

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
