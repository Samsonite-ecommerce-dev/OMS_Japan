﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using Samsonite.OMS.Encryption;
using Samsonite.Utility.Common;
using Samsonite.OMS.Service;
using Samsonite.OMS.Service.AppConfig;
using OMS.App.Helper;

namespace OMS.App.Controllers
{
    public class EmployeeOrderController : BaseController
    {
        //
        // GET: /EmployeeOrder/

        #region 查询
        [UserPowerAuthorize]
        public ActionResult Index()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;

            //菜单栏
            ViewBag.MenuBar = this.MenuBar(this.CurrentFunctionID);
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
            ViewBag.CustomerEmail = VariableHelper.SaferequestStr(Request.QueryString["email"]);

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
            string _time1 = VariableHelper.SaferequestStr(Request.Form["time1"]);
            string _time2 = VariableHelper.SaferequestStr(Request.Form["time2"]);
            int[] _order_status = VariableHelper.SaferequestIntArray(Request.Form["order_status"]);
            int[] _product_status = VariableHelper.SaferequestIntArray(Request.Form["product_status"]);
            string _email = VariableHelper.SaferequestStr(Request.Form["email"]);
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
            int _is_reserve = VariableHelper.SaferequestInt(Request.Form["is_reserve"]);
            int _is_bundle = VariableHelper.SaferequestInt(Request.Form["is_bundle"]);
            int _is_gift = VariableHelper.SaferequestInt(Request.Form["is_gift"]);
            int _is_monogram = VariableHelper.SaferequestInt(Request.Form["is_monogram"]);
            string _promotion_name = VariableHelper.SaferequestStr(Request.Form["promotion_name"]);
            string _coupon_code = VariableHelper.SaferequestStr(Request.Form["coupon_code"]);

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

                if (!string.IsNullOrEmpty(_time1))
                {
                    _SqlWhere.Add("datediff(second,[Order].CreateDate,'" + VariableHelper.SaferequestTime(_time1) + "')<=0");
                }

