using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using Samsonite.OMS.Encryption;
using Samsonite.Utility.Common;

using OMS.API.Interface.Warehouse;
using OMS.API.Models.Warehouse;
using OMS.API.Utils;

namespace OMS.API.Implments.Warehouse
{
    public class QueryService : IQueryService
    {
        private EntityRepository _entityRepository;
        public QueryService()
        {
            _entityRepository = new EntityRepository();
        }

        /// <summary>
        /// 订单集合列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public GetOrdersResponse GetOrders(GetOrdersRequest request)
        {
            GetOrdersResponse _result = new GetOrdersResponse();
            List<GetOrdersItem> _datas = new List<GetOrdersItem>();
            DateTime startDate = UtilsHelper.parseDate(request.StartDate);
            DateTime endDate = UtilsHelper.parseDate(request.EndDate);
            //默认倒序
            if (string.IsNullOrEmpty(request.OrderBy)) request.OrderBy = "DESC";
            bool _isASC = (request.OrderBy.ToUpper() == "ASC");
            using (var db = new ebEntities())
            {
                /***订单过滤条件
                1.只传递订单状态为Received的普通订单
                2.过滤订单类型:已关闭,未付款,已取消,换货新订单,错误订单,已删除订单,原始套装主订单
                3.通过IsStop为0来过滤预售订单
                4.仓库已经回复的订单
                5.如果该订单下属存在错误的子订单,则整个订单均不发送
                注:订单查询时间时根据订单创建时间和预售时间
                ***/
                var _list = from o in db.Order
                            join od in db.OrderDetail.Where(p => ((p.CreateDate >= startDate && p.CreateDate <= endDate) || (p.ReservationDate >= startDate && p.ReservationDate <= endDate)) && p.Status == (int)ProductStatus.Received && !p.IsStop && !p.IsSystemCancel && !p.IsExchangeNew && !p.IsSetOrigin && !p.IsError && !p.IsDelete && !(db.OrderWMSReply.Where(o => o.Status && o.SubOrderNo == p.SubOrderNo).Any())) on o.Id equals od.OrderId
                            join r in db.OrderReceive on od.SubOrderNo equals r.SubOrderNo
                            select new OrderQueryModel()
                            {
                                MallSapCode = o.MallSapCode,
                                OrderNo = o.OrderNo,
                                SubOrderNo = od.SubOrderNo,
                                PlatformType = o.PlatformType,
                                PaymentType = o.PaymentType,
                                OrderAmount = o.OrderAmount,
                                OrderPaymentAmount = o.PaymentAmount,
                                DeliveryFee = o.DeliveryFee,
                                Remark = o.Remark,
                                CreateDate = o.CreateDate,
                                ProductName = od.ProductName,
                                ProductId = od.ProductId,
                                Sku = od.SKU,
                                SupplyPrice = od.SupplyPrice,
                                SellingPrice = od.SellingPrice,
                                PaymentAmount = od.PaymentAmount,
                                ActualPaymentAmount = od.ActualPaymentAmount,
                                Quantity = od.Quantity,
                                Status = od.Status,
                                IsReservation = od.IsReservation,
                                ReservationDate = od.ReservationDate,
                                IsSet = od.IsSet,
                                SetCode = od.SetCode,
                                ShippingType = od.ShippingType.ToString(),
                                Receive = r.Receive,
                                ReceiveTel = r.ReceiveTel,
                                ReceiveCel = r.ReceiveCel,
                                ReceiveZipcode = r.ReceiveZipcode,
                                ReceiveProvince = r.Province,
                                ReceiveCity = r.City,
                                ReceiveDistrict = r.District,
                                ReceiveAddr = r.ReceiveAddr,
                                ReceiveAddr1 = r.Address1,
                                ReceiveAddr2 = r.Address2
                            };
                //获取分页集合
                var _pageView = _entityRepository.GetPage(request.PageIndex, request.PageSize, _list.AsQueryable().AsNoTracking(), p => p.CreateDate, _isASC);
                long _totalRecord = _pageView.TotalItems;
                int _totalPage = PagerHelper.CountTotalPage((int)_totalRecord, request.PageSize);
                //读取相关信息
                List<string> orderNos = _pageView.Items.Select(p => p.OrderNo).ToList();
                List<OrderModify> objOrderModify_List = new List<OrderModify>();
                List<OrderValueAddedService> objOrderValueAddedService_list = new List<OrderValueAddedService>();
                List<OrderGift> objGift_list = new List<OrderGift>();
                if (orderNos.Count > 0)
                {
                    //最新收货地址集合
                    objOrderModify_List = db.OrderModify.Where(p => orderNos.Contains(p.OrderNo) && p.Status == (int)ProcessStatus.ModifyComplete).ToList();
                    //增值服务信息
                    objOrderValueAddedService_list = db.OrderValueAddedService.Where(p => orderNos.Contains(p.OrderNo)).ToList();
                    //赠品集合
                    objGift_list = db.OrderGift.Where(p => orderNos.Contains(p.OrderNo)).ToList();
                }
                //获取套装产品列表
                List<Product> objProduct_List = db.Product.Where(p => p.IsSet).ToList();

                List<MonogramModel> monograms = new List<MonogramModel>();
                GiftCardModel giftCard = new GiftCardModel();
                string bundleName = string.Empty;
                bool isGifts = false;
                List<OrderGiftModel> gifts = new List<OrderGiftModel>();
                //循环
                foreach (var item in _pageView.Items)
                {
                    //读取最新订单收货信息
                    var objOrderModify = objOrderModify_List.Where(p => p.OrderNo == item.OrderNo && p.SubOrderNo == item.SubOrderNo).OrderByDescending(p => p.Id).FirstOrDefault();
                    if (objOrderModify != null)
                    {
                        item.Receive = objOrderModify.CustomerName;
                        item.ReceiveTel = objOrderModify.Tel;
                        item.ReceiveCel = objOrderModify.Mobile;
                        item.ReceiveZipcode = objOrderModify.Zipcode;
                        item.ReceiveProvince = objOrderModify.Province;
                        item.ReceiveCity = objOrderModify.City;
                        item.ReceiveDistrict = objOrderModify.District;
                        item.ReceiveAddr = objOrderModify.Addr;
                    }

                    //产品属性
                    monograms = new List<MonogramModel>();
                    List<OrderValueAddedService> orderValueAddedServices_monogram = objOrderValueAddedService_list.Where(p => p.SubOrderNo == item.SubOrderNo && p.Type == (int)ValueAddedServicesType.Monogram).ToList();
                    if (orderValueAddedServices_monogram.Count > 0)
                    {
                        foreach (var o in orderValueAddedServices_monogram)
                        {
                            var tmp = JsonHelper.JsonDeserialize<MonogramDto>(o.MonoValue);
                            tmp.Location = o.MonoLocation;
                            monograms.Add(new MonogramModel()
                            {
                                Text = tmp.Text,
                                Font = tmp.TextFont,
                                Color = tmp.TextColor,
                                Location = tmp.Location,
                                PatchID = tmp.PatchID
                            });
                        }
                    }

                    giftCard = new GiftCardModel();
                    OrderValueAddedService orderValueAddedService_giftcard = objOrderValueAddedService_list.Where(p => p.SubOrderNo == item.SubOrderNo && p.Type == (int)ValueAddedServicesType.GiftCard).FirstOrDefault();
                    if (orderValueAddedService_giftcard != null)
                    {
                        var tmp = JsonHelper.JsonDeserialize<GiftCardDto>(orderValueAddedService_giftcard.MonoValue);
                        giftCard.Message = tmp.Message;
                        giftCard.Recipient = tmp.Recipient;
                        giftCard.Sender = tmp.Sender;
                        giftCard.Font = tmp.Font;
                        giftCard.GiftCardID = tmp.GiftCardID;
                    }

                    //匹配套装名称
                    bundleName = string.Empty;
                    if (item.IsSet)
                    {
                        var _p = objProduct_List.Where(p => p.SKU == item.SetCode).FirstOrDefault();
                        if (_p != null)
                        {
                            bundleName = _p.Description;
                        }
                    }

                    //读取赠品
                    isGifts = false;
                    gifts = new List<OrderGiftModel>();
                    List<OrderGift> _Gifts = objGift_list.Where(p => p.SubOrderNo == item.SubOrderNo).ToList();
                    if (_Gifts.Count > 0)
                    {
                        isGifts = true;
                        foreach (var _g in _Gifts)
                        {
                            gifts.Add(new OrderGiftModel()
                            {
                                giftSku = _g.Sku,
                                giftQuantity = _g.Quantity
                            });
                        }
                    }

                    //解密数据
                    EncryptionFactory.Create(item, new string[] { "Receive", "ReceiveTel", "ReceiveCel", "ReceiveAddr", "ReceiveAddr1", "ReceiveAddr2" }).Decrypt();

                    //返回数据
                    GetOrdersItem _o = new GetOrdersItem
                    {
                        mallCode = item.MallSapCode,
                        orderNo = item.OrderNo,
                        subOrderNo = item.SubOrderNo,
                        orderDate = item.CreateDate.ToString("yyyyMMddHHmm"),
                        paymentType = APIHelper.GetPaymentType(item.PaymentType),
                        salePrice = item.SellingPrice,
                        //付款金额总金额(扣除折扣后面的真实支付金额)
                        orderPrice = item.ActualPaymentAmount,
                        sku = item.Sku,
                        quantity = item.Quantity,
                        productId = item.ProductId,
                        productName = item.ProductName,
                        productStatus = item.Status,
                        deliveryNo = string.Empty,
                        deliveryDoc = string.Empty,
                        isReservation = item.IsReservation,
                        reservationDate = (item.ReservationDate == null) ? "" : item.ReservationDate.Value.ToString("yyyyMMddHHmm"),
                        monograms = monograms,
                        giftWrapping = giftCard,
                        isSet = item.IsSet,
                        setName = bundleName,
                        isGifts = isGifts,
                        freeGiftID = gifts,
                        shippingType = item.ShippingType,
                        receiver = VariableHelper.SaferequestNull(item.Receive),
                        receiveTel = VariableHelper.SaferequestNull(item.ReceiveTel),
                        receiveCel = VariableHelper.SaferequestNull(item.ReceiveCel),
                        receiveZipcode = VariableHelper.SaferequestNull(item.ReceiveZipcode),
                        receiveProvince = VariableHelper.SaferequestNull(item.ReceiveProvince),
                        receiveCity = VariableHelper.SaferequestNull(item.ReceiveCity),
                        receiveDistrict = VariableHelper.SaferequestNull(item.ReceiveDistrict),
                        receiveAddr = VariableHelper.SaferequestNull(item.ReceiveAddr),
                        receiveAddr1 = VariableHelper.SaferequestNull(item.ReceiveAddr1),
                        receiveAddr2 = VariableHelper.SaferequestNull(item.ReceiveAddr2),
                        remark = VariableHelper.SaferequestNull(item.Remark)
                    };
                    _datas.Add(_o);
                }
                _result.Data = _datas;
                _result.totalRecord = _totalRecord;
                _result.totalPage = _totalPage;
                return _result;
            }
        }

