using System;


namespace Samsonite.OMS.ECommerce.Dto
{
    public class DeliveryResult
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
        /// 快递号
        /// </summary>
        public string InvoiceNo { get; set; }
    }
}
