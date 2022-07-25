﻿using Newtonsoft.Json.Linq;
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
                RequestTypeModel requestTypeModel = RequestTypeModel.FromJson(message);

                string requestTypeInLowercase = requestTypeModel.RequestType.ToLower();

                switch (requestTypeInLowercase)
                {
                    case "ping":
                        //do ping to client
                        break;
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
                        try
                        {
                            //deserialize message
                            RequestMessageModel requestMessage = RequestMessageModel.FromJson(message);
                            //broadcast to everyone
                            this.WebSocketServer.WebSocketServices["/"].Sessions.Broadcast(requestMessage.Message);

                            //notify back tosender
                            this.WebSocketServer.WebSocketServices["/"].Sessions.SendTo("Message Sent", clientId);
                        }
                        catch (Exception)
                        {
                            //notify back tosender
                            this.WebSocketServer.WebSocketServices["/"].Sessions.SendTo("Message Request Json Syntax Incorrect", clientId);
                        }
                        break;
                    case "sendtoserver":
                        try
                        {
                            //deserialize message
                            RequestMessageModel requestMessageForServer = RequestMessageModel.FromJson(message);

                            //update text status
                            this.ServerStatus += $"\n{clientName} Said : {requestMessageForServer.Message}";

                            //notify back to sender
                            this.WebSocketServer.WebSocketServices["/"].Sessions.SendTo("Message Sent", clientId);
                        }
                        catch (Exception)
                        {
                            //notify back to sender
                            this.WebSocketServer.WebSocketServices["/"].Sessions.SendTo("Message Request Json Syntax Incorrect", clientId);
                        }
                        break;
                    case "print":
                        //process print
                        ProcessPrint(message, clientId);
                        break;
                    default:
                        this.WebSocketServer.WebSocketServices["/"].Sessions.SendTo("incorrect request type", clientId);
                        break;
                }
            }
            catch (Exception ex)
            {
                this.WebSocketServer.WebSocketServices["/"].Sessions.SendTo($"Wrong request json syntax : {ex.Message}", clientId);
            }
        }

        private void ProcessPrint(string message, string clientId)
        {
            try
            {
                //deserialize request print
                RequestPrintMetaModel requestPrintMeta = RequestPrintMetaModel.FromJson(message);
                string printerName = requestPrintMeta.PrintMeta.PrinterName;

                JObject json = JObject.Parse(message);
                var printMeta = json.Last;
                var ff = printMeta.First;
                var printTemplateLayout = ff.Last;
                var printTemplateLayoutString = printTemplateLayout.First;
                var gg = printTemplateLayoutString.ToString();

                string printMethodInLower = requestPrintMeta.PrintMeta.PrintMethod.ToLower();

                try
                {
                    //deserialize print data
                    PrintTemplateModel printTemplateModel = PrintTemplateModel.FromJson("{ 'printTemplateLayout': { 'rows': [ { 'row': { 'rowBorderTop': 2, 'rowBorderRight': 2, 'rowBorderBottom': 2, 'rowBorderLeft': 2, 'rowBackground': 'red', 'columns': [ { 'column': { 'content': 'Row 1 Col 1', 'rowSpan': 2 } }, { 'column': { 'content': 'Row 1 Col 2', 'columnWidth': 200, 'columnBorderTop': 2, 'columnBorderRight': 2, 'columnBorderBottom': 2, 'columnBorderLeft': 2 } } ] } }, { 'row': { 'columns': [ { 'column': { 'content': 'Row 2 Col 2' } } ] } } ] } }");
                    
                    //process print
                    switch (printMethodInLower)
                    {
                        case "printandcut":
                            //do print and cut process
                            _doPrintAndCut(printTemplateModel, printerName, clientId);
                            break;
                        case "printandkickcashdrawer":
                            _doPrintAndKickCashDrawer(printTemplateModel, printerName, clientId);
                            break;
                        case "cut":
                            _doCut(printerName, clientId);
                            break;
                        case "kickcashdrawer":
                            _doKickCashDrawer(printerName, clientId);
                            break;
                    }
                }
                catch (Exception)
                {
                    this.WebSocketServer.WebSocketServices["/"].Sessions.SendTo("Wrong Print Data Format", clientId);
                }
            }
            catch (Exception)
            {
                this.WebSocketServer.WebSocketServices["/"].Sessions.SendTo("Incorrect Print Meta Format", clientId);
            }
        }

        private void _doPrintAndCut(PrintTemplateModel printTemplateModel, string printerName, string clientId, string documentName = "No Name")
        {
            //do print and cut process
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, e) =>
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    LocalPrintServer printServer = new LocalPrintServer();
                    PrintQueueCollection printQueues = printServer.GetPrintQueues();
                    PrintDialog dialog = new PrintDialog();
                    dialog.PrintQueue = printQueues.FirstOrDefault(x => x.Name == printerName);

                    PrintTemplate printTemplate = new PrintTemplate(printTemplateModel);
                    dialog.PrintVisual(printTemplate, documentName);
                });
            };
            worker.RunWorkerCompleted += (s, e) =>
            {
                //Notify Back to sender
                this.WebSocketServer.WebSocketServices["/"].Sessions.SendTo("Print Success", clientId);
            };
            worker.RunWorkerAsync();
        }

        private void _doPrintAndKickCashDrawer(PrintTemplateModel printTemplateModel, string printerName, string clientId, string documentName = "No Name")
        {
            //do print and cut process
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, e) =>
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    LocalPrintServer printServer = new LocalPrintServer();
                    PrintQueueCollection printQueues = printServer.GetPrintQueues();
                    PrintDialog dialog = new PrintDialog();
                    dialog.PrintQueue = printQueues.FirstOrDefault(x => x.Name == printerName);

                    PrintTemplate printTemplate = new PrintTemplate(printTemplateModel);
                    dialog.PrintVisual(printTemplate, documentName);

                    //do kick cash drawer process
                    PrintDocument printDocument = new PrintDocument();
                    printDocument.PrinterSettings.PrinterName = printerName;

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

                    }
                    printDocument.Dispose();
                });
            };
            worker.RunWorkerCompleted += (s, e) =>
            {
                //Notify Back to sender
                this.WebSocketServer.WebSocketServices["/"].Sessions.SendTo("Print Success", clientId);
            };
            worker.RunWorkerAsync();
        }
    
        private void _doKickCashDrawer(string printerName, string clientId)
        {
            PrintDocument printDocument = new PrintDocument();
            printDocument.PrinterSettings.PrinterName = printerName;

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
        }

        private void _doCut(string PrinterName, string clientId)
        {
            PrintDocument printDocumentForCut = new PrintDocument();
            printDocumentForCut.PrinterSettings.PrinterName = PrinterName;

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
        }
    }
}
