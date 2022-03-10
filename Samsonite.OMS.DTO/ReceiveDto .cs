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
    /// 收货信息-Dto
    /// </summary>
    public class ReceiveDto
    {
        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 子订单号
        /// </summary>
        public string SubOrderNo { get; set; }

        /// <summary>
        /// 收货人
        /// </summary>
        public string Receiver { get; set; }

        /// <summary>
        /// 用户邮箱
        /// </summary>
        public string ReceiverEmail { get; set; }

        /// <summary>
        /// 联系方式
        /// </summary>
        public string Tel { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        public string Mobile { get; set; }


        /// <summary>
        /// 邮编
        /// </summary>
        public string ZipCode { get; set; }

        /// <summary>
        /// 收货城市
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// 收货城市
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// 收货地址
        /// </summary>
        public string Address { get; set; }
    }
}
