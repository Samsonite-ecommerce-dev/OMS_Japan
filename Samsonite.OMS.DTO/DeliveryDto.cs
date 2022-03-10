// -----------------------------------------------------------------------
//  <copyright file="Delivery" company="Samsonite">
//      Copyright (c) 2014-2015 Samsonite. All rights reserved.
//  </copyright>
//  <last-editor>alanwang</last-editor>
//  <last-date>2015/9/15 9:33:03</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samsonite.OMS.DTO
{
    /// <summary>
    /// 物流信息-Dto
    /// </summary>
    public class DeliveryDto
    {
        public string Id { get; set; }

        /// <summary>
        /// 门店SapCode
        /// </summary>
        public string MallSapCode { get; set; }

        /// <summary>
        /// 订单Id
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 订单Id
        /// </summary>
        public string SubOrderNo { get; set; }

        /// <summary>
        /// 快递公司名称
        /// </summary>
        public string DeliveryName { get; set; }

        /// <summary>
        /// 快递公司编号
        /// </summary>
        public string DeliveryCode { get; set; }

        /// <summary>
        /// 发送日期
        /// </summary>
        public string DeliveryDate { get; set; }

        /// <summary>
        /// 快递单号
        /// </summary>
        public string DeliveryInvoice { get; set; }

        /// <summary>
        /// json 返回操作结果
        /// </summary>
        public bool Result { get; set; }

        /// <summary>
        /// json 返回操作信息
        /// </summary>
        public string ResultMsg { get; set; }
    }
}
