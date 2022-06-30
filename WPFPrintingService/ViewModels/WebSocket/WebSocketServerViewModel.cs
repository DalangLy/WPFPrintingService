using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Linq;
using System.Net;
using System.Printing;
using System.Windows.Controls;
using WebSocketSharp.Server;
using WPFPrintingService.Models;

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

        private bool _isServiceRunning;
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
            this.WebSocketServer.KeepClean = false;
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
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        //add client to list
                        Debug.WriteLine("Add");
                        this.ConnectedWebSocketClients.Add(new ClientWebSocketModel() { Name = connectedClientName, ID = connectedClientId, IP = connectedClientIp });
                        this.ServerStatus += $"\n{connectedClientName} has joined";
                    });
                },
                (sender, args, clientId, clientName, message) =>
                {
                    //on message received from client
                    this._onClientResponseMessage(clientId, clientName, message);
                },
                (sender, args, disconnectedClientId) =>
                {
                    //on client disconnected
                    App.Current.Dispatcher?.Invoke((Action)delegate
                    {
                        //remove client from list
                        ClientWebSocketModel foundClient = this.ConnectedWebSocketClients.Where(x => x.ID == disconnectedClientId).First();
                        this.ConnectedWebSocketClients.Remove(foundClient);
                        this.ServerStatus += $"\n{foundClient.Name} has left";
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

        private void _onClientResponseMessage(string clientId, string clientName, string message)
        {
            try
            {
                RequestCode requestCode = RequestCode.FromJson(message);

                if (requestCode.Code == null) return;

                switch (requestCode.Code.ToLower())
                {
                    case "requestprinterslist":
                        if (WebSocketServer != null && WebSocketServer.IsListening)
                        {
                            List<PrinterModel> printers = new List<PrinterModel>();
                            LocalPrintServer printServer = new LocalPrintServer();
                            PrintQueueCollection printQueues = printServer.GetPrintQueues();
                            foreach (PrintQueue printer in printQueues)
                            {
                                printers.Add(new PrinterModel()
                                {
                                    Name = printer.Name,
                                    HasToner = printer.HasToner,
                                    IsBusy = printer.IsBusy,
                                    IsDoorOpened = printer.IsDoorOpened,
                                    IsOnline = !printer.IsOffline,
                                    IsPrinting = printer.IsPrinting,
                                });
                            }
                            Debug.WriteLine("Hllll");
                            var json = System.Text.Json.JsonSerializer.Serialize(printers);
                            this.WebSocketServer.WebSocketServices["/"].Sessions.SendTo(json, clientId);

                            //update status
                            this.ServerStatus += $"\nClient Request Printers List";
                        }
                        break;
                    case "sendtoeveryone":
                        //deserialize message
                        RequestMessage requestMessage = RequestMessage.FromJson(message);

                        //broadcast to everyone
                        this.WebSocketServer.WebSocketServices["/"].Sessions.Broadcast(requestMessage.Message);

                        //notify back tosender
                        this.WebSocketServer.WebSocketServices["/"].Sessions.SendTo("Message Sent", clientId);
                        break;
                    case "sendtoserver":
                        
                        //deserialize message
                        RequestMessage requestMessageForServer = RequestMessage.FromJson(message);

                        //update text status
                        this.ServerStatus += $"\n{clientName} Said : {requestMessageForServer.Message}";

                        //notify back to sender
                        this.WebSocketServer.WebSocketServices["/"].Sessions.SendTo("Message Sent", clientId);
                        break;
                    case "print":
                        //process print
                        ProcessPrint(message, clientId);
                        break;
                }
            }
            catch (Exception ex)
            {
                this.WebSocketServer.WebSocketServices["/"].Sessions.SendTo($"Wrong Format : {ex.Message}", clientId);
            }
        }

        private void ProcessPrint(string message, string clientId)
        {
            //deserialize request print
            RequestPrint requestPrint = RequestPrint.FromJson(message);

            //deserialize request print data
            RequestPrintData requestPrintData = RequestPrintData.FromJson(requestPrint.Data.ToString());

            //check print template
            //switch (requestPrintData.TemplateName.ToLower())
            //{
            //    case "bill":
            //        //deserialize print data
            //        //RequestBillPrintData requestBillPrintData = RequestBillPrintData.FromJson(message);

            //        //create instance of bill print template from user control


            //        break;
            //}

            //check print method
            switch (requestPrintData.PrintMethod.ToLower())
            {
                case "printandcut":
                    //do print and cut process

                    BackgroundWorker worker = new BackgroundWorker();
                    worker.DoWork += (s, e) =>
                    {
                        App.Current.Dispatcher.Invoke((Action)delegate
                        {
                            LocalPrintServer printServer = new LocalPrintServer();
                            PrintQueueCollection printQueues = printServer.GetPrintQueues();
                            PrintDialog dialog = new PrintDialog();
                            dialog.PrintQueue = printQueues.FirstOrDefault(x => x.Name == "Microsoft Print to PDF");
                            TestPrintTemplate printTemplate = new TestPrintTemplate();
                            dialog.PrintVisual(printTemplate, "Test Print Template");
                        });
                    };
                    worker.RunWorkerCompleted += (s, e) =>
                    {
                        //Notify Back to sender
                        this.WebSocketServer.WebSocketServices["/"].Sessions.SendTo("Print Success", clientId);
                    };
                    worker.RunWorkerAsync();

                    break;
                case "cut":
                    PrintDocument printDocumentForCut = new PrintDocument();
                    printDocumentForCut.PrinterSettings.PrinterName = "Printer Name";

                    //cut command
                    string GS = Convert.ToString((char)29);
                    string ESC = Convert.ToString((char)27);
                    string COMMAND = "";
                    COMMAND = ESC + "@";
                    COMMAND += GS + "V" + (char)1;
                    bool _cutted = RawPrinterHelper.SendStringToPrinter(printDocumentForCut.PrinterSettings.PrinterName, COMMAND);
                    if (_cutted)
                    {
                        this.WebSocketServer.WebSocketServices["/"].Sessions.SendTo("Cut Success", clientId);
                    }
                    printDocumentForCut.Dispose();
                    break;
                case "printandkickcashdrawer":
                    break;
                case "kickcashdrawer":
                    PrintDocument printDocument = new PrintDocument();
                    printDocument.PrinterSettings.PrinterName = "Printer Name";

                    //open cash drawer command
                    const string ESC1 = "\u001B";
                    const string p = "\u0070";
                    const string m = "\u0000";
                    const string t1 = "\u0025";
                    const string t2 = "\u0250";
                    const string openTillCommand = ESC1 + p + m + t1 + t2;
                    bool _cashDrawerOpened = RawPrinterHelper.SendStringToPrinter(printDocument.PrinterSettings.PrinterName, openTillCommand);
                    if (_cashDrawerOpened)
                    {
                        this.WebSocketServer.WebSocketServices["/"].Sessions.SendTo("Cash Drawer Opened", clientId);
                    }
                    printDocument.Dispose();
                    break;
            }
        }
    }
}
