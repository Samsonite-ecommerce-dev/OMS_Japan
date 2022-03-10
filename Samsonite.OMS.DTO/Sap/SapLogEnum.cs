using System;
using System.Collections.Generic;

using Samsonite.OMS.Database;

namespace Samsonite.OMS.DTO.Sap
{
    /// <summary>
    /// Poslog日志类型
    /// </summary>
    public enum SapLogType
    {
        /// <summary>
        /// 销售记录
        /// </summary>
        KE = 1,
        /// <summary>
        /// 退货记录
        /// </summary>
        KR = 2,
        /// <summary>
        /// 库存调出(Store to Store)
        /// </summary>
        ZKA = 3,
        /// <summary>
        /// 库存调入(Store to Store)
        /// </summary>
        ZKB = 4,
        /// <summary>
        /// 库存退回(Store to Warehouse)
        /// </summary>
        KA = 5,
        /// <summary>
        /// 库存补充(Store to Warehouse)
        /// </summary>
        KB = 6,

        /// <summary>
        /// 包含ZKA/ZKB
        /// </summary>
        Transfer = 90,

        /// <summary>
        /// 包含KE/KR
        /// </summary>
        Transaction = 100
    }

    public enum TransactionItemsType
    {
        /// <summary>
        /// 销售
        /// </summary>
        Sale,
        /// <summary>
        /// Endless Aisle
        /// </summary>
        SendSale,
        /// <summary>
        /// Click & Collect
        /// </summary>
        ClickAndCollect,
        /// <summary>
        /// 退货
        /// </summary>
        Refund
    }

    public enum TaxCodeType
    {
        /// <summary>
        /// 含税
        /// </summary>
        Included = 1,

        /// <summary>
        /// 不含税
        /// </summary>
        NotIncluded = 0
    }

    /// <summary>
    /// 付款类型
    /// </summary>
    public enum PaymentType
    {
        /// <summary>
        /// 现金
        /// </summary>
        CASH,
        /// <summary>
        /// 公司优惠券
        /// </summary>
        CORP_COUPON
    }

    public enum CurrencyId
    {
        /// <summary>
        /// 韩元
        /// </summary>
        KRW,
        /// <summary>
        /// 美元
        /// </summary>
        USD,
        /// <summary>
        /// 日元
        /// </summary>
        JPY,
        /// <summary>
        /// 港币
        /// </summary>
        HKD,
        /// <summary>
        /// 马来西亚令吉
        /// </summary>
        MYR,
        /// <summary>
        /// 人民币
        /// </summary>
        CNY,
        /// <summary>
        /// 澳大利亚元
        /// </summary>
        AUD,
        /// <summary>
        /// 英镑
        /// </summary>
        GBP,
        /// <summary>
        /// 欧元
        /// </summary>
        EUR,
        /// <summary>
        /// 泰铢
        /// </summary>
        THB,
        /// <summary>
        /// 新加坡
        /// </summary>
        SGD
    }

    /// <summary>
    /// Poslog处理状态
    /// </summary>
    public enum SapState
    {
        /// <summary>
        /// 未上传
        /// </summary>
        UnUpload = 0,

        /// <summary>
        /// 已上传到Sap
        /// </summary>
        ToSap = 1,

        /// <summary>
        /// Sap上传失败
        /// </summary>
        Error = 2,

        /// <summary>
        /// Sap上传成功
        /// </summary>
        Success = 3,
    }
}
