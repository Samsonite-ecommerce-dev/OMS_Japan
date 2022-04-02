using System;
using System.Collections.Generic;

namespace OMS.API.Models.Platform
{
    public class GetOrdersDetailRequest : ApiRequest
    {
        /// <summary>
        /// 店铺SAP Code
        /// </summary>
        public string MallSapCode { get; set; }

        /// <summary>
        /// 订单号集合
        /// </summary>
        public List<string> OrderNos { get; set; }
    }
}