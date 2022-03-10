using System;

namespace OMS.API.Models.Warehouse
{
    public enum PostDetailType
    {
        /// <summary>
        /// 仓库物流状态
        /// </summary>
        WarehouseStatus = 1,

        /// <summary>
        /// 快递详情
        /// </summary>
        ExpressDetail = 2,

        /// <summary>
        /// 发票信息
        /// </summary>
        Invoice = 3
    }
}