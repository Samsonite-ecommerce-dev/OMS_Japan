using System;

namespace OMS.API.Models.Warehouse
{
    public class PostDeliverysRequest
    {
        /// <summary>
        /// 店铺sapcode
        /// </summary>
        public string MallCode { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 子订单号
        /// </summary>
        public string SubOrderNo { get; set; }

        /// <summary> 
        /// sku
        /// </summary>
        public string Sku { get; set; }

        /// <summary>
        ///快递公司编码
        /// </summary>
        public string DeliveryCode { get; set; }

        /// <summary> 
        /// 快递公司
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// 快递单号(多个快递号用逗号隔开)
        /// </summary>
        public string DeliveryNo { get; set; }

        /// <summary>
        /// 包裹数量
        /// </summary>
        public int Packages { get; set; }

        /// <summary>
        /// 快递类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 快递费用
        /// </summary>
        public decimal ReceiveCost { get; set; }

        /// <summary>
        /// 发货仓库
        /// </summary>
        public string Warehouse { get; set; }

        /// <summary>
        /// 接单时间
        /// </summary>
        public string ReceiveDate { get; set; }

        /// <summary>
        /// 理货时间
        /// </summary>
        public string DealDate { get; set; }

        /// <summary>
        /// 快递发货时间
        /// </summary>
        public string SendDate { get; set; }
    }
}