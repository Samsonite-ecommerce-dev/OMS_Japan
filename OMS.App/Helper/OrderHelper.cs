using System;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service.AppConfig;
using Samsonite.OMS.Service.AppLanguage;
using Samsonite.Utility.Common;

namespace OMS.App.Helper
{
    public class OrderHelper
    {
        #region 支付方式
        /// <summary>
        /// 支付方式集合
        /// </summary>
        /// <returns></returns>
        private static List<DefineEnum> PaymentTypeReflect()
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            List<DefineEnum> _result = new List<DefineEnum>();
            _result.Add(new DefineEnum() { ID = (int)PayType.CreditCard, Display = _LanguagePack["orderquery_index_search_payment_type_1"] });
            _result.Add(new DefineEnum() { ID = (int)PayType.WireTransfer, Display = _LanguagePack["orderquery_index_search_payment_type_2"] });
            _result.Add(new DefineEnum() { ID = (int)PayType.MobilePayment, Display = _LanguagePack["orderquery_index_search_payment_type_3"] });
            _result.Add(new DefineEnum() { ID = (int)PayType.SimplePay, Display = _LanguagePack["orderquery_index_search_payment_type_4"] });
            _result.Add(new DefineEnum() { ID = (int)PayType.VirtualPay, Display = _LanguagePack["orderquery_index_search_payment_type_5"] });
            _result.Add(new DefineEnum() { ID = (int)PayType.AliPay, Display = _LanguagePack["orderquery_index_search_payment_type_6"] });
            _result.Add(new DefineEnum() { ID = (int)PayType.WechatPay, Display = _LanguagePack["orderquery_index_search_payment_type_7"] });
            _result.Add(new DefineEnum() { ID = (int)PayType.LinePay, Display = _LanguagePack["orderquery_index_search_payment_type_8"] });
            _result.Add(new DefineEnum() { ID = (int)PayType.CashOnDelivery, Display = _LanguagePack["orderquery_index_search_payment_type_9"] });
            _result.Add(new DefineEnum() { ID = (int)PayType.PayPal, Display = _LanguagePack["orderquery_index_search_payment_type_10"] });
            _result.Add(new DefineEnum() { ID = (int)PayType.OTCPayment, Display = _LanguagePack["orderquery_index_search_payment_type_11"] });
            _result.Add(new DefineEnum() { ID = (int)PayType.BankPayment, Display = _LanguagePack["orderquery_index_search_payment_type_12"] });
            _result.Add(new DefineEnum() { ID = (int)PayType.Installment, Display = _LanguagePack["orderquery_index_search_payment_type_13"] });
            _result.Add(new DefineEnum() { ID = (int)PayType.MasterPass, Display = _LanguagePack["orderquery_index_search_payment_type_14"] });
            _result.Add(new DefineEnum() { ID = (int)PayType.HelloPay, Display = _LanguagePack["orderquery_index_search_payment_type_15"] });
            _result.Add(new DefineEnum() { ID = (int)PayType.Cybersource, Display = _LanguagePack["orderquery_index_search_payment_type_16"] });
            _result.Add(new DefineEnum() { ID = (int)PayType.Cash, Display = _LanguagePack["orderquery_index_search_payment_type_17"] });
            _result.Add(new DefineEnum() { ID = (int)PayType.Cheque, Display = _LanguagePack["orderquery_index_search_payment_type_18"] });
            _result.Add(new DefineEnum() { ID = (int)PayType.DebitCard, Display = _LanguagePack["orderquery_index_search_payment_type_19"] });
            _result.Add(new DefineEnum() { ID = (int)PayType.Redemption, Display = _LanguagePack["orderquery_index_search_payment_type_20"] });
            _result.Add(new DefineEnum() { ID = (int)PayType.Nets, Display = _LanguagePack["orderquery_index_search_payment_type_21"] });
            _result.Add(new DefineEnum() { ID = (int)PayType.Mixed, Display = _LanguagePack["orderquery_index_search_payment_type_22"] });
            _result.Add(new DefineEnum() { ID = (int)PayType.Atome, Display = _LanguagePack["orderquery_index_search_payment_type_23"] });
            _result.Add(new DefineEnum() { ID = (int)PayType.OtherPay, Display = _LanguagePack["orderquery_index_search_payment_type_0"] });
            return _result;
        }

        /// <summary>
        /// 支付方式列表
        /// </summary>
        /// <returns></returns>
        public static List<object[]> PaymentTypeObject()
        {
            List<object[]> _result = new List<object[]>();
            foreach (var _o in PaymentTypeReflect())
            {
                _result.Add(new object[] { _o.ID, _o.Display });
            }
            return _result;
        }

        /// <summary>
        /// 支付方式列表(读取站点配置的支付方式)
        /// </summary>
        /// <param name="objCurrentValue"></param>
        /// <returns></returns>
        public static List<object[]> PaymentTypeObject(List<int> objCurrentValue)
        {
            List<object[]> _result = new List<object[]>();
            foreach (var _o in PaymentTypeReflect())
            {
                if (objCurrentValue.Contains(_o.ID))
                {
                    _result.Add(new object[] { _o.ID, _o.Display });
                }
            }
            return _result;
        }

        /// <summary>
        /// 支付方式显示值
        /// </summary>
        /// <param name="objStatus"></param>
        /// <returns></returns>
        public static string GetPaymentTypeDisplay(int objStatus)
        {
            string _result = string.Empty;
            DefineEnum _O = PaymentTypeReflect().Where(p => p.ID == objStatus).SingleOrDefault();
            if (_O != null)
            {
                _result = _O.Display;
            }
            return _result;
        }

        /// <summary>
        /// 混合支付金额显示
        /// </summary>
        /// <param name="objOrderPaymentDetails"></param>
        /// <returns></returns>
        public static string MixedPaymentMessage(List<OrderPaymentDetail> objOrderPaymentDetails)
        {
            string _result = string.Empty;
            if (objOrderPaymentDetails.Count > 0)
            {
                _result += "<div class=\"text\"><ul>";
                foreach (var item in objOrderPaymentDetails)
                {
                    _result += $"<li><i class=\"fa fa-caret-right\"></i>{GetPaymentTypeDisplay(item.PaymentType)}:{VariableHelper.FormateMoney(item.PaymentAmount)}</li>";
                }
                _result += " </ul></div>";
            }
            return _result;
        }
        #endregion

