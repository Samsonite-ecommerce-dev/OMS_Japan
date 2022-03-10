using System;
using System.Collections.Generic;

namespace Samsonite.OMS.DTO
{
    /// <summary>
    /// 订单修改信息
    /// </summary>
    public class ClaimInfoDto
    {
        /// <summary>
        /// 订单id
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 子订单id
        /// </summary>
        public string SubOrderNo { get; set; }

        /// <summary>
        /// 店铺名称
        /// </summary>
        public string MallName { get; set; }

        /// <summary>
        /// 店铺名称Id
        /// </summary>
        public string MallSapCode { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        ///  订单价格
        /// </summary>
        public decimal OrderPrice { get; set; }

        /// <summary>
        ///  订单价格
        /// </summary>
        public string SKU { get; set; }

        /// <summary>
        /// 状态描述
        /// </summary>
        //public string ClaimStatus { get; set; }

        /// <summary>
        /// 操作类型状态
        /// </summary>
        public ClaimType ClaimType { get; set; }

        /// <summary>
        /// 信息类型
        /// </summary>
        public int ClaimReason { get; set; }

        /// <summary>
        /// 信息
        /// </summary>
        public string ClaimMemo { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ClaimDate { get; set; }

        /// <summary>
        /// 平台
        /// </summary>
        public int PlatformID { get; set; }

        /// <summary>
        /// 流程ID
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// 退货方式
        /// </summary>
        public int CollectionType { get; set; }

        /// <summary>
        /// 退货人
        /// </summary>
        public string CollectName { get; set; }

        /// <summary>
        /// 退货联系方式
        /// </summary>
        public string CollectPhone { get; set; }

        /// <summary>
        /// 退货地址
        /// </summary>
        public string CollectAddress { get; set; }

        /// <summary>
        /// 1.全部取消时候分摊的退款运费
        /// 2.退款时候的所产生的分摊取货运费
        /// </summary>
        public decimal ExpressFee { get; set; }

        /// <summary>
        /// 附加费用
        /// </summary>
        public decimal SurchargeFee { get; set; }

        /// <summary>
        /// Claim缓存ID
        /// </summary>
        public long CacheID { get; set; }
    }
}
