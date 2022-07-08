using System;


namespace Samsonite.OMS.ECommerce.Models
{
    public class ExpressResult
    {
        /// <summary>
        /// 店铺SapCode
        /// </summary>
        public string MallSapCode { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 子订单号
        /// </summary>
        public string SubOrderNo { get; set; }

        /// <summary>
        /// 子订单的物流状态或者平台状态
        /// </summary>
        public string ExpressStatus { get; set; }
    }
}
