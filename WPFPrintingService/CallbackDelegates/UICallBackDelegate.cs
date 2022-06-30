using System;

namespace WPFPrintingService.UICallBackDelegates
{
    public delegate void VoidCallBack(object source, EventArgs args);

    internal delegate void OnPrintResponse(object source, EventArgs args, string status);
    internal delegate void OnSendToEveryone(object source, EventArgs args, string message);
    internal delegate void OnSendToServer(object source, EventArgs args);

    internal delegate void OnOpenCallBack(object source, EventArgs args, string clientId, string clientIp, string clientName);
    internal delegate void onMessageCallBack(object source, EventArgs args, string clientId, string clientName, string message);
    internal delegate void OnCloseCallBack(object source, EventArgs args, string clientIp);
}
