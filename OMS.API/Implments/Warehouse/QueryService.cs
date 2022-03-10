using System;
using System.Collections.Generic;
using System.Linq;

using OMS.API.Interface.Warehouse;
using OMS.API.Models.Warehouse;
using OMS.API.Utils;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using Samsonite.OMS.Encryption;
using Samsonite.Utility.Common;

namespace OMS.API.Implments.Warehouse
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
            List<GetOrdersItem> _datas = new List<GetOrdersItem>();
            DateTime startDate = UtilsHelper.parseDate(request.StartDate);
            DateTime endDate = UtilsHelper.parseDate(request.EndDate);
            //默认倒序
            if (string.IsNullOrEmpty(request.OrderBy)) request.OrderBy = "DESC";
            string _orderBy = (request.OrderBy.ToUpper() == "ASC") ? "asc" : "desc";
            using (var db = new ebEntities())
            {
                using (var dr = new DynamicRepository())
                {
                    List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
                    /***订单过滤条件
                    1.只传递订单状态为Received的普通订单
                    2.过滤订单类型:已关闭,未付款,已取消,换货新订单,预售订单,错误订单,已删除订单,原始套装主订单
                    3.仓库已经回复的订单
                    4.如果该订单下属存在错误的未删除的子订单,则整个订单均不发送
                    5.如果该订单下存在Pending状态的未删除的子订单,则整个订单均不发送
                    ***/
                    string sql = $"select o.MallSapCode,od.OrderNo,o.PlatformType,o.PaymentType,o.OrderAmount,o.PaymentAmount as OrderPaymentAmount,o.DeliveryFee,o.Remark,od.CreateDate,od.SubOrderNo,od.ProductName,od.ProductId,od.SKU,od.SupplyPrice,od.SellingPrice,od.PaymentAmount,od.ActualPaymentAmount,od.Status,od.Quantity,od.IsReservation,od.ReservationDate,od.isSet,od.SetCode,r.Receive,r.ReceiveCel,r.ReceiveTel,r.ReceiveZipcode,r.Province as ReceiveProvince,r.City as ReceiveCity,r.District as ReceiveDistrict,r.ReceiveAddr,r.ShippingType,dl.InvoiceNo from OrderDetail as od inner join [order] as o on (od.orderId = o.id) inner join OrderReceive as r on(r.SubOrderNo= od.SubOrderNo) inner join Deliverys as dl on (od.SubOrderNo=dl.SubOrderNo) where od.createDate >= @0 And od.CreateDate <= @1 And od.isReservation=0 And od.IsSystemCancel=0 And od.IsExchangeNew=0 And od.IsSetOrigin=0 And od.IsError=0 And od.IsDelete=0 And od.Status={(int)ProductStatus.Received} And (select count(*) from OrderWMSReply as ows where Status=1 and ows.SubOrderNo=od.SubOrderNo)=0 And (select count(*) from OrderDetail as d1 where d1.OrderNo=od.OrderNo and (d1.IsError=1 or (d1.Status={(int)ProductStatus.Pending} and d1.IsSetOrigin=0) and d1.IsDelete=0))=0 order by od.createDate {_orderBy}";
                    //获取分页集合
                    var _pageView = dr.GetPage<OrderQueryModel>(request.PageIndex, request.PageSize, sql, startDate.ToString("yyyy-MM-dd HH:mm:ss"), endDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    long _totalRecord = _pageView.TotalItems;
                    int _totalPage = (int)Math.Ceiling(_totalRecord / (double)request.PageSize);
                    //读取相关信息
                    List<string> orderNos = _pageView.Items.Select(p => p.OrderNo).ToList();
                    List<OrderModify> objOrderModify_List = new List<OrderModify>();
                    List<DeliverysDocument> objDeliverysDocument_List = new List<DeliverysDocument>();
                    List<OrderGift> objGift_list = new List<OrderGift>();
                    if (orderNos.Count > 0)
                    {
                        //最新收货地址集合
                        objOrderModify_List = db.OrderModify.Where(p => orderNos.Contains(p.OrderNo) && p.Status == (int)Samsonite.OMS.DTO.ProcessStatus.ModifyComplete).ToList();
                        //文档信息
                        objDeliverysDocument_List = db.DeliverysDocument.Where(p => orderNos.Contains(p.OrderNo) && p.DocumentType == (int)ECommerceDocumentType.ShippingDoc).ToList();
                        //赠品集合
                        objGift_list = db.OrderGift.Where(p => orderNos.Contains(p.OrderNo)).ToList();
                    }
                    //获取套装产品列表
                    List<Product> objProduct_List = db.Product.Where(p => p.IsSet).ToList();

                    string docFile = string.Empty;
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

                        //文档信息
                        docFile = string.Empty;
                        var objDeliverysDocument = objDeliverysDocument_List.Where(p => p.SubOrderNo == item.SubOrderNo).FirstOrDefault();
                        if (objDeliverysDocument != null)
                        {
                            docFile = objDeliverysDocument.DocumentFile;
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
                        EncryptionFactory.Create(item, new string[] { "receive", "receiveTel", "receiveCel", "receiveAddr" }).Decrypt();

                        //返回数据
                        GetOrdersItem _o = new GetOrdersItem
                        {
                            orderNo = item.OrderNo,
                            orderDate = item.CreateDate.ToString("yyyyMMddHHmm"),
                            subOrderNo = item.SubOrderNo,
                            stockCode = "",
                            paymentType = APIHelper.GetPaymentType(item.PaymentType),
                            salePrice = item.SellingPrice,
                            //付款金额总金额(扣除折扣后面的真实支付金额)
                            orderPrice = item.ActualPaymentAmount,
                            sku = item.Sku,
                            quantity = item.Quantity,
                            productId = item.ProductId,
                            productName = item.ProductName,
                            mallCode = item.MallSapCode,
                            productStatus = item.Status,
                            receiver = item.Receive,
                            receiveTel = item.ReceiveTel,
                            receiveCel = item.ReceiveCel,
                            receiveZipcode = item.ReceiveZipcode,
                            receiveProvince = item.ReceiveProvince,
                            receiveCity = item.ReceiveCity,
                            receiveDistrict = item.ReceiveDistrict,
                            receiveAddr = item.ReceiveAddr,
                            deliveryNo = item.InvoiceNo,
                            deliveryDoc = docFile,
                            shippingType = item.ShippingType,
                            isReservation = item.IsReservation,
                            reservationDate = (item.ReservationDate == null) ? "" : item.ReservationDate.Value.ToString("yyyyMMddHHmm"),
                            isSet = item.IsSet,
                            setName = bundleName,
                            isGifts = isGifts,
                            freeGiftID = gifts,
                            remark = item.Remark
                        };
                        _datas.Add(_o);
                    }
                    _result.Data = _datas;
                    _result.totalRecord = _totalRecord;
                    _result.totalPage = _totalPage;
                    return _result;
                }
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
                //获取分页集合
                var _list = db.OrderChangeRecord.Where(p => p.AddDate >= startDate && p.AddDate <= endDate && !p.IsDelete).AsQueryable();
                long _totalRecord = _list.Count();
                int _totalPage = (int)Math.Ceiling(_totalRecord / (double)request.PageSize);
                //默认倒序
                if (string.IsNullOrEmpty(request.OrderBy)) request.OrderBy = "DESC";
                if (request.OrderBy.ToUpper() == "ASC")
                {
                    _list = _list.OrderBy(p => p.Id);
                }
                else
                {
                    _list = _list.OrderByDescending(p => p.Id);
                }
                var _pageView = _list.Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
                //循环
                foreach (var item in _pageView)
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
                string docFile = string.Empty;
                string bundleName = string.Empty;
                bool isGifts = false;
                List<OrderGiftModel> gifts = new List<OrderGiftModel>();
                /***订单过滤条件
                    1.如果该订单下存在Pending状态的未删除的子订单,则整个订单均不发送
                    ***/
                var item = db.Database.SqlQuery<OrderQueryModel>($"select o.MallSapCode,od.OrderNo,o.PlatformType,o.PaymentType,o.OrderAmount,o.PaymentAmount as OrderPaymentAmount,o.DeliveryFee,o.Remark,od.CreateDate,od.SubOrderNo,od.ProductName,od.ProductId,od.SKU,od.SupplyPrice,od.SellingPrice,od.PaymentAmount,od.ActualPaymentAmount,od.Status,od.Quantity,od.IsReservation,od.ReservationDate,od.isSet,od.SetCode,r.Receive,r.ReceiveCel,r.ReceiveTel,r.ReceiveZipcode,r.Province as ReceiveProvince,r.City as ReceiveCity,r.District as ReceiveDistrict,r.ReceiveAddr,r.ShippingType,dl.InvoiceNo from OrderDetail as od inner join [order] as o on (od.orderId = o.id) inner join OrderReceive as r on(r.SubOrderNo= od.SubOrderNo) inner join Deliverys as dl on (od.SubOrderNo=dl.SubOrderNo) where od.Status in ({(int)ProductStatus.Received},{(int)ProductStatus.ExchangeNew}) And (select count(*) from OrderDetail as d1 where d1.OrderNo=od.OrderNo and d1.IsError=1 and d1.IsDelete=0)=0 and od.Id={objRecord.DetailId}").SingleOrDefault();
                if (item != null)
                {
                    //如果是预售订单
                    if (item.IsReservation)
                    {
                        //读取最新收货地址
                        OrderModify objOrderModify = db.OrderModify.Where(p => p.OrderNo == objRecord.OrderNo && p.SubOrderNo == objRecord.SubOrderNo && p.Status == (int)ProcessStatus.ModifyComplete).OrderByDescending(p => p.Id).FirstOrDefault();
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
                    }
                    else
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
                    }

                    //读取文档信息
                    DeliverysDocument objDeliverysDocument = db.DeliverysDocument.Where(p => p.OrderNo == objRecord.OrderNo && p.SubOrderNo == objRecord.SubOrderNo && p.DocumentType == (int)ECommerceDocumentType.ShippingDoc).SingleOrDefault();
                    if (objDeliverysDocument != null)
                    {
                        docFile = objDeliverysDocument.DocumentFile;
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
                    EncryptionFactory.Create(item, new string[] { "receiver", "receiveTel", "receiveCel", "receiveAddr" }).Decrypt();

                    //返回数据
                    GetOrdersItem _o = new GetOrdersItem
                    {
                        orderNo = item.OrderNo,
                        orderDate = item.CreateDate.ToString("yyyyMMddHHmm"),
                        subOrderNo = item.SubOrderNo,
                        stockCode = "",
                        paymentType = APIHelper.GetPaymentType(item.PaymentType),
                        salePrice = item.SellingPrice,
                        //付款金额总金额(扣除折扣后面的真实支付金额)
                        orderPrice = item.ActualPaymentAmount,
                        sku = item.Sku,
                        quantity = item.Quantity,
                        productId = item.ProductId,
                        productName = item.ProductName,
                        mallCode = item.MallSapCode,
                        productStatus = item.Status,
                        receiver = item.Receive,
                        receiveTel = item.ReceiveTel,
                        receiveCel = item.ReceiveCel,
                        receiveZipcode = item.ReceiveZipcode,
                        receiveProvince = item.ReceiveProvince,
                        receiveCity = item.ReceiveCity,
                        receiveDistrict = item.ReceiveDistrict,
                        receiveAddr = item.ReceiveAddr,
                        deliveryNo = item.InvoiceNo,
                        deliveryDoc = docFile,
                        shippingType = item.ShippingType,
                        isReservation = item.IsReservation,
                        reservationDate = (item.ReservationDate == null) ? "" : item.ReservationDate.Value.ToString("yyyyMMddHHmm"),
                        isSet = item.IsSet,
                        setName = bundleName,
                        isGifts = isGifts,
                        freeGiftID = gifts,
                        remark = item.Remark
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