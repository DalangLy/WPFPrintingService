﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WPFPrintingService
{
    internal class SendMessageToAllClientsViewModel : BaseViewModel
    {
        //create singleton of this class
        private static SendMessageToAllClientsViewModel? _instance;
        public static SendMessageToAllClientsViewModel Instance
        {
            get { return _instance ?? (_instance = new SendMessageToAllClientsViewModel()); }
        }

        public ICommand SendMessageToAllClientsCommand { get; set; }
        public ICommand CloseSentMessageDialogCommand { get; set; }

        private bool _isSendingMessageToAllClients;

        public bool IsSendingMessageToAllClients
        {
            get { return _isSendingMessageToAllClients; }
            set
            {
                _isSendingMessageToAllClients = value;
                OnPropertyChanged();
            }
        }

        private bool _isSentMessageToAllClientsSuccess;

        public bool IsSentMessageToAllClientsSuccess
        {
            get { return _isSentMessageToAllClientsSuccess; }
            set
            {
                _isSentMessageToAllClientsSuccess = value;
                OnPropertyChanged();
            }
        }

        //inject websocket view model
        private WebSocketServerViewModel _webSocketClientViewModel;

        private SendMessageToAllClientsViewModel()
        {
            this._webSocketClientViewModel = WebSocketServerViewModel.Instance;

            this.SendMessageToAllClientsCommand = new SendMessageToAllClientCommand(async (p) => await InvokeSendMessageToAllClient(p));
            this.CloseSentMessageDialogCommand = new CloseSentMessageDialogCommand(this);
        }

        private async Task InvokeSendMessageToAllClient(object p)
        {
            Debug.WriteLine(p);
            if (p == null) return;
            string message = (string)p;
            this.IsSendingMessageToAllClients = true; //show sending progress dialog
            await Task.Delay(1000 * 5);//delay 5 seconds
            this._webSocketClientViewModel.WebSocketServer.WebSocketServices["/"].Sessions.Broadcast(message);
            this.IsSendingMessageToAllClients = false; //remove sending progress dialog
            this.IsSentMessageToAllClientsSuccess = true;//show sent dialog
        }
    }

    internal class CloseSentMessageDialogCommand : ICommand
    {
        private SendMessageToAllClientsViewModel _instance;

        public CloseSentMessageDialogCommand(SendMessageToAllClientsViewModel sendMessageToWebAllSocketClientsViewModel)
        {
            _instance = sendMessageToWebAllSocketClientsViewModel;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            this._instance.IsSentMessageToAllClientsSuccess = false;
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
}