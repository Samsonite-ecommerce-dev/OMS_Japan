using System;

namespace OMS.API.Models.ClickCollect
{
    public class GetOrdersRequest: ApiRequest
    {
        /// <summary>
        /// 门店SAP Code
        /// </summary>
        public string ShopSapCode { get; set; }

        /// <summary>
        /// 创建开始时间
        /// </summary>
        public string CreatedAfter { get; set; }

        /// <summary>
        /// 创建结束时间
        /// </summary>
        public string CreatedBefore { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 产品状态
        /// </summary>
        public int ProductStatus { get; set; }

        /// <summary>
        /// 编辑开始时间
        /// </summary>
        public string UpdateAfter { get; set; }

        /// <summary>
        /// 编辑结束时间
        /// </summary>
        public string UpdateBefore { get; set; }

        /// <summary>
        /// 当前页
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 页码
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 排序:asc/desc
        /// </summary>
        public string SortBy { get; set; }
    }
}