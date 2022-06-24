﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using WebSocketSharp;
using WebSocketSharp.Server;
using WPFPrintingService.UICallBackDelegates;
using System.Windows.Controls;
using System.Printing;
using System.Linq;
using WPFPrintingService.Print_Templates;
using System.Diagnostics;
using System.ComponentModel;
using WPFPrintingService.Print_Models;

namespace WPFPrintingService
{
    public partial class MainWindow : Window
    {
        private List<ClientWebSocketModel> _allConnectedWebSocketClients = new List<ClientWebSocketModel>();
        private WebSocketServer? _webSocketServer;
        private List<PrinterFromWindowsSystemModel> _allPrintersFromWindowsSystem = new List<PrinterFromWindowsSystemModel>();
        private bool _isWebSocketSeverRunning = false;
        public BorderViewModel ViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new BorderViewModel();

            this.DataContext = ViewModel;
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //initialize websocket server
            this._setupWebSocketServer();

            //load all printer from windows system
            this._loadAllPrintersFromWindowsSystem();

            //bind list of all connected websocket clients to list view
            this.lvConnectWebSocketClients.ItemsSource = _allConnectedWebSocketClients;

            //check if run on start up button is enable
            this._checkIfRunAtStartUpEnable();

            //check if start service on start up
            this._checkIfStartServiceAtStartUpIsEnable();
        }

        private void _setupWebSocketServer()
        {
            this._webSocketServer = new WebSocketServer(IPAddress.Parse(AppSingleton.GetInstance.SystemIP), AppSingleton.GetInstance.Port);
        }

        private void _loadAllPrintersFromWindowsSystem()
        {
            foreach (PrintQueue printer in GetAllSystemPrintersSingleton.GetInstance.Printers)
            {
                //Debug.WriteLine("\tThe shared printer : " + printer.Name);
                //Debug.WriteLine("\tHas Tonner: " + printer.HasToner);
                //Debug.WriteLine("\tIs Busy: " + printer.IsBusy);
                //Debug.WriteLine("\tIs Door Open: " + printer.IsDoorOpened);
                //Debug.WriteLine("\tIs Offline: " + printer.IsOffline);
                //Debug.WriteLine("\tIs Offline: " + printer.IsPrinting);
                _allPrintersFromWindowsSystem.Add(new PrinterFromWindowsSystemModel(printer.Name));
            }

            dgPrinters.ItemsSource = _allPrintersFromWindowsSystem;
        }

        private void _checkIfRunAtStartUpEnable()
        {
            bool _isRunAtStartUp = Properties.Settings.Default.is_run_at_start_up;
            chbRunOnStartUp.IsChecked = _isRunAtStartUp;
        }

        private void _checkIfStartServiceAtStartUpIsEnable()
        {
            bool _isStartServiceAtStartUp = Properties.Settings.Default.is_start_server_on_start_up;
            chbAutoRunService.IsChecked = _isStartServiceAtStartUp;
            if (_isStartServiceAtStartUp)
            {
                _startWebSocketServer();
            }
        }
        
        private void btnStartStopServer_Click(object sender, RoutedEventArgs e)
        {
            if (_isWebSocketSeverRunning)
            {
                CustomConfirmDialog customConfirmDialog = new CustomConfirmDialog("Stop The Service?");
                customConfirmDialog.OnConfirmClickCallBack += (sender, e) =>
                {
                    this._stopWebSocketServer();
                    this.mainGrid.Children.Remove((UserControl)sender);
                };
                this.mainGrid.Children.Add(customConfirmDialog);
            }
            else
            {
                this._startWebSocketServer();
            }
        }

        private void _startWebSocketServer()
        {
            if (this._webSocketServer == null) return;

            //add web socket server listeners
            this._webSocketServer.AddWebSocketService<WebSocketServerListener>("/", () => new WebSocketServerListener((sender, args, connectedClientId, connectedClientIp, connectedClientName) =>
                {
                    //on client connected
                    this._addConnectedWebSocketClientToListView(connectedClientId, connectedClientIp, connectedClientName);
                },
                (sender, args, clientId, clientName, message, onPrintResponse, onSendToServer, onSendToEveryone) =>
                {
                    //on message received from client
                    this._onClientResponseMessage(clientId, clientName, message, onPrintResponse, onSendToServer, onSendToEveryone);
                },
                (sender, args, disconnectedClientId) =>
                {
                    //on client disconnected
                    this._removeDisconnectedWebSocketClientFromListView(disconnectedClientId);
                }
            ));

            this._webSocketServer.Start();

            this.btnStartStopServer.Content = "Stop";
            this.txtServerStatus.Text = $"Service on ws://{AppSingleton.GetInstance.SystemIP}:{AppSingleton.GetInstance.Port}";
            this._isWebSocketSeverRunning = true;
        }