                if (!string.IsNullOrEmpty(_time2))
                {
                    _SqlWhere.Add("datediff(second,[Order].CreateDate,'" + VariableHelper.SaferequestTime(_time2) + "')>=0");
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

                if (!string.IsNullOrEmpty(_email))
                {
                    _SqlWhere.Add("((select top 1 Email from Customer where Customer.CustomerNo=[Order].CustomerNo)='" + EncryptionBase.EncryptString(_email) + "')");
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

                //员工订单
                _SqlWhere.Add("OrderDetail.IsEmployee=1");
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

                List<string> _orderNos = _list.Select(p => p.OrderNo).ToList();
                //获取收货信息
                List<OrderReceive> objOrderReceive_List = db.OrderReceive.Where(p => _orderNos.Contains(p.OrderNo)).ToList();
                //获取更新的地址信息
                List<OrderModify> objOrderModify_List = db.OrderModify.Where(p => _orderNos.Contains(p.OrderNo) && p.Status == (int)ProcessStatus.ModifyComplete).ToList();
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

                    //Employee Name和Emplyee Email需要解密出来
                    string _employeeName = EncryptionBase.DecryptString(dy.UserName);
                    string _employeeEmail = EncryptionBase.DecryptString(dy.UserEmail);
                    //数据解密并脱敏
                    EncryptionFactory.Create(dy, new string[] { "UserName", "UserEmail", "Receiver", "ReceiveTel", "ReceiveCel", "ReceiveAddr" }).HideSensitive();
                    //重新赋值
                    dy.UserName = _employeeName;
                    dy.UserEmail = _employeeEmail;
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
                               s8 = dy.UserEmail,
                               s9 = dy.Receiver,
                               s10 = dy.ReceiveTel,
                               s11 = dy.ReceiveAddr,
                               s12 = OrderHelper.GetOrderStatusDisplay(dy.Status, true),
                               s13 = OrderHelper.GetPaymentTypeDisplay(dy.PaymentType),
                               s14 = VariableHelper.FormateTime(dy.PaymentDate, "yyyy-MM-dd HH:mm:ss"),
                               s15 = VariableHelper.FormateTime(dy.CreateDate, "yyyy-MM-dd HH:mm:ss")
                           }
                };
            }
            return _result;
        }
        #endregion

        #region 生成文档
        [UserPowerAuthorize]
        public FileResult ExportExcel()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            //查询条件
            string _orderNo = VariableHelper.SaferequestStr(Request.Form["OrderNumber"]);
            int _order_type = VariableHelper.SaferequestInt(Request.Form["OrderType"]);
            string[] _storeid = VariableHelper.SaferequestStringArray(Request.Form["StoreName"]);
            string _sku_id = VariableHelper.SaferequestStr(Request.Form["SkuID"]);
            int _product_brand = VariableHelper.SaferequestInt(Request.Form["ProductBrand"]);
            string _price_min = VariableHelper.SaferequestNull(Request.Form["ProductPriceMin"]);
            string _price_max = VariableHelper.SaferequestNull(Request.Form["ProductPriceMax"]);
            string _time1 = VariableHelper.SaferequestStr(Request.Form["Time1"]);
            string _time2 = VariableHelper.SaferequestStr(Request.Form["Time2"]);
            int[] _order_status = VariableHelper.SaferequestIntArray(Request.Form["OrderStatus"]);
            int[] _product_status = VariableHelper.SaferequestIntArray(Request.Form["ProductStatus"]);
            string _email = VariableHelper.SaferequestStr(Request.Form["Email"]);
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
            int _is_reserve = VariableHelper.SaferequestInt(Request.Form["OrderReserve"]);
            int _is_bundle = VariableHelper.SaferequestInt(Request.Form["IsBundle"]);
            int _is_gift = VariableHelper.SaferequestInt(Request.Form["IsGift"]);
            int _is_monogram = VariableHelper.SaferequestInt(Request.Form["isMonogram"]);
            string _promotion_name = VariableHelper.SaferequestStr(Request.Form["PromotionName"]);
            string _coupon_code = VariableHelper.SaferequestStr(Request.Form["CouponCode"]);

            using (var db = new ebEntities())
            {
                //查询条件
                List<EntityRepository.SqlQueryCondition> _sqlWhere = new List<EntityRepository.SqlQueryCondition>();
                if (!string.IsNullOrEmpty(_orderNo))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "((od.OrderNo like {0}) or (od.SubOrderNo like {0}))", Param = "%" + _orderNo + "%" });
                }

                if (_order_type > 0)
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "o.OrderType={0}", Param = _order_type });
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
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "o.MallSapCode={0}", Param = _UserMalls.First() });
                }
                else
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "o.MallSapCode in (select item from strToIntTable('" + string.Join(",", _SearchStores) + "',','))", Param = null });
                }

                if (!string.IsNullOrEmpty(_sku_id))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "((od.SKU like {0}) or (od.SetCode like {0}))", Param = "%" + _sku_id + "%" });
                }

                if (_product_brand > 0)
                {
                    string _Brands = string.Join(",", BrandService.GetSons(_product_brand));
                    if (!string.IsNullOrEmpty(_Brands))
                    {
                        _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "charindex((select top 1 name from Product where Product.SKU=od.SKU),'" + _Brands + "')>0", Param = null });
                    }
                }

                if (!string.IsNullOrEmpty(_express))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "ds.InvoiceNo={0}", Param = "%" + _express + "%" });
                }

                if (!string.IsNullOrEmpty(_express_status))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "ds.ExpressStatus={0}", Param = VariableHelper.SaferequestInt(_express_status) });
                }

                if (!string.IsNullOrEmpty(_price_min))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "o.PaymentAmount>={0}", Param = VariableHelper.SaferequestDecimal(_price_min) });
                }

                if (!string.IsNullOrEmpty(_price_max))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "o.PaymentAmount<={0}", Param = VariableHelper.SaferequestDecimal(_price_max) });
                }

                if (!string.IsNullOrEmpty(_time1))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "datediff(second, o.CreateDate, {0}) <= 0", Param = VariableHelper.SaferequestTime(_time1) });
                }

                if (!string.IsNullOrEmpty(_time2))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "datediff(second, o.CreateDate, {0}) >= 0", Param = VariableHelper.SaferequestTime(_time2) });
                }

                if (_order_status != null)
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "o.Status in (" + string.Join(", ", _order_status) + ")", Param = null });
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
                        _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "od.Status in (" + string.Join(", ", _psList) + ")", Param = null });
                    }
                }

                if (!string.IsNullOrEmpty(_email))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "c.Email={0}", Param = EncryptionBase.EncryptString(_email) });
                }

                if (!string.IsNullOrEmpty(_customer))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "c.Name={0}", Param = EncryptionBase.EncryptString(_customer) });
                }

                if (!string.IsNullOrEmpty(_receiver))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "oe.Receive={0}", Param = EncryptionBase.EncryptString(_receiver) });
                }

                if (!string.IsNullOrEmpty(_contact))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "((oe.ReceiveTel={0}) or (oe.ReceiveCel={0}))", Param = EncryptionBase.EncryptString(_contact) });
                }

                if (!string.IsNullOrEmpty(_shipping_status))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "od.ShippingStatus={0}", Param = VariableHelper.SaferequestInt(_shipping_status) });
                }

                if (_payment_status != null)
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "o.PaymentType in (" + string.Join(",", _payment_status) + ")", Param = null });
                }

                if (_shippping_method > 0)
                {
                    if (_shippping_method == (int)ShippingMethod.ExpressShipping)
                    {
                        _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "o.ShippingMethod={0}", Param = (int)ShippingMethod.ExpressShipping });
                    }
                    else
                    {
                        _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "o.ShippingMethod={0}", Param = (int)ShippingMethod.StandardShipping });
                    }
                }

                if (!string.IsNullOrEmpty(_deliveryfee_min))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "o.DeliveryFee>={0}", Param = VariableHelper.SaferequestDecimal(_deliveryfee_min) });
                }

                if (!string.IsNullOrEmpty(_deliveryfee_max))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "o.DeliveryFee<={0}", Param = VariableHelper.SaferequestDecimal(_deliveryfee_max) });
                }

                if (_is_reserve > 0)
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "od.IsReservation={0}", Param = 1 });
                }

                if (_is_bundle > 0)
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "od.IsSet={0}", Param = 1 });
                }

                if (_is_gift > 0)
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "(select count(*) from OrderGift where o.OrderNo=OrderGift.OrderNo)>0", Param = null });
                }

                if (_is_monogram > 0)
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "(select count(*) from OrderValueAddedService where o.OrderNo=OrderValueAddedService.OrderNo and OrderValueAddedService.Type=" + (int)ValueAddedServicesType.Monogram + ")>0", Param = null });
                }

                if (!string.IsNullOrEmpty(_promotion_name))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "(select count(*) from OrderDetailAdjustment where o.OrderNo=OrderDetailAdjustment.OrderNo and OrderDetailAdjustment.LineitemText like '%" + _promotion_name + "%')>0", Param = null });
                }

                if (!string.IsNullOrEmpty(_coupon_code))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "(select count(*) from OrderDetailAdjustment where o.OrderNo=OrderDetailAdjustment.OrderNo and OrderDetailAdjustment.CouponId like '%" + _coupon_code + "%')>0", Param = null });
                }

                //员工订单
                _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "od.IsEmployee={0}", Param = 1 });
                //过滤套装原始订单
                _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "od.IsSetOrigin={0}", Param = 0 });
                //过滤无效的订单
                _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "od.IsDelete={0}", Param = 0 });

                DataTable dt = new DataTable();
                dt.Columns.Add(_LanguagePack["employeeorder_index_order_number"]);
                dt.Columns.Add(_LanguagePack["employeeorder_index_outerID"]);
                dt.Columns.Add(_LanguagePack["employeeorder_index_storecode"]);
                dt.Columns.Add(_LanguagePack["employeeorder_index_storename"]);
                dt.Columns.Add(_LanguagePack["employeeorder_index_ordertype"]);
                dt.Columns.Add(_LanguagePack["employeeorder_index_shippingmethod"]);
                dt.Columns.Add(_LanguagePack["employeeorder_index_order_amount"]);
                dt.Columns.Add(_LanguagePack["employeeorder_index_order_payment_amount"]);
                dt.Columns.Add(_LanguagePack["employeeorder_index_customer_name"]);
                dt.Columns.Add(_LanguagePack["employeeorder_index_customer_email"]);
                dt.Columns.Add(_LanguagePack["employeeorder_index_receiver"]);
                dt.Columns.Add(_LanguagePack["employeeorder_index_customer_tel"]);
                dt.Columns.Add(_LanguagePack["employeeorder_index_customer_zipcode"]);
                dt.Columns.Add(_LanguagePack["employeeorder_index_customer_addr"]);
                dt.Columns.Add(_LanguagePack["employeeorder_index_payment_type"]);
                dt.Columns.Add(_LanguagePack["employeeorder_index_paytime"]);
                dt.Columns.Add(_LanguagePack["employeeorder_index_ordertime"]);
                dt.Columns.Add(_LanguagePack["employeeorder_index_product_sku"]);
                dt.Columns.Add(_LanguagePack["employeeorder_index_productname"]);
                dt.Columns.Add(_LanguagePack["employeeorder_index_supply_price"]);
                dt.Columns.Add(_LanguagePack["employeeorder_index_product_price"]);
                dt.Columns.Add(_LanguagePack["employeeorder_index_product_quantity"]);
                dt.Columns.Add(_LanguagePack["employeeorder_index_payment_amount"]);
                dt.Columns.Add(_LanguagePack["employeeorder_index_actual_payment"]);
                dt.Columns.Add(_LanguagePack["employeeorder_index_invoince_no"]);
                dt.Columns.Add(_LanguagePack["employeeorder_index_product_sendtime"]);
                dt.Columns.Add(_LanguagePack["employeeorder_index_product_status"]);
                dt.Columns.Add(_LanguagePack["employeeorder_index_product_shipping_status"]);
                dt.Columns.Add(_LanguagePack["employeeorder_index_product_is_exchange_new"]);
                dt.Columns.Add(_LanguagePack["employeeorder_index_product_gift"]);
                //读取数据
                DataRow _dr = null;
                var _list = this.BaseEntityRepository.SqlQueryGetList<OrderQueryExport>(db, "select o.Id,o.OrderNo,o.MallName,o.MallSapCode,o.OrderType,o.OrderAmount,o.PaymentAmount As OrderPaymentAmount,o.PaymentType,o.DeliveryFee,o.BalanceAmount,o.PointAmount,o.OrderSource,o.ShippingMethod,o.PaymentDate,isnull(c.Name,'')As UserName,isnull(c.Email,'')As UserEmail,o.[Status],o.CreateDate as OrderTime,oe.[Receive],oe.ReceiveTel,oe.ReceiveCel,oe.ReceiveZipcode,oe.ReceiveAddr,oe.Province as ReceiveProvince,oe.City as ReceiveCity,oe.District as ReceiveDistrict,od.SubOrderNo,od.SKU,od.ProductName,od.RRPPrice,od.SupplyPrice,od.SellingPrice,od.PaymentAmount,od.ActualPaymentAmount,od.Quantity,od.CancelQuantity,od.ReturnQuantity,od.ExchangeQuantity,od.RejectQuantity,od.ReservationDate,od.ShippingStatus,od.[Status] As ProductStatus,od.IsExchangeNew,od.CreateDate,('') as Gifts,Isnull(ds.InvoiceNo,'') as InvoiceNo from OrderDetail as od inner join [order] as o on od.OrderNo =o.OrderNo inner join OrderReceive as oe on od.SubOrderNo = oe.SubOrderNo left join Deliverys as ds on od.SubOrderNo=ds.SubOrderNo left join Customer as c on o.CustomerNo = c.CustomerNo order by o.CreateDate desc", _sqlWhere);
                var _orderNos = _list.GroupBy(p => p.OrderNo).Select(o => o.Key).ToList();
                List<OrderModify> orderModifies = new List<OrderModify>();
                List<OrderGift> orderGifts = new List<OrderGift>();
                if (_orderNos.Count > 0)
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

                    //Employee Name和Emplyee Email需要解密出来
                    string _employeeName = EncryptionBase.DecryptString(dy.UserName);
                    string _employeeEmail = EncryptionBase.DecryptString(dy.UserEmail);
                    //数据解密并脱敏
                    EncryptionFactory.Create(dy, new string[] { "UserName", "UserEmail", "Receive", "ReceiveTel", "ReceiveCel", "ReceiveAddr" }).HideSensitive();
                    //重新赋值
                    dy.UserName = _employeeName;
                    dy.UserEmail = _employeeEmail;

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
                    _dr[9] = dy.UserEmail;
                    _dr[10] = dy.Receive;
                    _dr[11] = dy.ReceiveTel;
                    _dr[12] = dy.ReceiveZipcode;
                    _dr[13] = dy.ReceiveAddr;
                    _dr[14] = OrderHelper.GetPaymentTypeDisplay(dy.PaymentType);
                    _dr[15] = VariableHelper.FormateTime(dy.PaymentDate, "yyyy-MM-dd HH:mm:ss");
                    _dr[16] = dy.OrderTime.ToString("yyyy-MM-dd HH:mm:ss");
                    _dr[17] = dy.SKU;
                    _dr[18] = dy.ProductName;
                    _dr[19] = VariableHelper.FormateMoney(dy.SupplyPrice);
                    _dr[20] = VariableHelper.FormateMoney(dy.SellingPrice);
                    _dr[21] = string.Format("{0}/{1}/{2}/{3}/{4}", dy.Quantity, dy.CancelQuantity, dy.ReturnQuantity, dy.ExchangeQuantity, dy.RejectQuantity);
                    _dr[22] = VariableHelper.FormateMoney(dy.PaymentAmount);
                    _dr[23] = VariableHelper.FormateMoney(Math.Round(dy.ActualPaymentAmount, 0));
                    _dr[24] = dy.InvoiceNo;
                    _dr[25] = VariableHelper.FormateTime(dy.ReservationDate, "yyyy-MM-dd HH:mm:ss");
                    _dr[26] = OrderHelper.GetProductStatusDisplay(dy.ProductStatus, false);
                    _dr[27] = OrderHelper.GetWarehouseProcessStatusDisplay(dy.ShippingStatus);
                    _dr[28] = (dy.IsExchangeNew) ? 1 : 0;
                    _dr[29] = dy.Gifts;
                    dt.Rows.Add(_dr);
                }

                string _filepath = string.Format("~/UploadFile/CacheFile/{0}", DateTime.Now.ToString("yyyy-MM-dd"));
                if (!Directory.Exists(Server.MapPath(_filepath)))
                {
                    Directory.CreateDirectory(Server.MapPath(_filepath));
                }
                string _fileName = string.Format("EmployeeOrder_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss"));
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

