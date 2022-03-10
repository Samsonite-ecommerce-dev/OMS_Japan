using System;

namespace Samsonite.OMS.DTO
{
    /// <summary>
    /// 退款状态
    /// </summary>
    public enum RefundStatus
    {
        /// <summary>
        /// 未处理
        /// </summary>
        UnDeal = 0,
        /// <summary>
        /// 处理中
        /// </summary>
        Dealing = 1,
        /// <summary>
        /// 退款成功
        /// </summary>
        Finish = 2,
        /// <summary>
        /// 退款失败
        /// </summary>
        Refuse = 3,
        /// <summary>
        /// 退款关闭
        /// </summary>
        Close = 4

    }
}