        private void _stopWebSocketServer()
        {
            if (this._webSocketServer == null) return;

            this._webSocketServer.RemoveWebSocketService("/");

            this._webSocketServer.Stop(CloseStatusCode.Away, "Server Stop");

            this.btnStartStopServer.Content = "Start";
            this.txtServerStatus.Text = "Server Stopped";
            this._isWebSocketSeverRunning = false;
        }

        private void _addConnectedWebSocketClientToListView(string id, string ip, string name)
        {
            lvConnectWebSocketClients.Dispatcher.BeginInvoke(new Action(() =>
            {
                this._allConnectedWebSocketClients.Add(new ClientWebSocketModel(id, ip, name));
                this.lvConnectWebSocketClients.ItemsSource = this._allConnectedWebSocketClients;
                this.lvConnectWebSocketClients.Items.Refresh();

                //update text status
                this.txtServerStatus.Text += $"\n{name} has joined";
            }), DispatcherPriority.Background);
        }

        private void _removeDisconnectedWebSocketClientFromListView(string disconnectedClientId)
        {
            lvConnectWebSocketClients.Dispatcher.BeginInvoke(new Action(() =>
            {
                //find disconnected client from list
                ClientWebSocketModel? _disconnectedWebSocketClient = _allConnectedWebSocketClients.Find(webSocketClient => webSocketClient.Id == disconnectedClientId);
                if (_disconnectedWebSocketClient == null) return;

                //remove disconnected client from list and listview
                this._allConnectedWebSocketClients.Remove(_disconnectedWebSocketClient);
                this.lvConnectWebSocketClients.ItemsSource = _allConnectedWebSocketClients;
                this.lvConnectWebSocketClients.Items.Refresh();

                //update text status
                this.txtServerStatus.Text += $"\n{_disconnectedWebSocketClient.Name} has Left";
            }), DispatcherPriority.Background);
        }

        private void _onClientResponseMessage(string clientId, string clientName, string message, OnPrintResponse onPrintResponse, OnSendToServer onSendToServer, OnSendToEveryone onSendToEveryone)
        {
            try
            {
                PrintTemplateModel printTemplateModel = PrintTemplateModel.FromJson(message);
                if (printTemplateModel == null)
                {
                    onPrintResponse(this, EventArgs.Empty, "Wrong Format");
                    return;
                }
                switch (printTemplateModel.Code)
                {
                    case "RequestPrinters":
                        if(_webSocketServer != null && _webSocketServer.IsListening)
                        {
                            var json = System.Text.Json.JsonSerializer.Serialize(_allPrintersFromWindowsSystem);
                            _webSocketServer.WebSocketServices["/"].Sessions.SendTo(json, clientId);
                        }
                        break;
                    case "SendToEveryone":
                        onSendToEveryone(this, EventArgs.Empty, $"{clientName} Said : {printTemplateModel.Data}");
                        break;
                    case "SendToServer":

                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            //update text status
                            txtServerStatus.Text += $"\n {clientName} Said : {printTemplateModel.Data}";

                        }), DispatcherPriority.Background);

                        onSendToServer(this, EventArgs.Empty);
                        break;
                    case "Print":
                        try
                        {
                            if (printTemplateModel?.Data == null)
                            {
                                onPrintResponse(this, EventArgs.Empty, "Wrong Data Format");
                                return;
                            }

                            //find printer
                            PrinterFromWindowsSystemModel? _foundPrinterModel = _allPrintersFromWindowsSystem.Find(printerModel => printerModel.PrinterName.Equals(printTemplateModel?.Data?.PrinterName));
                            if (_foundPrinterModel == null)
                            {
                                onPrintResponse(this, EventArgs.Empty, "Can't Find Printer");
                                return;
                            }

                            switch (printTemplateModel?.Data?.PrintMethod)
                            {
                                case "CutOnly":
                                    this._cutOnly(printTemplateModel?.Data.PrinterName, clientId);
                                    break;
                                case "OpenCashDrawer":
                                    this._kickCashDrawer(printTemplateModel?.Data.PrinterName, clientId);
                                    break;
                                case "PrintAndKickCashDrawer":
                                    this._printAndKickCashDrawer(printTemplateModel?.Data.PrinterName, printTemplateModel?.Data?.PrintModel, clientId);
                                    break;
                                default:
                                    this._printAndCut(printTemplateModel?.Data.PrinterName, printTemplateModel?.Data?.PrintModel, clientId);
                                    break;
                            }
                        }
                        catch (Exception)
                        {

                            throw;
                        }

