using System;


namespace Samsonite.OMS.ECommerce.Dto
{
    public class PriceResult
    {
        /// <summary>
        /// 店铺SapCode
        /// </summary>
        public string MallSapCode { get; set; }

        /// <summary>
        /// Sku
        /// </summary>
        public string SKU { get; set; }

        /// <summary>
        /// Material-Gridval
        /// </summary>
        public string ProductID { get; set; }

        /// <summary>
        /// 市场价
        /// </summary>
        public decimal MarketPrice { get; set; }

        /// <summary>
        /// 销售价
        /// </summary>
        public decimal SalesPrice { get; set; }
    }
}
