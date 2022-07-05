using System;

namespace Samsonite.OMS.DTO
{
    /// <summary>
    /// 订单状态
    /// </summary>
    public enum OrderStatus
    {
        New = 1,
        /// <summary>
        /// 表示订单已经在被 WMS 处理或者订单处理中（表示订单正在被Cancel,Return,Exchange 处理中）
        /// </summary>
        Processing = 100,
        Complete = 999,
    }

    /// <summary>
    /// 产品状态
    /// </summary>
    public enum ProductStatus
    {
        /// <summary>
        /// 未付款
        /// </summary>
        UnPaid = -999,
        /// <summary>
        /// 已关闭
        /// </summary>
        Close = -10,
        ///// <summary>
        ///// 待处理 
        ///// </summary>
        //Pending = -1,
        /// <summary>
        /// 新商品
        /// </summary>
        Received = 0,
        /// <summary>
        /// 仓库已处理
        /// </summary>
        Processing = 1,
        /// <summary>
        /// 已发货
        /// </summary>
        InDelivery = 2,
        /// <summary>
        /// 店铺确认(O2O)
        /// </summary>
        //Acknowledge = 2,
        /// <summary>
        /// 运输中(O2O)
        /// </summary>
        //InTransit = 3,
        /// <summary>
        /// 门店到货(C&C)
        /// </summary>
        ReceivedGoods = 4,
        /// <summary>
        /// 客户已取货(O2O)
        /// </summary>
        //PickupGoods = 5,
        /// <summary>
        /// 已发货完成(客户收到货物)
        /// </summary>
        Delivered = 10,
        /// <summary>
        /// 取消
        /// </summary>
        Cancel = 100,
        /// <summary>
        /// 取消成功
        /// </summary>
        CancelComplete = 199,
        /// <summary>
        /// 退货
        /// </summary>
        Return = 200,
        /// <summary>
        /// 退货成功
        /// </summary>
        ReturnComplete = 299,
        /// <summary>
        /// 换货
        /// </summary>
        Exchange = 300,
        /// <summary>
        /// 换货新发产品
        /// </summary>
        ExchangeNew = 301,
        /// <summary>
        /// 换货成功
        /// </summary>
        ExchangeComplete = 399,
        /// <summary>
        /// 编辑
        /// </summary>
        Modify = 400,
        /// <summary>
        /// 拒收
        /// </summary>
        Reject = 500,
        /// <summary>
        /// 换货成功
        /// </summary>
        RejectComplete = 599,
        /// <summary>
        /// 已完结
        /// </summary>
        Complete = 999
    }
}
