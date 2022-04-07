using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.Utility.Common;
using Samsonite.OMS.Service.AppConfig;
using Samsonite.OMS.ECommerce;

using OMS.API.Models.Platform;
using OMS.API.Interface.Platform;
using Samsonite.OMS.Encryption;
using OMS.API.Utils;

namespace OMS.API.Implments.Platform
{
    public class QueryService : IQueryService
    {
        private EntityRepository _entityRepository;
        public QueryService()
        {
            _entityRepository = new EntityRepository();
        }

        #region store
        /// <summary>
        /// 订单线下店铺列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public GetStoresResponse GetStores(GetStoresRequest request)
        {
            GetStoresResponse _result = new GetStoresResponse();
            using (var db = new ebEntities())
            {
                var _list = db.View_MallDetail.AsQueryable();

                if (!string.IsNullOrEmpty(request.StoreSapCode))
                {
                    _list = _list.Where(p => p.RelatedBrandStore.Contains(request.StoreSapCode));
                }

                //只给线下店铺
                _list = _list.Where(p => p.MallType == (int)MallType.OffLine && p.IsUsed);
                //分页查询
                var _pageView = _entityRepository.GetPage(request.PageIndex, request.PageSize, _list.AsNoTracking(), p => p.SortID, true);
                //返回数据
                _result.Stores = _pageView.Items.Select(p => new GetStoresResponse.Store()
                {
                    StoreID = p.SapCode,
                    StoreName = p.MallName,
                    City = p.City,
                    District = p.District,
                    Address = p.Address,
                    ZipCode = p.ZipCode,
                    Contacts = p.ContactReceiver,
                    Phone = p.ContactPhone,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    StoreType = p.StoreType
                }).ToList();
                _result.totalRecord = _pageView.TotalItems;
                _result.totalPage = PagerHelper.CountTotalPage((int)_result.totalRecord, request.PageSize);
            }
            return _result;
        }
        #endregion

