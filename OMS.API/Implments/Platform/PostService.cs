using System;
using System.Collections.Generic;
using Samsonite.Utility.Common;
using System.Linq;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using Samsonite.OMS.ECommerce.Japan.Tumi;
using Samsonite.OMS.Service.AppConfig;

using OMS.API.Interface.Platform;
using OMS.API.Models.Warehouse;
using OMS.API.Models.Platform;

namespace OMS.API.Implments.Platform
{
    public class PostService : IPostService
    {
        private TumiAPI _tumiAPI;
        private int _amountAccuracy = 0;
        public PostService()
        {
            _tumiAPI = new TumiAPI();
            //金额精准度
            _amountAccuracy = ConfigService.GetAmountAccuracyConfig();
        }

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
                        var tradeDtos = new List<TradeDto>();
                        foreach (var item in datas)
                        {
                            try
                            {
                                if (string.IsNullOrEmpty(item.OrderNo))
                                {
                                    throw new Exception("Required key [order_no] not found!");
                                }

                                if (string.IsNullOrEmpty(item.MallSapCode))
                                {
                                    throw new Exception("Required key [store_id] not found!");
                                }
                                else
                                {
                                    var mall = malls.Where(p => p.SapCode == item.MallSapCode).SingleOrDefault();
                                    if (mall == null)
                                    {
                                        throw new Exception("The mall dose not exists!");
                                    }
                                }

                                if (string.IsNullOrEmpty(item.OrderDate))
                                {
                                    throw new Exception("Required key [order_date] not found!");
                                }

                                if (item.CustomerInfo == null)
                                {
                                    throw new Exception("Required key [customer] not found!");
                                }
                                else
                                {
                                    if (item.CustomerInfo.BillingAddressInfo == null)
                                    {
                                        throw new Exception("Required key [billing_address] not found!");
                                    }
                                }

                                if (item.Products == null)
                                {
                                    throw new Exception("Required key [products] not found!");
                                }
                                else
                                {
                                    if (!item.Products.Any())
                                    {
                                        throw new Exception("Require at least one [product]");
                                    }
                                }

                                if (item.Shipments == null)
                                {
                                    throw new Exception("Required key [shipments] not found!");
                                }
                                else
                                {
                                    if (!item.Shipments.Any())
                                    {
                                        throw new Exception("Require at least one [shipment]");
                                    }
                                    else
                                    {
                                        if (item.Shipments.FirstOrDefault().ShipmentAddressInfo == null)
                                        {
                                            throw new Exception("Required key [shipping_address] not found!");
                                        }
                                    }
                                }

                                if (item.TotalsInfo == null)
                                {
                                    throw new Exception("Required key [totals] not found!");
                                }

                                string dateString = JsonHelper.JsonSerialize(item);
                                //查询缓存中是否存在未处理的订单
                                var orderCache = db.OrderCache.Where(p => p.OrderNo == item.OrderNo && p.MallSapCode == item.MallSapCode && p.Status == 0).FirstOrDefault();
                                if (orderCache != null)
                                {
                                    //更新订单详情
                                    orderCache.DataString = dateString;
                                    orderCache.AddDate = DateTime.Now;
                                }
                                else
                                {
                                    //缓存订单信息
                                    db.OrderCache.Add(new OrderCache()
                                    {
                                        OrderNo = item.OrderNo,
                                        MallSapCode = item.MallSapCode,
                                        DataString = dateString,
                                        AddDate = DateTime.Now,
                                        Status = 0,
                                        ErrorCount = 0,
                                        ErrorMessage = string.Empty
                                    });
                                }
                                db.SaveChanges();

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