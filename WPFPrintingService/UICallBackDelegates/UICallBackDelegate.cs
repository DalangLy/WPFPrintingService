using System;

namespace WPFPrintingService.UICallBackDelegates
{
    public delegate void VoidCallBack(object source, EventArgs args);

    public delegate void OnPrintResponse(object source, EventArgs args, string status);
    public delegate void OnSendToEveryone(object source, EventArgs args, string message);
    public delegate void OnSendToServer(object source, EventArgs args);

    public delegate void OnOpenCallBack(object source, EventArgs args, string clientId, string clientIp, string clientName);
    public delegate void onMessageCallBack(object source, EventArgs args, string clientId, string clientName, string message, OnPrintResponse onPrintResponse, OnSendToServer onSendToServer, OnSendToEveryone onSendToEveryone);
    public delegate void OnCloseCallBack(object source, EventArgs args, string clientIp);
}
