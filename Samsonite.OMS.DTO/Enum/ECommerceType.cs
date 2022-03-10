using System;

namespace Samsonite.OMS.DTO
{
    public enum ECommerceDocumentType
    {
        /// <summary>
        /// 发货文档
        /// </summary>
        InvoiceDoc = 1,
        /// <summary>
        /// 快递文档
        /// </summary>
        ShippingDoc = 2,
        /// <summary>
        /// 装货单
        /// </summary>
        ManifestDoc = 3
    }

    public enum ECommercePushType
    {
        /// <summary>
        /// 获取快递号
        /// </summary>
        RequireTrackingCode = 1,
        /// <summary>
        /// 推送快递号
        /// </summary>
        PushTrackingCode = 2,
        /// <summary>
        /// 推送库存
        /// </summary>
        PushInventory = 3,
        /// <summary>
        /// 推送警告库存
        /// </summary>
        PushWarningInventory = 4,
        /// <summary>
        /// 推送价格
        /// </summary>
        PushPrice = 5
    }
}
