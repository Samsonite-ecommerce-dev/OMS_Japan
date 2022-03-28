using System;

namespace OMS.API.Models.Warehouse
{
    /// <summary>
    /// 更新仓库状态
    /// </summary>
    public class UpdateWMSStatusResponse : BaseResponse
    {
        /// <summary>
        /// 店铺sapCode
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
        /// 状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>
        public string Remark { get; set; }
    }
}