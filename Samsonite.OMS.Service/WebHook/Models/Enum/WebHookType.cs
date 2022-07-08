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
        Return = 2,
        Exchange = 3,
        Complete = 99
    }
}
