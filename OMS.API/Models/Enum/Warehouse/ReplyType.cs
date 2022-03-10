using System;

namespace OMS.API.Models.Warehouse
{
    public enum ReplyType
    {
        /// <summary>
        /// 表示订单已经被WNS所获取处理
        /// </summary>
        OrderIsRead = 0,

        /// <summary>
        /// 取消订单
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
        /// 订单修改
        /// </summary>
        Modify = 4,

        ///// <summary>
        ///// 删除取消
        ///// </summary>
        //DeleteCancel = 5,

        ///// <summary>
        ///// 删除退货
        ///// </summary>
        //DeleteReturn = 6,

        ///// <summary>
        ///// 删除换货
        ///// </summary>
        //DeleteExchange = 7,

        ///// <summary>
        ///// 删除换货
        ///// </summary>
        //DeleteModify = 8,

        /// <summary>
        /// 表示需要发货的新订单
        /// </summary>
        NewOrder = 9,

        ///// <summary>
        ///// 紧急订单
        ///// </summary>
        //UrgentOrder = 10
    }
}