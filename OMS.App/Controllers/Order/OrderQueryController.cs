using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Encryption;
using Samsonite.OMS.Service;
using Samsonite.OMS.Service.AppConfig;
using Samsonite.Utility.Common;
using OMS.App.Helper;

namespace OMS.App.Controllers
{
    public class OrderQueryController : BaseController
    {
        //
        // GET: /OrderQuery/

        #region 查询
        [UserPowerAuthorize]
        public ActionResult Index()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;
            //菜单栏
            ViewBag.MenuBar = this.MenuBar();
            //功能权限
            ViewBag.FunctionPower = this.FunctionPowers();
            //订单类型
            ViewData["order_type"] = OrderHelper.OrderTypeObject();
            //品牌列表
            ViewData["brand_list"] = BrandService.GetBrands();
            //订单状态
            ViewData["orderstate_list"] = OrderHelper.OrderStatusObject();
            //产品状态
            ViewData["productstate_list"] = OrderHelper.ProductStatusObject();
            //仓库流程状态
            ViewData["wmsprocessstate_list"] = OrderHelper.WarehouseProcessStatusObject();
            //快递状态
            ViewData["expressstate_list"] = OrderHelper.ExpressStatusObject();
            //物流类型
            ViewData["shippingmethod_list"] = OrderHelper.ShippingMethodObject();
            //快速时间选项
            ViewData["quicktime_list"] = QuickTimeHelper.QuickTimeOption();
            //付款方式
            ViewData["payment_list"] = OrderHelper.PaymentTypeObject(ConfigCache.Instance.Get().PaymentTypeConfig);
            //订单来源
            ViewData["ordersource_list"] = OrderHelper.OrderSourceObject();
            //默认参数
            ViewBag.OrderNo = VariableHelper.SaferequestStr(Request.QueryString["orderid"]);
            //默认线上订单
            ViewBag.StoreID = VariableHelper.SaferequestStr(Request.QueryString["store"]);
            ViewBag.Sku = VariableHelper.SaferequestStr(Request.QueryString["sku"]);
            ViewBag.Brand = VariableHelper.SaferequestInt(Request.QueryString["brand"]);
            ViewBag.ProductStatus = VariableHelper.SaferequestStr(Request.QueryString["product_status"]).Split(',');
            ViewBag.Customer = VariableHelper.SaferequestStr(Request.QueryString["customer"]);
            string _time = VariableHelper.SaferequestStr(Request.QueryString["time"]);
            string _time1 = VariableHelper.SaferequestStr(Request.QueryString["time1"]);
            string _time2 = VariableHelper.SaferequestStr(Request.QueryString["time2"]);
            if (!string.IsNullOrEmpty(_time))
            {
                //确定是年/月/日3种时间类型
                string[] _d = _time.Split('-');
                if (_d.Length == 1)
                {
                    ViewBag.StTime = $"{_d[0]}-1-1 00:00:00";
                    ViewBag.EdTime = $"{_d[0]}-12-31 23:59:59";
                }
                else if (_d.Length == 2)
                {
                    ViewBag.StTime = $"{_d[0]}-{_d[1]}-1 00:00:00";
                    ViewBag.EdTime = VariableHelper.SaferequestTime($"{_d[0]}-{_d[1]}-1").AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd  23:59:59");
                }
                else
                {
                    ViewBag.StTime = VariableHelper.SaferequestTime(_time).ToString("yyyy-MM-dd 00:00:00");
                    ViewBag.EdTime = VariableHelper.SaferequestTime(_time).ToString("yyyy-MM-dd 23:59:59");
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(_time1))
                {
                    ViewBag.StTime = VariableHelper.SaferequestTime(_time1).ToString("yyyy-MM-dd 00:00:00");
                }
                else
                {
                    ViewBag.StTime = "";
                }

                if (!string.IsNullOrEmpty(_time2))
                {
                    ViewBag.EdTime = VariableHelper.SaferequestTime(_time2).ToString("yyyy-MM-dd 23:59:59");
                }
                else
                {
                    ViewBag.EdTime = "";
                }
            }

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult Index_Message()
        {
            //加载语言包
            var _LanguagePack = GetLanguagePack;

            JsonResult _result = new JsonResult();
            List<string> _SqlMalls = new List<string>();
            List<string> _SqlWhere = new List<string>();
            List<string> _SqlOrder = new List<string>();
            string _orderNo = VariableHelper.SaferequestStr(Request.Form["orderid"]);
            int _order_type = VariableHelper.SaferequestInt(Request.Form["order_type"]);
            string[] _storeid = VariableHelper.SaferequestStringArray(Request.Form["store"]);
            string _sku_id = VariableHelper.SaferequestStr(Request.Form["sku_id"]);
            int _product_brand = VariableHelper.SaferequestInt(Request.Form["product_brand"]);
            string _price_min = VariableHelper.SaferequestNull(Request.Form["price_min"]);
            string _price_max = VariableHelper.SaferequestNull(Request.Form["price_max"]);
            int _time_type = VariableHelper.SaferequestInt(Request.Form["time_type"]);
            string _time1 = VariableHelper.SaferequestStr(Request.Form["time1"]);
            string _time2 = VariableHelper.SaferequestStr(Request.Form["time2"]);
            int[] _order_status = VariableHelper.SaferequestIntArray(Request.Form["order_status"]);
            int[] _product_status = VariableHelper.SaferequestIntArray(Request.Form["product_status"]);
            string _customer = VariableHelper.SaferequestStr(Request.Form["customer"]);
            string _receiver = VariableHelper.SaferequestStr(Request.Form["receiver"]);
            string _contact = VariableHelper.SaferequestStr(Request.Form["contact"]);
            string _shipping_status = VariableHelper.SaferequestStr(Request.Form["shipping_status"]);
            string _express = VariableHelper.SaferequestStr(Request.Form["express"]);
            string _express_status = VariableHelper.SaferequestStr(Request.Form["express_status"]);
            int _shippping_method = VariableHelper.SaferequestInt(Request.Form["shippping_method"]);
            string _deliveryfee_min = VariableHelper.SaferequestNull(Request.Form["deliveryfee_min"]);
            string _deliveryfee_max = VariableHelper.SaferequestNull(Request.Form["deliveryfee_max"]);
            int[] _payment_status = VariableHelper.SaferequestIntArray(Request.Form["payment_status"]);
            //int[] _order_source = VariableHelper.SaferequestIntArray(Request.Form["order_source"]);
            int _is_reserve = VariableHelper.SaferequestInt(Request.Form["is_reserve"]);
            int _is_bundle = VariableHelper.SaferequestInt(Request.Form["is_bundle"]);
            int _is_gift = VariableHelper.SaferequestInt(Request.Form["is_gift"]);
            int _is_monogram = VariableHelper.SaferequestInt(Request.Form["is_monogram"]);
            string _promotion_name = VariableHelper.SaferequestStr(Request.Form["promotion_name"]);
            string _coupon_code = VariableHelper.SaferequestStr(Request.Form["coupon_code"]);
            //int _point_amount = VariableHelper.SaferequestInt(Request.Form["point_amount"]);
            //排序
            string _sort = VariableHelper.SaferequestStr(Request.Form["sort"]);
            string _order = VariableHelper.SaferequestStr(Request.Form["order"]);

            using (var db = new ebEntities())
            {
                //搜索条件
                if (!string.IsNullOrEmpty(_orderNo))
                {
                    _SqlWhere.Add("((OrderDetail.OrderNo like '%" + _orderNo + "%') or (OrderDetail.SubOrderNo like '%" + _orderNo + "%'))");
                }

                if (_order_type > 0)
                {
                    _SqlWhere.Add("[Order].OrderType=" + _order_type);
                }

                //默认显示当前账号允许看到的店铺订单
                var _UserMalls = this.CurrentLoginUser.UserMalls;
                if (_storeid != null)
                {
                    _SqlMalls = _storeid.Where(p => _UserMalls.Contains(p)).ToList();
                }
                else
                {
                    _SqlMalls = _UserMalls;
                }

                if (!string.IsNullOrEmpty(_sku_id))
                {
                    _SqlWhere.Add("((OrderDetail.SKU like '%" + _sku_id + "%') or (OrderDetail.SetCode like '%" + _sku_id + "%'))");
                }

                if (_product_brand > 0)
                {
                    string _Brands = string.Join(",", BrandService.GetSons(_product_brand));
                    if (!string.IsNullOrEmpty(_Brands))
                    {
                        _SqlWhere.Add("charindex((select top 1 name from Product where Product.SKU=OrderDetail.SKU),'" + _Brands + "')>0");
                    }
                }

                if (!string.IsNullOrEmpty(_express))
                {
                    _SqlWhere.Add("Deliverys.InvoiceNo like '%" + _express + "%'");
                }

                if (!string.IsNullOrEmpty(_express_status))
                {
                    _SqlWhere.Add("Deliverys.ExpressStatus=" + VariableHelper.SaferequestInt(_express_status));
                }

                if (!string.IsNullOrEmpty(_price_min))
                {
                    _SqlWhere.Add("[Order].PaymentAmount>=" + VariableHelper.SaferequestDecimal(_price_min));
                }

                if (!string.IsNullOrEmpty(_price_max))
                {
                    _SqlWhere.Add("[Order].PaymentAmount<=" + VariableHelper.SaferequestDecimal(_price_max));
                }

                switch (_time_type)
                {
                    case 1:
                        if (!string.IsNullOrEmpty(_time1))
                        {
                            _SqlWhere.Add("datediff(second,[Order].CreateDate,'" + VariableHelper.SaferequestTime(_time1).ToString("yyyy-MM-dd HH:mm:ss") + "')<=0");
                        }

                        if (!string.IsNullOrEmpty(_time2))
                        {
                            _SqlWhere.Add("datediff(second,[Order].CreateDate,'" + VariableHelper.SaferequestTime(_time2).ToString("yyyy-MM-dd HH:mm:ss") + "')>=0");
                        }
                        break;
                    case 2:
                        if (!string.IsNullOrEmpty(_time1))
                        {
                            _SqlWhere.Add("datediff(second,[Order].PaymentDate,'" + VariableHelper.SaferequestTime(_time1).ToString("yyyy-MM-dd HH:mm:ss") + "')<=0");
                        }

                        if (!string.IsNullOrEmpty(_time2))
                        {
                            _SqlWhere.Add("datediff(second,[Order].PaymentDate,'" + VariableHelper.SaferequestTime(_time2).ToString("yyyy-MM-dd HH:mm:ss") + "')>=0");
                        }
                        break;
                    case 3:
                        if (!string.IsNullOrEmpty(_time1))
                        {
                            _SqlWhere.Add("datediff(second,OrderDetail.CompleteDate,'" + VariableHelper.SaferequestTime(_time1).ToString("yyyy-MM-dd HH:mm:ss") + "')<=0");
                        }

                        if (!string.IsNullOrEmpty(_time2))
                        {
                            _SqlWhere.Add("datediff(second,OrderDetail.CompleteDate,'" + VariableHelper.SaferequestTime(_time2).ToString("yyyy-MM-dd HH:mm:ss") + "')>=0");
                        }
                        break;
                    default:
                        break;
                }

                if (_order_status != null)
                {
                    _SqlWhere.Add("[Order].Status in (" + string.Join(",", _order_status) + ")");
                }

                if (_product_status != null)
                {
                    List<int> _psList = new List<int>();
                    if (_product_status.Contains((int)ProductStatus.Received))
                    {
                        _psList.Add((int)ProductStatus.Received);
                    }
                    if (_product_status.Contains((int)ProductStatus.Processing))
                    {
                        _psList.Add((int)ProductStatus.Processing);
                    }
                    if (_product_status.Contains((int)ProductStatus.InDelivery))
                    {
                        _psList.Add((int)ProductStatus.InDelivery);
                    }
                    if (_product_status.Contains((int)ProductStatus.Delivered))
                    {
                        _psList.Add((int)ProductStatus.Delivered);
                    }
                    if (_product_status.Contains((int)ProductStatus.Modify))
                    {
                        _psList.Add((int)ProductStatus.Modify);
                    }
                    if (_product_status.Contains((int)ProductStatus.Cancel))
                    {
                        _psList.Add((int)ProductStatus.Cancel);
                        _psList.Add((int)ProductStatus.CancelComplete);
                    }
                    if (_product_status.Contains((int)ProductStatus.Return))
                    {
                        _psList.Add((int)ProductStatus.Return);
                        _psList.Add((int)ProductStatus.ReturnComplete);
                    }
                    if (_product_status.Contains((int)ProductStatus.Exchange))
                    {
                        _psList.Add((int)ProductStatus.Exchange);
                        _psList.Add((int)ProductStatus.ExchangeComplete);
                    }
                    if (_product_status.Contains((int)ProductStatus.Reject))
                    {
                        _psList.Add((int)ProductStatus.Reject);
                    }
                    if (_psList.Count > 0)
                    {
                        _SqlWhere.Add("OrderDetail.[Status] in (" + string.Join(",", _psList) + ")");
                    }
                }

                if (!string.IsNullOrEmpty(_customer))
                {
                    _SqlWhere.Add("((select top 1 Name from Customer where Customer.CustomerNo=[Order].CustomerNo)='" + EncryptionBase.EncryptString(_customer) + "')");
                }

                if (!string.IsNullOrEmpty(_receiver))
                {
                    _SqlWhere.Add("OrderReceive.Receive='" + EncryptionBase.EncryptString(_receiver) + "'");
                }

                if (!string.IsNullOrEmpty(_contact))
                {
                    _SqlWhere.Add("((OrderReceive.ReceiveTel='" + EncryptionBase.EncryptString(_contact) + "') or (OrderReceive.ReceiveCel ='" + EncryptionBase.EncryptString(_contact) + "'))");
                }

                if (!string.IsNullOrEmpty(_shipping_status))
                {
                    _SqlWhere.Add("OrderDetail.ShippingStatus=" + VariableHelper.SaferequestInt(_shipping_status));
                }

                if (_shippping_method > 0)
                {
                    if (_shippping_method == (int)ShippingMethod.ExpressShipping)
                    {
                        _SqlWhere.Add("[Order].ShippingMethod=" + (int)ShippingMethod.ExpressShipping);
                    }
                    else
                    {
                        _SqlWhere.Add("[Order].ShippingMethod=" + (int)ShippingMethod.StandardShipping);
                    }
                }

                if (!string.IsNullOrEmpty(_deliveryfee_min))
                {
                    _SqlWhere.Add("[Order].DeliveryFee>=" + VariableHelper.SaferequestDecimal(_deliveryfee_min));
                }

                if (!string.IsNullOrEmpty(_deliveryfee_max))
                {
                    _SqlWhere.Add("[Order].DeliveryFee<=" + VariableHelper.SaferequestDecimal(_deliveryfee_max));
                }

                if (_payment_status != null)
                {
                    _SqlWhere.Add("[Order].PaymentType in (" + string.Join(",", _payment_status) + ")");
                }

                //if (_order_source != null)
                //{
                //    _SqlWhere.Add("[Order].OrderSource in (" + string.Join(",", _order_source) + ")");
                //}

                if (_is_reserve > 0)
                {
                    _SqlWhere.Add("OrderDetail.IsReservation=1");
                }

                if (_is_bundle > 0)
                {
                    _SqlWhere.Add("OrderDetail.IsSet=1");
                }

                if (_is_gift > 0)
                {
                    _SqlWhere.Add("(select count(*) from OrderGift where [order].OrderNo=OrderGift.OrderNo)>0");
                }

                if (_is_monogram > 0)
                {
                    _SqlWhere.Add("(select count(*) from OrderValueAddedService where [order].OrderNo=OrderValueAddedService.OrderNo and OrderValueAddedService.Type=" + (int)ValueAddedServicesType.Monogram + ")>0");
                }

                if (!string.IsNullOrEmpty(_promotion_name))
                {
                    _SqlWhere.Add("(select count(*) from OrderDetailAdjustment where [order].OrderNo=OrderDetailAdjustment.OrderNo and OrderDetailAdjustment.LineitemText like '%" + _promotion_name + "%')>0");
                }

                if (!string.IsNullOrEmpty(_coupon_code))
                {
                    _SqlWhere.Add("(select count(*) from OrderDetailAdjustment where [order].OrderNo=OrderDetailAdjustment.OrderNo and OrderDetailAdjustment.CouponId like '%" + _coupon_code + "%')>0");
                }

                //过滤套装原始订单
                _SqlWhere.Add("OrderDetail.IsSetOrigin=0");
                //过滤无效的订单
                _SqlWhere.Add("OrderDetail.IsDelete=0");

                //排序条件
                if (!string.IsNullOrEmpty(_sort))
                {
                    string[] _sort_array = _sort.Split(',');
                    string[] _order_array = _order.Split(',');
                    for (int t = 0; t < _sort_array.Length; t++)
                    {
                        if (_sort_array[t] == "s5")
                        {
                            _SqlOrder.Add(string.Format("{0} {1}", "OrderAmount", (_order_array[t] == "desc") ? "desc" : "asc"));
                        }
                        else if (_sort_array[t] == "s6")
                        {
                            _SqlOrder.Add(string.Format("{0} {1}", "PaymentAmount", (_order_array[t] == "desc") ? "desc" : "asc"));
                        }
                    }
                }
                else
                {
                    _SqlOrder.Add("CreateDate desc");
                }

                //查询
                int _TotalCount = db.Database.SqlQuery<int>("Proc_SearchOrder {0},{1},{2},{3},{4},{5}", string.Join(",", _SqlMalls), string.Join(" and ", _SqlWhere), "", 0, 0, 0).SingleOrDefault();
                List<OrderQuery> _list = db.Database.SqlQuery<OrderQuery>("Proc_SearchOrder {0},{1},{2},{3},{4},{5}", string.Join(",", _SqlMalls), string.Join(" and ", _SqlWhere), string.Join(",", _SqlOrder), VariableHelper.SaferequestInt(Request.Form["rows"]), VariableHelper.SaferequestInt(Request.Form["page"]), 1).ToList();

                List<string> _Orders = _list.Select(p => p.OrderNo).ToList();
                //获取收货信息
                List<OrderReceive> objOrderReceive_List = db.OrderReceive.Where(p => _Orders.Contains(p.OrderNo)).ToList();
                //获取更新的地址信息
                List<OrderModify> objOrderModify_List = db.OrderModify.Where(p => _Orders.Contains(p.OrderNo) && p.Status == (int)ProcessStatus.ModifyComplete).ToList();
                foreach (var dy in _list)
                {
                    //读取收货信息
                    List<OrderReceive> objOrderReceives = objOrderReceive_List.Where(p => p.OrderNo == dy.OrderNo).ToList();
                    dy.IsMultipleReceive = (objOrderReceives.GroupBy(o => o.ReceiveAddr).Count() > 1);
                    OrderReceive objOrderReceive = objOrderReceives.FirstOrDefault();
                    if (objOrderReceive != null)
                    {
                        dy.Receiver = objOrderReceive.Receive;
                        dy.ReceiveTel = objOrderReceive.ReceiveTel;
                        dy.ReceiveCel = objOrderReceive.ReceiveCel;
                        dy.ReceiveAddr = objOrderReceive.ReceiveAddr;
                    }

                    OrderModify objOrderModify = objOrderModify_List.Where(p => p.OrderNo == dy.OrderNo && p.SubOrderNo == objOrderReceive.SubOrderNo).OrderByDescending(p => p.Id).FirstOrDefault();
                    if (objOrderModify != null)
                    {
                        dy.Receiver = objOrderModify.CustomerName;
                        dy.ReceiveTel = objOrderModify.Tel;
                        dy.ReceiveCel = objOrderModify.Mobile;
                        dy.ReceiveAddr = objOrderModify.Addr;
                    }

                    //数据解密并脱敏
                    EncryptionFactory.Create(dy, new string[] { "UserName", "Receiver", "ReceiveTel", "ReceiveCel", "ReceiveAddr" }).HideSensitive();
                }
                //返回信息
                _result.Data = new
                {
                    total = _TotalCount,
                    rows = from dy in _list
                           select new
                           {
                               ck = dy.Id,
                               s1 = string.Format("<a href=\"javascript:void(0);\" onclick=\"artDialogExtend.Dialog.Open('" + Url.Action("Detail", "OrderQuery") + "?ID={0}',{{title:'{1}-{0}',width:'100%',height:'100%'}});\">{0}</a>", dy.OrderNo, _LanguagePack["orderquery_detail_title"]),
                               s2 = dy.MallName,
                               s3 = OrderHelper.GetOrderTypeDisplay(dy.OrderType, true),
                               s4 = OrderHelper.GetShippingMethodDisplay(dy.ShippingMethod, true),
                               s5 = VariableHelper.FormateMoney(dy.OrderAmount),
                               s6 = VariableHelper.FormateMoney(dy.PaymentAmount),
                               s7 = dy.UserName,
                               s8 = dy.Receiver,
                               s9 = dy.ReceiveTel,
                               s10 = dy.ReceiveCel,
                               s11 = string.Format("{0}{1}", (dy.IsMultipleReceive) ? "<i class=\"fa fa-venus-double color_primary\"></i>" : "", dy.ReceiveAddr),
                               s12 = OrderHelper.GetOrderStatusDisplay(dy.Status, true),
                               s13 = OrderHelper.GetPaymentTypeDisplay(dy.PaymentType),
                               s14 = VariableHelper.FormateTime(dy.PaymentDate, "yyyy-MM-dd HH:mm:ss"),
                               s15 = OrderHelper.GetOrderSourceDisplay(dy.OrderSource),
                               s16 = VariableHelper.FormateTime(dy.CreateDate, "yyyy-MM-dd HH:mm:ss")
                           }
                };
            }
            return _result;
        }