        /// <summary>
        /// 编辑/取消/退货/预售订单/换货新订单列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public GetChangedOrdersResponse GetChangedOrders(GetChangedOrdersRequest request)
        {
            GetChangedOrdersResponse _result = new GetChangedOrdersResponse();
            List<GetClaimsItem> _data = new List<GetClaimsItem>();
            using (var db = new ebEntities())
            {
                //套装产品库
                List<Product> objProduct_List = db.Product.Where(p => p.IsSet).ToList();
                DateTime startDate = UtilsHelper.parseDate(request.StartDate);
                DateTime endDate = UtilsHelper.parseDate(request.EndDate);
                //默认倒序
                if (string.IsNullOrEmpty(request.OrderBy)) request.OrderBy = "DESC";
                bool _isASC = (request.OrderBy.ToUpper() == "ASC");
                //查询
                var _list = db.OrderChangeRecord.AsQueryable().Where(p => p.AddDate >= startDate && p.AddDate <= endDate && !p.IsDelete);
                //获取分页集合
                var _pageView = _entityRepository.GetPage(request.PageIndex, request.PageSize, _list.AsNoTracking(), p => p.Id, _isASC);
                long _totalRecord = _pageView.TotalItems;
                int _totalPage = PagerHelper.CountTotalPage((int)_totalRecord, request.PageSize);
                //循环
                foreach (var item in _pageView.Items)
                {
                    GetClaimsItem _o = new GetClaimsItem
                    {
                        recordId = item.Id,
                        mallCode = string.Empty,
                        subOrderNo = item.SubOrderNo,
                        orderNo = item.OrderNo,
                        addDate = item.AddDate.ToString("yyyyMMddHHmm"),
                        type = item.Type,
                        data = null,
                        remark = item.Remarks,
                    };
                    _o = this.QueryData(item, _o, objProduct_List, db);
                    //如果找不到对应信息,则不显示该条记录
                    if (_o != null)
                    {
                        _data.Add(_o);
                    }
                }
                _result.Data = _data;
                _result.totalRecord = _totalRecord;
                _result.totalPage = _totalPage;
                return _result;
            }
        }

