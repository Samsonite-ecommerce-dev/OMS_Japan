using System;


namespace Samsonite.OMS.ECommerce.Result
{
    public class InventoryResult
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
        /// 数量
        /// </summary>
        public int Quantity { get; set; }
    }
}