                        break;
                    default:
                        onPrintResponse(this, EventArgs.Empty, "Wrong Code");
                        break;
                }
            }
            catch (Exception ex)
            {
                onPrintResponse(this, EventArgs.Empty, $"Wrong Format : {ex.Message}");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //hide windows to system tray
            Hide();
            myNotifyIcon.Visibility = Visibility.Visible;
            e.Cancel = true;
        }

        private void btnExitPrintingServiceViaSystemTray_Click(object sender, RoutedEventArgs e)
        {
            this._shutdownThisApplication();
        }

        private void _shutdownThisApplication()
        {
            Application.Current.Shutdown();
        }

        private void myNotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            //hide windows to system tray
            Show();
            myNotifyIcon.Visibility = Visibility.Collapsed;
        }

        private void chbRunOnStartUp_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.is_run_at_start_up = true;
            Properties.Settings.Default.Save();

            //set run on start up
            RegistryKey? key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            Assembly curAssembly = Assembly.GetExecutingAssembly();
            //key.SetValue(curAssembly.GetName().Name, curAssembly.Location);
            key!.SetValue("DX Printing Service", curAssembly.Location);
        }

        private void chbRunOnStartUp_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.is_run_at_start_up = false;
            Properties.Settings.Default.Save();

            //remove run on start up
            RegistryKey? key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            Assembly curAssembly = Assembly.GetExecutingAssembly();
            //key.SetValue(curAssembly.GetName().Name, curAssembly.Location);
            key!.DeleteValue("DX Printing Service");
        }

        private void btnSendToEveryClients_Click(object sender, RoutedEventArgs e)
        {
            if (this._webSocketServer == null) return;
            if (!this._webSocketServer.IsListening)
            {
                MessageBox.Show("Start Service First");
                return;
            }
            if(this._allConnectedWebSocketClients.Count < 1)
            {
                MessageBox.Show("No Client Connected");
                return;
            }


            this._webSocketServer.WebSocketServices["/"].Sessions.Broadcast(txtMessage.Text);
            
            SnackbarFive.IsActive = true;
            //this.mainGrid.Children.Add(new CustomMessageDialog());
        }

        private void btnRefreshPrinterList_Click(object sender, RoutedEventArgs e)
        {
            GetAllSystemPrintersSingleton.GetInstance.RefreshPrinters();
        }

        private void chbAutoRunService_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.is_start_server_on_start_up = true;
            Properties.Settings.Default.Save();
        }

        private void chbAutoRunService_Unchecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.is_start_server_on_start_up = true;
            Properties.Settings.Default.Save();
        }

        private void btnPrintAndCut_Click(object sender, RoutedEventArgs e)
        {
            PrinterFromWindowsSystemModel? printer = ((FrameworkElement)sender).DataContext as PrinterFromWindowsSystemModel;
            if (printer == null) return;



            //LocalPrintServer printServer = new LocalPrintServer();
            //PrintQueueCollection printQueuesOnLocalServer = printServer.GetPrintQueues();
            //PrintDialog printDialog = new PrintDialog();
            //printDialog.PrintQueue = printQueuesOnLocalServer.FirstOrDefault(x => x.Name == printer.PrinterName);
            //printDialog.PrintVisual(new CashDrawerTemplate(
            //    new List<GG>() { 
            //        new GG(){ Title = "KHR"},
            //        new GG(){ Title = "USD"},
            //    },
            //    DateTime.Now.ToShortDateString()
            //    ), "Cash Drawer");
        }

        private void btnPrintAndKickDrawer_Click(object sender, RoutedEventArgs e)
        {
            PrinterFromWindowsSystemModel? printer = ((FrameworkElement)sender).DataContext as PrinterFromWindowsSystemModel;
            if (printer == null) return;

            PrintDocument printDocument = new PrintDocument();
            printDocument.PrintPage += (o, ev) =>
            {
                if (ev.Graphics == null) return;
                ev.Graphics.DrawString(
                    "Print Test",
                    new Font("Arial", 10),
                    Brushes.Black,
                    ev.MarginBounds.Left,
                    0,
                    new StringFormat()
                );
            };
            printDocument.PrinterSettings.PrinterName = printer.PrinterName;
            printDocument.EndPrint += (o, ev) =>
            {
                //open cash drawer command
                const string ESC1 = "\u001B";
                const string p = "\u0070";
                const string m = "\u0000";
                const string t1 = "\u0025";
                const string t2 = "\u0250";
                const string openTillCommand = ESC1 + p + m + t1 + t2;
                RawPrinterHelper.SendStringToPrinter(printDocument.PrinterSettings.PrinterName, openTillCommand);

                MessageBox.Show("Print Success");
            };
            printDocument.Print();
            printDocument.Dispose();
        }

        private void btnPrintOnly_Click(object sender, RoutedEventArgs e)
        {
            //PrinterFromWindowsSystemModel? printer = ((FrameworkElement)sender).DataContext as PrinterFromWindowsSystemModel;
            //if (printer == null) return;

            MessageBox.Show("Sorry this feature is in progress");
        }

        private void btnCutOnly_Click(object sender, RoutedEventArgs e)
        {
            PrinterFromWindowsSystemModel? printer = ((FrameworkElement)sender).DataContext as PrinterFromWindowsSystemModel;
            if (printer == null) return;

            PrintDocument printDocument = new PrintDocument();
            printDocument.PrinterSettings.PrinterName = printer.PrinterName;

            //cut command
            string GS = Convert.ToString((char)29);
            string ESC = Convert.ToString((char)27);
            string COMMAND = "";
            COMMAND = ESC + "@";
            COMMAND += GS + "V" + (char)1;
            bool _cutted = RawPrinterHelper.SendStringToPrinter(printDocument.PrinterSettings.PrinterName, COMMAND);
            if (_cutted)
            {
                MessageBox.Show("Cut Success");
            }
            printDocument.Dispose();
        }

        private void btnKickDrawer_Click(object sender, RoutedEventArgs e)
        {
            PrinterFromWindowsSystemModel? printer = ((FrameworkElement)sender).DataContext as PrinterFromWindowsSystemModel;
            if (printer == null) return;

            PrintDocument printDocument = new PrintDocument();
            printDocument.PrinterSettings.PrinterName = printer.PrinterName;

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
                MessageBox.Show("Cash Drawer Opened");
            }
            printDocument.Dispose();
        }

        //default windows print functions
        private void _printAndCut(string printerName, PrintModel printModel, string clientId)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    LocalPrintServer printServer = new LocalPrintServer();
                    PrintQueueCollection printQueues = printServer.GetPrintQueues();
                    CashDrawerTemplate c = new CashDrawerTemplate(
                         printModel.Prices.ToList(),
                         printModel.Date
                    );
                    PrintDialog dialog = new PrintDialog();
                    dialog.PrintQueue = printQueues.FirstOrDefault(x => x.Name == "Microsoft Print to PDF");
                    dialog.PrintVisual(c, "Cash Drawer");
                }));
            };
            worker.RunWorkerCompleted += (s, e) =>
            {
                Debug.WriteLine("PrintSuccess");
            };
            worker.RunWorkerAsync();
        }

        private void _printAndKickCashDrawer(string printerName, PrintModel printModel, string clientId)
        {
            //print and kick out cash drawer using default windows print document
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    LocalPrintServer printServer = new LocalPrintServer();
                    PrintQueueCollection printQueues = printServer.GetPrintQueues();
                    CashDrawerTemplate c = new CashDrawerTemplate(
                         printModel.Prices.ToList(),
                         printModel.Date
                    );
                    PrintDialog dialog = new PrintDialog();
                    dialog.PrintQueue = printQueues.FirstOrDefault(x => x.Name == "Microsoft Print to PDF");
                    dialog.PrintVisual(c, "Cash Drawer");
                }));
            };
            worker.RunWorkerCompleted += (s, e) =>
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
                    if (_webSocketServer != null && _webSocketServer.IsListening)
                        _webSocketServer.WebSocketServices["/"].Sessions.SendTo("Cash Drawer Opened", clientId);
                }
                printDocument.Dispose();
            };
            worker.RunWorkerAsync();
        }

        private void _cutOnly(string printerName, string clientId)
        {
            //cut only using default windows print document

            PrintDocument printDocument = new PrintDocument();
            printDocument.PrinterSettings.PrinterName = printerName;

            //cut command
            string GS = Convert.ToString((char)29);
            string ESC = Convert.ToString((char)27);
            string COMMAND = "";
            COMMAND = ESC + "@";
            COMMAND += GS + "V" + (char)1;
            bool _cutted = RawPrinterHelper.SendStringToPrinter(printDocument.PrinterSettings.PrinterName, COMMAND);
            if (_cutted)
            {
                if (_webSocketServer != null && _webSocketServer.IsListening)
                    _webSocketServer.WebSocketServices["/"].Sessions.SendTo("Cut Success", clientId);
            }
            printDocument.Dispose();
        }

        private void _kickCashDrawer(string printerName, string clientId)
        {
            //kick out cash drawer using default windows print document

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
                if (_webSocketServer != null && _webSocketServer.IsListening)
                    _webSocketServer.WebSocketServices["/"].Sessions.SendTo("Cash Drawer Opened", clientId);
            }
            printDocument.Dispose();
        }

        private void DialogHost_DialogClosing(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {
            if (eventArgs.Parameter == null) return;
            bool isExit = (bool) eventArgs.Parameter;
            if (isExit)
                _shutdownThisApplication();
        }
    }

    public class BorderViewModel : INotifyPropertyChanged
    {
        private bool borderVisible = true;

        public bool BorderVisible
        {
            get
            {
                return borderVisible;
            }

            set
            {
                borderVisible = value;
                NotifyPropertyChanged("BorderVisible");
            }
        }

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
