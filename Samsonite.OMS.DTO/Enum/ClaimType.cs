using System;

namespace Samsonite.OMS.DTO
{
    /// <summary>
    /// Shoplinker的取消/退货/换货类型
    /// </summary>
    public enum ClaimType
    {
        /// <summary>
        /// 其它
        /// </summary>
        Unknow = 0,
        /// <summary>
        /// 取消
        /// </summary>
        Cancel = 1,
        /// <summary>
        /// 换货
        /// </summary>
        Exchange = 2,
        /// <summary>
        /// 退货
        /// </summary>
        Return = 3,
        /// <summary>
        /// 拒收
        /// </summary>
        Reject = 4
    }
}
