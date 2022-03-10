using System;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Encryption;
using Samsonite.Utility.Common;

using OMS.API.Models.ClickCollect;
using OMS.API.Interface.ClickCollect;

namespace OMS.API.Implments.ClickCollect
{
    public class QueryService : IQueryService
    {
        /// <summary>
        /// 订单集合列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public GetOrdersResponse GetOrders(GetOrdersRequest request)
        {
            GetOrdersResponse _result = new GetOrdersResponse();
            //默认倒序
            if (string.IsNullOrEmpty(request.SortBy)) request.SortBy = "DESC";
            string _orderBy = (request.SortBy.ToUpper() == "ASC") ? "asc" : "desc";
            using (var dr = new DynamicRepository())
            {
                List<DynamicRepository.SQLCondition> _sqlWhere = new List<DynamicRepository.SQLCondition>();
                /***订单过滤条件
                1.只传递订单状态为In Delivery之后以及物流状态为Picked之后的普通订单
                2.过滤订单类型:已关闭,未付款,换货新订单,预售订单,错误订单,已删除订单,原始套装主订单
                ***/

                //搜索条件
                if (!string.IsNullOrEmpty(request.ShopSapCode))
                {
                    _sqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "[order].OffLineSapCode={0}", Param = request.ShopSapCode });
                }

                if (!string.IsNullOrEmpty(request.CreatedAfter))
                {
                    _sqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(second,[Order].CreateDate,{0})<=0", Param = VariableHelper.SaferequestTime(request.CreatedAfter) });
                }

                if (!string.IsNullOrEmpty(request.CreatedBefore))
                {
                    _sqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(second,[Order].CreateDate,{0})>=0", Param = VariableHelper.SaferequestTime(request.CreatedBefore) });
                }

                if (!string.IsNullOrEmpty(request.UpdateAfter))
                {
                    _sqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(second,[Order].EditDate,{0})<=0", Param = VariableHelper.SaferequestTime(request.UpdateAfter) });
                }

                if (!string.IsNullOrEmpty(request.UpdateBefore))
                {
                    _sqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(second,[Order].EditDate,{0})>=0", Param = VariableHelper.SaferequestTime(request.UpdateBefore) });
                }

