using System;
using System.Collections.Generic;

namespace OMS.API.Models.Warehouse
{
    /// <summary>
    /// 其它信息提交
    /// </summary>
    public class PostDetailRequest
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

        /// <summary>
        /// 类型对应详情
        /// </summary>
        public object Data { get; set; }
    }
}