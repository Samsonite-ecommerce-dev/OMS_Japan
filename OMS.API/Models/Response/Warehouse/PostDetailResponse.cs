using System;

namespace OMS.API.Models.Warehouse
{
    /// <summary>
    /// 物流状态
    /// </summary>
    public class PostShippingStatusResponse
    {
        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; }
    }

    /// <summary>
    /// 物流详情
    /// </summary>
    public class PostExpressDetailResponse
    {
        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 详情
        /// </summary>
        public string Detail { get; set; }
    }

    /// <summary>
    /// 其它信息提交结果
    /// </summary>
    public class PostDetailResponse : BaseResponse
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
        /// 类型
        /// </summary>
        public int Type { get; set; }
    }
}