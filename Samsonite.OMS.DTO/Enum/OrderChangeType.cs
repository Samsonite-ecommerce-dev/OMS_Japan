// -----------------------------------------------------------------------
//  <copyright file="OrderChangeType" company="Samsonite">
//      Copyright (c) 2014-2015 Samsonite. All rights reserved.
//  </copyright>
//  <last-editor>alanwang</last-editor>
//  <last-date>2015/10/15 11:02:23</last-date>
// -----------------------------------------------------------------------
using System;

namespace Samsonite.OMS.DTO
{
    public enum OrderChangeType
    {
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

        /// <summary>
        /// 表示需要发货的新订单
        /// </summary>
        NewOrder = 9
    }
}
