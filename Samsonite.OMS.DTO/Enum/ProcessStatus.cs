using System;

namespace Samsonite.OMS.DTO
{
    /// <summary>
    /// 流程状态
    /// </summary>
    public enum ProcessStatus
    {
        /// <summary>
        /// 取消中
        /// </summary>
        Cancel = 100,
        /// <summary>
        /// 仓库确认
        /// </summary>
        CancelWHSure = 101,
        /// <summary>
        /// 等待退款
        /// </summary>
        WaitRefund = 102,
        /// <summary>
        /// 取消成功
        /// </summary>
        CancelComplete = 110,
        /// <summary>
        /// 取消失败
        /// </summary>
        CancelFail = 111,
        /// <summary>
        /// 退货中
        /// </summary>
        Return = 200,
        /// <summary>
        /// 退货仓库确认
        /// </summary>
        ReturnWHSure = 201,
        /// <summary>
        /// 确认收货
        /// </summary>
        ReturnAcceptComfirm = 202,
        /// <summary>
        /// 退款成功
        /// </summary>
        ReturnComplete = 210,
        /// <summary>
        /// 退款失败
        /// </summary>
        ReturnFail = 211,
        /// <summary>
        /// 换货中
        /// </summary>
        Exchange = 300,
        /// <summary>
        /// 换货仓库确认
        /// </summary>
        ExchangeWHSure = 301,
        /// <summary>
        /// 换货的确认收货
        /// </summary>
        ExchangeAcceptComfirm = 302,
        /// <summary>
        /// 换货成功
        /// </summary>
        ExchangeComplete = 310,
        /// <summary>
        /// 换货失败
        /// </summary>
        ExchangeFail = 311,
        /// <summary>
        /// 修改中
        /// </summary>
        Modify = 400,
        /// <summary>
        /// 修改中
        /// </summary>
        ModifyWHSure = 401,
        /// <summary>
        /// 修改成功
        /// </summary>
        ModifyComplete = 410,
        /// <summary>
        /// 修改失败
        /// </summary>
        ModifyFail = 411,
        /// <summary>
        /// 拒收
        /// </summary>
        Reject = 500,
        /// <summary>
        /// 拒收成功
        /// </summary>
        RejectComplete = 510,
        /// <summary>
        /// 已删除
        /// </summary>
        Delete = 998,
        /// <summary>
        /// 已完结
        /// </summary>
        Complete = 999
    }

    /// <summary>
    /// 退货类型
    /// </summary>
    public enum CollectType
    {
        /// <summary>
        /// 快递
        /// </summary>
        ByExpress = 1,
        /// <summary>
        /// 本人
        /// </summary>
        InPerson = 2
    }
}