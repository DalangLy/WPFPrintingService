using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Linq;
using System.Net;
using System.Printing;
using System.Windows;
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

        public WebSocketServer? WebSocketServer;

        private bool _isDeviceWirelessConnectionFailed;

        public bool IsDeviceWirelessConnectionFailed
        {
            get { return _isDeviceWirelessConnectionFailed; }
            set { 
                _isDeviceWirelessConnectionFailed = value;
                OnPropertyChanged();
            }
        }


        private WebSocketServerViewModel()
        {
            //initial websocket server
            try
            {
                IsDeviceWirelessConnectionFailed = false;

                this.WebSocketServer = new WebSocketServer(IPAddress.Parse(AppSingleton.GetInstance.SystemIP), AppSingleton.GetInstance.Port);
                this.WebSocketServer.KeepClean = false;
            }
            catch (Exception)
            {
                IsDeviceWirelessConnectionFailed = true;
            }
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
            if (this.WebSocketServer == null) return;
            //add web socket server listeners
            this.WebSocketServer.AddWebSocketService<WebSocketServerListener>("/", () => new WebSocketServerListener((sender, args, connectedClientId, connectedClientIp, connectedClientName) =>
                {
                    //on client connected
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        //add client to list
                        this.ConnectedWebSocketClients.Add(new ClientWebSocketModel() { Name = connectedClientName, ID = connectedClientId, IP = connectedClientIp });
                        this.ServerStatus += $"\n{connectedClientName} has joined";

                        //broadcast to everyone
                        for (int eachClientIndex = 0; eachClientIndex < ConnectedWebSocketClients.Count; eachClientIndex++)
                        {
                            string eachId = ConnectedWebSocketClients[eachClientIndex].ID;
                            if (eachId == connectedClientId) continue;
                            this.WebSocketServer.WebSocketServices["/"].Sessions.SendTo($"{connectedClientName} Has Joined", eachId);
                        }
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
                RequestTypeModel? requestTypeModel = RequestTypeModel.FromJson(message);
                if (requestTypeModel == null) throw new CustomException("json must be valid");

                string requestType = requestTypeModel.RequestType.ToLower();
                switch (requestType)
                {
                    case "print":
                        _checkPrintMethod(message, clientId);
                        break;
                    case "sendtoeveryone":
                        _sentMessageToEveryone(message, clientId, clientName);
                        break;
                    case "sendtoserver":
                        _sentMessageToServer(clientName, message, clientId);
                        break;
                    case "requestprinterslist":
                        _sendPrinterListToRequestedClient(clientId, clientName);
                        break;
                    case "ping":
                        _pingFromClient(clientId, clientName);
                        break;
                    default:
                        throw new CustomException("the request code is not valid");
                }
            }
            catch (CustomException ex)
            {
                this.WebSocketServer.WebSocketServices["/"].Sessions.SendTo(ex.Message, clientId);
            }
            catch (Exception ex)
            {
                this.WebSocketServer.WebSocketServices["/"].Sessions.SendTo($"Wrong request json syntax : {ex.Message}", clientId);
            }
        }

        private void _pingFromClient(string clientId, string clientName)
        {
            //update text status
            this.ServerStatus += $"\nPing From : {clientName}";

            //notify back to sender
            this.WebSocketServer.WebSocketServices["/"].Sessions.SendTo("Server Replied", clientId);
        }

        private void _sentMessageToEveryone(string message, string clientId, string clientName)
        {
            try
            {
                MessageModel? messageModel = MessageModel.FromJson(message);
                if (messageModel == null) throw new CustomException("message must be valid");

                //update text status
                this.ServerStatus += $"\n{clientName} Has Sent Message to Everyone : {messageModel.Message}";

                //broadcast to everyone
                for (int eachClientIndex = 0; eachClientIndex < ConnectedWebSocketClients.Count; eachClientIndex++)
                {
                    string eachId = ConnectedWebSocketClients[eachClientIndex].ID;
                    if (eachId == clientId) continue;
                    this.WebSocketServer.WebSocketServices["/"].Sessions.SendTo($"{clientName} Said : {messageModel.Message}", eachId);
                }

                //notify back tosender
                this.WebSocketServer.WebSocketServices["/"].Sessions.SendTo("Message Sent to Everyone", clientId);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CustomException($"wrong message syntax : {ex.Message}");
            }
        }

        private void _sentMessageToServer(string clientName, string message, string clientId)
        {
            try
            {
                MessageModel? messageModel = MessageModel.FromJson(message);
                if (messageModel == null) throw new CustomException("message must be valid");


                //update text status
                this.ServerStatus += $"\n{clientName} Said : {messageModel.Message}";

                //notify back to sender
                this.WebSocketServer.WebSocketServices["/"].Sessions.SendTo("Message Sent to Server", clientId);
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CustomException($"wrong message syntax : {ex.Message}");
            }
        }

        private void _sendPrinterListToRequestedClient(string clientId, string clientName)
        {
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
                var json = System.Text.Json.JsonSerializer.Serialize(printers);
                this.WebSocketServer.WebSocketServices["/"].Sessions.SendTo(json, clientId);

                //update status
                this.ServerStatus += $"\n{clientName} Request Printers List";
            }
        }

        private void _checkPrintMethod(string message, string clientId)
        {
            try
            {
                PrintMetaModel? myPrintMetaModel = PrintMetaModel.FromJson(message);
                if (myPrintMetaModel == null) throw new CustomException("print meta must be valid");

                string printerName = myPrintMetaModel.PrintMeta.PrinterName;
                if (printerName == "") throw new CustomException("printer name must be valid");
                //find printer by name
                ///
                ///

                string printMethod = myPrintMetaModel.PrintMeta.PrintMethod.ToLower();
                if (printMethod == "") throw new CustomException("print method must be valid");

                //deserialize print template layout
                JObject json = JObject.Parse(message);
                JToken? printMeta = json["printMeta"];
                if (printMeta == null) throw new CustomException("invalid print meta");

                //check if print template layout is not valid
                JToken? printTemplateLayoutObject = printMeta["printTemplateLayout"];
                if (printTemplateLayoutObject == null || printTemplateLayoutObject.First == null) throw new CustomException("invalid print template layout");



                switch (printMethod)
                {
                    case "printandcut":
                        _printAndCut(printTemplateLayoutObject.ToString(), printerName, clientId);
                        break;
                    case "cut":
                        _doCut(printerName, clientId);
                        break;
                    case "printandkickcashdrawer":
                        _printAndKickCashDrawer(printMeta.ToString(), printerName, clientId);
                        break;
                    case "kickcashdrawer":
                        _doKickCashDrawer(printerName, clientId);
                        break;
                    default:
                        throw new CustomException("invalid print method");
                }
            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CustomException($"wrong print meta syntax : {ex.Message}");
            }
        }

        private void _printAndCut(string printTemplateLayout, string printerName, string clientId)
        {
            try
            {
                //deserialize print layout model
                PrintTemplateLayoutModel? printTemplateLayoutModel = PrintTemplateLayoutModel.FromJson(printTemplateLayout);
                if (printTemplateLayoutModel == null) throw new CustomException("Print Template Layout must be valid");


                //process print in background
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += (s, e) =>
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        try
                        {
                            LocalPrintServer printServer = new LocalPrintServer();
                            PrintQueueCollection printQueues = printServer.GetPrintQueues();
                            PrintDialog dialog = new PrintDialog();
                            PrintQueue? selectedPrinter = printQueues.FirstOrDefault(x => x.Name == printerName);
                            if (selectedPrinter == null) throw new CustomException("Printer Not Found");
                            dialog.PrintQueue = selectedPrinter;

                            PrintTemplate printTemplate = new PrintTemplate(printTemplateLayoutModel);
                            printTemplate.Measure(new Size(Double.MaxValue, Double.MaxValue));
                            Size visualSize = printTemplate.DesiredSize;
                            printTemplate.Arrange(new Rect(new Point(0, 0), visualSize));
                            printTemplate.UpdateLayout();
                            dialog.PrintTicket.PageMediaSize = new PageMediaSize(visualSize.Width, visualSize.Height);
                            dialog.PrintVisual(printTemplate, "Print Document");
                        }
                        catch(CustomException ex)
                        {
                            e.Cancel = true;
                            //Notify Back to sender
                            this.WebSocketServer.WebSocketServices["/"].Sessions.SendTo(ex.Message, clientId);
                        }
                        catch (Exception ex)
                        {
                            e.Cancel = true;
                            //Notify Back to sender
                            this.WebSocketServer.WebSocketServices["/"].Sessions.SendTo($"Print Failed : {ex.Message}", clientId);
                        }
                    });
                };
                worker.RunWorkerCompleted += (s, e) =>
                {
                    if (e.Cancelled) return;
                    //Notify Back to sender
                    this.WebSocketServer.WebSocketServices["/"].Sessions.SendTo("Print and Cut Finished", clientId);
                };
                worker.RunWorkerAsync();


            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CustomException($"wrong print template layout syntax : {ex.Message}");
            }
        }

        private void _printAndKickCashDrawer(string printMetaJsonString, string printerName, string clientId)
        {
            try
            {
                //deserialize print layout model
                PrintTemplateLayoutModel? printTemplateLayoutModel = PrintTemplateLayoutModel.FromJson(printMetaJsonString);
                if (printTemplateLayoutModel == null) throw new CustomException("Print Template Layout must be valid");




                //process print in background
                //do print and cut process
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += (s, e) =>
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        try
                        {
                            LocalPrintServer printServer = new LocalPrintServer();
                            PrintQueueCollection printQueues = printServer.GetPrintQueues();
                            PrintDialog dialog = new PrintDialog();
                            PrintQueue? selectedPrinter = printQueues.FirstOrDefault(x => x.Name == printerName);
                            if (selectedPrinter == null) throw new CustomException("Printer Not Found");
                            dialog.PrintQueue = selectedPrinter;

                            PrintTemplate printTemplate = new PrintTemplate(printTemplateLayoutModel);
                            printTemplate.Measure(new Size(Double.MaxValue, Double.MaxValue));
                            Size visualSize = printTemplate.DesiredSize;
                            printTemplate.Arrange(new Rect(new Point(0, 0), visualSize));
                            printTemplate.UpdateLayout();
                            dialog.PrintTicket.PageMediaSize = new PageMediaSize(visualSize.Width, visualSize.Height);
                            dialog.PrintVisual(printTemplate, "Print Document");

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
                        }
                        catch (CustomException ex)
                        {
                            e.Cancel = true;
                            //Notify Back to sender
                            this.WebSocketServer.WebSocketServices["/"].Sessions.SendTo(ex.Message, clientId);
                        }
                        catch (Exception ex)
                        {
                            e.Cancel = true;
                            //Notify Back to sender
                            this.WebSocketServer.WebSocketServices["/"].Sessions.SendTo($"Print Failed : {ex.Message}", clientId);
                        }
                    });
                };
                worker.RunWorkerCompleted += (s, e) =>
                {
                    if (e.Cancelled) return;
                    //Notify Back to sender
                    this.WebSocketServer.WebSocketServices["/"].Sessions.SendTo("Print And Kick Cash Drawer Success", clientId);
                };
                worker.RunWorkerAsync();


            }
            catch (CustomException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CustomException($"wrong print template layout syntax : {ex.Message}");
            }
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
