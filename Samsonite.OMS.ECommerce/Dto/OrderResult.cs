using System;


namespace Samsonite.OMS.ECommerce.Dto
{
    public class OrderResult
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
        /// 日期
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
