using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OMS.API.Models.Warehouse
{
    public class UpdateShipmentStatusRequest
    {
        /// <summary>
        /// 快递号
        /// </summary>
        public string DeliveryNo { get; set; }

        /// <summary>
        /// 快递公司
        /// </summary>
        public string DeliveryCompany { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public string UpdateDate { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>
        public string Remark { get; set; }
    }
}