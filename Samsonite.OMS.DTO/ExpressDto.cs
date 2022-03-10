using System;

namespace Samsonite.OMS.DTO
{
    /// <summary>
    /// 快递状态信息
    /// </summary>
    public class ExpressStatusDto
    {
        /// <summary>
        /// 快递号
        /// </summary>
        public string ConsignmentNo { get; set; }

        /// <summary>
        /// 关联订单号
        /// </summary>
        public string RefOrderNO { get; set; }

        /// <summary>
        /// 状态号
        /// </summary>
        public string StatusCode { get; set; }

        /// <summary>
        /// 状态描述
        /// </summary>
        public string StatusDesc { get; set; }

        /// <summary>
        /// 状态更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 系统状态号
        /// </summary>
        public int ExpressStatus { get; set; }
    }
}