        #region order
        /// <summary>
        /// 获取订单详情信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public GetOrdersDetailResponse GetOrdersDetail(GetOrdersDetailRequest request)
        {
            GetOrdersDetailResponse _result = new GetOrdersDetailResponse()
            {
                Orders = new List<GetOrdersDetailResponse.Order>()
            };

            using (var db = new ebEntities())
            {
                if (!string.IsNullOrEmpty(request.OrderNos))
                {
                    var _list = db.Order.AsQueryable();

                    var _orderNoArrays = request.OrderNos.Split(',').ToList();
                    if (_orderNoArrays.Any())
                    {
                        _list = _list.Where(p => p.MallSapCode == request.StoreSapCode && _orderNoArrays.Contains(p.OrderNo)).AsNoTracking();
                    }
                    if (_list.Any())
                    {
                        var _orderNos = _list.Select(p => p.OrderNo).ToList();
                        var _customerNos = _list.GroupBy(p => p.CustomerNo).Select(o => o.Key).ToList();
                        //订单详情
                        var orderDetails = db.OrderDetail.Where(p => _orderNos.Contains(p.OrderNo) && !p.IsExchangeNew).ToList();
                        foreach (var detail in orderDetails)
                        {
                            //如果是套装,只返回原始订单
                            if (detail.IsSet)
                            {
                                //如果是套装主订单
                                if (detail.IsSetOrigin)
                                {
                                    //状态取最新的子订单状态
                                    detail.Status = orderDetails.Where(p => p.OrderId == detail.OrderId && p.IsSet && !p.IsSetOrigin && p.SetCode == detail.SetCode).OrderByDescending(p => p.EditDate).FirstOrDefault().Status;
                                }
                            }
                        }
                        //收货信息
                        var orderReceives = db.OrderReceive.Where(p => _orderNos.Contains(p.OrderNo)).ToList();
                        //解密相关字段信息
                        foreach (var item in orderReceives)
                        {
                            EncryptionFactory.Create(item).Decrypt();
                        }
                        //客户信息
                        var customers = db.Customer.Where(p => _customerNos.Contains(p.CustomerNo)).ToList();
                        //解密相关字段信息
                        foreach (var item in customers)
                        {
                            EncryptionFactory.Create(item).Decrypt();
                        }
                        //赠品
                        var orderGifts = db.OrderGift.Where(p => _orderNos.Contains(p.OrderNo)).ToList();
                        //快递信息
                        var deliverys = db.Deliverys.Where(p => _orderNos.Contains(p.OrderNo)).ToList();
                        //支付信息
                        var payments = db.OrderPayment.Where(p => _orderNos.Contains(p.OrderNo)).ToList();
                        var paymentGifts = db.OrderPaymentGift.Where(p => _orderNos.Contains(p.OrderNo)).ToList();
                        var detailAdjustments = db.OrderDetailAdjustment.Where(p => _orderNos.Contains(p.OrderNo)).ToList();

                        //循环
                        foreach (var item in _list)
                        {
                            //基础信息
                            var tmpOrderDetail = orderDetails.Where(p => p.OrderId == item.Id).ToList();
                            var tmpOrderReceive = orderReceives.Where(p => p.OrderId == item.Id).ToList();
                            var tmpCustomer = customers.Where(p => p.CustomerNo == item.CustomerNo).SingleOrDefault();
                            var tmpOrderGift = orderGifts.Where(p => p.OrderNo == item.OrderNo).ToList();
                            var tmpDeliverys = deliverys.Where(p => p.OrderNo == item.OrderNo).ToList();
                            var tmpPayments = payments.Where(p => p.OrderNo == item.OrderNo).ToList();
                            var tmpPaymentGifts = paymentGifts.Where(p => p.OrderNo == item.OrderNo).ToList();
                            var tmpDetailAdjustments = detailAdjustments.Where(p => p.OrderNo == item.OrderNo).ToList();

                            //计算退款金额
                            decimal _total_refund_amount = 0;
                            string _gift_status = string.Empty;
                            /**********产品信息**********/
                            List<GetOrdersDetailResponse.ProductItem> productItems = new List<GetOrdersDetailResponse.ProductItem>();
                            foreach (var detail in tmpOrderDetail)
                            {
                                //快递号
                                string _trackingNumber = string.Empty;
                                string _delivery_date = string.Empty;
                                Deliverys objDeliverys = new Deliverys();
                                if (detail.IsSet && detail.IsSetOrigin)
                                {
                                    //如果是套装原始订单,因为没有对应的快递号,所以取第一条套装子订单的快递号
                                    objDeliverys = tmpDeliverys.Where(p => p.OrderNo == detail.OrderNo).FirstOrDefault();
                                }
                                else
                                {
                                    objDeliverys = tmpDeliverys.Where(p => p.SubOrderNo == detail.SubOrderNo).SingleOrDefault();
                                }
                                //普通状态产品数量
                                int _commonQuantity = detail.Quantity - detail.CancelQuantity - detail.ReturnQuantity - detail.ExchangeQuantity - detail.RejectQuantity;
                                /**********普通状态产品(指Received,Processing,InDelivery,Delivered,Modify**********/
                                if (_commonQuantity > 0)
                                {
                                    string _productStatus = APIHelper.MatchProductStatusToDW(detail.Status);
                                    //如果退货数量或者换货数量超过1,则表示该子订单肯定已经发货
                                    if (detail.ReturnQuantity > 0 || detail.ExchangeQuantity > 0)
                                    {
                                        _productStatus = "Delivered";
                                    }
                                    productItems.Add(new GetOrdersDetailResponse.ProductItem()
                                    {
                                        ProductId = detail.MallProductId,
                                        ProductName = detail.ProductName,
                                        Sku = detail.SKU,
                                        Quantity = _commonQuantity,
                                        Unit = "",
                                        ProductStatus = _productStatus,
                                        ProductPrice = detail.SellingPrice * _commonQuantity,
                                        ProductTotalDiscount = detail.SellingPrice * detail.Quantity - detail.ActualPaymentAmount,
                                        TrackingNumber = _trackingNumber,
                                        DeliveryDate = _delivery_date
                                    });
                                    //赠品状态
                                    _gift_status = _productStatus;
                                }
                                /**********Cancel Status**********/
                                if (detail.CancelQuantity > 0)
                                {
                                    List<OrderCancel> tmpOrderCancels = new List<OrderCancel>();
                                    //如果是套装
                                    if (detail.IsSet)
                                    {
                                        var orderCancels = db.OrderCancel.Where(p => p.OrderNo == detail.OrderNo && p.SubOrderNo.Contains(detail.SubOrderNo) && p.Status != (int)ProcessStatus.Delete).ToList();
                                        var _requestIDs = orderCancels.GroupBy(p => p.RequestId).Select(o => o.Key).ToList();
                                        foreach (var _o in _requestIDs)
                                        {
                                            var _c = orderCancels.Where(p => p.RequestId == _o).ToList();
                                            OrderCancel objOrderCancel = orderCancels.FirstOrDefault();
                                            if (objOrderCancel != null)
                                            {
                                                objOrderCancel.RefundPoint = _c.Sum(p => p.RefundPoint);
                                                objOrderCancel.RefundAmount = _c.Sum(p => p.RefundAmount);
                                                objOrderCancel.RefundExpress = _c.Sum(p => p.RefundExpress);
                                            }
                                            tmpOrderCancels.Add(objOrderCancel);
                                        }
                                    }
                                    else
                                    {
                                        tmpOrderCancels = db.OrderCancel.Where(p => p.OrderNo == detail.OrderNo && p.SubOrderNo == detail.SubOrderNo && p.Status != (int)ProcessStatus.Delete).ToList();
                                    }
                                    //循环多次请求
                                    foreach (var cancel in tmpOrderCancels)
                                    {
                                        //需要增加快递费用
                                        decimal _refundAmount = cancel.RefundAmount + cancel.RefundExpress;
                                        string _remark = (!string.IsNullOrEmpty(ECommerceBaseService.GetCancelReasonText(cancel.Reason))) ? ECommerceBaseService.GetCancelReasonText(cancel.Reason) : cancel.Remark;
                                        productItems.Add(new GetOrdersDetailResponse.ProductItem()
                                        {
                                            RequestId = cancel.RequestId,
                                            ProductId = detail.MallProductId,
                                            ProductName = detail.ProductName,
                                            Sku = detail.SKU,
                                            Quantity = cancel.Quantity,
                                            Unit = "",
                                            ProductStatus = APIHelper.MatchProcessStatusToDW(cancel.Status),
                                            ProductPrice = detail.SellingPrice * detail.CancelQuantity,
                                            ProductTotalDiscount = detail.SellingPrice * detail.Quantity - detail.ActualPaymentAmount,
                                            RefundAmount = _refundAmount,
                                            Reason = _remark
                                        });
                                        //计算总退款金额
                                        _total_refund_amount += _refundAmount;
                                        //赠品状态
                                        _gift_status = APIHelper.MatchProcessStatusToDW(cancel.Status);
                                    }
                                }
                                /**********Return Status**********/
                                if (detail.ReturnQuantity > 0)
                                {
                                    List<OrderReturn> tmpOrderReturns = new List<OrderReturn>();
                                    //如果是套装
                                    if (detail.IsSet)
                                    {
                                        var orderReturns = db.OrderReturn.Where(p => p.OrderNo == detail.OrderNo && p.SubOrderNo.Contains(detail.SubOrderNo) && !p.IsFromExchange && p.Status != (int)ProcessStatus.Delete).ToList();
                                        var _requestIDs = orderReturns.GroupBy(p => p.RequestId).Select(o => o.Key).ToList();
                                        foreach (var _o in _requestIDs)
                                        {
                                            var _r = orderReturns.Where(p => p.RequestId == _o).ToList();
                                            OrderReturn objOrderReturn = orderReturns.FirstOrDefault();
                                            if (objOrderReturn != null)
                                            {
                                                objOrderReturn.RefundPoint = _r.Sum(p => p.RefundPoint);
                                                objOrderReturn.RefundAmount = _r.Sum(p => p.RefundAmount);
                                                objOrderReturn.RefundExpress = _r.Sum(p => p.RefundExpress);
                                            }
                                            tmpOrderReturns.Add(objOrderReturn);
                                        }
                                    }
                                    else
                                    {
                                        tmpOrderReturns = db.OrderReturn.Where(p => p.OrderNo == detail.OrderNo && p.SubOrderNo == detail.SubOrderNo && !p.IsFromExchange && p.Status != (int)ProcessStatus.Delete).ToList();
                                    }
                                    //解密数据
                                    foreach (var e in tmpOrderReturns)
                                    {
                                        EncryptionFactory.Create(e).Decrypt();
                                    }
                                    //循环多次请求
                                    foreach (var @return in tmpOrderReturns)
                                    {
                                        //换货时候的退款金额需要考虑退货产生的快递费
                                        decimal _refundAmount = @return.RefundAmount - @return.RefundExpress;
                                        string _remark = (!string.IsNullOrEmpty(ECommerceBaseService.GetReturnReasonText(@return.Reason))) ? ECommerceBaseService.GetReturnReasonText(@return.Reason) : @return.Remark;
                                        //退货
                                        productItems.Add(new GetOrdersDetailResponse.ProductItem()
                                        {
                                            RequestId = @return.RequestId,
                                            ProductId = detail.MallProductId,
                                            ProductName = detail.ProductName,
                                            Sku = detail.SKU,
                                            Quantity = @return.Quantity,
                                            Unit = "",
                                            ProductStatus = APIHelper.MatchProcessStatusToDW(@return.Status),
                                            ProductPrice = detail.SellingPrice * detail.CancelQuantity,
                                            ProductTotalDiscount = detail.SellingPrice * detail.Quantity - detail.ActualPaymentAmount,
                                            TrackingNumber = @return.ShippingNo,
                                            CollectionName = @return.CustomerName,
                                            CollectionPhone = @return.Tel,
                                            CollectionAddress = @return.Addr,
                                            DeliveryDate = _delivery_date,
                                            RefundAmount = _refundAmount,
                                            Reason = _remark
                                        });
                                        //计算总退款金额
                                        _total_refund_amount += _refundAmount;
                                        //赠品状态
                                        _gift_status = APIHelper.MatchProcessStatusToDW(@return.Status);
                                    }
                                }
                                /**********Exchange Status**********/
                                if (detail.ExchangeQuantity > 0)
                                {
                                    List<OrderExchange> tmpOrderExchanges = new List<OrderExchange>();
                                    //如果是套装
                                    if (detail.IsSet)
                                    {
                                        var orderExchanges = db.OrderExchange.Where(p => p.OrderNo == detail.OrderNo && p.SubOrderNo.Contains(detail.SubOrderNo) && p.Status != (int)ProcessStatus.Delete).ToList();
                                        var _requestIDs = orderExchanges.GroupBy(p => p.RequestId).Select(o => o.Key).ToList();
                                        foreach (var _o in _requestIDs)
                                        {
                                            var _r = orderExchanges.Where(p => p.RequestId == _o).ToList();
                                            var objOrderExchange = _r.FirstOrDefault();
                                            tmpOrderExchanges.Add(objOrderExchange);
                                        }
                                    }
                                    else
                                    {
                                        tmpOrderExchanges = db.OrderExchange.Where(p => p.OrderNo == detail.OrderNo && p.SubOrderNo.Contains(detail.SubOrderNo) && p.Status != (int)ProcessStatus.Delete).ToList();
                                    }
                                    //解密数据
                                    foreach (var e in tmpOrderExchanges)
                                    {
                                        EncryptionFactory.Create(e).Decrypt();
                                    }
                                    //循环多次请求
                                    foreach (var exchange in tmpOrderExchanges)
                                    {
                                        string _remark = (!string.IsNullOrEmpty(ECommerceBaseService.GetExchangeReasonText(exchange.Reason))) ? ECommerceBaseService.GetExchangeReasonText(exchange.Reason) : exchange.Remark;
                                        //换货
                                        productItems.Add(new GetOrdersDetailResponse.ProductItem()
                                        {
                                            RequestId = exchange.RequestId,
                                            ProductId = detail.MallProductId,
                                            ProductName = detail.ProductName,
                                            Sku = detail.SKU,
                                            Quantity = exchange.Quantity,
                                            Unit = "",
                                            ProductStatus = APIHelper.MatchProcessStatusToDW(exchange.Status),
                                            ProductPrice = detail.SellingPrice * detail.CancelQuantity,
                                            ProductTotalDiscount = detail.SellingPrice * detail.Quantity - detail.ActualPaymentAmount,
                                            TrackingNumber = exchange.ShippingNo,
                                            CollectionName = exchange.CustomerName,
                                            CollectionPhone = exchange.Tel,
                                            CollectionAddress = exchange.Addr,
                                            DeliveryDate = _delivery_date,
                                            Reason = _remark
                                        });
                                        //赠品状态
                                        _gift_status = APIHelper.MatchProcessStatusToDW(exchange.Status);
                                    }
                                }
                                if (detail.RejectQuantity > 0)
                                {
                                    //只能进行全部拒收
                                    productItems.Add(new GetOrdersDetailResponse.ProductItem()
                                    {
                                        ProductId = detail.MallProductId,
                                        ProductName = detail.ProductName,
                                        Sku = detail.SKU,
                                        Quantity = detail.RejectQuantity,
                                        Unit = "",
                                        ProductStatus = APIHelper.MatchProcessStatusToDW(detail.Status),
                                        ProductPrice = detail.SellingPrice * detail.RejectQuantity,
                                        ProductTotalDiscount = detail.SellingPrice * detail.Quantity - detail.ActualPaymentAmount,
                                        TrackingNumber = _trackingNumber,
                                        DeliveryDate = _delivery_date
                                    });
                                    //赠品状态
                                    _gift_status = APIHelper.MatchProcessStatusToDW(detail.Status);
                                }

                                /**********非系统生成的赠品返回给DW**********/
                                foreach (var gift in tmpOrderGift.Where(p => p.SubOrderNo == detail.SubOrderNo && !p.IsSystemGift))
                                {
                                    productItems.Add(new GetOrdersDetailResponse.ProductItem()
                                    {
                                        ProductId = gift.MallProductId,
                                        ProductName = gift.ProductName,
                                        Sku = gift.Sku,
                                        Quantity = gift.Quantity,
                                        Unit = "",
                                        ProductStatus = _gift_status,
                                        ProductPrice = gift.Price * gift.Quantity,
                                        ProductTotalDiscount = gift.Price * gift.Quantity,
                                        TrackingNumber = string.Empty,
                                        DeliveryDate = string.Empty
                                    });
                                }
                            }

                            /**********支付信息**********/
                            string _payment_method = string.Empty;
                            decimal _payment_amount = 0;
                            var objPayments = tmpPayments.FirstOrDefault();
                            if (objPayments != null)
                            {
                                _payment_method = objPayments.ProcessorId;
                                _payment_amount = objPayments.Amount;
                            }
                            //decimal _gifts_amount = tmpDetailAdjustments.Where(o => o.PromotionId.ToLower().Contains("bonusproduct")).Sum(o => o.GrossPrice);
                            ////排除赠品的折扣
                            //decimal _discount = Math.Abs(tmpDetailAdjustments.Where(o => !o.PromotionId.ToLower().Contains("bonusproduct")).Sum(o => o.GrossPrice));
                            //decimal _discount = 0;
                            //_discount += tmpPaymentGifts.Sum(o => o.Amount);

                            /**********创建对象**********/
                            _result.Orders.Add(new GetOrdersDetailResponse.Order()
                            {
                                OrderNo = item.OrderNo,
                                MallSapCode = item.MallSapCode,
                                StatusInfo = new GetOrdersDetailResponse.OrderStatus()
                                {
                                    Status = APIHelper.MatchOrderStatusToDW(item.Status),
                                    PaymentStatus = item.PaymentStatus,
                                    //退款金额
                                    //注意:只有在取消或者退货时候需要放入此退款值
                                    Amount = _total_refund_amount
                                },
                                Summary = new GetOrdersDetailResponse.OrderSummary()
                                {
                                    OrderDate = item.CreateDate.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"),
                                    Products = productItems
                                },
                                DetailInfo = new GetOrdersDetailResponse.OrderDetail()
                                {
                                    CustomerInfo = new GetOrdersDetailResponse.Customer()
                                    {
                                        CustomerNo = tmpCustomer.PlatformUserNo,
                                        CustomerName = tmpCustomer.Name,
                                        CustomerEmail = tmpCustomer.Email,
                                        CellPhone = tmpCustomer.Mobile
                                    },
                                    ShippingAddressInfo = new GetOrdersDetailResponse.ShippingAddress()
                                    {
                                        LastName = tmpOrderReceive.FirstOrDefault().Receive,
                                        Address1 = tmpOrderReceive.FirstOrDefault().Address1,
                                        Address2 = tmpOrderReceive.FirstOrDefault().Address2,
                                        CountryCode = tmpCustomer.CountryCode,
                                        CellPhone = tmpOrderReceive.FirstOrDefault().ReceiveTel,
                                    },
                                    Payments = new List<GetOrdersDetailResponse.Payment>()
                                           {
                                               new GetOrdersDetailResponse.Payment()
                                               {
                                                    MethodName=_payment_method,
                                                    Amount=_payment_amount,
                                                    TotalBeforeDiscount=item.OrderAmount,
                                                    ShippingFee=item.DeliveryFee,
                                                    Discount=item.DiscountAmount,
                                                    TotalPaid=item.PaymentAmount
                                               }
                                           }
                                }
                            });
                        }
                    }
                }
            }
            return _result;
        }
        #endregion

