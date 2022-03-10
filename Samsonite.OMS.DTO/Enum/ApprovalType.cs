using System;

namespace Samsonite.OMS.DTO
{
    /// <summary>
    /// Shoplinker的取消/退货/换货类型
    /// </summary>
    public enum ApprovalType
    {
        /// <summary>
        /// 套装流程
        /// </summary>
        Bunlde = 1,
        /// <summary>
        /// 促销流程
        /// </summary>
        Promotion = 2
    }

    public enum ApprovalIdentify
    {
        /// <summary>
        /// 销售审核
        /// </summary>
        SaleApproval = 1,
        /// <summary>
        /// 仓库审核
        /// </summary>
        WHApproval = 2
    }
}
