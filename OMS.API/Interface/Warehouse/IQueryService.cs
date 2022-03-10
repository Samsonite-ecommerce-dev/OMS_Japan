using System;

using OMS.API.Models.Warehouse;

namespace OMS.API.Interface.Warehouse
{
    public interface IQueryService
    {
        /// <summary>
        /// 订单集合列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        GetOrdersResponse GetOrders(GetOrdersRequest request);

        /// <summary>
        /// 编辑/取消/退货/预售订单/换货新订单列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        GetChangedOrdersResponse GetChangedOrders(GetChangedOrdersRequest request);
    }
}