using System;
using System.Collections.Generic;

using OMS.API.Models.Warehouse;

namespace OMS.API.Interface.Platform
{
    public interface IPostService
    {
        /// <summary>
        /// 保存订单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        List<PostOrdersResponse> SaveOrders(string request);
    }
}