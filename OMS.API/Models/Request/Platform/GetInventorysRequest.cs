using System;
using System.Collections.Generic;

namespace OMS.API.Models.Platform
{
    public class GetInventorysRequest : ApiRequest
    {
        /// <summary>
        /// 店铺SAP Code
        /// </summary>
        public string StoreSapCode { get; set; }

        /// <summary>
        /// Demandware的产品集合
        /// 例如:at-132660-1195
        /// </summary>
        public string ProductIds { get; set; }

        /// <summary>
        /// 当前页
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 页码
        /// </summary>
        public int PageSize { get; set; }
    }
}