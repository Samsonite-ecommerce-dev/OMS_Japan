using System;
using System.Collections.Generic;

using OMS.API.Models;
using OMS.API.Models.ClickCollect;

namespace OMS.API.Interface.ClickCollect
{
    public interface IPostService
    {
        /// <summary>
        /// 更新状态(仓库到货)
        /// </summary>
        /// <param name="request"></param>
        List<SetStatusToShopArrivedResponse> SetStatusToShopArrived(string request);

        /// <summary>
        /// 更新状态(取货完成)
        /// </summary>
        /// <param name="request"></param>
        List<SetStatusToDeliveredResponse> SetStatusToDelivered(string request);

        /// <summary>
        /// 门店处理已经取消的到店产品
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        List<CanceledItemHandleResponse> CanceledItemHandle(string request);
    }
}