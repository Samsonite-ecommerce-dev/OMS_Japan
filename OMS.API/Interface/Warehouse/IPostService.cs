using System;
using System.Collections.Generic;

using OMS.API.Models.Warehouse;

namespace OMS.API.Interface.Warehouse
{
    public interface IPostService
    {
        /// <summary>
        /// 更新库存
        /// </summary>
        /// <param name="inventorys"></param>
        /// <param name="reduceQuantitys"></param>
        /// <returns></returns>
        List<PostInventoryResponse> SaveInventorys(List<PostInventoryRequest> inventorys, Dictionary<string, int> reduceQuantitys);

        /// <summary>
        /// 更新快递号
        /// </summary>
        /// <param name="deliverys"></param>
        /// <returns></returns>
        List<PostDeliverysResponse> SaveDeliverys(List<PostDeliverysRequest> deliverys);

        /// <summary>
        /// 回复操作状态
        /// </summary>
        /// <param name="postReplys"></param>
        /// <returns></returns>
        List<PostReplyResponse> SavePostReplys(List<PostReplyRequest> postReplys);

        /// <summary>
        /// 更新物流状态
        /// </summary>
        /// <param name="shipmentStatus"></param>
        /// <returns></returns>
        List<UpdateShipmentStatusResponse> SaveShipmentStatus(List<UpdateShipmentStatusRequest> shipmentStatus);

        /// <summary>
        /// 更新仓库状态
        /// </summary>
        /// <param name="wmsStatus"></param>
        /// <returns></returns>
        List<UpdateWMSStatusResponse> SaveWMSStatus(List<UpdateWMSStatusRequest> wmsStatus);
    }
}