        [UserLoginAuthorize]
        public ContentResult Index_Message_Detail()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            ContentResult _result = new ContentResult();
            Int64 _ID = VariableHelper.SaferequestInt64(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                //不显示套装原订单和已删除的订单
                StringBuilder objStr = new StringBuilder();
                var _list = db.Database.SqlQuery<OrderQueryDetail>("select d.Id, d.OrderNo, d.SubOrderNo, d.SKU, d.ProductName, d.RRPPrice, d.SellingPrice, d.PaymentAmount, d.ActualPaymentAmount, d.Status, d.Quantity, d.CancelQuantity, d.ReturnQuantity, d.ExchangeQuantity, d.RejectQuantity, d.ShippingStatus, d.IsReservation, d.ReservationDate, d.CreateDate, d.IsError, d.IsSet, d.IsSetOrigin, d.IsPre, d.IsUrgent, d.IsExchangeNew, d.IsDelete, isnull(ds.InvoiceNo, '')As InvoiceNo, isnull((select GroupDesc from Product as p where p.sku = d.sku), '') as [collection], isnull((select count(*) from OrderValueAddedService as vas where vas.SubOrderNo = d.SubOrderNo and vas.Type = " + (int)ValueAddedServicesType.Monogram + "),0) as IsMonogram from OrderDetail as d inner join[Order] as o on d.OrderNo = o.OrderNo left join Deliverys as ds on ds.SubOrderNo = d.SubOrderNo where d.OrderId ={0} and d.IsSetOrigin = 0 and d.IsDelete = 0 order by d.SetCode asc, d.Id asc", _ID);
                string _orderNo = _list.FirstOrDefault().OrderNo;
                //获取赠品信息
                List<OrderGift> orderGifts = db.OrderGift.Where(p => p.OrderNo == _orderNo).ToList();
                //获取文档信息
                List<DeliverysDocument> deliverysDocuments = db.DeliverysDocument.Where(p => p.OrderNo == _orderNo && p.DocumentType == (int)ECommerceDocumentType.ShippingDoc).ToList();
                objStr.Append("<table class=\"common_table\">");
                objStr.Append("<tr>");
                objStr.AppendFormat("<th>{0}</th>", _LanguagePack["orderquery_index_outerID"]);
                objStr.AppendFormat("<th>{0}</th>", _LanguagePack["orderquery_index_product_sku"]);
                objStr.AppendFormat("<th>{0}</th>", _LanguagePack["orderquery_index_productname"]);
                objStr.AppendFormat("<th>{0}</th>", _LanguagePack["orderquery_index_product_rrp"]);
                objStr.AppendFormat("<th>{0}</th>", _LanguagePack["orderquery_index_product_price"]);
                objStr.AppendFormat("<th>{0}</th>", _LanguagePack["orderquery_index_product_quantity"]);
                objStr.AppendFormat("<th>{0}</th>", _LanguagePack["orderquery_index_payment_amount"]);
                objStr.AppendFormat("<th>{0}</th>", _LanguagePack["orderquery_index_actual_payment"]);
                objStr.AppendFormat("<th>{0}</th>", _LanguagePack["orderquery_index_product_status"]);
                objStr.AppendFormat("<th>{0}</th>", _LanguagePack["orderquery_index_product_shipping_status"]);
                objStr.AppendFormat("<th>{0}</th>", _LanguagePack["orderquery_index_express"]);
                //objStr.AppendFormat("<th>{0}</th>", _LanguagePack["orderquery_index_product_sendtime"]);
                objStr.Append("</tr>");
                foreach (var dy in _list)
                {
                    //赠品
                    List<OrderGift> _Gifts = orderGifts.Where(p => p.SubOrderNo == dy.SubOrderNo).ToList();
                    //文档
                    DeliverysDocument _Doc = deliverysDocuments.Where(p => p.SubOrderNo == dy.SubOrderNo).FirstOrDefault();
                    if (dy.Id == _list.LastOrDefault().Id && _Gifts.Count == 0)
                    {
                        objStr.Append("<tr class=\"last\">");
                    }
                    else
                    {
                        objStr.Append("<tr>");
                    }
                    objStr.AppendFormat("<td class=\"textalign_left\">{0}{1}</td>", dy.SubOrderNo, OrderHelper.GetOrderNatureLabel(new OrderHelper.OrderNature() { IsReservation = dy.IsReservation, IsPre = dy.IsPre, IsUrgent = dy.IsUrgent, IsSet = dy.IsSet, IsSetOrigin = dy.IsSetOrigin, IsExchangeNew = dy.IsExchangeNew, IsMonogram = (dy.IsMonogram > 0), IsError = dy.IsError }));
                    objStr.AppendFormat("<td>{0}</td>", dy.SKU);
                    objStr.AppendFormat("<td class=\"textalign_left\" style=\"width:20%;\"><label class=\"font-bold\">{0}</label><br/>{1}</td>", dy.Collection, dy.ProductName);
                    objStr.AppendFormat("<td>{0}</td>", VariableHelper.FormateMoney(dy.RRPPrice));
                    objStr.AppendFormat("<td>{0}</td>", VariableHelper.FormateMoney(dy.SellingPrice));
                    objStr.AppendFormat("<td>{0}/<label class=\"color_danger\">{1}</label>/<label class=\"color_primary\">{2}</label>/<label class=\"color_info\">{3}</label>/<label class=\"color_warning\">{4}</label></td>", dy.Quantity, dy.CancelQuantity, dy.ReturnQuantity, dy.ExchangeQuantity, dy.RejectQuantity);
                    objStr.AppendFormat("<td>{0}</td>", VariableHelper.FormateMoney(dy.PaymentAmount));
                    objStr.AppendFormat("<td>{0}</td>", VariableHelper.FormateMoney(OrderHelper.MathRound(dy.ActualPaymentAmount)));
                    objStr.AppendFormat("<td>{0}</td>", OrderHelper.GetProductStatusDisplay(dy.Status, true));
                    objStr.AppendFormat("<td>{0}</td>", OrderHelper.GetWarehouseProcessStatusDisplay(dy.ShippingStatus, true));
                    objStr.AppendFormat("<td>{0}{1}</td>", dy.InvoiceNo, (_Doc != null) ? $"<a href=\"" + Url.Action("DocumentDetail", "OrderQuery") + "?id=" + dy.Id + "\" target=\"_blank\"><i class=\"fa fa-print color_success\"></i></a>" : "");
                    //objStr.AppendFormat("<td>{0}</td>", VariableHelper.FormateTime(dy.ReservationDate, "yyyy-MM-dd"));
                    objStr.Append("</tr>");
                    //赠品
                    foreach (var _gf in _Gifts)
                    {
                        if (dy.Id == _list.LastOrDefault().Id && _gf.ID == _Gifts.LastOrDefault().ID)
                        {
                            objStr.Append("<tr class=\"last\">");
                        }
                        else
                        {
                            objStr.Append("<tr>");
                        }
                        objStr.AppendFormat("<td class=\"textalign_left\">{0}{1}</td>", _gf.GiftNo, OrderHelper.GetOrderNatureLabel(new OrderHelper.OrderNature() { IsGift = true }));
                        objStr.AppendFormat("<td>{0}</td>", _gf.Sku);
                        objStr.AppendFormat("<td class=\"textalign_left\" style=\"width:20%;\">{0}</td>", _gf.ProductName);
                        objStr.Append("<td>0</td>");
                        objStr.AppendFormat("<td>{0}</td>", VariableHelper.FormateMoney(_gf.Price));
                        objStr.AppendFormat("<td>{0}</td>", _gf.Quantity);
                        objStr.Append("<td>0</td>");
                        objStr.Append("<td>0</td>");
                        objStr.Append("<td colspan=\"4\">--</td>");
                        objStr.Append("</tr>");
                    }
                }
                objStr.Append("</table>");
                _result.ContentEncoding = System.Text.Encoding.UTF8;
                _result.Content = objStr.ToString();
                return _result;
            }
        }
        #endregion

