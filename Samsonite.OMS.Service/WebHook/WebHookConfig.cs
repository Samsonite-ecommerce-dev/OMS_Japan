﻿using System;

namespace Samsonite.OMS.Service.WebHook
{
    /// <summary>
    /// 
    /// </summary>
    public class WebHookConfig
    {
        //读取最近90天的数据
        public const int timeAgo = -90;
    }

    public class CRMApiConfig
    {
        public const string ApiUrl = "http://oms-exp-api-tumi-apac.us-e2.cloudhub.io";

        public const string UserName = "tomsapi";

        public const string Password = "0msapi@220720";
    }
}
