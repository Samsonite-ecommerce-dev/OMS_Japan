using System;

namespace OMS.API.Models.Warehouse
{
    public class GetOrdersRequest: ApiRequest
    {
        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public string StartUpdateDate { get; set; }

        public string EndUpdateDate { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public string OrderBy { get; set; }
    }
}