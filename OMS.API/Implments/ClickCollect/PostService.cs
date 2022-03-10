using System;
using System.Collections.Generic;
using System.Linq;

using OMS.API.Models.ClickCollect;
using OMS.API.Interface.ClickCollect;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.Utility.Common;
using Samsonite.OMS.Service;

namespace OMS.API.Implments.ClickCollect
{
    public class PostService : IPostService
    {

        /// <summary>
        /// 更新状态(仓库到货)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public List<SetStatusToShopArrivedResponse> SetStatusToShopArrived(string request)
        {
            List<SetStatusToShopArrivedResponse> _result = new List<SetStatusToShopArrivedResponse>();
            using (var db = new ebEntities())
            {
                try
                {
                    if (!string.IsNullOrEmpty(request))
                    {
                        var datas = JsonHelper.JsonDeserialize<List<SetStatusToShopArrivedRequest>>(request);
                        foreach (var item in datas)
                        {
                            try
                            {
                                var objView_OrderDetail = db.View_OrderDetail.Where(p => p.OrderNo == item.OrderNo && p.SubOrderNo == item.SubOrderNo && p.OrderType == (int)OrderType.ClickCollect).SingleOrDefault();
                                if (objView_OrderDetail != null)
                                {
                                    OrderService.OrderStatus_InDeliveryToReceivedGoods(objView_OrderDetail, "The store had received the product", db);

                                    //返回信息
                                    _result.Add(new SetStatusToShopArrivedResponse()
                                    {
                                        OrderNo = item.OrderNo,
                                        SubOrderNo = item.SubOrderNo,
                                        Result = true,
                                        Message = string.Empty
                                    });
                                }
                                else
                                {
                                    throw new Exception("The Order dose not exsits!");
                                }
                            }
                            catch (Exception ex)
                            {
                                //返回信息
                                _result.Add(new SetStatusToShopArrivedResponse()
                                {
                                    OrderNo = item.OrderNo,
                                    SubOrderNo = item.SubOrderNo,
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

        /// <summary>
        /// 更新状态(取货完成)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public List<SetStatusToDeliveredResponse> SetStatusToDelivered(string request)
        {
            List<SetStatusToDeliveredResponse> _result = new List<SetStatusToDeliveredResponse>();
            using (var db = new ebEntities())
            {
                if (!string.IsNullOrEmpty(request))
                {
                    var datas = JsonHelper.JsonDeserialize<List<SetStatusToShopArrivedRequest>>(request);
                    foreach (var item in datas)
                    {
                        try
                        {
                            var objView_OrderDetail = db.View_OrderDetail.Where(p => p.OrderNo == item.OrderNo && p.SubOrderNo == item.SubOrderNo && p.OrderType == (int)OrderType.ClickCollect).SingleOrDefault();
                            if (objView_OrderDetail != null)
                            {
                                OrderService.OrderStatus_ReceivedGoodsToDelivered(objView_OrderDetail, "The customer had taken the product", db);
                                //返回信息
                                _result.Add(new SetStatusToDeliveredResponse()
                                {
                                    OrderNo = item.OrderNo,
                                    SubOrderNo = item.SubOrderNo,
                                    Result = true,
                                    Message = string.Empty
                                });
                            }
                            else
                            {
                                throw new Exception("The Order dose not exsits!");
                            }
                        }
                        catch (Exception ex)
                        {
                            //返回信息
                            _result.Add(new SetStatusToDeliveredResponse()
                            {
                                OrderNo = item.OrderNo,
                                SubOrderNo = item.SubOrderNo,
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
            return _result;
        }

        /// <summary>
        /// 更新状态(取货完成)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public List<CanceledItemHandleResponse> CanceledItemHandle(string request)
        {
            List<CanceledItemHandleResponse> _result = new List<CanceledItemHandleResponse>();

            using (var db = new ebEntities())
            {
                if (!string.IsNullOrEmpty(request))
                {
                    var datas = JsonHelper.JsonDeserialize<List<CanceledItemHandleRequest>>(request);
                    foreach (var item in datas)
                    {
                        try
                        {
                            if (!(new List<int>() { 1, 2 }).Contains(item.OperType))
                            {
                                throw new Exception("Oper Type is incorrect!");
                            }

                            //如果留店,则完成cancel流程
                            if (item.OperType == 1)
                            {
                                var objView_OrderDetail = db.View_OrderDetail.Where(p => p.OrderNo == item.OrderNo && p.SubOrderNo == item.SubOrderNo && p.OrderType == (int)OrderType.ClickCollect).SingleOrDefault();
                                if (objView_OrderDetail != null)
                                {
                                    View_OrderCancel objView_OrderCancel = db.View_OrderCancel.Where(p => p.OrderNo == objView_OrderDetail.OrderNo && p.SubOrderNo == objView_OrderDetail.SubOrderNo && p.Type == (int)OrderChangeType.Cancel).OrderByDescending(p => p.Id).FirstOrDefault();
                                    if (objView_OrderCancel != null)
                                    {
                                        OrderCancelProcessService.WHResponse(objView_OrderCancel.ChangeID, (int)WarehouseStatus.DealSuccessful, DateTime.Now, "", db);
                                    }
                                    else
                                    {
                                        throw new Exception("The Cancel request dose not exsits!");
                                    }
                                }
                                else
                                {
                                    throw new Exception("The Order dose not exsits!");
                                }
                            }
                            else
                            {

                            }
                            //返回信息
                            _result.Add(new CanceledItemHandleResponse()
                            {
                                OrderNo = item.OrderNo,
                                SubOrderNo = item.SubOrderNo,
                                OperType = item.OperType,
                                Result = true,
                                Message = string.Empty
                            });
                        }
                        catch (Exception ex)
                        {
                            //返回信息
                            _result.Add(new CanceledItemHandleResponse()
                            {
                                OrderNo = item.OrderNo,
                                SubOrderNo = item.SubOrderNo,
                                OperType = item.OperType,
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
            return _result;
        }
    }
}