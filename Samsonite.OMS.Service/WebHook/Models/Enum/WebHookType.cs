using System;

namespace Samsonite.OMS.Service.WebHook.Models
{
    public enum WebHookPushTarget
    {
        /// <summary>
        /// CRM
        /// </summary>
        CRM = 1
    }

    public enum WebHookPushType
    {
        New = 0,
        Cancel = 1,
        Exchange = 2,
        Return = 3,
        Complete = 99
    }
}
