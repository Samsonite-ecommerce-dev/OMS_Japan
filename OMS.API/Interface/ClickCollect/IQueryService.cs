using System;

using OMS.API.Models.ClickCollect;

namespace OMS.API.Interface.ClickCollect
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
        /// 获取单条订单数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        GetOrderItemsResponse GetOrderItems(GetOrderItemsRequest request);
    }
}