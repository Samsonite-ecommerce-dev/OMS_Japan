using System;
using System.Collections.Generic;
using Samsonite.Utility.Common;
using System.Linq;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;

using OMS.API.Interface.Platform;
using OMS.API.Models.Warehouse;
using OMS.API.Models.Platform;

namespace OMS.API.Implments.Platform
{
    public class PostService : IPostService
    {
        #region order
        /// <summary>
        /// 保存订单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public List<PostOrdersResponse> SaveOrders(string request)
        {
            List<PostOrdersResponse> _result = new List<PostOrdersResponse>();
            using (var db = new ebEntities())
            {
                try
                {
                    if (!string.IsNullOrEmpty(request))
                    {
                        var malls = db.Mall.Where(p => p.PlatformCode == (int)PlatformType.TUMI_Japan).ToList();
                        var datas = JsonHelper.JsonDeserialize<List<PostOrdersRequest>>(request);
                        foreach (var item in datas)
                        {
                            try
                            {
                                if (!malls.Exists(p => p.SapCode == item.MallSapCode))
                                {
                                    throw new Exception("The mall dose not exists!");
                                }

                                //返回信息
                                _result.Add(new PostOrdersResponse()
                                {
                                    MallSapCode = item.MallSapCode,
                                    OrderNo = item.OrderNo,
                                    Result = true,
                                    Message = string.Empty
                                });
                            }
                            catch (Exception ex)
                            {
                                _result.Add(new PostOrdersResponse()
                                {
                                    MallSapCode = item.MallSapCode,
                                    OrderNo = item.OrderNo,
                                    Result = false,
                                    Message = ex.Message
                                });
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Please input a request data!");
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return _result;
        }
        #endregion
    }
}