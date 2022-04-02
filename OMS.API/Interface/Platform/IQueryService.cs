using System;

using OMS.API.Models.Platform;

namespace OMS.API.Interface.Platform
{
    public interface IQueryService
    {
        /// <summary>
        /// 线下店铺集合列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        GetStoresResponse GetStores(GetStoresRequest request);

        /// <summary>
        /// 获取订单详情信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        GetOrdersDetailResponse GetOrdersDetail(GetOrdersDetailRequest request);

        /// <summary>
        /// 获取库存信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        GetInventorysResponse GetInventorys(GetInventorysRequest request);
    }
}