        #region 编辑/取消/退货/预售订单/换货新订单列表函数
        private GetClaimsItem QueryData(OrderChangeRecord objRecord, GetClaimsItem objModel, List<Product> objProductList, ebEntities db)
        {
            //取消订单
            if (objRecord.Type == (int)OrderChangeType.Cancel)
            {
                var objOrderCancel = (from oc in db.OrderCancel.Where(p => p.Id == objRecord.DetailId)
                                      join od in db.OrderDetail on oc.SubOrderNo equals od.SubOrderNo
                                      select new
                                      {
                                          MallSapCode = oc.MallSapCode,
                                          Quantity = oc.Quantity,
                                          Remark = oc.Remark,
                                          Sku = od.SKU,
                                          ProductId = od.ProductId
                                      }).SingleOrDefault();
                if (objOrderCancel != null)
                {
                    CancelData _data = new CancelData
                    {
                        sku = objOrderCancel.Sku,
                        productId = objOrderCancel.ProductId,
                        quantity = objOrderCancel.Quantity,
                        remark = objOrderCancel.Remark
                    };
                    objModel.mallCode = objOrderCancel.MallSapCode;
                    objModel.data = _data;
                    return objModel;
                }
                else
                {
                    return null;
                }
            }
            //退货订单(换货分为先退货然后生成新订单)
            else if (objRecord.Type == (int)OrderChangeType.Exchange)
            {
                var objOrderExchange = (from oe in db.OrderReturn.Where(p => p.Id == objRecord.DetailId)
                                        join od in db.OrderDetail on oe.SubOrderNo equals od.SubOrderNo
                                        select new
                                        {
                                            MallSapCode = oe.MallSapCode,
                                            Quantity = oe.Quantity,
                                            Sku = od.SKU,
                                            ProductId = od.ProductId,
                                            IsGift = od.IsGift,
                                            ShippingCompany = oe.ShippingCompany,
                                            ShippingNo = oe.ShippingNo,
                                            Receiver = oe.CustomerName,
                                            Tel = oe.Tel,
                                            Mobile = oe.Mobile,
                                            Zipcode = oe.Zipcode,
                                            Addr = oe.Addr,
                                            Remark = oe.Remark

                                        }).SingleOrDefault();
                var objOrderExchange_dynamic = GenericHelper.ConvertToDynamic(objOrderExchange);
                if (objOrderExchange_dynamic != null)
                {
                    //解密数据
                    EncryptionFactory.Create(objOrderExchange_dynamic, new string[] { "Receiver", "Tel", "Mobile", "Addr" }).Decrypt();
                    ExchangeData _data = new ExchangeData
                    {
                        sku = objOrderExchange_dynamic.Sku,
                        quantity = objOrderExchange_dynamic.Quantity,
                        isGifts = objOrderExchange_dynamic.IsGift,
                        productId = objOrderExchange_dynamic.ProductId,
                        expressCompany = objOrderExchange_dynamic.ShippingCompany,
                        expressNo = objOrderExchange_dynamic.ShippingNo,
                        receiver = objOrderExchange_dynamic.Receiver,
                        tel = objOrderExchange_dynamic.Tel,
                        mobile = objOrderExchange_dynamic.Mobile,
                        zipcode = objOrderExchange_dynamic.Zipcode,
                        address = objOrderExchange_dynamic.Addr,
                        remark = objOrderExchange_dynamic.Remark
                    };
                    objModel.mallCode = objOrderExchange.MallSapCode;
                    objModel.data = _data;
                    return objModel;
                }
                else
                {
                    return null;
                }
            }
            //退货订单(换货分为先退货然后生成新订单)
            else if (objRecord.Type == (int)OrderChangeType.Return)
            {
                var objOrderReturn = (from oe in db.OrderReturn.Where(p => p.Id == objRecord.DetailId)
                                      join od in db.OrderDetail on oe.SubOrderNo equals od.SubOrderNo
                                      select new
                                      {
                                          MallSapCode = oe.MallSapCode,
                                          Quantity = oe.Quantity,
                                          Sku = od.SKU,
                                          ProductId = od.ProductId,
                                          IsGift = od.IsGift,
                                          ShippingCompany = oe.ShippingCompany,
                                          ShippingNo = oe.ShippingNo,
                                          Receiver = oe.CustomerName,
                                          Tel = oe.Tel,
                                          Mobile = oe.Mobile,
                                          Zipcode = oe.Zipcode,
                                          Addr = oe.Addr,
                                          Remark = oe.Remark

                                      }).SingleOrDefault();
                var objOrderReturn_dynamic = GenericHelper.ConvertToDynamic(objOrderReturn);
                if (objOrderReturn_dynamic != null)
                {
                    //解密数据
                    EncryptionFactory.Create(objOrderReturn_dynamic, new string[] { "Receiver", "Tel", "Mobile", "Addr" }).Decrypt();
                    ReturnData _data = new ReturnData
                    {
                        sku = objOrderReturn_dynamic.Sku,
                        quantity = objOrderReturn_dynamic.Quantity,
                        productId = objOrderReturn_dynamic.ProductId,
                        isGifts = objOrderReturn_dynamic.IsGift,
                        expressCompany = objOrderReturn_dynamic.ShippingCompany,
                        expressNo = objOrderReturn_dynamic.ShippingNo,
                        receiver = objOrderReturn_dynamic.Receiver,
                        tel = objOrderReturn_dynamic.Tel,
                        mobile = objOrderReturn_dynamic.Mobile,
                        zipcode = objOrderReturn_dynamic.Zipcode,
                        address = objOrderReturn_dynamic.Addr,
                        remark = objOrderReturn_dynamic.Remark
                    };
                    objModel.mallCode = objOrderReturn.MallSapCode;
                    objModel.data = _data;
                    return objModel;
                }
                else
                {
                    return null;
                }
            }
            //编辑订单
            else if (objRecord.Type == (int)OrderChangeType.Modify)
            {
                OrderModify objOrderModify = db.OrderModify.Where(p => p.Id == objRecord.DetailId).SingleOrDefault();
                if (objOrderModify != null)
                {
                    //解密数据
                    EncryptionFactory.Create(objOrderModify).Decrypt();
                    ModifyData _data = new ModifyData
                    {
                        receiver = objOrderModify.CustomerName,
                        receiveCel = objOrderModify.Mobile,
                        receiveTel = objOrderModify.Tel,
                        zipcode = objOrderModify.Zipcode,
                        province = objOrderModify.Province,
                        city = objOrderModify.City,
                        district = objOrderModify.District,
                        address = objOrderModify.Addr,
                        remark = objOrderModify.Remark
                    };
                    objModel.mallCode = objOrderModify.MallSapCode;
                    objModel.data = _data;
                    return objModel;
                }
                else
                {
                    return null;
                }
            }
            //新订单(换货新订单或者预售订单)
            else if (objRecord.Type == (int)OrderChangeType.NewOrder)
            {
                string bundleName = string.Empty;
                List<MonogramModel> monograms = new List<MonogramModel>();
                GiftCardModel giftCard = new GiftCardModel();
                bool isGifts = false;
                List<OrderGiftModel> gifts = new List<OrderGiftModel>();
                /***订单过滤条件
                1.如果该订单下存在Pending状态的未删除的子订单,则整个订单均不发送
                ***/
                var item = (from o in db.Order
                            join od in db.OrderDetail.Where(p => (new List<int>() { (int)ProductStatus.Received, (int)ProductStatus.ExchangeNew }).Contains(p.Status) && !p.IsStop && !p.IsSystemCancel && !p.IsExchangeNew && !p.IsSetOrigin && !p.IsError && !p.IsDelete && !(db.OrderWMSReply.Where(o => o.Status && o.SubOrderNo == p.SubOrderNo).Any()) && p.Id == objRecord.DetailId) on o.Id equals od.OrderId
                            join r in db.OrderReceive on od.SubOrderNo equals r.SubOrderNo
                            select new OrderQueryModel()
                            {
                                MallSapCode = o.MallSapCode,
                                OrderNo = o.OrderNo,
                                SubOrderNo = od.SubOrderNo,
                                PlatformType = o.PlatformType,
                                PaymentType = o.PaymentType,
                                OrderAmount = o.OrderAmount,
                                OrderPaymentAmount = o.PaymentAmount,
                                DeliveryFee = o.DeliveryFee,
                                Remark = o.Remark,
                                CreateDate = o.CreateDate,
                                ProductName = od.ProductName,
                                ProductId = od.ProductId,
                                Sku = od.SKU,
                                SupplyPrice = od.SupplyPrice,
                                SellingPrice = od.SellingPrice,
                                PaymentAmount = od.PaymentAmount,
                                ActualPaymentAmount = od.ActualPaymentAmount,
                                Quantity = od.Quantity,
                                Status = od.Status,
                                IsReservation = od.IsReservation,
                                ReservationDate = od.ReservationDate,
                                IsSet = od.IsSet,
                                SetCode = od.SetCode,
                                ShippingType = od.ShippingType.ToString(),
                                Receive = r.Receive,
                                ReceiveTel = r.ReceiveTel,
                                ReceiveCel = r.ReceiveCel,
                                ReceiveZipcode = r.ReceiveZipcode,
                                ReceiveProvince = r.Province,
                                ReceiveCity = r.City,
                                ReceiveDistrict = r.District,
                                ReceiveAddr = r.ReceiveAddr,
                                ReceiveAddr1 = r.Address1,
                                ReceiveAddr2 = r.Address2
                            }).SingleOrDefault();
                if (item != null)
                {
                    //从换货流程中读取最新地址
                    var objOrderExchange = (from oe in db.OrderExchange.Where(p => p.NewSubOrderNo == objRecord.SubOrderNo)
                                            join od in db.View_OrderReturn on oe.ReturnDetailId equals od.ChangeID
                                            select new
                                            {
                                                Receiver = od.CustomerName,
                                                Tel = od.Tel,
                                                Mobile = od.Mobile,
                                                Zipcode = od.Zipcode,
                                                Addr = od.Addr
                                            }).SingleOrDefault();
                    if (objOrderExchange != null)
                    {
                        item.Receive = objOrderExchange.Receiver;
                        item.ReceiveTel = objOrderExchange.Tel;
                        item.ReceiveCel = objOrderExchange.Mobile;
                        item.ReceiveZipcode = objOrderExchange.Zipcode;
                        item.ReceiveAddr = objOrderExchange.Addr;
                    }

                    //产品属性
                    var orderValueAddedServices = db.OrderValueAddedService.Where(p => p.OrderNo == item.OrderNo).ToList();
                    monograms = new List<MonogramModel>();
                    List<OrderValueAddedService> orderValueAddedServices_monogram = orderValueAddedServices.Where(p => p.SubOrderNo == item.SubOrderNo && p.Type == (int)ValueAddedServicesType.Monogram).ToList();
                    if (orderValueAddedServices_monogram.Count > 0)
                    {
                        foreach (var o in orderValueAddedServices_monogram)
                        {
                            var tmp = JsonHelper.JsonDeserialize<MonogramDto>(o.MonoValue);
                            tmp.Location = o.MonoLocation;
                            monograms.Add(new MonogramModel()
                            {
                                Text = tmp.Text,
                                Font = tmp.TextFont,
                                Color = tmp.TextColor,
                                Location = tmp.Location,
                                PatchID = tmp.PatchID
                            });
                        }
                    }

                    giftCard = new GiftCardModel();
                    OrderValueAddedService orderValueAddedService_giftcard = orderValueAddedServices.Where(p => p.SubOrderNo == item.SubOrderNo && p.Type == (int)ValueAddedServicesType.GiftCard).FirstOrDefault();
                    if (orderValueAddedService_giftcard != null)
                    {
                        var tmp = JsonHelper.JsonDeserialize<GiftCardDto>(orderValueAddedService_giftcard.MonoValue);
                        giftCard.Message = tmp.Message;
                        giftCard.Recipient = tmp.Recipient;
                        giftCard.Sender = tmp.Sender;
                        giftCard.Font = tmp.Font;
                        giftCard.GiftCardID = tmp.GiftCardID;
                    }

                    //如果是套装
                    if (item.IsSet)
                    {
                        var _p = objProductList.Where(p => p.SKU == item.SetCode).FirstOrDefault();
                        if (_p != null)
                        {
                            bundleName = _p.Description;
                        }
                    }

                    //读取赠品信息
                    List<OrderGift> objGift_list = db.OrderGift.Where(p => p.OrderNo == objRecord.OrderNo && p.SubOrderNo == objRecord.SubOrderNo).ToList();
                    if (objGift_list.Count > 0)
                    {
                        isGifts = true;
                        gifts = new List<OrderGiftModel>();
                        foreach (var _g in objGift_list)
                        {
                            gifts.Add(new OrderGiftModel()
                            {
                                giftSku = _g.Sku,
                                giftQuantity = _g.Quantity
                            });
                        }
                    }
                    //解密数据
                    EncryptionFactory.Create(item, new string[] { "Receive", "ReceiveTel", "ReceiveCel", "ReceiveAddr", "ReceiveAddr1", "ReceiveAddr2" }).Decrypt();

                    //返回数据
                    GetOrdersItem _o = new GetOrdersItem
                    {
                        mallCode = item.MallSapCode,
                        orderNo = item.OrderNo,
                        subOrderNo = item.SubOrderNo,
                        orderDate = item.CreateDate.ToString("yyyyMMddHHmm"),
                        paymentType = APIHelper.GetPaymentType(item.PaymentType),
                        salePrice = item.SellingPrice,
                        //付款金额总金额(扣除折扣后面的真实支付金额)
                        orderPrice = item.ActualPaymentAmount,
                        sku = item.Sku,
                        quantity = item.Quantity,
                        productId = item.ProductId,
                        productName = item.ProductName,
                        productStatus = item.Status,
                        deliveryNo = string.Empty,
                        deliveryDoc = string.Empty,
                        isReservation = item.IsReservation,
                        reservationDate = (item.ReservationDate == null) ? "" : item.ReservationDate.Value.ToString("yyyyMMddHHmm"),
                        monograms = monograms,
                        giftWrapping = giftCard,
                        isSet = item.IsSet,
                        setName = bundleName,
                        isGifts = isGifts,
                        freeGiftID = gifts,
                        shippingType = item.ShippingType,
                        receiver = VariableHelper.SaferequestNull(item.Receive),
                        receiveTel = VariableHelper.SaferequestNull(item.ReceiveTel),
                        receiveCel = VariableHelper.SaferequestNull(item.ReceiveCel),
                        receiveZipcode = VariableHelper.SaferequestNull(item.ReceiveZipcode),
                        receiveProvince = VariableHelper.SaferequestNull(item.ReceiveProvince),
                        receiveCity = VariableHelper.SaferequestNull(item.ReceiveCity),
                        receiveDistrict = VariableHelper.SaferequestNull(item.ReceiveDistrict),
                        receiveAddr = VariableHelper.SaferequestNull(item.ReceiveAddr),
                        receiveAddr1 = VariableHelper.SaferequestNull(item.ReceiveAddr1),
                        receiveAddr2 = VariableHelper.SaferequestNull(item.ReceiveAddr2),
                        remark = VariableHelper.SaferequestNull(item.Remark)
                    };
                    objModel.mallCode = item.MallSapCode;
                    objModel.data = _o;
                    return objModel;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        #endregion
    }
}