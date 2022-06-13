using System;
using WebSocketSharp;
using WebSocketSharp.Server;
using WPFPrintingService.UICallBackDelegates;

namespace WPFPrintingService
{
    internal class WebSocketServerListener : WebSocketBehavior
    {
        public event OnOpenCallBack? OnClientConnected;
        public event onMessageCallBack? OnMessageCallBack;
        public event OnCloseCallBack? OnClientDisconnected;

        protected override void OnOpen()
        {
            base.OnOpen();
            System.Console.WriteLine(Context.QueryString["name"]);
            System.Console.WriteLine(Sessions);
            System.Console.WriteLine(State);
            System.Console.WriteLine(StartTime);
            System.Console.WriteLine(Protocol);
            System.Console.WriteLine(OriginValidator);
            System.Console.WriteLine(ID);
            System.Console.WriteLine(EmitOnPing);
            if (OnClientConnected == null) return;

            this.OnClientConnected(this, EventArgs.Empty, _getClientId() ,_getClientIP(), _getClientName());

            Send("Connected");
            Sessions.Broadcast($"{_getClientName()} Has Joined");
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);

            if (OnMessageCallBack == null) return;

            this.OnMessageCallBack(
                this, 
                EventArgs.Empty,
                _getClientId(),
                _getClientName(), 
                e.Data,
                (s, ev, status) =>
                {
                    //on print response
                    Send(status);
                },
                (s, ev) =>
                {
                    //on send to server
                    Send("Sent");
                },
                (s, ev, message) =>
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
            if (OnClientDisconnected == null) return;

            this.OnClientDisconnected(this, e, _getClientId());

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
