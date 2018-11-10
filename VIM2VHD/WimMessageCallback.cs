using System;

namespace VIM2VHD
{
    ///<summary>
    ///User-defined function used with the RegisterMessageCallback or UnregisterMessageCallback function.
    ///</summary>
    ///<param name="MessageId">Specifies the message being sent.</param>
    ///<param name="wParam">Specifies additional message information. The contents of this parameter depend on the value of the
    ///MessageId parameter.</param>
    ///<param name="lParam">Specifies additional message information. The contents of this parameter depend on the value of the
    ///MessageId parameter.</param>
    ///<param name="UserData">Specifies the user-defined value passed to RegisterCallback.</param>
    ///<returns>
    ///To indicate success and to enable other subscribers to process the message return WIM_MSG_SUCCESS.
    ///To prevent other subscribers from receiving the message, return WIM_MSG_DONE.
    ///To cancel an image apply or capture, return WIM_MSG_ABORT_IMAGE when handling the WIM_MSG_PROCESS message.
    ///</returns>
    public delegate uint WimMessageCallback(uint MessageId, IntPtr wParam, IntPtr lParam, IntPtr UserData);
}
