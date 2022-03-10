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
        /// <param name="objPostInventoryRequestList"></param>
        /// <param name="objReduceQuantitys"></param>
        /// <returns></returns>
        List<PostInventoryResponse> SaveInventorys(List<PostInventoryRequest> objPostInventoryRequestList, Dictionary<string, int> objReduceQuantitys);

        /// <summary>
        /// 更新快递号
        /// </summary>
        /// <param name="objPostDeliverysRequestList"></param>
        /// <returns></returns>
        List<PostDeliverysResponse> SaveDeliverys(List<PostDeliverysRequest> objPostDeliverysRequestList);

        /// <summary>
        /// 回复信息
        /// </summary>
        /// <param name="objPostReplyRequestList"></param>
        /// <returns></returns>
        List<PostReplyResponse> SavePostReplys(List<PostReplyRequest> objPostReplyRequestList);

        /// <summary>
        /// 其它回复信息
        /// </summary>
        /// <param name="objPostDetailRequestList"></param>
        /// <returns></returns>
        List<PostDetailResponse> SaveReplyDetails(List<PostDetailRequest> objPostDetailRequestList);
    }
}