        #region inventory
        /// <summary>
        /// 获取库存信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public GetInventorysResponse GetInventorys(GetInventorysRequest request)
        {
            try
            {
                GetInventorysResponse _result = new GetInventorysResponse();
                using (var db = new ebEntities())
                {
                    var mall = db.Mall.Where(p => p.SapCode == request.StoreSapCode && p.PlatformCode == (int)PlatformType.TUMI_Japan).SingleOrDefault();
                    if (mall != null)
                    {
                        //库存警告数量
                        int _WarningInventory = ConfigService.GetWarningInventoryNumTumiConfig();

                        var _list = db.View_MallProductInventory.AsQueryable();

                        //只推送上架中和有效的产品,不推送赠品
                        _list = _list.Where(p => p.MallSapCode == request.StoreSapCode && p.ProductType != (int)ProductType.Gift && p.IsOnSale && p.IsUsed);

                        if (!string.IsNullOrEmpty(request.ProductIds))
                        {
                            var _idArrays = request.ProductIds.Split(',').ToList();
                            _list = _list.Where(p => request.ProductIds.Contains(p.MallProductId));
                        }

                        //分页查询
                        var _pageView = _entityRepository.GetPage(request.PageIndex, request.PageSize, _list.AsNoTracking(), p => p.ID, true);
                        //返回数据
                        _result.MallSapCode = mall.SapCode;
                        _result.ListId = "JP-inventory";
                        _result.DefaultInstock = false;
                        _result.Description = "JP-inventory Inventory ( 5640 )";
                        _result.UseBundleInventoryOnly = false;
                        _result.Records = _pageView.Items.Select(p => new GetInventorysResponse.Inventory()
                        {
                            ProductId = p.MallProductId,
                            Allocation = (p.Quantity <= _WarningInventory) ? 0 : p.Quantity,
                            Timestamp = VariableHelper.SaferequestUTCTime(DateTime.Now)
                        }).ToList();
                        _result.totalRecord = _pageView.TotalItems;
                        _result.totalPage = PagerHelper.CountTotalPage((int)_result.totalRecord, request.PageSize);
                    }
                    else
                    {
                        throw new Exception("The mall dose not exists!");
                    }
                }
                return _result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}



