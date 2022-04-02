using System;

namespace OMS.API.Models.Platform
{
    public class GetStoresRequest : ApiRequest
    {
        /// <summary>
        /// 门店SAP Code
        /// </summary>
        public string StoreSapCode { get; set; }

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