                if (request.Status > 0)
                {
                    _sqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "[order].Status={0}", Param = request.Status });
                }

                if (request.ProductStatus > 0)
                {
                    _sqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "(select count(*) from OrderDetail where [order].Id=OrderDetail.OrderId and OrderDetail.[Status]={0})>0", Param = request.ProductStatus });
                }

                //取C&C订单
                _sqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "[Order].OrderType={0} ", Param = (int)OrderType.ClickCollect });
                //取In Delivery状态的订单
                _sqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "(select count(*) from OrderDetail as od where od.OrderId=[Order].Id and od.Status<" + (int)ProductStatus.InDelivery + ")=0", Param = null });

                //查询
                var _list = dr.GetPage<Order>($"select * from [Order] order by Id {_orderBy}", _sqlWhere, request.PageSize, request.PageIndex);
                List<long> orderIds = _list.Items.Select(p => p.Id).ToList();
                long _totalRecord = _list.TotalItems;
                int _totalPage = PagerHelper.CountTotalPage((int)_totalRecord, request.PageSize);
                using (var db = new ebEntities())
                {
                    List<GetOrdersResponse.Trade> objTrades = new List<GetOrdersResponse.Trade>();
                    List<OrderDetail> objOrderDetails = db.OrderDetail.Where(p => orderIds.Contains(p.OrderId)).ToList();
                    foreach (var item in _list.Items)
                    {
                        objTrades.Add(new GetOrdersResponse.Trade()
                        {
                            MallSapCode = item.MallSapCode,
                            OrderNo = item.OrderNo,
                            OrderType = item.OrderType,
                            PaymentType = item.PlatformType,
                            ShopSapCode = item.OffLineSapCode,
                            Status = item.Status,
                            OrderDate = item.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"),
                            Items = objOrderDetails.Where(p => p.OrderId == item.Id).Select(o => new GetOrdersResponse.Item()
                            {
                                SubOrderNo = o.SubOrderNo,
                                SKU = o.SKU,
                                Quantity = o.Quantity,
                                Status = o.Status
                            }).ToList()
                        });
                    }

                    //返回信息
                    _result.Trades = objTrades;
                    _result.totalRecord = _totalRecord;
                    _result.totalPage = _totalPage;
                }
            }
            return _result;
        }

        /// <summary>
        /// 获取单条订单数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public GetOrderItemsResponse GetOrderItems(GetOrderItemsRequest request)
        {
            GetOrderItemsResponse _result = new GetOrderItemsResponse();
            using (var db = new ebEntities())
            {
                Order objOrder = db.Order.Where(p => p.OrderNo == request.OrderNo).SingleOrDefault();
                if (objOrder != null)
                {
                    //收货信息
                    OrderReceive objOrderReceive = db.OrderReceive.Where(p => p.OrderId == objOrder.Id).FirstOrDefault();
                    //解密数据
                    EncryptionFactory.Create(objOrderReceive).Decrypt();
                    _result.TradeInfo = new GetOrderItemsResponse.Trade()
                    {
                        MallSapCode = objOrder.MallSapCode,
                        OrderNo = objOrder.OrderNo,
                        OrderType = objOrder.OrderType,
                        PaymentType = objOrder.PaymentType,
                        ShopSapCode = objOrder.OffLineSapCode,
                        ReceiveName = objOrderReceive.Receive,
                        ReceiveMobile = (!string.IsNullOrEmpty(objOrderReceive.ReceiveCel)) ? objOrderReceive.ReceiveCel : objOrderReceive.ReceiveTel,
                        OrderDate = objOrder.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        Items = new List<GetOrderItemsResponse.Item>()
                    };

                    //最新收货地址集合
                    List<OrderModify> objOrderModifys = db.OrderModify.Where(p => p.OrderNo == objOrder.OrderNo && p.Status == (int)ProcessStatus.ModifyComplete).ToList();
                    //赠品信息
                    List<OrderGift> objOrderGifts = db.OrderGift.Where(p => p.OrderNo == objOrder.OrderNo).ToList();
                    //增值服务信息
                    List<OrderValueAddedService> objOrderValueAddedServices = db.OrderValueAddedService.Where(p => p.OrderNo == objOrder.OrderNo).ToList();

                    List<GetOrderItemsResponse.Gift> gifts = new List<GetOrderItemsResponse.Gift>();
                    List<GetOrderItemsResponse.VAS> vases = new List<GetOrderItemsResponse.VAS>();
                    //子订单
                    var objOrderDetails = db.Database.SqlQuery<GetOrderItemsResponse.Item>("select od.SubOrderNo,od.Status,od.Quantity,Isnull(p.SKU,'')as SKU,Isnull(p.EAN,'') as EAN,Isnull(p.Name,'') as Brand,Isnull(p.GroupDesc, '') as [Collection], Isnull(p.Description, '') as ProductName, Isnull(p.ImageUrl, '') as ProductImage, Isnull(d.InvoiceNo, '') as TrackingNo, Isnull(d.ExpressMsg, '') as TrackingMsg from OrderDetail as od left join Product as p on od.SKU = p.SKU left join Deliverys as d on od.SubOrderNo = d.SubOrderNo where od.OrderId={0}", objOrder.Id);
                    foreach (var item in objOrderDetails)
                    {
                        //读取最新订单收货信息
                        var objOrderModify = objOrderModifys.Where(p => p.SubOrderNo == item.SubOrderNo).OrderByDescending(p => p.Id).FirstOrDefault();
                        if (objOrderModify != null)
                        {
                            //解密数据
                            EncryptionFactory.Create(objOrderModify).Decrypt();

                            _result.TradeInfo.ReceiveName = objOrderModify.CustomerName;
                            _result.TradeInfo.ReceiveMobile = (!string.IsNullOrEmpty(objOrderModify.Mobile)) ? objOrderModify.Mobile : objOrderModify.Tel;
                        }

                        //赠品信息
                        gifts = new List<GetOrderItemsResponse.Gift>();
                        List<OrderGift> _Gifts = objOrderGifts.Where(p => p.SubOrderNo == item.SubOrderNo).ToList();
                        foreach (var _g in _Gifts)
                        {
                            gifts.Add(new GetOrderItemsResponse.Gift()
                            {
                                GiftSku = _g.Sku,
                                GiftQuantity = _g.Quantity
                            });
                        }
                        item.Gifts = gifts;

                        //增值服务信息
                        vases = new List<GetOrderItemsResponse.VAS>();
                        List<OrderValueAddedService> _Vases = objOrderValueAddedServices.Where(p => p.SubOrderNo == item.SubOrderNo).ToList();
                        foreach (var _v in _Vases)
                        {
                            vases.Add(new GetOrderItemsResponse.VAS()
                            {
                                Type = _v.Type,
                                Location = _v.MonoLocation,
                                Value = _v.MonoValue
                            });
                        }
                        item.VASs = vases;

                        //返回信息
                        _result.TradeInfo.Items.Add(item);
                    }
                }
                else
                {
                    throw new Exception("The Order dose not exsits!");
                }
            }

            return _result;
        }
    }
}