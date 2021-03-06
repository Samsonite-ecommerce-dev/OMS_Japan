using System;

namespace OMS.API.Models.Warehouse
{
    public class PostDeliverysResponse : BaseResponse
    {
        /// <summary>
        /// 店铺code
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
        /// 快递号对应类型
        /// 0.普通订单的快递号
        /// 2.换货订单的新快递号
        public int DeliveryType { get; set; }

        /// <summary> 
        /// 快递公司
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// 快递单号(多个快递号用逗号隔开)
        /// </summary>
        public string DeliveryNo { get; set; }
    }
}