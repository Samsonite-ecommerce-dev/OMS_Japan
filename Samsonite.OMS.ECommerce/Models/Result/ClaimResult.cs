using System;

using Samsonite.OMS.DTO;

namespace Samsonite.OMS.ECommerce.Models
{
    public class ClaimResult
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
        /// 流水号
        /// </summary>
        public string RequestID { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public ClaimType ClaimType { get; set; }

        /// <summary>
        /// Sku
        /// </summary>
        public string SKU { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }
    }
}