        #region 订单详情
        [UserLoginAuthorize]
        public ActionResult Detail()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;
            //功能权限
            ViewBag.FunctionPower = this.FunctionPowers();

            string _OrderID = VariableHelper.SaferequestStr(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                var _OrderInfo_ = (from o in db.Order.Where(p => p.OrderNo == _OrderID)
                                   join c in db.Customer on o.CustomerNo equals c.CustomerNo
                                   into tmp
                                   from c in tmp.DefaultIfEmpty()
                                   select new
                                   {
                                       OrderNo = o.OrderNo,
                                       MallName = o.MallName,
                                       OrderType = o.OrderType,
                                       PlatformType = o.PlatformType,
                                       PaymentType = o.PaymentType,
                                       OrderAmount = o.OrderAmount,
                                       PaymentAmount = o.PaymentAmount,
                                       DiscountAmount = o.DiscountAmount,
                                       PaymentDate = o.PaymentDate,
                                       BalanceAmount = o.BalanceAmount,
                                       PointAmount = o.PointAmount,
                                       Point = o.Point,
                                       Status = o.Status,
                                       EBStatus = o.EBStatus,
                                       DeliveryFee = o.DeliveryFee,
                                       ShippingMethod = o.ShippingMethod,
                                       CreateDate = o.CreateDate,
                                       Remark = o.Remark,
                                       InvoiceMessage = o.InvoiceMessage,
                                       PlatformUserName = c.PlatformUserName ?? "",
                                       CustomerName = c.Name ?? ""
                                   }).SingleOrDefault();
                if (_OrderInfo_ != null)
                {
                    dynamic _OrderInfo = GenericHelper.ConvertToDynamic(_OrderInfo_);
                    string _orderNo = _OrderInfo.OrderNo;
                    //数据解密
                    EncryptionFactory.Create(_OrderInfo, new string[] { "PlatformUserName", "CustomerName" }).Decrypt();
                    List<OrderReceive> objOrderReceive_List = db.OrderReceive.Where(p => p.OrderNo == _orderNo).ToList();
                    foreach (var objOrderReceive in objOrderReceive_List)
                    {
                        //数据解密
                        EncryptionFactory.Create(objOrderReceive).Decrypt();
                        //查询是否修改过收货信息
                        OrderModify objOrderModify = db.OrderModify.Where(p => p.OrderNo == objOrderReceive.OrderNo && p.SubOrderNo == objOrderReceive.SubOrderNo && p.Status == (int)ProcessStatus.ModifyComplete).OrderByDescending(p => p.Id).FirstOrDefault();
                        if (objOrderModify != null)
                        {
                            //数据解密
                            EncryptionFactory.Create(objOrderModify).Decrypt();
                            if (objOrderReceive.Receive != objOrderModify.CustomerName)
                            {
                                objOrderReceive.Receive = $"<label class=\"color_fail line-throgth\">{objOrderReceive.Receive}</label><br/>{objOrderModify.CustomerName}";
                            }
                            if (objOrderReceive.ReceiveTel != objOrderModify.Tel)
                            {
                                objOrderReceive.ReceiveTel = $"<label class=\"color_fail line-throgth\">{objOrderReceive.ReceiveTel}</label><br/>{objOrderModify.Tel}";
                            }
                            if (objOrderReceive.ReceiveCel != objOrderModify.Mobile)
                            {
                                objOrderReceive.ReceiveCel = $"<label class=\"color_fail line-throgth\">{objOrderReceive.ReceiveCel}</label><br/>{objOrderModify.Mobile}";
                            }
                            if (objOrderReceive.ReceiveZipcode != objOrderModify.Zipcode)
                            {
                                objOrderReceive.ReceiveZipcode = $"<label class=\"color_fail line-throgth\">{objOrderReceive.ReceiveZipcode}</label><br/>{objOrderModify.Zipcode}";
                            }
                            if (objOrderReceive.ReceiveAddr != objOrderModify.Addr)
                            {
                                objOrderReceive.ReceiveAddr = $"<label class=\"color_fail line-throgth\">{objOrderReceive.ReceiveAddr}</label><br/>{objOrderModify.Addr}";
                            }
                        }
                    }
                    //收货信息
                    ViewData["order_receive"] = objOrderReceive_List;
                    //子订单信息
                    var orderDetails = db.OrderDetail.Where(p => p.OrderNo == _orderNo).ToList();
                    ViewData["detail_list"] = orderDetails;
                    //产品信息
                    List<string> skus = orderDetails.GroupBy(p => p.SKU).Select(o => o.Key).ToList();
                    ViewData["product_list"] = db.Product.Where(p => skus.Contains(p.SKU)).ToList();
                    //物流信息
                    ViewData["delivery_list"] = db.Deliverys.Where(p => p.OrderNo == _orderNo).ToList();
                    ////信用卡信息
                    //OrderBilling objOrderBilling = db.OrderBilling.Where(p => p.OrderNo == _orderNo).FirstOrDefault();
                    ////解密数据
                    //EncryptionFactory.Create(objOrderBilling).Decrypt();
                    //促销活动信息
                    ViewData["adjustment_list"] = db.OrderDetailAdjustment.Where(p => p.OrderNo == _orderNo).ToList();
                    //运费折扣信息(过滤0折扣记录)
                    var objOrderShippingAdjustment_List = db.OrderShippingAdjustment.Where(p => p.OrderNo == _orderNo).Where(p => p.AdjustmentGrossPrice < 0).ToList();
                    ViewData["shippingAdjustment_list"] = objOrderShippingAdjustment_List;
                    //增值服务
                    ViewData["vas_list"] = db.OrderValueAddedService.Where(p => p.OrderNo == _orderNo).ToList();
                    //赠品信息
                    ViewData["gift_list"] = db.OrderGift.Where(p => p.OrderNo == _orderNo).ToList();
                    //文档shipment下载
                    ViewData["document_list"] = db.DeliverysDocument.Where(p => p.OrderNo == _orderNo && p.DocumentType == (int)ECommerceDocumentType.ShippingDoc).ToList();
                    //混合支付
                    ViewData["payment_detail_list"] = db.OrderPaymentDetail.Where(p => p.OrderNo == _orderNo).ToList();

                    return View(_OrderInfo);
                }
                else
                {
                    return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.NoMessage });
                }
            }
        }

        /// <summary>
        /// 手动完结订单
        /// </summary>
        /// <returns></returns>
        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult Delivered_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            Int64 _ID = VariableHelper.SaferequestInt64(Request.Form["detailid"]);
            using (var db = new ebEntities())
            {
                try
                {
                    View_OrderDetail objView_OrderDetail = db.View_OrderDetail.Where(p => p.DetailID == _ID).SingleOrDefault();
                    if (objView_OrderDetail != null)
                    {
                        OrderService.OrderStatus_InDeliveryToDelivered(objView_OrderDetail, "Manually set Express Status", db);
                        //修改物流状态
                        Deliverys objDeliverys = db.Deliverys.Where(p => p.OrderNo == objView_OrderDetail.OrderNo && p.SubOrderNo == objView_OrderDetail.SubOrderNo).SingleOrDefault();
                        if (objDeliverys != null)
                        {
                            objDeliverys.ExpressStatus = (int)ExpressStatus.Signed;
                            objDeliverys.ExpressMsg = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} PM Delivered (OMS) ({this.CurrentLoginUser.UserName} Manual confirmation)<br/>{objDeliverys.ExpressMsg}";
                            db.SaveChanges();
                        }
                        //如果是Tumi/Micros订单
                        if (objView_OrderDetail.PlatformType == (int)PlatformType.TUMI_Japan || objView_OrderDetail.PlatformType == (int)PlatformType.Micros_Japan)
                        {
                            //如果是COD的订单,更新付款时间
                            if (objView_OrderDetail.PaymentType == (int)PayType.CashOnDelivery)
                            {
                                db.Database.ExecuteSqlCommand("Update [Order] set PaymentDate={0},PaymentStatus={1} where Id={2}", DateTime.Now, "PAID", objView_OrderDetail.OrderId);
                            }
                        }

                        //返回信息
                        _result.Data = new
                        {
                            result = true,
                            msg = _LanguagePack["common_data_save_success"]
                        };
                    }
                    else
                    {
                        throw new Exception(_LanguagePack["common_data_load_false"]);
                    }
                }
                catch (Exception ex)
                {
                    _result.Data = new
                    {
                        result = false,
                        msg = ex.Message
                    };
                }
            }
            return _result;
        }

        /// <summary>
        /// 订单日志
        /// </summary>
        /// <returns></returns>
        [UserLoginAuthorize(Type = UserLoginAuthorize.ResultType.Json)]
        public JsonResult Log_Message()
        {
            JsonResult _result = new JsonResult();
            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            string _orderNo = VariableHelper.SaferequestStr(Request.Form["orderid"]);
            string _subOrderid = VariableHelper.SaferequestStr(Request.Form["subOrderid"]);
            using (var db = new DynamicRepository())
            {
                //搜索条件
                if (string.IsNullOrEmpty(_subOrderid))
                {
                    using (var db1 = new ebEntities())
                    {
                        //默认取第一个子订单(过滤套装原始订单)
                        OrderDetail objOrderDetail = db1.OrderDetail.Where(p => p.OrderNo == _orderNo && !p.IsSetOrigin).FirstOrDefault();
                        if (objOrderDetail != null)
                        {
                            _subOrderid = objOrderDetail.SubOrderNo;
                        }
                    }
                }
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "SubOrderNo={0}", Param = _subOrderid });
                //查询
                var _list = db.GetPage<dynamic>("select SubOrderNo, NewStatus, isnull((select RealName from UserInfo where UserInfo.UserId = OrderLog.UserId), '')As UserName, CreateDate, Msg from OrderLog", _SqlWhere, VariableHelper.SaferequestInt(Request.Form["rows"]), VariableHelper.SaferequestInt(Request.Form["page"]));
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               s1 = dy.SubOrderNo,
                               s2 = OrderHelper.GetProductStatusDisplay(dy.NewStatus),
                               s3 = dy.UserName,
                               s4 = dy.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"),
                               s5 = dy.Msg
                           }
                };
                return _result;
            }
        }

        private class CardBalanceMutations
        {
            public string Points { get; set; }
        }
        #endregion

        #region 文档详情
        [UserLoginAuthorize]
        public ActionResult DocumentDetail()
        {
            Int64 _ID = VariableHelper.SaferequestInt64(Request.QueryString["id"]);
            using (var db = new ebEntities())
            {
                OrderDetail objOrderDetail = db.OrderDetail.Where(p => p.Id == _ID).SingleOrDefault();
                if (objOrderDetail != null)
                {
                    List<DeliverysDocument> objDoc_List = db.DeliverysDocument.Where(p => p.OrderNo == objOrderDetail.OrderNo && p.SubOrderNo == objOrderDetail.SubOrderNo).ToList();
                    var _d1 = objDoc_List.Where(p => p.DocumentType == (int)ECommerceDocumentType.InvoiceDoc).SingleOrDefault();
                    ViewBag.InvoiceDoc = (_d1 != null) ? DeliverysDocumentService.ExistDocMapPath(_d1.DocumentFile) : null;
                    var _d2 = objDoc_List.Where(p => p.DocumentType == (int)ECommerceDocumentType.ShippingDoc).SingleOrDefault();
                    ViewBag.ShippingDoc = (_d2 != null) ? DeliverysDocumentService.ExistDocMapPath(_d2.DocumentFile) : null;

                    return View();
                }
                else
                {
                    return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.NoMessage });
                }
            }
        }
        #endregion

        #region 生成文档
        [UserPowerAuthorize]
        public FileResult ExportExcel()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            string _orderNo = VariableHelper.SaferequestStr(Request.Form["OrderNumber"]);
            int _order_type = VariableHelper.SaferequestInt(Request.Form["OrderType"]);
            string[] _storeid = VariableHelper.SaferequestStringArray(Request.Form["StoreName"]);
            string _sku_id = VariableHelper.SaferequestStr(Request.Form["SkuID"]);
            int _product_brand = VariableHelper.SaferequestInt(Request.Form["ProductBrand"]);
            string _price_min = VariableHelper.SaferequestNull(Request.Form["ProductPriceMin"]);
            string _price_max = VariableHelper.SaferequestNull(Request.Form["ProductPriceMax"]);
            int _time_type = VariableHelper.SaferequestInt(Request.Form["Time_type"]);
            string _time1 = VariableHelper.SaferequestStr(Request.Form["Time1"]);
            string _time2 = VariableHelper.SaferequestStr(Request.Form["Time2"]);
            int[] _order_status = VariableHelper.SaferequestIntArray(Request.Form["OrderStatus"]);
            int[] _product_status = VariableHelper.SaferequestIntArray(Request.Form["ProductStatus"]);
            string _customer = VariableHelper.SaferequestStr(Request.Form["Customer"]);
            string _receiver = VariableHelper.SaferequestStr(Request.Form["Receiver"]);
            string _contact = VariableHelper.SaferequestStr(Request.Form["contact"]);
            string _shipping_status = VariableHelper.SaferequestStr(Request.Form["ShippingStatus"]);
            string _express_status = VariableHelper.SaferequestStr(Request.Form["ExpressStatus"]);
            string _express = VariableHelper.SaferequestStr(Request.Form["Express"]);
            int _shippping_method = VariableHelper.SaferequestInt(Request.Form["ShippingMethod"]);
            string _deliveryfee_min = VariableHelper.SaferequestNull(Request.Form["DeliveryFeeMin"]);
            string _deliveryfee_max = VariableHelper.SaferequestNull(Request.Form["DeliveryFeeMax"]);
            int[] _payment_status = VariableHelper.SaferequestIntArray(Request.Form["PaymentStatus"]);
            //int[] _order_source = VariableHelper.SaferequestIntArray(Request.Form["OrderSource"]);
            int _is_reserve = VariableHelper.SaferequestInt(Request.Form["OrderReserve"]);
            int _is_bundle = VariableHelper.SaferequestInt(Request.Form["IsBundle"]);
            int _is_gift = VariableHelper.SaferequestInt(Request.Form["IsGift"]);
            int _is_monogram = VariableHelper.SaferequestInt(Request.Form["IsMonogram"]);
            string _promotion_name = VariableHelper.SaferequestStr(Request.Form["PromotionName"]);
            string _coupon_code = VariableHelper.SaferequestStr(Request.Form["CouponCode"]);

            using (var db = new ebEntities())
            {
                //查询条件
                List<String> _SqlWhere = new List<string>();
                if (!string.IsNullOrEmpty(_orderNo))
                {
                    _SqlWhere.Add("((od.OrderNo like '%" + _orderNo + "%') or (od.SubOrderNo like '%" + _orderNo + "%'))");
                }

                if (_order_type > 0)
                {
                    _SqlWhere.Add("o.OrderType =" + _order_type);
                }

                //默认显示当前账号允许看到的店铺订单
                var _UserMalls = this.CurrentLoginUser.UserMalls;
                List<string> _SearchStores = new List<string>();
                if (_storeid != null)
                {
                    _SearchStores = _storeid.Where(p => _UserMalls.Contains(p)).ToList();
                }
                else
                {
                    _SearchStores = _UserMalls;
                }

                if (_UserMalls.Count == 1)
                {
                    _SqlWhere.Add("o.MallSapCode=" + _UserMalls.First());
                }
                else
                {
                    _SqlWhere.Add("o.MallSapCode in (select item from strToIntTable('" + string.Join(",", _SearchStores) + "',','))");
                }

                if (!string.IsNullOrEmpty(_sku_id))
                {
                    _SqlWhere.Add("((od.SKU like '%" + _sku_id + "%') or (od.SetCode like '%" + _sku_id + "%'))");
                }

                if (_product_brand > 0)
                {
                    string _Brands = string.Join(",", BrandService.GetSons(_product_brand));
                    if (!string.IsNullOrEmpty(_Brands))
                    {
                        _SqlWhere.Add("charindex((select top 1 name from Product where Product.SKU=od.SKU),'" + _Brands + "')>0");
                    }
                }

                if (!string.IsNullOrEmpty(_express))
                {
                    _SqlWhere.Add("ds.InvoiceNo like '%" + _express + "%'");
                }

                if (!string.IsNullOrEmpty(_express_status))
                {
                    _SqlWhere.Add("ds.ExpressStatus =" + VariableHelper.SaferequestInt(_express_status));
                }

                if (!string.IsNullOrEmpty(_price_min))
                {
                    _SqlWhere.Add("o.PaymentAmount>=" + VariableHelper.SaferequestDecimal(_price_min));
                }

                if (!string.IsNullOrEmpty(_price_max))
                {
                    _SqlWhere.Add("o.PaymentAmount<=" + VariableHelper.SaferequestDecimal(_price_max));
                }

                switch (_time_type)
                {
                    case 1:
                        if (!string.IsNullOrEmpty(_time1))
                        {
                            _SqlWhere.Add("datediff(second,o.CreateDate,'" + _time1 + "')<=0");
                        }

                        if (!string.IsNullOrEmpty(_time2))
                        {
                            _SqlWhere.Add("datediff(second,o.CreateDate,'" + _time2 + "')>=0");
                        }
                        break;
                    case 2:
                        if (!string.IsNullOrEmpty(_time1))
                        {
                            _SqlWhere.Add("datediff(second,o.PaymentDate,'" + _time1 + "')<=0");
                        }

                        if (!string.IsNullOrEmpty(_time2))
                        {
                            _SqlWhere.Add("datediff(second,o.PaymentDate,'" + _time2 + "')>=0");
                        }
                        break;
                    case 3:
                        if (!string.IsNullOrEmpty(_time1))
                        {
                            _SqlWhere.Add("datediff(second,od.CompleteDate,'" + _time1 + "')<=0");
                        }

                        if (!string.IsNullOrEmpty(_time2))
                        {
                            _SqlWhere.Add("datediff(second,od.CompleteDate,'" + _time2 + "')>=0");
                        }
                        break;
                    default:
                        break;
                }

                if (_order_status != null)
                {
                    _SqlWhere.Add("o.Status in (" + string.Join(",", _order_status) + ")");
                }

                if (_product_status != null)
                {
                    List<int> _psList = new List<int>();
                    if (_product_status.Contains((int)ProductStatus.Received))
                    {
                        _psList.Add((int)ProductStatus.Received);
                    }
                    if (_product_status.Contains((int)ProductStatus.Processing))
                    {
                        _psList.Add((int)ProductStatus.Processing);
                    }
                    if (_product_status.Contains((int)ProductStatus.InDelivery))
                    {
                        _psList.Add((int)ProductStatus.InDelivery);
                    }
                    if (_product_status.Contains((int)ProductStatus.Delivered))
                    {
                        _psList.Add((int)ProductStatus.Delivered);
                    }
                    if (_product_status.Contains((int)ProductStatus.Modify))
                    {
                        _psList.Add((int)ProductStatus.Modify);
                    }
                    if (_product_status.Contains((int)ProductStatus.Cancel))
                    {
                        _psList.Add((int)ProductStatus.Cancel);
                        _psList.Add((int)ProductStatus.CancelComplete);
                    }
                    if (_product_status.Contains((int)ProductStatus.Return))
                    {
                        _psList.Add((int)ProductStatus.Return);
                        _psList.Add((int)ProductStatus.ReturnComplete);
                    }
                    if (_product_status.Contains((int)ProductStatus.Exchange))
                    {
                        _psList.Add((int)ProductStatus.Exchange);
                        _psList.Add((int)ProductStatus.ExchangeComplete);
                    }
                    if (_product_status.Contains((int)ProductStatus.Reject))
                    {
                        _psList.Add((int)ProductStatus.Reject);
                    }
                    if (_psList.Count > 0)
                    {
                        _SqlWhere.Add("od.[Status] in (" + string.Join(",", _psList) + ")");
                    }
                }

                if (!string.IsNullOrEmpty(_customer))
                {
                    _SqlWhere.Add("c.Name='" + EncryptionBase.EncryptString(_customer) + "'");
                }

                if (!string.IsNullOrEmpty(_receiver))
                {
                    _SqlWhere.Add("oe.Receive='" + EncryptionBase.EncryptString(_receiver) + "'");
                }

                if (!string.IsNullOrEmpty(_contact))
                {
                    _SqlWhere.Add("((oe.ReceiveTel='" + EncryptionBase.EncryptString(_contact) + "') or (oe.ReceiveCel='" + EncryptionBase.EncryptString(_contact) + "'))");
                }

                if (!string.IsNullOrEmpty(_shipping_status))
                {
                    _SqlWhere.Add("od.ShippingStatus=" + VariableHelper.SaferequestInt(_shipping_status));
                }

                if (_payment_status != null)
                {
                    _SqlWhere.Add("o.PaymentType in (" + string.Join(",", _payment_status) + ")");
                }

                if (_shippping_method > 0)
                {
                    if (_shippping_method == (int)ShippingMethod.ExpressShipping)
                    {
                        _SqlWhere.Add("o.ShippingMethod=" + (int)ShippingMethod.ExpressShipping);
                    }
                    else
                    {
                        _SqlWhere.Add("o.ShippingMethod=" + (int)ShippingMethod.StandardShipping);
                    }
                }

                if (!string.IsNullOrEmpty(_deliveryfee_min))
                {
                    _SqlWhere.Add("o.DeliveryFee>=" + VariableHelper.SaferequestDecimal(_deliveryfee_min));
                }

                if (!string.IsNullOrEmpty(_deliveryfee_max))
                {
                    _SqlWhere.Add("o.DeliveryFee<=" + VariableHelper.SaferequestDecimal(_deliveryfee_max));
                }

                //if (_order_source != null)
                //{
                //    _SqlWhere.Add("o.OrderSource in (" + string.Join(",", _order_source) + ")");
                //}

                if (_is_reserve > 0)
                {
                    _SqlWhere.Add("od.IsReservation=1");
                }

                if (_is_bundle > 0)
                {
                    _SqlWhere.Add("od.IsSet=1");
                }

                if (_is_gift > 0)
                {
                    _SqlWhere.Add("(select count(*) from OrderGift where o.OrderNo=OrderGift.OrderNo)>0");
                }

                if (_is_monogram > 0)
                {
                    _SqlWhere.Add("(select count(*) from OrderValueAddedService where o.OrderNo=OrderValueAddedService.OrderNo and OrderValueAddedService.Type=" + (int)ValueAddedServicesType.Monogram + ")>0");
                }

                if (!string.IsNullOrEmpty(_promotion_name))
                {
                    _SqlWhere.Add("(select count(*) from OrderDetailAdjustment where o.OrderNo=OrderDetailAdjustment.OrderNo and OrderDetailAdjustment.LineitemText like '%" + _promotion_name + "%')>0");
                }

                if (!string.IsNullOrEmpty(_coupon_code))
                {
                    _SqlWhere.Add("(select count(*) from OrderDetailAdjustment where o.OrderNo=OrderDetailAdjustment.OrderNo and OrderDetailAdjustment.CouponId like '%" + _coupon_code + "%')>0");
                }

                //过滤套装原始订单
                _SqlWhere.Add("od.IsSetOrigin=0");
                //过滤无效的订单
                _SqlWhere.Add("od.IsDelete=0");

                DataTable dt = new DataTable();
                dt.Columns.Add(_LanguagePack["orderquery_index_order_number"]);
                dt.Columns.Add(_LanguagePack["orderquery_index_outerID"]);
                dt.Columns.Add(_LanguagePack["orderquery_index_storecode"]);
                dt.Columns.Add(_LanguagePack["orderquery_index_storename"]);
                dt.Columns.Add(_LanguagePack["orderquery_index_ordertype"]);
                dt.Columns.Add(_LanguagePack["orderquery_index_shippingmethod"]);
                dt.Columns.Add(_LanguagePack["orderquery_index_order_amount"]);
                dt.Columns.Add(_LanguagePack["orderquery_index_order_payment_amount"]);
                dt.Columns.Add(_LanguagePack["orderquery_index_customer_name"]);
                dt.Columns.Add(_LanguagePack["orderquery_index_receiver"]);
                dt.Columns.Add(_LanguagePack["orderquery_index_customer_tel"]);
                dt.Columns.Add(_LanguagePack["orderquery_index_customer_mobile"]);
                dt.Columns.Add(_LanguagePack["orderquery_index_customer_zipcode"]);
                dt.Columns.Add(_LanguagePack["orderquery_index_customer_addr"]);
                dt.Columns.Add(_LanguagePack["orderquery_index_payment_type"]);
                dt.Columns.Add(_LanguagePack["orderquery_index_paytime"]);
                //dt.Columns.Add(_LanguagePack["orderquery_index_source"]);
                dt.Columns.Add(_LanguagePack["orderquery_index_ordertime"]);
                dt.Columns.Add(_LanguagePack["orderquery_index_product_sku"]);
                dt.Columns.Add(_LanguagePack["orderquery_index_productname"]);
                //dt.Columns.Add(_LanguagePack["orderquery_index_supply_price"]);
                dt.Columns.Add(_LanguagePack["orderquery_index_product_rrp"]);
                dt.Columns.Add(_LanguagePack["orderquery_index_product_price"]);
                dt.Columns.Add(_LanguagePack["orderquery_index_product_quantity"]);
                dt.Columns.Add(_LanguagePack["orderquery_index_payment_amount"]);
                dt.Columns.Add(_LanguagePack["orderquery_index_actual_payment"]);
                dt.Columns.Add(_LanguagePack["orderquery_index_invoince_no"]);
                dt.Columns.Add(_LanguagePack["orderquery_index_product_sendtime"]);
                dt.Columns.Add(_LanguagePack["orderquery_index_product_status"]);
                dt.Columns.Add(_LanguagePack["orderquery_index_product_shipping_status"]);
                dt.Columns.Add(_LanguagePack["orderquery_index_product_is_exchange_new"]);
                dt.Columns.Add(_LanguagePack["orderquery_index_product_gift"]);
                //读取数据
                DataRow _dr = null;
                var _list = db.Database.SqlQuery<OrderQueryExport>("select o.Id,o.OrderNo,o.MallName,o.MallSapCode,o.OrderType,o.OrderAmount,o.PaymentAmount As OrderPaymentAmount,o.PaymentType,o.DeliveryFee,o.BalanceAmount,o.PointAmount,o.OrderSource,o.ShippingMethod,o.PaymentDate,isnull(c.Name,'')As UserName,o.[Status],o.CreateDate as OrderTime,oe.[Receive],oe.ReceiveTel,oe.ReceiveCel,oe.ReceiveZipcode,oe.ReceiveAddr,oe.Province as ReceiveProvince,oe.City as ReceiveCity,oe.District as ReceiveDistrict,od.SubOrderNo,od.SKU,od.ProductName,od.RRPPrice,od.SupplyPrice,od.SellingPrice,od.PaymentAmount,od.ActualPaymentAmount,od.Quantity,od.CancelQuantity,od.ReturnQuantity,od.ExchangeQuantity,od.RejectQuantity,od.ReservationDate,od.ShippingStatus,od.[Status] As ProductStatus,od.IsExchangeNew,od.CreateDate,('') as Gifts,Isnull(ds.InvoiceNo,'') as InvoiceNo from OrderDetail as od inner join [order] as o on od.OrderNo =o.OrderNo inner join OrderReceive as oe on od.SubOrderNo = oe.SubOrderNo left join Deliverys as ds on od.SubOrderNo=ds.SubOrderNo left join Customer as c on o.CustomerNo = c.CustomerNo " + ((_SqlWhere.Count > 0) ? " where " + string.Join(" and ", _SqlWhere) : "") + " order by o.CreateDate desc");
                List<string> _Orders = _list.Select(p => "'" + (string)p.OrderNo + "'").ToList();
                var _orderNos = _list.GroupBy(p => p.OrderNo).Select(o => o.Key).ToList();
                List<OrderModify> orderModifies = new List<OrderModify>();
                List<OrderGift> orderGifts = new List<OrderGift>();
                if (_Orders.Count > 0)
                {
                    //获取更新的地址信息
                    orderModifies = db.OrderModify.Where(p => _orderNos.Contains(p.OrderNo) && p.Status == (int)ProcessStatus.ModifyComplete).ToList();
                    //读取赠品
                    orderGifts = db.OrderGift.Where(p => _orderNos.Contains(p.OrderNo)).ToList();
                }

                foreach (var dy in _list)
                {
                    //读取最新订单收货信息
                    var objOrderModify = orderModifies.Where(p => p.OrderNo == dy.OrderNo && p.SubOrderNo == dy.SubOrderNo).OrderByDescending(p => p.Id).FirstOrDefault();
                    if (objOrderModify != null)
                    {
                        dy.Receive = objOrderModify.CustomerName;
                        dy.ReceiveTel = objOrderModify.Tel;
                        dy.ReceiveCel = objOrderModify.Mobile;
                        dy.ReceiveAddr = objOrderModify.Addr;
                    }
                    //赠品
                    var objOrderGifts = orderGifts.Where(p => p.SubOrderNo == dy.SubOrderNo).ToList();
                    foreach (var _gf in objOrderGifts)
                    {
                        if (string.IsNullOrEmpty(dy.Gifts))
                        {
                            dy.Gifts = $"{_gf.Sku}*{_gf.Quantity}";
                        }
                        else
                        {
                            dy.Gifts += $",{_gf.Sku}*{_gf.Quantity}";
                        }
                    }

                    //数据解密并脱敏
                    EncryptionFactory.Create(dy, new string[] { "UserName", "Receive", "ReceiveTel", "ReceiveCel", "ReceiveAddr" }).HideSensitive();

                    _dr = dt.NewRow();
                    _dr[0] = dy.OrderNo;
                    _dr[1] = dy.SubOrderNo;
                    _dr[2] = dy.MallSapCode;
                    _dr[3] = dy.MallName;
                    _dr[4] = OrderHelper.GetOrderTypeDisplay(dy.OrderType);
                    _dr[5] = OrderHelper.GetShippingMethodDisplay(dy.ShippingMethod);
                    _dr[6] = VariableHelper.FormateMoney(dy.OrderAmount);
                    _dr[7] = VariableHelper.FormateMoney(dy.OrderPaymentAmount);
                    _dr[8] = dy.UserName;
                    _dr[9] = dy.Receive;
                    _dr[10] = dy.ReceiveTel;
                    _dr[11] = dy.ReceiveCel;
                    _dr[12] = dy.ReceiveZipcode;
                    _dr[13] = dy.ReceiveAddr;
                    _dr[14] = OrderHelper.GetPaymentTypeDisplay(dy.PaymentType);
                    _dr[15] = VariableHelper.FormateTime(dy.PaymentDate, "yyyy-MM-dd HH:mm:ss");
                    //_dr[16] = OrderHelper.GetOrderSourceDisplay(_dy.OrderSource);
                    _dr[16] = dy.OrderTime.ToString("yyyy-MM-dd HH:mm:ss");
                    _dr[17] = dy.SKU;
                    _dr[18] = dy.ProductName;
                    _dr[19] = VariableHelper.FormateMoney(dy.RRPPrice);
                    _dr[20] = VariableHelper.FormateMoney(dy.SellingPrice);
                    _dr[21] = string.Format("{0}/{1}/{2}/{3}/{4}", dy.Quantity, dy.CancelQuantity, dy.ReturnQuantity, dy.ExchangeQuantity, dy.RejectQuantity);
                    _dr[22] = VariableHelper.FormateMoney(dy.PaymentAmount);
                    _dr[23] = VariableHelper.FormateMoney(OrderHelper.MathRound(dy.ActualPaymentAmount));
                    _dr[24] = dy.InvoiceNo;
                    _dr[25] = VariableHelper.FormateTime(dy.ReservationDate, "yyyy-MM-dd HH:mm:ss");
                    _dr[26] = OrderHelper.GetProductStatusDisplay(dy.ProductStatus, false);
                    _dr[27] = OrderHelper.GetWarehouseProcessStatusDisplay(dy.ShippingStatus);
                    _dr[28] = (dy.IsExchangeNew) ? 1 : 0;
                    _dr[29] = dy.Gifts;
                    dt.Rows.Add(_dr);
                }

                string _filepath = $"~{AppGlobalService.UPLOAD_CACHE_PATH}/{DateTime.Now.ToString("yyyy-MM-dd")}";
                if (!Directory.Exists(Server.MapPath(_filepath)))
                {
                    Directory.CreateDirectory(Server.MapPath(_filepath));
                }
                string _fileName = string.Format("Order_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss"));
                _fileName = System.Web.HttpUtility.UrlEncode(_fileName, Encoding.GetEncoding("UTF-8"));
                string _path = Server.MapPath(string.Format("{0}/{1}", _filepath, _fileName));
                ExcelHelper objExcelHelper = new ExcelHelper(_path);
                objExcelHelper.DataTableToExcel(dt, "order_1", true);
                return File(_path, "application/ms-excel", _fileName);
            }
        }
        #endregion
    }
}
