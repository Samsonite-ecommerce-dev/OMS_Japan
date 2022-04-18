using System;


namespace Samsonite.OMS.ECommerce.Result
{
    public class PoslogResult
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
        /// 类型
        /// </summary>
        public int LogType { get; set; }
    }
}
