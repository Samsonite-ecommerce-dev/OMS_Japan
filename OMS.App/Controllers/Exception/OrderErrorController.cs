using System;
using System.Web;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Encryption;
using Samsonite.OMS.Service;
using Samsonite.Utility.Common;
using OMS.App.Helper;
using Samsonite.OMS.Service.AppConfig;

namespace OMS.App.Controllers
{
    public class OrderErrorController : BaseController
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
            //快速时间选项
            ViewData["quicktime_list"] = QuickTimeHelper.QuickTimeOption();

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult Index_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            string _orderid = VariableHelper.SaferequestStr(Request.Form["orderid"]);
            string _storeid = VariableHelper.SaferequestStr(Request.Form["store"]);
            string _time1 = VariableHelper.SaferequestStr(Request.Form["time1"]);
            string _time2 = VariableHelper.SaferequestStr(Request.Form["time2"]);
            int _status = VariableHelper.SaferequestInt(Request.Form["status"]);
            using (var db = new DynamicRepository())
            {
                //搜索条件
                if (!string.IsNullOrEmpty(_orderid))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "((OrderNo like {0}) or (SubOrderNo like {0}))", Param = "%" + _orderid + "%" });
                }

                if (!string.IsNullOrEmpty(_storeid))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "MallSapCode={0}", Param = _storeid });
                }
                else
                {
                    //默认显示当前账号允许看到的店铺订单
                    var _UserMalls = this.CurrentLoginUser.UserMalls;
                    if (_UserMalls.Count == 0)
                        _UserMalls.Add("0");
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "MallSapCode in (select item from strToIntTable('" + string.Join(",", _UserMalls) + "',','))", Param = null });
                }

                if (!string.IsNullOrEmpty(_time1))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,OrderTime,{0})<=0", Param = VariableHelper.SaferequestTime(_time1) });
                }

                if (!string.IsNullOrEmpty(_time2))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,OrderTime,{0})>=0", Param = VariableHelper.SaferequestTime(_time2) });
                }

                if (_status == 1)
                {
                    //已处理的错误订单
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "IsError=0", Param = null });
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "IsDelete=0", Param = null });
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "ErrorMsg!=''", Param = null });
                }
                else if (_status == 2)
                {
                    //已删除订单
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "IsDelete=1", Param = null });
                }
                else
                {
                    //错误订单
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "IsError=1", Param = null });
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "IsDelete=0", Param = null });
                }

                //查询
                var _list = db.GetPage<dynamic>("select Id,DetailID,OrderNo,SubOrderNo,MallName,OrderTime,PaymentDate,SKU,ProductName,Quantity,SellingPrice,ProductStatus,PaymentAmount,ActualPaymentAmount,ShippingStatus,IsError,ErrorMsg,ErrorRemark from View_OrderDetail order by OrderTime desc", _SqlWhere, VariableHelper.SaferequestInt(Request.Form["rows"]), VariableHelper.SaferequestInt(Request.Form["page"]));
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.DetailID,
                               s1 = string.Format("<a href=\"javascript:void(0);\" onclick=\"artDialogExtend.Dialog.Open('" + Url.Action("Detail", "OrderQuery") + "?ID={0}',{{title:'{1}-{0}',width:'100%',height:'100%'}});\">{0}</a>", dy.OrderNo, _LanguagePack["ordererror_detail_title"]),
                               s2 = dy.SubOrderNo,
                               s3 = dy.MallName,
                               s4 = dy.SKU,
                               s5 = dy.ProductName,
                               s6 = VariableHelper.FormateMoney(dy.SellingPrice),
                               s7 = dy.Quantity,
                               s8 = VariableHelper.FormateMoney(dy.PaymentAmount),
                               s9 = VariableHelper.FormateMoney(OrderHelper.MathRound(dy.ActualPaymentAmount)),
                               s10 = OrderHelper.GetProductStatusDisplay(dy.ProductStatus, true),
                               s11 = VariableHelper.FormateTime(dy.OrderTime, "yyyy-MM-dd HH:mm:ss"),
                               s12 = dy.ErrorMsg,
                               s13 = dy.ErrorRemark
                           }
                };
            }
            return _result;
        }
        #endregion

        #region 编辑
        [UserPowerAuthorize]
        public ActionResult Edit()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;
            ViewBag.LanguagePack = _LanguagePack;

            Int64 _ID = VariableHelper.SaferequestInt64(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                var objOrderDetail = db.OrderDetail.Where(p => p.Id == _ID && !p.IsDelete).SingleOrDefault();
                if (objOrderDetail != null)
                {
                    if (objOrderDetail.IsError)
                    {
                        //店铺ID
                        ViewBag.MallSapCode = db.Order.Where(p => p.Id == objOrderDetail.OrderId).SingleOrDefault().MallSapCode;
                        //收货信息
                        OrderReceive objOrderReceive = db.OrderReceive.Where(p => p.OrderId == objOrderDetail.OrderId && p.SubOrderNo == objOrderDetail.SubOrderNo).SingleOrDefault();
                        //数据解密
                        EncryptionFactory.Create(objOrderReceive).Decrypt();
                        ViewData["order_receive"] = objOrderReceive;

                        return View(objOrderDetail);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.NoMessage });
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.NoMessage });
                }
            }
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Edit_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            Int64 _ID = VariableHelper.SaferequestInt64(Request.Form["ID"]);
            string _MallSapCode = VariableHelper.SaferequestStr(Request.Form["MallSapCode"]);
            string _SKU = VariableHelper.SaferequestStr(Request.Form["SKU"]);
            string _Tel = VariableHelper.SaferequestStr(Request.Form["Tel"]);
            string _Mobile = VariableHelper.SaferequestStr(Request.Form["Mobile"]);
            int _OrderType = VariableHelper.SaferequestInt(Request.Form["Otype"]);
            string _SetCode = VariableHelper.SaferequestStr(Request.Form["ProductSet"]);
            string _Remark = VariableHelper.SaferequestEditor(Request.Form["Remark"]);

            int _AmountAccuracy = ConfigCache.Instance.Get().AmountAccuracy;

            //数据加密
            _Tel = EncryptionBase.EncryptString(_Tel);
            _Mobile = EncryptionBase.EncryptString(_Mobile);

            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_MallSapCode))
                        {
                            throw new Exception(_LanguagePack["ordererror_edit_message_no_mallsapcode"]);
                        }

                        if (string.IsNullOrEmpty(_Tel) && string.IsNullOrEmpty(_Mobile))
                        {
                            throw new Exception(_LanguagePack["ordererror_edit_message_no_contact"]);
                        }

                        string _MallName = db.Mall.Where(p => p.SapCode == _MallSapCode).SingleOrDefault().Name;

                        OrderDetail objData = db.OrderDetail.Where(p => p.Id == _ID && !p.IsDelete).SingleOrDefault();
                        if (objData != null)
                        {
                            if (objData.IsError)
                            {
                                Order objOrder = db.Order.Where(p => p.OrderNo == objData.OrderNo).SingleOrDefault();
                                if (objOrder != null)
                                {
                                    //如果是套装
                                    if (_OrderType == 1)
                                    {
                                        ProductSet objProductSet = db.ProductSet.Where(p => p.SetCode == _SetCode).SingleOrDefault();
                                        if (objProductSet != null)
                                        {
                                            //收货地址信息
                                            OrderReceive objDataReceive = db.OrderReceive.Where(p => p.OrderNo == objData.OrderNo && p.SubOrderNo == objData.SubOrderNo).SingleOrDefault();
                                            OrderDetail objOrderDetail = new OrderDetail();
                                            OrderReceive objOrderReceive = new OrderReceive();
                                            var objProductSetDetail_List = (from psd in db.ProductSetDetail.Where(p => p.ProductSetId == objProductSet.Id)
                                                                            join p in db.Product on psd.SKU equals p.SKU into tmp
                                                                            from p in tmp.DefaultIfEmpty()
                                                                            select new
                                                                            {
                                                                                Price = psd.Price,
                                                                                Quantity = psd.Quantity,
                                                                                SKU = psd.SKU,
                                                                                IsPrimary = psd.IsPrimary,
                                                                                Parent = psd.Parent,
                                                                                RRPPrice = p.MarketPrice,
                                                                                ProductName = p.Description ?? "",
                                                                                ProductImage = p.ImageUrl,
                                                                                ProductId = p.ProductId ?? ""
                                                                            }
                                                                        ).ToList();
                                            decimal _SupplyPrice = 0;
                                            decimal _PaymentAmount = 0;
                                            decimal _r_PaymentAmount = objData.PaymentAmount;
                                            decimal _ActualPayment = 0;
                                            decimal _r_ActualPayment = objData.ActualPaymentAmount;
                                            decimal _SetTotalPrice = objProductSetDetail_List.Sum(p => (decimal)p.Price * (int)p.Quantity);
                                            int _k = 0;
                                            string _SubOrderNo = string.Empty;
                                            string _gift_subOrderNo = string.Empty;
                                            List<OrderDetail> _tmp_OrderDetail_List = new List<OrderDetail>();
                                            foreach (var _dy in objProductSetDetail_List.OrderByDescending(p => p.IsPrimary))
                                            {
                                                //如果套装子产品数量可能大于1
                                                for (int i = 0; i < _dy.Quantity; i++)
                                                {
                                                    _k++;
                                                    //最后一个子订单在分摊实际付款金额时,使用减法计算
                                                    if (_k == objProductSetDetail_List.Sum(p => p.Quantity))
                                                    {
                                                        _PaymentAmount = _r_PaymentAmount;
                                                        _ActualPayment = _r_ActualPayment;
                                                    }
                                                    else
                                                    {
                                                        if (_SetTotalPrice > 0)
                                                        {
                                                            _PaymentAmount = Math.Round((_dy.Price * objData.PaymentAmount / _SetTotalPrice), _AmountAccuracy);
                                                            _r_PaymentAmount -= _PaymentAmount;
                                                            _ActualPayment = Math.Round((_dy.Price * objData.ActualPaymentAmount / _SetTotalPrice), _AmountAccuracy);
                                                            _r_ActualPayment -= _ActualPayment;
                                                        }
                                                        else
                                                        {
                                                            _PaymentAmount = 0;
                                                            _ActualPayment = 0;
                                                        }
                                                    }
                                                    _SubOrderNo = OrderService.CreateSetSubOrderNO(objData.SubOrderNo, _dy.SKU, objProductSetDetail_List.Sum(p => p.Quantity), _k);
                                                    _SupplyPrice = OrderHelper.MathRound((_ActualPayment / (decimal)1.1));
                                                    string _ParentSubOrderNo = string.Empty;
                                                    //如果是次级产品
                                                    if (!_dy.IsPrimary)
                                                    {
                                                        var _tmp_Parent = _tmp_OrderDetail_List.Where(p => p.SKU == _dy.Parent).FirstOrDefault();
                                                        if (_tmp_Parent != null)
                                                        {
                                                            _ParentSubOrderNo = _tmp_Parent.SubOrderNo;
                                                        }
                                                    }
                                                    objOrderDetail = new OrderDetail()
                                                    {
                                                        OrderId = objData.OrderId,
                                                        OrderNo = objData.OrderNo,
                                                        SubOrderNo = _SubOrderNo,
                                                        ParentSubOrderNo = _ParentSubOrderNo,
                                                        CreateDate = objData.CreateDate,
                                                        MallProductId = objData.MallProductId,
                                                        MallSkuId = objData.MallSkuId,
                                                        SetCode = objProductSet.SetCode,
                                                        ProductName = _dy.ProductName,
                                                        ProductId = _dy.ProductId,
                                                        ProductPic = _dy.ProductImage,
                                                        SkuProperties = string.Empty,
                                                        SKU = _dy.SKU,
                                                        SkuGrade = string.Empty,
                                                        Quantity = 1,
                                                        RRPPrice = _dy.RRPPrice,
                                                        SupplyPrice = _SupplyPrice,
                                                        SellingPrice = _dy.Price,
                                                        PaymentAmount = _PaymentAmount,
                                                        ActualPaymentAmount = _ActualPayment,
                                                        Status = objData.Status,
                                                        EBStatus = objData.EBStatus,
                                                        ShippingProvider = objData.ShippingProvider,
                                                        ShippingType = objData.ShippingType,
                                                        ShippingStatus = objData.ShippingStatus,
                                                        DeliveringPlant = objData.DeliveringPlant,
                                                        CancelQuantity = objData.CancelQuantity,
                                                        ReturnQuantity = objData.ReturnQuantity,
                                                        ExchangeQuantity = objData.ExchangeQuantity,
                                                        RejectQuantity = objData.RejectQuantity,
                                                        Tax = objData.Tax,
                                                        TaxRate = objData.TaxRate,
                                                        IsReservation = objData.IsReservation,
                                                        ReservationDate = objData.ReservationDate,
                                                        ReservationRemark = objData.ReservationRemark,
                                                        IsSet = true,
                                                        IsSetOrigin = false,
                                                        IsPre = objData.IsPre,
                                                        IsGift = objData.IsGift,
                                                        IsUrgent = objData.IsUrgent,
                                                        IsExchangeNew = objData.IsExchangeNew,
                                                        IsSystemCancel = objData.IsSystemCancel,
                                                        IsEmployee = objData.IsEmployee,
                                                        ExtraRequest = objData.ExtraRequest,
                                                        AddDate = DateTime.Now,
                                                        EditDate = null,
                                                        CompleteDate = null,
                                                        IsStop = objData.IsStop,
                                                        IsError = false,
                                                        ErrorMsg = string.Empty,
                                                        ErrorRemark = string.Empty,
                                                        IsDelete = false
                                                    };
                                                    objOrderReceive = GenericHelper.TCopyValue<OrderReceive>(objDataReceive);
                                                    objOrderReceive.SubOrderNo = _SubOrderNo;
                                                    db.OrderReceive.Add(objOrderReceive);
                                                    //赠品附加到第一个子套装产品上
                                                    if (_k == 1)
                                                    {
                                                        _gift_subOrderNo = _SubOrderNo;
                                                    }
                                                    _tmp_OrderDetail_List.Add(objOrderDetail);
                                                }
                                            }
                                            db.OrderDetail.AddRange(_tmp_OrderDetail_List);
                                            //如果SellingPrice是0,则需要重置市场价
                                            if (objData.SellingPrice == 0)
                                            {
                                                Product objProduct = db.Product.Where(p => p.SKU == _SetCode && p.IsSet).FirstOrDefault();
                                                if (objProduct != null)
                                                {
                                                    objData.SellingPrice = objProduct.MarketPrice;
                                                }
                                            }
                                            objData.SetCode = objProductSet.SetCode;
                                            objData.SKU = objProductSet.SetCode;
                                            objData.IsSet = true;
                                            objData.IsSetOrigin = true;
                                            objData.IsError = false;
                                            objData.ErrorRemark = _Remark;
                                            objData.IsDelete = false;
                                            objData.EditDate = DateTime.Now;
                                            db.SaveChanges();
                                            //此处需要增加赠品的解析
                                            //获取促销列表
                                            var ProductPromotionList = PromotionService.GetProductPromotions();
                                            TradeDto t = new TradeDto();
                                            t.Order = objOrder;
                                            t.OrderDetail = objData;
                                            foreach (var pp in ProductPromotionList.Where(p => (DateTime.Compare(p.BeginDate, t.Order.CreateDate) <= 0) && (DateTime.Compare(p.EndDate, t.Order.CreateDate) >= 0) && p.Malls.Contains(t.Order.MallSapCode)))
                                            {
                                                t = PromotionService.ParseProductPromotion(new List<TradeDto>() { t }, t, pp, _gift_subOrderNo);
                                            }
                                            //保存赠品
                                            foreach (var item in t.OrderGifts)
                                            {
                                                db.Database.ExecuteSqlCommand($"INSERT INTO [dbo].[OrderGift]([OrderNo],[SubOrderNo],[GiftNo],[Sku],[MallProductId],[ProductName],[Price],[Quantity],[AddDate])  VALUES('{item.OrderNo}','{item.SubOrderNo}','{item.GiftNo}','{item.Sku}','{item.MallProductId}','{item.ProductName}','{item.Price}',{item.Quantity},'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}')");
                                            }
                                            //套装减套装库存,同时需要减少子商品库存
                                            ProductService.UpdateBundleProductInventory(_SetCode, objData.Quantity);
                                            foreach (dynamic _dy in objProductSetDetail_List)
                                            {
                                                ProductService.UpdateCommonProductInventory(_dy.SKU, _dy.Quantity);
                                            }
                                        }
                                        else
                                        {
                                            throw new Exception(_LanguagePack["ordererror_edit_message_no_setcode"]);
                                        }
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        //判断sku是否有效
                                        Product objProduct = db.Product.Where(p => p.SKU == _SKU && !p.IsSet).FirstOrDefault();
                                        if (objProduct != null)
                                        {
                                            //如果SellingPrice是0,则需要重置市场价
                                            if (objData.SellingPrice == 0)
                                            {
                                                objData.SellingPrice = objProduct.MarketPrice;
                                                //重算订单总金额和折扣金额
                                            }
                                            if (string.IsNullOrEmpty(objData.ProductPic))
                                            {
                                                objData.ProductPic = objProduct.ImageUrl;
                                            }
                                            //普通订单
                                            objData.RRPPrice = objProduct.MarketPrice;
                                            objData.SKU = objProduct.SKU;
                                            objData.ProductId = objProduct.ProductId;
                                            objData.IsSet = false;
                                            objData.IsSetOrigin = false;
                                            objData.IsError = false;
                                            objData.ErrorRemark = _Remark;
                                            objData.IsDelete = false;
                                            objData.EditDate = DateTime.Now;
                                            db.SaveChanges();

                                            //此处需要增加赠品的解析
                                            //获取促销列表
                                            var ProductPromotionList = PromotionService.GetProductPromotions();
                                            TradeDto t = new TradeDto();
                                            t.Order = objOrder;
                                            t.OrderDetail = objData;
                                            foreach (var pp in ProductPromotionList.Where(p => (DateTime.Compare(p.BeginDate, t.Order.CreateDate) <= 0) && (DateTime.Compare(p.EndDate, t.Order.CreateDate) >= 0) && p.Malls.Contains(t.Order.MallSapCode)))
                                            {
                                                t = PromotionService.ParseProductPromotion(new List<TradeDto>() { t }, t, pp);
                                            }
                                            //保存赠品
                                            foreach (var item in t.OrderGifts)
                                            {
                                                db.Database.ExecuteSqlCommand($"INSERT INTO [dbo].[OrderGift]([OrderNo],[SubOrderNo],[GiftNo],[Sku],[MallProductId],[ProductName],[Price],[Quantity],[AddDate])  VALUES('{item.OrderNo}','{item.SubOrderNo}','{item.GiftNo}','{item.Sku}','{item.MallProductId}','{item.ProductName}','{item.Price}',{item.Quantity},'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}')");
                                            }
                                            //减少库存
                                            ProductService.UpdateCommonProductInventory(objProduct.SKU, objData.Quantity);
                                        }
                                        else
                                        {
                                            throw new Exception(_LanguagePack["ordererror_edit_message_no_sku"]);
                                        }
                                    }
                                    //更新主订单
                                    objOrder.MallSapCode = _MallSapCode;
                                    objOrder.MallName = _MallName;
                                    db.SaveChanges();
                                    //更新收货信息
                                    db.Database.ExecuteSqlCommand("update OrderReceive set ReceiveTel={0},ReceiveCel={1} where OrderNo={2} and SubOrderNo={3}", _Tel, _Mobile, objData.OrderNo, objData.SubOrderNo);
                                    Trans.Commit();
                                    ////如果是Lazada和Zalora,Shopee的订单需要重算总金额
                                    //if (objOrder.PlatformType == (int)PlatformType.LAZADA_Singapore || objOrder.PlatformType == (int)PlatformType.ZALORA_Singapore || objOrder.PlatformType == (int)PlatformType.SHOPEE_Singapore)
                                    //{
                                    //    db.Database.ExecuteSqlCommand("update [Order] set OrderAmount=(select sum(SellingPrice*Quantity) from OrderDetail where OrderDetail.OrderNo=[Order].OrderNo and not(IsSetOrigin=0 and IsSet=1)) where ID={0}", objOrder.Id);
                                    //    db.Database.ExecuteSqlCommand("update [Order] set DiscountAmount=OrderAmount-PaymentAmount+DeliveryFee where ID={0}", objOrder.Id);
                                    //}
                                    //添加日志
                                    AppLogService.UpdateLog<OrderDetail>(objData, objData.Id.ToString());
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
                            else
                            {
                                throw new Exception(_LanguagePack["common_data_load_false"]);
                            }
                        }
                        else
                        {
                            throw new Exception(_LanguagePack["common_data_load_false"]);
                        }
                    }
                    catch (Exception ex)
                    {
                        Trans.Rollback();
                        _result.Data = new
                        {
                            result = false,
                            msg = ex.Message
                        };
                    }
                }
                return _result;
            }
        }
        #endregion

        //#region 删除
        //[UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        //public JsonResult Delete_Message()
        //{
        //    //加载语言包
        //    var _LanguagePack = this.GetLanguagePack;

        //    JsonResult _result = new JsonResult();
        //    string _IDs = VariableHelper.SaferequestStr(Request.Form["ID"]);
        //    using (var db = new ebEntities())
        //    {
        //        try
        //        {
        //            if (string.IsNullOrEmpty(_IDs))
        //            {
        //                throw new Exception(_LanguagePack["common_data_need_one"]);
        //            }

        //            OrderDetail objOrderDetail = new OrderDetail();
        //            foreach (string _str in _IDs.Split(','))
        //            {
        //                Int64 _ID = VariableHelper.SaferequestInt64(_str);
        //                objOrderDetail = db.OrderDetail.Where(p => p.Id == _ID).SingleOrDefault();
        //                if (objOrderDetail != null)
        //                {
        //                    //如果是错误订单
        //                    if (objOrderDetail.IsError)
        //                    {
        //                        objOrderDetail.IsDelete = true;
        //                        objOrderDetail.EditDate = DateTime.Now;
        //                    }
        //                    else
        //                    {
        //                        throw new Exception(string.Format("{0}:{1}", _str, _LanguagePack["common_data_no_exsit"]));
        //                    }
        //                }
        //                else
        //                {
        //                    throw new Exception(string.Format("{0}:{1}", _str, _LanguagePack["common_data_no_exsit"]));
        //                }
        //            }
        //            db.SaveChanges();
        //            //返回信息
        //            _result.Data = new
        //            {
        //                result = true,
        //                msg = _LanguagePack["common_data_delete_success"]
        //            };
        //        }
        //        catch (Exception ex)
        //        {
        //            _result.Data = new
        //            {
        //                result = false,
        //                msg = ex.Message
        //            };
        //        }
        //        return _result;
        //    }
        //}
        //#endregion

        //#region 恢复
        //[UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json,IsAntiForgeryToken =true)]
        //public JsonResult Restore_Message()
        //{
        //    //加载语言包
        //    var _LanguagePack = this.GetLanguagePack;

        //    JsonResult _result = new JsonResult();
        //    string _IDs = VariableHelper.SaferequestStr(Request.Form["ID"]);
        //    using (var db = new ebEntities())
        //    {
        //        try
        //        {
        //            if (string.IsNullOrEmpty(_IDs))
        //            {
        //                throw new Exception(_LanguagePack["common_data_need_one"]);
        //            }

        //            OrderDetail objOrderDetail = new OrderDetail();
        //            foreach (string _str in _IDs.Split(','))
        //            {
        //                Int64 _ID = VariableHelper.SaferequestInt64(_str);
        //                objOrderDetail = db.OrderDetail.Where(p => p.Id == _ID).SingleOrDefault();
        //                if (objOrderDetail != null)
        //                {
        //                    //如果是错误订单
        //                    if (objOrderDetail.IsError)
        //                    {
        //                        objOrderDetail.IsDelete = false;
        //                        objOrderDetail.EditDate = DateTime.Now;
        //                    }
        //                    else
        //                    {
        //                        throw new Exception(string.Format("{0}:{1}", _str, _LanguagePack["common_data_no_exsit"]));
        //                    }
        //                }
        //                else
        //                {
        //                    throw new Exception(string.Format("{0}:{1}", _str, _LanguagePack["common_data_no_exsit"]));
        //                }
        //            }
        //            db.SaveChanges();
        //            //返回信息
        //            _result.Data = new
        //            {
        //                result = true,
        //                msg = _LanguagePack["common_data_recover_success"]
        //            };
        //        }
        //        catch (Exception ex)
        //        {
        //            _result.Data = new
        //            {
        //                result = false,
        //                msg = ex.Message
        //            };
        //        }
        //        return _result;
        //    }
        //}
        //#endregion

        #region 生成文档
        [UserPowerAuthorize]
        public FileResult ExportExcel()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            string _orderid = VariableHelper.SaferequestStr(Request.Form["OrderNumber"]);
            string _storeid = VariableHelper.SaferequestStr(Request.Form["StoreName"]);
            string _time1 = VariableHelper.SaferequestStr(Request.Form["Time1"]);
            string _time2 = VariableHelper.SaferequestStr(Request.Form["Time2"]);
            int _valid = VariableHelper.SaferequestInt(Request.Form["Valid"]);

            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            using (var db = new DynamicRepository())
            {
                //搜索条件
                if (!string.IsNullOrEmpty(_orderid))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "((OrderNo like {0}) or (SubOrderNo like {0}))", Param = "%" + _orderid + "%" });
                }

                if (!string.IsNullOrEmpty(_storeid))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "MallSapCode={0}", Param = _storeid });
                }
                else
                {
                    //默认显示当前账号允许看到的店铺订单
                    var _UserMalls = this.CurrentLoginUser.UserMalls;
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "MallSapCode in (select item from strToIntTable('" + string.Join(",", _UserMalls) + "',','))", Param = null });
                }

                if (!string.IsNullOrEmpty(_time1))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,OrderTime,{0})<=0", Param = VariableHelper.SaferequestTime(_time1) });
                }

                if (!string.IsNullOrEmpty(_time2))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,OrderTime,{0})>=0", Param = VariableHelper.SaferequestTime(_time2) });
                }

                if (_valid == 1)
                {
                    //已处理的错误订单
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "IsError=0", Param = null });
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "IsDelete=0", Param = null });
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "ErrorMsg!=''", Param = null });
                }
                else if (_valid == 2)
                {
                    //已删除订单
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "IsDelete=1", Param = null });
                }
                else
                {
                    //错误订单
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "IsError=1", Param = null });
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "IsDelete=0", Param = null });
                }

                DataTable dt = new DataTable();
                dt.Columns.Add(_LanguagePack["ordererror_index_order_no"]);
                dt.Columns.Add(_LanguagePack["ordererror_index_sub_order_no"]);
                dt.Columns.Add(_LanguagePack["ordererror_index_store"]);
                dt.Columns.Add(_LanguagePack["ordererror_index_sku"]);
                dt.Columns.Add(_LanguagePack["ordererror_index_productname"]);
                dt.Columns.Add(_LanguagePack["ordererror_index_price"]);
                dt.Columns.Add(_LanguagePack["ordererror_index_quantity"]);
                dt.Columns.Add(_LanguagePack["ordererror_index_payment"]);
                dt.Columns.Add(_LanguagePack["ordererror_index_actual_pay"]);
                dt.Columns.Add(_LanguagePack["ordererror_index_product_status"]);
                dt.Columns.Add(_LanguagePack["ordererror_index_ordertime"]);
                dt.Columns.Add(_LanguagePack["ordererror_index_error"]);

                //读取数据
                DataRow _dr = null;
                //默认显示当前账号允许看到的店铺订单
                List<dynamic> _List = db.Fetch<dynamic>("select OrderNo,SubOrderNo,MallName,OrderTime,PaymentDate,SKU,ProductName,Quantity,SellingPrice,ProductStatus,PaymentAmount,ActualPaymentAmount,ShippingStatus,IsError,ErrorMsg from View_OrderDetail order by OrderTime desc", _SqlWhere);
                foreach (dynamic _dy in _List)
                {
                    _dr = dt.NewRow();
                    _dr[0] = _dy.OrderNo;
                    _dr[1] = _dy.SubOrderNo;
                    _dr[2] = _dy.MallName;
                    _dr[3] = _dy.SKU;
                    _dr[4] = _dy.ProductName;
                    _dr[5] = VariableHelper.FormateMoney(_dy.SellingPrice);
                    _dr[6] = _dy.Quantity;
                    _dr[7] = VariableHelper.FormateMoney(_dy.PaymentAmount);
                    _dr[8] = VariableHelper.FormateMoney(OrderHelper.MathRound(_dy.ActualPaymentAmount));
                    _dr[9] = OrderHelper.GetProductStatusDisplay(_dy.ProductStatus, false);
                    _dr[10] = VariableHelper.FormateTime(_dy.OrderTime, "yyyy-MM-dd HH:mm:ss");
                    _dr[11] = _dy.ErrorMsg;
                    dt.Rows.Add(_dr);
                }

                string _filepath = $"~{AppGlobalService.UPLOAD_CACHE_PATH}/{DateTime.Now.ToString("yyyy-MM-dd")}";
                if (!Directory.Exists(Server.MapPath(_filepath)))
                {
                    Directory.CreateDirectory(Server.MapPath(_filepath));
                }
                string _fileName = string.Format("ExceptionOrder_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss"));
                _fileName = HttpUtility.UrlEncode(_fileName, Encoding.GetEncoding("UTF-8"));
                string _path = Server.MapPath(string.Format("{0}/{1}", _filepath, _fileName));
                ExcelHelper objExcelHelper = new ExcelHelper(_path);
                objExcelHelper.DataTableToExcel(dt, "Sheet1", true);
                return File(_path, "application/ms-excel", _fileName);
            }
        }
        #endregion
    }
}