        #region 订单类型
        /// <summary>
        /// 订单类型集合
        /// </summary>
        /// <returns></returns>
        private static List<DefineEnum> OrderTypeReflect()
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            List<DefineEnum> _result = new List<DefineEnum>();
            _result.Add(new DefineEnum() { ID = (int)OrderType.OnLine, Display = _LanguagePack["orderquery_index_search_order_type_1"], Css = "color_success" });
            _result.Add(new DefineEnum() { ID = (int)OrderType.MallSale, Display = _LanguagePack["orderquery_index_search_order_type_2"], Css = "color_info" });
            _result.Add(new DefineEnum() { ID = (int)OrderType.ClickCollect, Display = _LanguagePack["orderquery_index_search_order_type_3"], Css = "color_primary" });
            return _result;
        }

        /// <summary>
        /// 订单类型列表
        /// </summary>
        /// <returns></returns>
        public static List<object[]> OrderTypeObject()
        {
            List<object[]> _result = new List<object[]>();
            foreach (var _o in OrderTypeReflect())
            {
                _result.Add(new object[] { _o.ID, _o.Display });
            }
            return _result;
        }

        /// <summary>
        /// 订单类型显示值
        /// </summary>
        /// <param name="objStatus"></param>
        /// <param name="objCss"></param>
        /// <returns></returns>
        public static string GetOrderTypeDisplay(int objStatus, bool objCss = false)
        {
            string _result = string.Empty;
            DefineEnum _O = OrderTypeReflect().Where(p => p.ID == objStatus).SingleOrDefault();
            if (_O != null)
            {
                if (objCss)
                {
                    _result = string.Format("<label class=\"{0}\">{1}</label>", _O.Css, _O.Display);
                }
                else
                {
                    _result = _O.Display;
                }
            }
            return _result;
        }
        #endregion

        #region 订单来源
        /// <summary>
        /// 订单来源集合
        /// </summary>
        /// <returns></returns>
        private static List<DefineEnum> OrderSourceReflect()
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            List<DefineEnum> _result = new List<DefineEnum>();
            _result.Add(new DefineEnum() { ID = (int)OrderSource.PC, Display = _LanguagePack["orderquery_index_search_source_1"] });
            _result.Add(new DefineEnum() { ID = (int)OrderSource.Mobile, Display = _LanguagePack["orderquery_index_search_source_2"] });
            return _result;
        }

        /// <summary>
        /// 订单来源列表
        /// </summary>
        /// <returns></returns>
        public static List<object[]> OrderSourceObject()
        {
            List<object[]> _result = new List<object[]>();
            foreach (var _o in OrderSourceReflect())
            {
                _result.Add(new object[] { _o.ID, _o.Display });
            }
            return _result;
        }

        /// <summary>
        /// 订单来源显示值
        /// </summary>
        /// <param name="objStatus"></param>
        /// <returns></returns>
        public static string GetOrderSourceDisplay(int objStatus)
        {
            string _result = string.Empty;
            DefineEnum _O = OrderSourceReflect().Where(p => p.ID == objStatus).SingleOrDefault();
            if (_O != null)
            {
                _result = _O.Display;
            }
            return _result;
        }
        #endregion

        #region 订单状态
        /// <summary>
        /// 订单状态集合
        /// </summary>
        /// <returns></returns>
        private static List<DefineEnum> OrderStatusReflect()
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            List<DefineEnum> _result = new List<DefineEnum>();
            _result.Add(new DefineEnum() { ID = (int)OrderStatus.New, Display = _LanguagePack["common_orderstatus_new"], Css = "color_default" });
            _result.Add(new DefineEnum() { ID = (int)OrderStatus.Processing, Display = _LanguagePack["common_orderstatus_processing"], Css = "color_primary" });
            _result.Add(new DefineEnum() { ID = (int)OrderStatus.Complete, Display = _LanguagePack["common_orderstatus_complete"], Css = "color_success" });
            return _result;
        }

        /// <summary>
        /// 订单状态列表
        /// </summary>
        /// <returns></returns>
        public static List<object[]> OrderStatusObject()
        {
            List<object[]> _result = new List<object[]>();
            foreach (var _o in OrderStatusReflect())
            {
                _result.Add(new object[] { _o.ID, _o.Display });
            }
            return _result;
        }

        /// <summary>
        /// 订单状态显示值
        /// </summary>
        /// <param name="objStatus"></param>
        /// <param name="objCss"></param>
        /// <returns></returns>
        public static string GetOrderStatusDisplay(int objStatus, bool objCss = false)
        {
            string _result = string.Empty;
            DefineEnum _O = OrderStatusReflect().Where(p => p.ID == objStatus).SingleOrDefault();
            if (_O != null)
            {
                if (objCss)
                {
                    _result = string.Format("<label class=\"{0}\">{1}</label>", _O.Css, _O.Display);
                }
                else
                {
                    _result = _O.Display;
                }
            }
            return _result;
        }
        #endregion

        #region 商品状态
        /// <summary>
        /// 订单状态集合
        /// </summary>
        /// <returns></returns>
        private static List<DefineEnum> ProductStatusReflect()
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            List<DefineEnum> _result = new List<DefineEnum>();
            _result.Add(new DefineEnum() { ID = (int)ProductStatus.Pending, Display = _LanguagePack["common_productstatus_nopay"], Css = "color_warning" });
            _result.Add(new DefineEnum() { ID = (int)ProductStatus.Received, Display = _LanguagePack["common_productstatus_new"], Css = "color_default" });
            _result.Add(new DefineEnum() { ID = (int)ProductStatus.Processing, Display = _LanguagePack["common_productstatus_processing"], Css = "color_info" });
            _result.Add(new DefineEnum() { ID = (int)ProductStatus.InDelivery, Display = _LanguagePack["common_productstatus_in_delivery"], Css = "color_info" });
            //_result.Add(new DefineEnum() { ID = (int)ProductStatus.Acknowledge, Display = _LanguagePack["common_productstatus_acknowledge"], Css = "color_primary" });
            //_result.Add(new DefineEnum() { ID = (int)ProductStatus.InTransit, Display = _LanguagePack["common_productstatus_in_transit"], Css = "color_info" });
            _result.Add(new DefineEnum() { ID = (int)ProductStatus.ReceivedGoods, Display = _LanguagePack["common_productstatus_received_goods"], Css = "color_info" });
            //_result.Add(new DefineEnum() { ID = (int)ProductStatus.PickupGoods, Display = _LanguagePack["common_productstatus_pickup_goods"], Css = "color_success" });
            _result.Add(new DefineEnum() { ID = (int)ProductStatus.Delivered, Display = _LanguagePack["common_productstatus_delivered"], Css = "color_success" });
            _result.Add(new DefineEnum() { ID = (int)ProductStatus.Cancel, Display = _LanguagePack["common_productstatus_cancel"], Css = "color_danger" });
            _result.Add(new DefineEnum() { ID = (int)ProductStatus.CancelComplete, Display = _LanguagePack["common_productstatus_cancel_complete"], Css = "color_danger" });
            _result.Add(new DefineEnum() { ID = (int)ProductStatus.Return, Display = _LanguagePack["common_productstatus_return"], Css = "color_primary" });
            _result.Add(new DefineEnum() { ID = (int)ProductStatus.ReturnComplete, Display = _LanguagePack["common_productstatus_return_complete"], Css = "color_primary" });
            _result.Add(new DefineEnum() { ID = (int)ProductStatus.Exchange, Display = _LanguagePack["common_productstatus_exchange"], Css = "color_info" });
            _result.Add(new DefineEnum() { ID = (int)ProductStatus.ExchangeComplete, Display = _LanguagePack["common_productstatus_exchange_complete"], Css = "color_info" });
            _result.Add(new DefineEnum() { ID = (int)ProductStatus.ExchangeNew, Display = _LanguagePack["common_productstatus_exchange_new"], Css = "color_warning" });
            _result.Add(new DefineEnum() { ID = (int)ProductStatus.Modify, Display = _LanguagePack["common_productstatus_modify"], Css = "color_success" });
            _result.Add(new DefineEnum() { ID = (int)ProductStatus.Reject, Display = _LanguagePack["common_productstatus_reject"], Css = "color_danger" });
            _result.Add(new DefineEnum() { ID = (int)ProductStatus.RejectComplete, Display = _LanguagePack["common_productstatus_reject_complete"], Css = "color_danger" });
            //_result.Add(new DefineEnum() { ID = (int)ProductStatus.Close, Display = _LanguagePack["common_productstatus_close"], Css = "color_danger" });
            _result.Add(new DefineEnum() { ID = (int)ProductStatus.Complete, Display = _LanguagePack["common_productstatus_complete"], Css = "color_success" });
            return _result;
        }

        /// <summary>
        /// 商品状态列表
        /// </summary>
        /// <returns></returns>
        public static List<object[]> ProductStatusObject()
        {
            List<object[]> _result = new List<object[]>();
            List<int> _PL = new List<int>() { (int)ProductStatus.Pending, (int)ProductStatus.Close, (int)ProductStatus.Received, (int)ProductStatus.InDelivery, (int)ProductStatus.Delivered, (int)ProductStatus.Cancel, (int)ProductStatus.Return, (int)ProductStatus.Exchange, (int)ProductStatus.Modify, (int)ProductStatus.Reject };
            foreach (var _o in ProductStatusReflect().Where(p => _PL.Contains(p.ID)).OrderBy(p => p.ID))
            {
                _result.Add(new object[] { _o.ID, _o.Display });
            }
            return _result;
        }

        /// <summary>
        /// 商品状态显示值
        /// </summary>
        /// <param name="objStatus"></param>
        /// <param name="objCss"></param>
        /// <returns></returns>
        public static string GetProductStatusDisplay(int objStatus, bool objCss = false)
        {
            string _result = string.Empty;
            DefineEnum _O = ProductStatusReflect().Where(p => p.ID == objStatus).SingleOrDefault();
            if (_O != null)
            {
                if (objCss)
                {
                    _result = string.Format("<label class=\"{0}\">{1}</label>", _O.Css, _O.Display);
                }
                else
                {
                    _result = _O.Display;
                }
            }
            return _result;
        }
        #endregion

        #region 流程状态
        /// <summary>
        /// 订单状态集合
        /// </summary>
        /// <returns></returns>
        private static List<DefineEnum> ProcessStatusReflect()
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            List<DefineEnum> _result = new List<DefineEnum>();
            _result.Add(new DefineEnum() { ID = (int)ProcessStatus.Delete, Display = _LanguagePack["common_productprocesstatus_delete"], Css = "color_fail" });
            _result.Add(new DefineEnum() { ID = (int)ProcessStatus.Complete, Display = _LanguagePack["common_productprocesstatus_complete"], Css = "color_success" });
            _result.Add(new DefineEnum() { ID = (int)ProcessStatus.Cancel, Display = _LanguagePack["common_productprocesstatus_cancel"], Css = "color_info" });
            _result.Add(new DefineEnum() { ID = (int)ProcessStatus.CancelWHSure, Display = _LanguagePack["common_productprocesstatus_cancel_wh"], Css = "color_primary" });
            _result.Add(new DefineEnum() { ID = (int)ProcessStatus.WaitRefund, Display = _LanguagePack["common_productprocesstatus_cancel_refund"], Css = "color_primary" });
            _result.Add(new DefineEnum() { ID = (int)ProcessStatus.CancelComplete, Display = _LanguagePack["common_productprocesstatus_cancel_complete"], Css = "color_success" });
            _result.Add(new DefineEnum() { ID = (int)ProcessStatus.CancelFail, Display = _LanguagePack["common_productprocesstatus_cancel_fail"], Css = "color_danger" });
            _result.Add(new DefineEnum() { ID = (int)ProcessStatus.Return, Display = _LanguagePack["common_productprocesstatus_return"], Css = "color_info" });
            _result.Add(new DefineEnum() { ID = (int)ProcessStatus.ReturnWHSure, Display = _LanguagePack["common_productprocesstatus_return_wh"], Css = "color_primary" });
            _result.Add(new DefineEnum() { ID = (int)ProcessStatus.ReturnAcceptComfirm, Display = _LanguagePack["common_productprocesstatus_return_receive"], Css = "color_warning" });
            _result.Add(new DefineEnum() { ID = (int)ProcessStatus.ReturnComplete, Display = _LanguagePack["common_productprocesstatus_return_complete"], Css = "color_success" });
            _result.Add(new DefineEnum() { ID = (int)ProcessStatus.ReturnFail, Display = _LanguagePack["common_productprocesstatus_return_fail"], Css = "color_danger" });
            _result.Add(new DefineEnum() { ID = (int)ProcessStatus.Exchange, Display = _LanguagePack["common_productprocesstatus_exchange"], Css = "color_info" });
            _result.Add(new DefineEnum() { ID = (int)ProcessStatus.ExchangeWHSure, Display = _LanguagePack["common_productprocesstatus_exchange_wh"], Css = "color_primary" });
            _result.Add(new DefineEnum() { ID = (int)ProcessStatus.ExchangeAcceptComfirm, Display = _LanguagePack["common_productprocesstatus_exchange_receive"], Css = "color_warning" });
            _result.Add(new DefineEnum() { ID = (int)ProcessStatus.ExchangeComplete, Display = _LanguagePack["common_productprocesstatus_exchange_complete"], Css = "color_success" });
            _result.Add(new DefineEnum() { ID = (int)ProcessStatus.ExchangeFail, Display = _LanguagePack["common_productprocesstatus_exchange_fail"], Css = "color_danger" });
            _result.Add(new DefineEnum() { ID = (int)ProcessStatus.Modify, Display = _LanguagePack["common_productprocesstatus_modify"], Css = "color_info" });
            _result.Add(new DefineEnum() { ID = (int)ProcessStatus.ModifyWHSure, Display = _LanguagePack["common_productprocesstatus_modify_wh"], Css = "color_primary" });
            _result.Add(new DefineEnum() { ID = (int)ProcessStatus.ModifyComplete, Display = _LanguagePack["common_productprocesstatus_modify_complete"], Css = "color_success" });
            _result.Add(new DefineEnum() { ID = (int)ProcessStatus.ModifyFail, Display = _LanguagePack["common_productprocesstatus_modify_fail"], Css = "color_danger" });
            _result.Add(new DefineEnum() { ID = (int)ProcessStatus.Reject, Display = _LanguagePack["common_productprocesstatus_reject"], Css = "color_danger" });
            _result.Add(new DefineEnum() { ID = (int)ProcessStatus.RejectComplete, Display = _LanguagePack["common_productprocesstatus_reject_complete"], Css = "color_success" });
            return _result;
        }

        /// <summary>
        /// 修改流程状态列表
        /// </summary>
        /// <returns></returns>
        public static List<object[]> ProccessModifyStatusObject()
        {
            List<object[]> _result = new List<object[]>();
            //List<int> _PL = new List<int>() { (int)ProcessStatus.Modify, (int)ProcessStatus.ModifyWHSure, (int)ProcessStatus.ModifyComplete, (int)ProcessStatus.ModifyFail, (int)ProcessStatus.Delete };
            List<int> _PL = new List<int>() { (int)ProcessStatus.Modify, (int)ProcessStatus.ModifyComplete, (int)ProcessStatus.ModifyFail, (int)ProcessStatus.Delete };
            foreach (var _o in ProcessStatusReflect().Where(p => _PL.Contains(p.ID)).OrderBy(p => p.ID))
            {
                _result.Add(new object[] { _o.ID, _o.Display });
            }
            return _result;
        }

        /// <summary>
        /// 取消流程状态列表
        /// </summary>
        /// <returns></returns>
        public static List<object[]> ProccessCancelStatusObject()
        {
            List<object[]> _result = new List<object[]>();
            //List<int> _PL = new List<int>() { (int)ProcessStatus.Cancel, (int)ProcessStatus.CancelWHSure, (int)ProcessStatus.WaitRefund, (int)ProcessStatus.CancelComplete, (int)ProcessStatus.CancelFail, (int)ProcessStatus.Delete };
            List<int> _PL = new List<int>() { (int)ProcessStatus.Cancel, (int)ProcessStatus.WaitRefund, (int)ProcessStatus.CancelComplete, (int)ProcessStatus.CancelFail, (int)ProcessStatus.Delete };
            foreach (var _o in ProcessStatusReflect().Where(p => _PL.Contains(p.ID)).OrderBy(p => p.ID))
            {
                _result.Add(new object[] { _o.ID, _o.Display });
            }
            return _result;
        }

        /// <summary>
        /// 退货流程状态列表
        /// </summary>
        /// <returns></returns>
        public static List<object[]> ProccessReturnStatusObject()
        {
            List<object[]> _result = new List<object[]>();
            //List<int> _PL = new List<int>() { (int)ProcessStatus.Return, (int)ProcessStatus.ReturnWHSure, (int)ProcessStatus.ReturnAcceptComfirm, (int)ProcessStatus.ReturnComplete, (int)ProcessStatus.ReturnFail, (int)ProcessStatus.Delete };
            List<int> _PL = new List<int>() { (int)ProcessStatus.Return, (int)ProcessStatus.ReturnAcceptComfirm, (int)ProcessStatus.ReturnComplete, (int)ProcessStatus.ReturnFail, (int)ProcessStatus.Delete };
            foreach (var _o in ProcessStatusReflect().Where(p => _PL.Contains(p.ID)).OrderBy(p => p.ID))
            {
                _result.Add(new object[] { _o.ID, _o.Display });
            }
            return _result;
        }

        /// <summary>
        /// 换货流程状态列表
        /// </summary>
        /// <returns></returns>
        public static List<object[]> ProccessExchangeStatusObject()
        {
            List<object[]> _result = new List<object[]>();
            //List<int> _PL = new List<int>() { (int)ProcessStatus.Exchange, (int)ProcessStatus.ExchangeWHSure, (int)ProcessStatus.ExchangeAcceptComfirm, (int)ProcessStatus.ExchangeComplete, (int)ProcessStatus.ExchangeFail, (int)ProcessStatus.Delete };
            List<int> _PL = new List<int>() { (int)ProcessStatus.Exchange, (int)ProcessStatus.ExchangeAcceptComfirm, (int)ProcessStatus.ExchangeComplete, (int)ProcessStatus.ExchangeFail, (int)ProcessStatus.Delete };
            foreach (var _o in ProcessStatusReflect().Where(p => _PL.Contains(p.ID)).OrderBy(p => p.ID))
            {
                _result.Add(new object[] { _o.ID, _o.Display
    });
            }
            return _result;
        }

        /// <summary>
        /// 拒收流程状态列表
        /// </summary>
        /// <returns></returns>
        public static List<object[]> ProccessRejectStatusObject()
        {
            List<object[]> _result = new List<object[]>();
            List<int> _PL = new List<int>() { (int)ProcessStatus.Reject, (int)ProcessStatus.RejectComplete };
            foreach (var _o in ProcessStatusReflect().Where(p => _PL.Contains(p.ID)).OrderBy(p => p.ID))
            {
                _result.Add(new object[] { _o.ID, _o.Display });
            }
            return _result;
        }

        /// <summary>
        /// 商品状态显示值
        /// </summary>
        /// <param name="objStatus"></param>
        /// <param name="objCss"></param>
        /// <returns></returns>
        public static string GetProcessStatusDisplay(int objStatus, bool objCss = false)
        {
            string _result = string.Empty;
            DefineEnum _O = ProcessStatusReflect().Where(p => p.ID == objStatus).SingleOrDefault();
            if (_O != null)
            {
                if (objCss)
                {
                    _result = string.Format("<label class=\"{0}\">{1}</label>", _O.Css, _O.Display);
                }
                else
                {
                    _result = _O.Display;
                }
            }
            return _result;
        }
        #endregion

        #region 仓库流程状态
        /// <summary>
        /// 仓库流程状态集合
        /// </summary>
        /// <returns></returns>
        private static List<DefineEnum> WarehouseProcessStatusReflect()
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            List<DefineEnum> _result = new List<DefineEnum>();
            //_result.Add(new DefineEnum() { ID = (int)WarehouseProcessStatus.Delete, Display = "", Css = "color_fail" });
            _result.Add(new DefineEnum() { ID = (int)WarehouseProcessStatus.Wait, Display = _LanguagePack["common_wmsprocessstatus_p0"], Css = "color_info" });
            _result.Add(new DefineEnum() { ID = (int)WarehouseProcessStatus.ToWMS, Display = _LanguagePack["common_wmsprocessstatus_p1"], Css = "color_primary" });
            _result.Add(new DefineEnum() { ID = (int)WarehouseProcessStatus.Picked, Display = _LanguagePack["common_wmsprocessstatus_p2"], Css = "color_info" });
            _result.Add(new DefineEnum() { ID = (int)WarehouseProcessStatus.Packed, Display = _LanguagePack["common_wmsprocessstatus_p3"], Css = "color_warning" });
            //_result.Add(new DefineEnum() { ID = (int)WarehouseProcessStatus.Delivered, Display = _LanguagePack["common_wmsprocessstatus_p4"], Css = "color_success" });
            //_result.Add(new DefineEnum() { ID = (int)WarehouseProcessStatus.Canceled, Display = _LanguagePack["common_wmsprocessstatus_p9"], Css = "color_danger" });
            return _result;
        }

        /// <summary>
        /// 仓库流程状态列表
        /// </summary>
        /// <returns></returns>
        public static List<object[]> WarehouseProcessStatusObject()
        {
            List<object[]> _result = new List<object[]>();
            foreach (var _o in WarehouseProcessStatusReflect().OrderBy(p => p.ID))
            {
                _result.Add(new object[] { _o.ID, _o.Display });
            }
            return _result;
        }

        /// <summary>
        /// 仓库流程状态显示值
        /// </summary>
        /// <param name="objStatus"></param>
        /// <param name="objCss"></param>
        /// <returns></returns>
        public static string GetWarehouseProcessStatusDisplay(int objStatus, bool objCss = false)
        {
            string _result = string.Empty;
            DefineEnum _O = WarehouseProcessStatusReflect().Where(p => p.ID == objStatus).SingleOrDefault();
            if (_O != null)
            {
                if (objCss)
                {
                    _result = string.Format("<label class=\"{0}\">{1}</label>", _O.Css, _O.Display);
                }
                else
                {
                    _result = _O.Display;
                }
            }
            return _result;
        }
        #endregion

        #region 快递物流状态
        /// <summary>
        /// 快递状态集合
        /// </summary>
        /// <returns></returns>
        private static List<DefineEnum> ExpressStatusReflect()
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            List<DefineEnum> _result = new List<DefineEnum>();
            _result.Add(new DefineEnum() { ID = (int)ExpressStatus.PendingPickUp, Display = _LanguagePack["common_expressstatus_p0"], Css = "color_primary" });
            _result.Add(new DefineEnum() { ID = (int)ExpressStatus.PickedUp, Display = _LanguagePack["common_expressstatus_p1"], Css = "color_primary" });
            _result.Add(new DefineEnum() { ID = (int)ExpressStatus.InTransit, Display = _LanguagePack["common_expressstatus_p2"], Css = "color_info" });
            //_result.Add(new DefineEnum() { ID = (int)ExpressStatus.OutForDelivery, Display = _LanguagePack["common_expressstatus_p3"], Css = "color_info" });
            _result.Add(new DefineEnum() { ID = (int)ExpressStatus.Signed, Display = _LanguagePack["common_expressstatus_p4"], Css = "color_success" });
            _result.Add(new DefineEnum() { ID = (int)ExpressStatus.RepeatSend, Display = _LanguagePack["common_expressstatus_p5"], Css = "color_warning" });
            _result.Add(new DefineEnum() { ID = (int)ExpressStatus.Return, Display = _LanguagePack["common_expressstatus_p6"], Css = "color_warning" });
            _result.Add(new DefineEnum() { ID = (int)ExpressStatus.ReturnSigned, Display = _LanguagePack["common_expressstatus_p7"], Css = "color_success" });
            _result.Add(new DefineEnum() { ID = (int)ExpressStatus.Canceled, Display = _LanguagePack["common_expressstatus_p8"], Css = "color_danger" });
            _result.Add(new DefineEnum() { ID = (int)ExpressStatus.DeliveryFailed, Display = _LanguagePack["common_expressstatus_p9"], Css = "color_danger" });
            return _result;
        }

        /// <summary>
        /// 快递状态列表
        /// </summary>
        /// <returns></returns>
        public static List<object[]> ExpressStatusObject()
        {
            List<object[]> _result = new List<object[]>();
            foreach (var _o in ExpressStatusReflect().OrderBy(p => p.ID))
            {
                _result.Add(new object[] { _o.ID, _o.Display });
            }
            return _result;
        }

        /// <summary>
        /// 快递状态显示值
        /// </summary>
        /// <param name="objStatus"></param>
        /// <param name="objCss"></param>
        /// <returns></returns>
        public static string GetExpressStatusDisplay(int objStatus, bool objCss = false)
        {
            string _result = string.Empty;
            DefineEnum _O = ExpressStatusReflect().Where(p => p.ID == objStatus).SingleOrDefault();
            if (_O != null)
            {
                if (objCss)
                {
                    _result = string.Format("<label class=\"{0}\">{1}</label>", _O.Css, _O.Display);
                }
                else
                {
                    _result = _O.Display;
                }
            }
            return _result;
        }
        #endregion

        #region 仓库处理状态
        /// <summary>
        /// 仓库状态列表
        /// </summary>
        /// <returns></returns>
        public static List<object[]> WarehouseStatusReflect()
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            List<object[]> _result = new List<object[]>();
            _result.Add(new object[] { (int)WarehouseStatus.UnDeal + 1, _LanguagePack["common_wh_response_unprocess"] });
            _result.Add(new object[] { (int)WarehouseStatus.Dealing + 1, _LanguagePack["common_wh_response_process"] });
            _result.Add(new object[] { (int)WarehouseStatus.DealSuccessful + 1, _LanguagePack["common_wh_response_process_success"] });
            _result.Add(new object[] { (int)WarehouseStatus.DealFail + 1, _LanguagePack["common_wh_response_process_fail"] });
            return _result;
        }

        /// <summary>
        /// 仓库状态显示值
        /// </summary>
        /// <param name="objIsread"></param>
        /// <param name="objStauts"></param>
        /// <param name="objCss"></param>
        /// <returns></returns>
        public static string GetWarehouseStatusDisplay(bool objIsread, int objStauts, bool objCss = false)
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            string _result = string.Empty;
            if (!objIsread)
            {
                if (objCss)
                {
                    _result = string.Format("<label class=\"color_primary\">{0}</label>", _LanguagePack["common_wh_response_unprocess"]);
                }
                else
                {
                    _result = _LanguagePack["common_wh_response_unprocess"];
                }
            }
            else
            {
                if (objStauts == (int)WarehouseStatus.DealSuccessful)
                {
                    if (objCss)
                    {
                        _result = string.Format("<label class=\"color_success\">{0}</label>", _LanguagePack["common_wh_response_process_success"]);
                    }
                    else
                    {
                        _result = _LanguagePack["common_wh_response_process_success"];
                    }
                }
                else if (objStauts == (int)WarehouseStatus.DealFail)
                {
                    if (objCss)
                    {
                        _result = string.Format("<label class=\"color_danger\">{0}</label>", _LanguagePack["common_wh_response_process_fail"]);
                    }
                    else
                    {
                        _result = _LanguagePack["common_wh_response_process_fail"];
                    }
                }
                else
                {
                    if (objCss)
                    {
                        _result = string.Format("<label class=\"color_warning\">{0}</label>", _LanguagePack["common_wh_response_process"]);
                    }
                    else
                    {
                        _result = _LanguagePack["common_wh_response_process"];
                    }
                }
            }
            return _result;
        }
        #endregion

        #region 发货类型
        /// <summary>
        /// 发货类型集合
        /// </summary>
        /// <returns></returns>
        private static List<DefineEnum> ShippingMethodReflect()
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            List<DefineEnum> _result = new List<DefineEnum>();
            _result.Add(new DefineEnum() { ID = (int)ShippingMethod.StandardShipping, Display = _LanguagePack["common_shippingmethod_0"], Css = "color_primary" });
            _result.Add(new DefineEnum() { ID = (int)ShippingMethod.ExpressShipping, Display = _LanguagePack["common_shippingmethod_1"], Css = "color_warning" });
            return _result;
        }

        /// <summary>
        /// 发货类型列表
        /// </summary>
        /// <returns></returns>
        public static List<object[]> ShippingMethodObject()
        {
            List<object[]> _result = new List<object[]>();
            foreach (var _o in ShippingMethodReflect().OrderBy(p => p.ID))
            {
                _result.Add(new object[] { _o.ID, _o.Display });
            }
            return _result;
        }

        /// <summary>
        /// 发货类型显示值
        /// </summary>
        /// <param name="objIsread"></param>
        /// <param name="objStauts"></param>
        /// <param name="objCss"></param>
        /// <returns></returns>
        public static string GetShippingMethodDisplay(int objStatus, bool objCss = false)
        {
            string _result = string.Empty;
            DefineEnum _O = ShippingMethodReflect().Where(p => p.ID == objStatus).SingleOrDefault();
            if (_O != null)
            {
                if (objCss)
                {
                    _result = string.Format("<label class=\"{0}\">{1}</label>", _O.Css, _O.Display);
                }
                else
                {
                    _result = _O.Display;
                }
            }
            return _result;
        }
        #endregion

        #region 取消/退货/换货删除状态
        /// <summary>
        /// 取消/退货/换货删除显示值
        /// </summary>
        /// <param name="objIsread"></param>
        /// <param name="objStauts"></param>
        /// <param name="objCss"></param>
        /// <returns></returns>
        public static string GetWarehouseDeleteStatusDisplay(bool objIsread, int objStauts, bool objCss = false)
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            string _result = string.Empty;
            if (!objIsread)
            {
                if (objCss)
                {
                    _result = string.Format("<label class=\"color_fail\">{0}</label>", _LanguagePack["common_delete_progress"]);
                }
                else
                {
                    _result = _LanguagePack["common_delete_progress"];
                }
            }
            else
            {
                if (objStauts == (int)WarehouseStatus.DealSuccessful)
                {
                    if (objCss)
                    {
                        _result = string.Format("<label class=\"color_success\">{0}</label>", _LanguagePack["common_delete_success"]);
                    }
                    else
                    {
                        _result = _LanguagePack["common_delete_success"];
                    }
                }
                else if (objStauts == (int)WarehouseStatus.DealFail)
                {
                    if (objCss)
                    {
                        _result = string.Format("<label class=\"color_danger\">{0}</label>", _LanguagePack["common_delete_false"]);
                    }
                    else
                    {
                        _result = _LanguagePack["common_delete_false"];
                    }
                }
                else
                {
                    if (objCss)
                    {
                        _result = string.Format("<label class=\"color_fail\">{0}</label>", _LanguagePack["common_delete_progress"]);
                    }
                    else
                    {
                        _result = _LanguagePack["common_delete_progress"];
                    }
                }
            }
            return _result;
        }
        #endregion

        #region 订单属性
        public class OrderNature
        {
            /// <summary>
            /// 是否预定订单(有自己的流程)
            /// </summary>
            private bool _isReservation = false;
            public bool IsReservation
            {
                get { return _isReservation; }
                set { _isReservation = value; }
            }

            /// <summary>
            /// 是否预售订单
            /// </summary>
            private bool _isPre = false;
            public bool IsPre
            {
                get { return _isPre; }
                set { _isPre = value; }
            }

            /// <summary>
            /// 是否紧急订单
            /// </summary>
            private bool _isUrgent = false;
            public bool IsUrgent
            {
                get { return _isUrgent; }
                set { _isUrgent = value; }
            }

            /// <summary>
            /// 是否原始套装
            /// </summary>
            private bool _isSetOrigin = false;
            public bool IsSetOrigin
            {
                get { return _isSetOrigin; }
                set { _isSetOrigin = value; }
            }

            /// <summary>
            /// 是否套装
            /// </summary>
            private bool _isSet = false;
            public bool IsSet
            {
                get { return _isSet; }
                set { _isSet = value; }
            }

            /// <summary>
            /// 是否套装主要产品
            /// </summary>
            private bool _isSetPrimary = false;
            public bool IsSetPrimary
            {
                get { return _isSetPrimary; }
                set { _isSetPrimary = value; }
            }

            /// <summary>
            /// 是否赠品
            /// </summary>
            private bool _isGift = false;
            public bool IsGift
            {
                get { return _isGift; }
                set { _isGift = value; }
            }

            /// <summary>
            /// 是否换货新订单
            /// </summary>
            private bool _isExchangeNew = false;
            public bool IsExchangeNew
            {
                get { return _isExchangeNew; }
                set { _isExchangeNew = value; }
            }

            /// <summary>
            /// 是否Monogram
            /// </summary>
            private bool _isMonogram = false;
            public bool IsMonogram
            {
                get { return _isMonogram; }
                set { _isMonogram = value; }
            }

            /// <summary>
            /// 是否错误
            /// </summary>
            private bool _isError = false;
            public bool IsError
            {
                get { return _isError; }
                set { _isError = value; }
            }
        }

        /// <summary>
        /// 订单类型(预售订单\套装\赠品\换货新订单)
        /// </summary>
        /// <param name="objOrderNatureLabel"></param>
        /// <returns></returns>
        public static string GetOrderNatureLabel(OrderNature objOrderNatureLabel)
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            string _result = string.Empty;
            if (objOrderNatureLabel.IsReservation)
            {
                _result += string.Format("<br/><label class=\"label-info\">{0}</label>", _LanguagePack["ordertype_is_reverse"]);
            }
            if (objOrderNatureLabel.IsPre)
            {
                _result += string.Format("<br/><label class=\"label-info\">{0}</label>", _LanguagePack["ordertype_is_pre"]);
            }
            if (objOrderNatureLabel.IsUrgent)
            {
                _result += string.Format("<br/><label class=\"label-warning\">{0}</label>", _LanguagePack["ordertype_is_urgent"]);
            }
            //套装的原始订单不显示套装字样
            if (objOrderNatureLabel.IsSet)
            {
                if (objOrderNatureLabel.IsSetOrigin)
                {
                    _result += string.Format("<br/><label class=\"label-danger\">{0}</label>", _LanguagePack["ordertype_is_set_origin"]);
                }
                else
                {
                    _result += string.Format("<br/><label class=\"label-success\">{0}</label>", _LanguagePack["ordertype_is_set"]);
                    if (objOrderNatureLabel.IsSetPrimary)
                    {
                        _result += string.Format("<br/><label class=\"label-primary\">{0}</label>", _LanguagePack["ordertype_is_set_primary"]);
                    }
                }
            }
            if (objOrderNatureLabel.IsGift)
            {
                _result += string.Format("<br/><label class=\"label-primary\">{0}</label>", _LanguagePack["ordertype_is_gift"]);
            }
            if (objOrderNatureLabel.IsExchangeNew)
            {
                _result += string.Format("<br/><label class=\"label-danger\">{0}</label>", _LanguagePack["ordertype_is_exchange_new"]);
            }
            if (objOrderNatureLabel.IsMonogram)
            {
                _result += string.Format("<br/><label class=\"label-info\">{0}</label>", _LanguagePack["ordertype_is_monogram"]);
            }
            if (objOrderNatureLabel.IsError)
            {
                _result += string.Format("<br/><label class=\"label-warning\">{0}</label>", _LanguagePack["ordertype_is_error"]);
            }
            return _result;
        }
        #endregion

        #region 流程属性
        public class ProcessNature
        {
            /// <summary>
            /// 是否系统编辑
            /// </summary>
            private bool _isSystemModify = false;
            public bool IsSystemModify
            {
                get { return _isSystemModify; }
                set { _isSystemModify = value; }
            }

            /// <summary>
            /// 是否系统取消
            /// </summary>
            private bool _isSystemCancel = false;
            public bool IsSystemCancel
            {
                get { return _isSystemCancel; }
                set { _isSystemCancel = value; }
            }

            /// <summary>
            /// 是否手工操作
            /// </summary>
            private bool _isManual = false;
            public bool IsManual
            {
                get { return _isManual; }
                set { _isManual = value; }
            }

            /// <summary>
            /// 是否虚拟收货
            /// </summary>
            private bool _isVirtualProductReturn = false;
            public bool IsVirtualProductReturn
            {
                get { return _isVirtualProductReturn; }
                set { _isVirtualProductReturn = value; }
            }
        }

        /// <summary>
        /// 流程类型(系统取消\手工操作\虚拟收货)
        /// </summary>
        /// <param name="objProcessNatureLabel"></param>
        /// <returns></returns>
        public static string GetProcessNatureLabel(ProcessNature objProcessNatureLabel)
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            string _result = string.Empty;
            if (objProcessNatureLabel.IsSystemModify)
            {
                _result += string.Format("<br/><label class=\"label-warning\">{0}</label>", _LanguagePack["processtype_is_systemmodify"]);
            }
            if (objProcessNatureLabel.IsSystemCancel)
            {
                _result += string.Format("<br/><label class=\"label-danger\">{0}</label>", _LanguagePack["processtype_is_systemcancel"]);
            }
            if (objProcessNatureLabel.IsManual)
            {
                _result += string.Format("<br/><label class=\"label-info\">{0}</label>", _LanguagePack["processtype_manual"]);
            }
            //if (objProcessNatureLabel.IsVirtualProductReturn)
            //{
            //    _result += string.Format("<br/><label class=\"label-success\">{0}</label>", _LanguagePack["processtype_virtualproductreturn"]);
            //}
            return _result;
        }
        #endregion

        #region 取消/退货/换货/拒收类型        
        /// <summary>
        /// 取消/退货/换货/拒收/类型集合
        /// </summary>
        /// <returns></returns>
        private static List<DefineEnum> ClaimTypeReflect()
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            List<DefineEnum> _result = new List<DefineEnum>();
            _result.Add(new DefineEnum() { ID = (int)ClaimType.Cancel, Display = _LanguagePack["common_orderstatus_cancel"], Css = "color_danger" });
            _result.Add(new DefineEnum() { ID = (int)ClaimType.Exchange, Display = _LanguagePack["common_orderstatus_exchange"], Css = "color_info" });
            _result.Add(new DefineEnum() { ID = (int)ClaimType.Return, Display = _LanguagePack["common_orderstatus_return"], Css = "color_primary" });
            _result.Add(new DefineEnum() { ID = (int)ClaimType.Reject, Display = _LanguagePack["common_orderstatus_reject"], Css = "color_warning" });
            return _result;
        }

        /// <summary>
        /// 取消/退货/换货/拒收类型列表
        /// </summary>
        /// <returns></returns>
        public static List<object[]> ClaimTypeObject()
        {
            List<object[]> _result = new List<object[]>();
            foreach (var _o in ClaimTypeReflect())
            {
                _result.Add(new object[] { _o.ID, _o.Display });
            }
            return _result;
        }

        /// <summary>
        /// 取消/退货/换货/拒收类型显示值
        /// </summary>
        /// <param name="objStatus"></param>
        /// <param name="objCss"></param>
        /// <returns></returns>
        public static string GetClaimTypeDisplay(int objStatus, bool objCss = false)
        {
            string _result = string.Empty;
            DefineEnum _O = ClaimTypeReflect().Where(p => p.ID == objStatus).SingleOrDefault();
            if (_O != null)
            {
                if (objCss)
                {
                    _result = string.Format("<label class=\"{0}\">{1}</label>", _O.Css, _O.Display);
                }
                else
                {
                    _result = _O.Display;
                }
            }
            return _result;
        }
        #endregion

        #region 退货方式
        /// <summary>
        /// 退货方式
        /// </summary>
        /// <returns></returns>
        private static List<DefineEnum> CollectTypeReflect()
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            List<DefineEnum> _result = new List<DefineEnum>();
            _result.Add(new DefineEnum() { ID = (int)CollectType.ByExpress, Display = _LanguagePack["common_collecttype_1"], Css = "color_default" });
            _result.Add(new DefineEnum() { ID = (int)CollectType.InPerson, Display = _LanguagePack["common_collecttype_2"], Css = "color_default" });
            return _result;
        }

        /// <summary>
        /// 退货方式列表
        /// </summary>
        /// <returns></returns>
        public static List<object[]> CollectTypeObject()
        {
            List<object[]> _result = new List<object[]>();
            foreach (var _o in CollectTypeReflect().OrderBy(p => p.ID))
            {
                _result.Add(new object[] { _o.ID, _o.Display });
            }
            return _result;
        }

        /// <summary>
        /// 退货方式显示值
        /// </summary>
        /// <param name="objStatus"></param>
        /// <param name="objCss"></param>
        /// <returns></returns>
        public static string CollectTypeDisplay(int objStatus, bool objCss = false)
        {
            string _result = string.Empty;
            DefineEnum _O = CollectTypeReflect().Where(p => p.ID == objStatus).SingleOrDefault();
            if (_O != null)
            {
                if (objCss)
                {
                    _result = string.Format("<label class=\"{0}\">{1}</label>", _O.Css, _O.Display);
                }
                else
                {
                    _result = _O.Display;
                }
            }
            return _result;
        }
        #endregion

        #region 紧急订单类型
        /// <summary>
        /// 紧急订单类型
        /// </summary>
        /// <returns></returns>
        private static List<DefineEnum> UrgentTypeReflect()
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            List<DefineEnum> _result = new List<DefineEnum>();
            _result.Add(new DefineEnum() { ID = (int)UrgentType.Delay, Display = _LanguagePack["orderurgent_index_type_1"] });
            _result.Add(new DefineEnum() { ID = (int)UrgentType.Cancel, Display = _LanguagePack["orderurgent_index_type_2"] });
            _result.Add(new DefineEnum() { ID = (int)UrgentType.ReturnOrExcange, Display = _LanguagePack["orderurgent_index_type_3"] });
            _result.Add(new DefineEnum() { ID = (int)UrgentType.Gift, Display = _LanguagePack["orderurgent_index_type_4"] });
            _result.Add(new DefineEnum() { ID = (int)UrgentType.Other, Display = _LanguagePack["orderurgent_index_type_10"] });
            return _result;
        }

        /// <summary>
        /// 紧急订单类型列表
        /// </summary>
        /// <returns></returns>
        public static List<object[]> UrgentTypeObject()
        {
            List<object[]> _result = new List<object[]>();
            foreach (var _o in UrgentTypeReflect())
            {
                _result.Add(new object[] { _o.ID, _o.Display });
            }
            return _result;
        }

        /// <summary>
        /// 紧急订单类型显示值
        /// </summary>
        /// <param name="objStatus"></param>
        /// <returns></returns>
        public static string UrgentTypeDisplay(int objStatus)
        {
            string _result = string.Empty;
            DefineEnum _O = UrgentTypeReflect().Where(p => p.ID == objStatus).SingleOrDefault();
            if (_O != null)
            {
                _result = _O.Display;
            }
            return _result;
        }
        #endregion

        #region 平台状态
        ///// <summary>
        ///// 平台状态集合
        ///// </summary>
        ///// <returns></returns>
        //private static List<DefineEnum> ECommerceStatusReflect()
        //{
        //    List<DefineEnum> _result = new List<DefineEnum>();          
        //    return _result;
        //}

        ///// <summary>
        ///// 获取平台状态
        ///// </summary>
        ///// <param name="objPlatformID"></param>
        ///// <param name="objEBStatus"></param>
        ///// <returns></returns>
        //public static string GetECommerceStatusDisplay(int objPlatformID, string objEBStatus)
        //{
        //    string _result = string.Empty;
        //    var _O = ECommerceStatusReflect().Where(p => p.ID == objPlatformID && p.Value == objEBStatus).SingleOrDefault();
        //    if (_O != null)
        //    {
        //        _result = _O.Display;
        //    }
        //    else
        //    {
        //        _result = objEBStatus;
        //    }
        //    return _result;
        //}
        #endregion

        #region 退款状态
        /// <summary>
        /// 退款类型
        /// </summary>
        /// <returns></returns>
        private static List<DefineEnum> RefundReflect()
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            List<DefineEnum> _result = new List<DefineEnum>();
            _result.Add(new DefineEnum() { ID = (int)RefundStatus.UnDeal, Display = _LanguagePack["common_refundstatus_unprocess"] });
            _result.Add(new DefineEnum() { ID = (int)RefundStatus.Dealing, Display = _LanguagePack["common_refundstatus_processing"] });
            _result.Add(new DefineEnum() { ID = (int)RefundStatus.Finish, Display = _LanguagePack["common_refundstatus_finish"] });
            _result.Add(new DefineEnum() { ID = (int)RefundStatus.Refuse, Display = _LanguagePack["common_refundstatus_refuse"] });
            _result.Add(new DefineEnum() { ID = (int)RefundStatus.Close, Display = _LanguagePack["common_refundstatus_close"] });
            return _result;
        }

        /// <summary>
        /// 退款类型列表
        /// </summary>
        /// <returns></returns>
        public static List<object[]> RefundObject()
        {
            List<object[]> _result = new List<object[]>();
            foreach (var _o in RefundReflect())
            {
                _result.Add(new object[] { _o.ID, _o.Display });
            }
            return _result;
        }

        /// <summary>
        /// 退款类型显示值
        /// </summary>
        /// <param name="objStatus"></param>
        /// <returns></returns>
        public static string GetRefundDisplay(int objStatus)
        {
            string _result = string.Empty;
            DefineEnum _O = RefundReflect().Where(p => p.ID == objStatus).SingleOrDefault();
            if (_O != null)
            {
                _result = _O.Display;
            }
            return _result;
        }
        #endregion

        #region 增值服务
        #region Monogram
        /// <summary>
        /// 字体信息
        /// </summary>
        /// <param name="objMonogramDto"></param>
        /// <returns></returns>
        public static string GetMonogramFont(MonogramDto objMonogramDto)
        {
            string _result = string.Empty;
            if (!string.IsNullOrEmpty(objMonogramDto.Text))
            {
                //显示内容
                string _style = "font-size:18px;";
                string _color = "#000;";
                if (!string.IsNullOrEmpty(objMonogramDto.TextColor))
                {
                    _color = objMonogramDto.TextColor;
                    if (_color.IndexOf("_") > -1)
                    {
                        _color = _color.Substring(_color.IndexOf("_") + 1);
                        if (_color.IndexOf("#") == -1) _color = $"#{_color}";
                    }
                }
                _style += $"color:{_color};";
                if (!string.IsNullOrEmpty(objMonogramDto.TextFont))
                {
                    _style += $"font-family:{objMonogramDto.TextFont};";
                }
                //如果是白色,则增加黑色底色
                if (_color.ToUpper().IndexOf("WHITE") > -1 || _color.ToUpper().IndexOf("FFFFFF") > -1)
                {
                    _style += "background-color:#000;border-radius:3px;padding:0 1px;";
                }
                _result += $"<label style=\"{_style}\">{MonogramFontMatch(objMonogramDto.Text)}</label>";

                //提示内容
                List<string> _title = new List<string>();
                if (!string.IsNullOrEmpty(objMonogramDto.TextFont))
                {
                    _title.Add($"<strong>Font</strong>:&nbsp;{objMonogramDto.TextFont}");
                }
                if (!string.IsNullOrEmpty(objMonogramDto.TextColor))
                {
                    _title.Add($"<strong>Color</strong>:&nbsp;{objMonogramDto.TextColor}");
                }
                if (!string.IsNullOrEmpty(objMonogramDto.Text))
                {
                    _title.Add($"<strong>Text</strong>:&nbsp;{objMonogramDto.Text}");
                }
                _result = $"<a href=\"#\" title=\"{string.Join("<br/>", _title)}\" class=\"easyui-tooltip href-black\">{_result}</a>";
            }
            return _result;
        }

        /// <summary>
        /// Patch信息
        /// </summary>
        /// <param name="objMonogramDto"></param>
        /// <returns></returns>
        public static string GetMonogramPatch(MonogramDto objMonogramDto)
        {
            string _result = string.Empty;
            if (!string.IsNullOrEmpty(objMonogramDto.PatchID))
            {
                //显示内容
                string _style = "font-size:18px;";
                string _color = "#000;";
                if (!string.IsNullOrEmpty(objMonogramDto.PatchColor))
                {
                    _color = objMonogramDto.PatchColor;
                    if (_color.IndexOf("_") > -1)
                    {
                        _color = _color.Substring(0, _color.IndexOf("_"));
                    }
                }
                _style += $"color:{_color};";
                //如果是白色,则增加黑色底色
                if (_color.ToUpper().IndexOf("WHITE") > -1 || _color.ToUpper().IndexOf("FFFFFF") > -1)
                {
                    _style += "background-color:#000;";
                }
                _result += $"<label style=\"{_style}\">{objMonogramDto.PatchID}</label>";

                //提示内容
                List<string> _title = new List<string>();
                if (!string.IsNullOrEmpty(objMonogramDto.PatchColor))
                {
                    _title.Add($"<strong>Patch Color</strong>:&nbsp;{objMonogramDto.PatchColor}");
                }
                if (!string.IsNullOrEmpty(objMonogramDto.PatchID))
                {
                    _title.Add($"<strong>Patch ID</strong>:&nbsp;{objMonogramDto.PatchID}");
                }
                _result = $"<a href=\"#\" title=\"{string.Join("<br/>", _title)}\" class=\"easyui-tooltip href-black\">{_result}</a>";
            }
            return _result;
        }

        /// <summary>
        /// 将Monogram转成字符串描述
        /// </summary>
        /// <param name="objMonogramDto"></param>
        /// <returns></returns>
        public static List<string> ConvertMonogramToText(MonogramDto objMonogramDto)
        {
            List<string> _result = new List<string>();
            if (!string.IsNullOrEmpty(objMonogramDto.TextFont))
            {
                _result.Add(objMonogramDto.TextFont);
            }
            if (!string.IsNullOrEmpty(objMonogramDto.TextColor))
            {
                _result.Add(objMonogramDto.TextColor);
            }
            if (!string.IsNullOrEmpty(objMonogramDto.Text))
            {
                _result.Add(objMonogramDto.Text);
            }
            if (!string.IsNullOrEmpty(objMonogramDto.PatchColor))
            {
                _result.Add(objMonogramDto.PatchColor);
            }
            if (!string.IsNullOrEmpty(objMonogramDto.PatchID))
            {
                _result.Add(objMonogramDto.PatchID);
            }
            return _result;
        }
        #endregion

        #region GiftBox
        /// <summary>
        /// GiftBox信息
        /// </summary>
        /// <param name="objGiftBoxDto"></param>
        /// <returns></returns>
        public static string GetGiftBox(GiftBoxDto objGiftBoxDto)
        {
            string _result = string.Empty;
            if (objGiftBoxDto.IsGiftBox)
            {
                _result += $"<i style=\"font-size:20px;\" class=\"fa fa-gift color_primary\"></i>{objGiftBoxDto.GiftBoxMsg}";
                //提示内容
                if (!string.IsNullOrEmpty(objGiftBoxDto.GiftBoxMsg))
                {
                    _result = $"<a href=\"#\" title=\"{objGiftBoxDto.GiftBoxMsg}\" class=\"easyui-tooltip href-black\">{_result}</a>";
                }
            }
            return _result;
        }

        /// <summary>
        /// 将GiftBox转成字符串描述
        /// </summary>
        /// <param name="objGiftBoxDto"></param>
        /// <returns></returns>
        public static List<string> ConvertGiftBoxToText(GiftBoxDto objGiftBoxDto)
        {
            List<string> _result = new List<string>();
            if (!string.IsNullOrEmpty(objGiftBoxDto.GiftBoxMsg))
            {
                _result.Add(objGiftBoxDto.GiftBoxMsg);
            }
            return _result;
        }
        #endregion

        #region Gift Card
        /// <summary>
        /// Gift Card信息
        /// </summary>
        /// <param name="objGiftCardDto"></param>
        /// <returns></returns>
        public static string GetGiftCard(GiftCardDto objGiftCardDto)
        {
            string _result = string.Empty;
            if (!string.IsNullOrEmpty(objGiftCardDto.Message))
            {
                _result += $"{objGiftCardDto.Message}";
            }
            else
            {
                _result += "Detail";
            }
            //提示内容
            List<string> _title = new List<string>();
            if (!string.IsNullOrEmpty(objGiftCardDto.Recipient))
            {
                _title.Add($"<br />Recipient's Name:{objGiftCardDto.Recipient}");
            }
            if (!string.IsNullOrEmpty(objGiftCardDto.Sender))
            {
                _title.Add($"<br />Sender's Name:{objGiftCardDto.Sender}");
            }
            if (!string.IsNullOrEmpty(objGiftCardDto.Message))
            {
                _title.Add($"<br />Message:{objGiftCardDto.Message}");
            }
            if (!string.IsNullOrEmpty(objGiftCardDto.GiftCardID))
            {
                _title.Add($"<br />Gift card ID:{objGiftCardDto.GiftCardID}");
            }
            //提示内容
            if (_title.Count > 0)
            {
                _result = $"<a href=\"#\" title=\"{string.Join("<br />", _title)}\" class=\"easyui-tooltip href-black\">{_result}</a>";
            }
            return _result;
        }

        /// <summary>
        /// 将Gift Card转成字符串描述
        /// </summary>
        /// <param name="objGiftCardDto"></param>
        /// <returns></returns>
        public static List<string> ConvertGiftCardToText(GiftCardDto objGiftCardDto)
        {
            List<string> _result = new List<string>();
            if (!string.IsNullOrEmpty(objGiftCardDto.Message))
            {
                _result.Add(objGiftCardDto.Message);
            }
            if (!string.IsNullOrEmpty(objGiftCardDto.Recipient))
            {
                _result.Add(objGiftCardDto.Recipient);
            }
            if (!string.IsNullOrEmpty(objGiftCardDto.Sender))
            {
                _result.Add(objGiftCardDto.Sender);
            }
            if (!string.IsNullOrEmpty(objGiftCardDto.Font))
            {
                _result.Add(objGiftCardDto.Font);
            }
            if (!string.IsNullOrEmpty(objGiftCardDto.GiftCardID))
            {
                _result.Add(objGiftCardDto.GiftCardID);
            }
            return _result;
        }
        #endregion

        /// <summary>
        /// Monogram字符替换
        /// </summary>
        /// <param name="objText"></param>
        /// <returns></returns>
        public static string MonogramFontMatch(string objText)
        {
            //匹配列表
            IDictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("(1)", "#");
            dict.Add("(2)", "<i class=\"iconfont icon-heart\"></i>");
            dict.Add("(3)", "@");
            dict.Add("(4)", "<i class=\"iconfont icon-face\"></i>");
            dict.Add("(5)", "<i class=\"iconfont icon-star\"></i>");
            dict.Add("(6)", "<i class=\"iconfont icon-clover\"></i>");
            dict.Add("(7)", "<i class=\"iconfont icon-instagram\"></i>");
            dict.Add("(8)", "<i class=\"iconfont icon-facebook\"></i>");
            dict.Add("(9)", "!");
            dict.Add("(10)", ".");
            dict.Add("(11)", ",");
            dict.Add("(12)", "-");
            dict.Add("(13)", "?");
            dict.Add("(14)", "/");
            dict.Add("(15)", "+");
            dict.Add("(16)", "&");
            dict.Add("(17)", "~");
            dict.Add("(18)", "<i class=\"iconfont icon-Broken-Heart\"></i>");
            dict.Add("(19)", "<i class=\"iconfont icon-birdxiaoniao\"></i>");
            dict.Add("(20)", "<i class=\"iconfont icon-snapchat-ghost\"></i>");
            dict.Add("(21)", "<i class=\"iconfont icon-st-sad-face\"></i>");
            dict.Add("(22)", "'");
            dict.Add("(23)", "<i class=\"iconfont icon-mclaren\"></i>");
            dict.Add("(24)", "$");
            //替换
            foreach (var item in dict)
            {
                objText = objText.Replace(item.Key, item.Value);
            }
            return objText;
        }
        #endregion

        #region 金额小数点显示
        /// <summary>
        /// 金额小数点取整
        /// </summary>
        /// <param name="objPrice"></param>
        public static decimal MathRound(decimal objPrice)
        {
            return Math.Round(objPrice, ConfigCache.Instance.Get().AmountAccuracy);
        }
        #endregion
    }
}