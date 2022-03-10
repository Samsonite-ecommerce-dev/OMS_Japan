using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;
using System.Web.Mvc;
using OMS.App.Helper;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Encryption;
using Samsonite.OMS.Service;
using Samsonite.OMS.Service.AppConfig;
using Samsonite.Utility.Common;
using Samsonite.OMS.ECommerce.Japan;

namespace OMS.App.Controllers
{
    public class GoodsReturnController : BaseController
    {
        //
        // GET: /GoodsReturn/

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
            //订单号
            ViewBag.OrderID = VariableHelper.SaferequestStr(Request.QueryString["OrderID"]);
            //付款方式
            ViewData["payment_list"] = OrderHelper.PaymentTypeObject(ConfigCache.Instance.Get().PaymentTypeConfig);
            //快速时间选项
            ViewData["quicktime_list"] = QuickTimeHelper.QuickTimeOption();
            //流程状态
            ViewData["proccess_list"] = OrderHelper.ProccessReturnStatusObject();
            //仓库状态
            ViewData["wh_list"] = OrderHelper.WarehouseStatusReflect();
            //理由
            ViewData["reason_list"] = OrderReturnProcessService.ReasonReflect();

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
            string _time1 = VariableHelper.SaferequestStr(Request.Form["time1"]);
            string _time2 = VariableHelper.SaferequestStr(Request.Form["time2"]);
            string[] _storeid = VariableHelper.SaferequestStringArray(Request.Form["store"]);
            string _customer = VariableHelper.SaferequestStr(Request.Form["customer"]);
            string _paytype = VariableHelper.SaferequestStr(Request.Form["paytype"]);
            int _reason = VariableHelper.SaferequestInt(Request.Form["reason"]);
            int _process_status = VariableHelper.SaferequestInt(Request.Form["proccess_status"]);
            int _stock_status = VariableHelper.SaferequestInt(Request.Form["stock_status"]);
            using (var db = new DynamicRepository())
            {
                //搜索条件
                if (!string.IsNullOrEmpty(_orderid))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "((vr.OrderNo like {0}) or (vr.SubOrderNo like {0}))", Param = "%" + _orderid + "%" });
                }

                if (!string.IsNullOrEmpty(_time1))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,vr.CreateDate,{0})<=0", Param = VariableHelper.SaferequestTime(_time1) });
                }

                if (!string.IsNullOrEmpty(_time2))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,vr.CreateDate,{0})>=0", Param = VariableHelper.SaferequestTime(_time2) });
                }

                //默认显示当前账号允许看到的店铺订单
                var _UserMalls = new List<string>();
                if (_storeid != null)
                {
                    _UserMalls = _storeid.Where(p => this.CurrentLoginUser.UserMalls.Contains(p)).ToList();
                }
                else
                {
                    _UserMalls = this.CurrentLoginUser.UserMalls;
                }
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vr.MallSapCode in (select item from strToIntTable('" + string.Join(",", _UserMalls) + "',','))", Param = null });

                if (!string.IsNullOrEmpty(_customer))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "(oe.Receive={0} or c.Name={0})", Param = EncryptionBase.EncryptString(_customer) });
                }

                if (!string.IsNullOrEmpty(_paytype))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "od.PaymentType={0}", Param = _paytype });
                }

                if (_reason > -1)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vr.Reason={0}", Param = _reason });
                }

                if (_process_status > 0)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vr.Status={0}", Param = _process_status });
                }

                if (_stock_status > 0)
                {
                    switch (_stock_status)
                    {
                        case 1:
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vr.ApiIsRead={0}", Param = 0 });
                            break;
                        case 2:
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vr.ApiIsRead={0}", Param = 1 });
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vr.ApiStatus={0}", Param = (int)WarehouseStatus.DealSuccessful });
                            break;
                        case 3:
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vr.ApiIsRead={0}", Param = 1 });
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vr.ApiStatus={0}", Param = (int)WarehouseStatus.DealFail });
                            break;
                        case 4:
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vr.ApiIsRead={0}", Param = 1 });
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vr.ApiStatus={0}", Param = (int)WarehouseStatus.Dealing });
                            break;
                        default:
                            break;
                    }
                }

                //不显示换货生成的退款记录
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vr.IsFromExchange={0}", Param = 0 });

                //查询
                var _list = db.GetPage<dynamic>("select vr.Id,vr.OrderNo,vr.SubOrderNo,vr.Quantity,vr.Remark,vr.CreateDate,vr.ApiIsRead,vr.Quantity as ReturnQuantity,vr.[Status],vr.[Type],vr.AcceptUserId,vr.ManualUserID,vr.ApiReplyDate,vr.ApiReplyMsg,vr.AcceptUserDate,vr.AcceptRemark,vr.RefundUserDate,vr.RefundRemark,vr.IsDelete,od.SKU,od.ProductName,vr.[Status] as ProcessStatus,vr.RefundAmount,vr.RefundPoint,vr.RefundExpress,vr.RefundSurcharge,vr.IsFromExchange,vr.ShippingCompany,vr.ShippingNo,vr.ApiStatus,vr.AddUserName,vr.AcceptUserName,vr.RefundUserName,vr.MallName,od.PaymentType,oe.Receive,isnull(c.Name,'')As CustomerName from View_OrderReturn as vr inner join View_OrderDetail as od on vr.SubOrderNo=od.SubOrderNo inner join OrderReceive as oe on vr.SubOrderNo=oe.SubOrderNo inner join Customer as c on od.CustomerNo=c.CustomerNo order by vr.Id desc,vr.ChangeID desc", _SqlWhere, VariableHelper.SaferequestInt(Request.Form["rows"]), VariableHelper.SaferequestInt(Request.Form["page"]));
                //数据解密并脱敏
                foreach (var item in _list.Items)
                {
                    EncryptionFactory.Create(item, new string[] { "Receive", "CustomerName" }).HideSensitive();
                }
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.Id,
                               s0 = dy.Status,
                               s1 = string.Format("<a href=\"javascript:void(0);\" onclick=\"artDialogExtend.Dialog.Open('" + Url.Action("Detail", "OrderQuery") + "?ID={0}',{{title:'{0}',width:'100%',height:'100%'}});\">{0}</a>{1}", dy.OrderNo, OrderHelper.GetProcessNatureLabel(new OrderHelper.ProcessNature() { IsManual = (dy.ManualUserID > 0) })),
                               s2 = dy.SubOrderNo,
                               s3 = dy.MallName,
                               s4 = dy.CustomerName,
                               s5 = dy.Receive,
                               s6 = dy.SKU,
                               s7 = dy.ProductName,
                               s8 = OrderHelper.GetPaymentTypeDisplay(dy.PaymentType),
                               s9 = dy.ReturnQuantity,
                               s10 = VariableHelper.FormateMoney(OrderHelper.MathRound(dy.RefundPoint)),
                               s11 = VariableHelper.FormateMoney(OrderHelper.MathRound(dy.RefundExpress)),
                               s12 = VariableHelper.FormateMoney(OrderHelper.MathRound(dy.RefundSurcharge)),
                               s13 = VariableHelper.FormateMoney(OrderHelper.MathRound(dy.RefundAmount + dy.RefundExpress - dy.RefundSurcharge)),
                               s14 = OrderHelper.GetProcessStatusDisplay(dy.Status, true),
                               s15 = string.Format("{0}<br/>{1}{2}", dy.AddUserName, dy.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"), (!string.IsNullOrEmpty(dy.Remark)) ? "<br/>" + dy.Remark : ""),
                               s16 = OrderHelper.GetWarehouseStatusDisplay(dy.ApiIsRead, dy.ApiStatus, true) + ((dy.ApiReplyDate != null) ? "<br/>" + dy.ApiReplyDate.ToString("yyyy-MM-dd HH:mm:ss") : "") + ((dy.ApiReplyMsg != null) ? "<br/>" + dy.ApiReplyMsg : ""),
                               s17 = string.Format("{0}", ((dy.AcceptUserDate != null) ? dy.AcceptUserName + "<br/>" + VariableHelper.FormateTime(dy.AcceptUserDate, "yyyy-MM-dd HH:mm:ss") : "")),
                               s18 = string.Format("{0}", ((dy.RefundUserDate != null) ? dy.RefundUserName + "<br/>" + VariableHelper.FormateTime(dy.RefundUserDate, "yyyy-MM-dd HH:mm:ss") : ""))
                           }
                };
            }
            return _result;
        }
        #endregion

        #region 添加
        [UserPowerAuthorize]
        public ActionResult Add()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;

            Int64 _ID = VariableHelper.SaferequestInt64(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                Order objOrder = db.Order.Where(p => p.Id == _ID).SingleOrDefault();
                if (objOrder != null)
                {
                    ViewData["order"] = objOrder;
                    List<OrderDetail> objOrderDetail_List = db.OrderDetail.Where(p => p.OrderId == objOrder.Id && !p.IsSetOrigin && !p.IsDelete && !p.IsExchangeNew).ToList();
                    //快递公司
                    ViewData["express_list"] = ExpressCompanyService.GetExpressCompanyObject();
                    //最新收货信息集合
                    List<ReceiveDto> objReceiveDto_List = new List<ReceiveDto>();
                    foreach (var _o in objOrderDetail_List)
                    {
                        objReceiveDto_List.Add(OrderReceiveService.GetNewestReceive(_o.OrderNo, _o.SubOrderNo));
                    }
                    ViewData["receive_list"] = objReceiveDto_List;
                    //促销活动信息
                    ViewData["adjustment_list"] = db.OrderDetailAdjustment.Where(p => p.OrderNo == objOrder.OrderNo).ToList();
                    //运费活动信息
                    ViewData["shipping_adjustment_list"] = db.OrderShippingAdjustment.Where(p => p.OrderNo == objOrder.OrderNo && p.AdjustmentGrossPrice < 0).ToList();
                    //混合支付
                    ViewData["payment_detail_list"] = db.OrderPaymentDetail.Where(p => p.OrderNo == objOrder.OrderNo).ToList();
                    //理由
                    ViewData["reason_list"] = OrderReturnProcessService.ReasonReflect();
                    //未退快递费
                    ViewBag.WaitDeliveryFee = OrderProcessService.CountRemainDeliveryFee(objOrder.OrderNo, objOrder.DeliveryFee);

                    return View(objOrderDetail_List);
                }
                else
                {
                    return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.NoMessage });
                }
            }
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Add_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();

            string _OrderNo = VariableHelper.SaferequestStr(Request.Form["OrderNo"]);
            DateTime _ReturnTime = VariableHelper.SaferequestTime(Request.Form["ReturnTime"]);
            decimal _ExpressFee = (VariableHelper.SaferequestInt(Request.Form["IsExpressFee"]) == 1) ? VariableHelper.SaferequestDecimal(Request.Form["ExpressFee"]) : 0;
            decimal _ReduceExpressFee = (VariableHelper.SaferequestInt(Request.Form["IsReduceExpressFee"]) == 1) ? VariableHelper.SaferequestDecimal(Request.Form["ReduceExpressFee"]) : 0;
            decimal _TotalExpressFee = _ExpressFee - _ReduceExpressFee;
            int _Reason = VariableHelper.SaferequestInt(Request.Form["Reason"]);
            string _Remark = VariableHelper.SaferequestStr(Request.Form["Remark"]);
            string _Receiver = VariableHelper.SaferequestStr(Request.Form["Receiver"]);
            string _Tel = VariableHelper.SaferequestStr(Request.Form["Tel"]);
            string _Mobile = VariableHelper.SaferequestStr(Request.Form["Mobile"]);
            string _Zipcode = VariableHelper.SaferequestStr(Request.Form["Zipcode"]);
            string _Addr = VariableHelper.SaferequestStr(Request.Form["Addr"]);
            string _PickUpTime = VariableHelper.SaferequestStr(Request.Form["PickUpTime"]);
            int _PickUpTimeInterval = VariableHelper.SaferequestInt(Request.Form["PickUpTimeInterval"]);
            string _SelectIDs = Request.Form["SelectID"];
            string _Quantitys = Request.Form["Quantity"];
            //string _RefundPoints = Request.Form["RefundPoint"];
            string _RefundAmounts = Request.Form["RefundAmount"];
            string _ShippingCompanys = Request.Form["ShippingCompany"];
            string _ShippingNos = Request.Form["ShippingNo"];

            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        Order objOrder = db.Order.Where(p => p.OrderNo == _OrderNo).SingleOrDefault();
                        if (objOrder != null)
                        {
                            if (string.IsNullOrEmpty(_SelectIDs))
                            {
                                throw new Exception(_LanguagePack["goodsreturn_edit_message_at_least_one_product"]);
                            }

                            //sendsale订单无法进行退货
                            if (objOrder.OrderType == (int)OrderType.MallSale)
                            {
                                throw new Exception(_LanguagePack["goodsreturn_edit_message_order_no_allow"]);
                            }

                            //读取子订单
                            List<OrderDetail> objOrderDetail_List = db.OrderDetail.Where(p => p.OrderNo == _OrderNo && !p.IsSetOrigin && !p.IsExchangeNew).ToList();
                            OrderDetail objOrderDetail = new OrderDetail();
                            OrderReturn objOrderReturn = new OrderReturn();
                            OrderChangeRecord objOrderChangeRecord = new OrderChangeRecord();
                            OrderLog objOrderLog = new OrderLog();
                            int _OrgStatus = 0;
                            //要退货的子订单
                            string[] _SelectID_Array = _SelectIDs.Split(',');
                            string[] _Quantity_Array = _Quantitys.Split(',');
                            //string[] _RefundPoint_Array = _RefundPoints.Split(',');
                            string[] _RefundAmount_Array = _RefundAmounts.Split(',');
                            string[] _ShippingCompany_Array = _ShippingCompanys.Split(',');
                            string[] _ShippingNo_Array = _ShippingNos.Split(',');
                            int _Select_Length = _SelectID_Array.Length;
                            int _Quantity = 0;
                            int _Effect_Quantity = 0;
                            decimal _Avag_ExpressFee = 0;
                            decimal _Avag_SurchargeFee = 0;
                            //套装不允许单个退货
                            List<string> objSetCodes = objOrderDetail_List.Where(p => _SelectID_Array.Contains(p.Id.ToString()) && p.IsSet).GroupBy(p => p.SetCode).Select(o => o.Key).ToList();
                            if (objSetCodes.Count > 0)
                            {
                                foreach (string _setCodes in objSetCodes)
                                {
                                    //读取该套装信息
                                    List<string> objSets = objOrderDetail_List.Where(p => p.SetCode == _setCodes).Select(o => o.Id.ToString()).ToList();
                                    foreach (string _str in objSets)
                                    {
                                        if (!_SelectID_Array.Contains(_str))
                                        {
                                            throw new Exception(_LanguagePack["goodsreturn_edit_message_set_rule"]);
                                        }
                                    }
                                }
                            }
                            for (int t = 0; t < _Select_Length; t++)
                            {
                                ////Demandware,Tumi和Micros的订单需要上传快递号
                                //if (objOrder.PlatformType == (int)PlatformType.DEMANDWARE_Singapore || objOrder.PlatformType == (int)PlatformType.TUMI_Singapore || objOrder.PlatformType == (int)PlatformType.Micros_Singapore)
                                //{
                                //    if (string.IsNullOrEmpty(_ShippingNo_Array[t]))
                                //    {
                                //        throw new Exception(_LanguagePack["goodsreturn_edit_message_no_shippingno"]);
                                //    }
                                //}

                                Int64 _id = VariableHelper.SaferequestInt64(_SelectID_Array[t]);
                                _Quantity = VariableHelper.SaferequestPage(_Quantity_Array[t]);
                                //平摊快递费(按照退货订单数平摊)
                                if (t == _Select_Length - 1)
                                {
                                    _Avag_ExpressFee = _ExpressFee - OrderHelper.MathRound(_ExpressFee / _Select_Length * (_Select_Length - 1));
                                    _Avag_SurchargeFee = _ReduceExpressFee - OrderHelper.MathRound(_ReduceExpressFee / _Select_Length * (_Select_Length - 1));
                                }
                                else
                                {
                                    _Avag_ExpressFee = OrderHelper.MathRound(_ExpressFee / _Select_Length);
                                    _Avag_SurchargeFee = OrderHelper.MathRound(_ReduceExpressFee / _Select_Length);
                                }
                                objOrderDetail = objOrderDetail_List.Where(p => p.Id == _id).SingleOrDefault();
                                if (objOrderDetail != null)
                                {
                                    //流程ID
                                    string _RequestId = OrderReturnProcessService.CreateRequestID(objOrderDetail.SubOrderNo);

                                    _OrgStatus = objOrderDetail.Status;
                                    //是否无效信息
                                    if (objOrderDetail.IsDelete || objOrderDetail.IsError)
                                    {
                                        throw new Exception(string.Format("{0}:{1}", objOrderDetail.SubOrderNo, _LanguagePack["common_data_no_avail"]));
                                    }

                                    //判断是否允许退货信息,需要在已发送快递的情况下
                                    List<int> objAllowStatus = new List<int>() { (int)ProductStatus.Delivered, (int)ProductStatus.Complete };
                                    if (!objAllowStatus.Contains(objOrderDetail.Status))
                                    {
                                        throw new Exception(string.Format("{0}:{1}", objOrderDetail.SubOrderNo, _LanguagePack["goodsreturn_edit_message_state_no_allow"]));
                                    }

                                    //有效数量
                                    _Effect_Quantity = VariableHelper.SaferequestPositiveInt(objOrderDetail.Quantity - objOrderDetail.CancelQuantity - objOrderDetail.ReturnQuantity - objOrderDetail.ExchangeQuantity - objOrderDetail.RejectQuantity);
                                    if (_Quantity > _Effect_Quantity)
                                        _Quantity = _Effect_Quantity;
                                    //退货数量需要大于零
                                    if (_Quantity <= 0)
                                    {
                                        throw new Exception(string.Format("{0}:{1}", objOrderDetail.SubOrderNo, _LanguagePack["goodsreturn_edit_message_quantity_error"]));
                                    }

                                    //退货流程表
                                    objOrderReturn = new OrderReturn()
                                    {
                                        OrderNo = objOrderDetail.OrderNo,
                                        MallSapCode = objOrder.MallSapCode,
                                        UserId = this.CurrentLoginUser.Userid,
                                        AddDate = DateTime.Now,
                                        CreateDate = _ReturnTime,
                                        Reason = _Reason,
                                        Remark = _Remark,
                                        FromApi = false,
                                        SubOrderNo = objOrderDetail.SubOrderNo,
                                        Quantity = _Quantity,
                                        Status = (int)ProcessStatus.Return,
                                        ShippingCompany = (!string.IsNullOrEmpty(_ShippingNo_Array[t])) ? _ShippingCompany_Array[t] : "",
                                        ShippingNo = _ShippingNo_Array[t],
                                        ShippingPickUpDay = _PickUpTime,
                                        ShippingPickUpTimeType = _PickUpTimeInterval,
                                        RequestId = _RequestId,
                                        CollectionType = 0,
                                        CustomerName = _Receiver,
                                        Tel = _Tel,
                                        Mobile = _Mobile,
                                        Zipcode = _Zipcode,
                                        Addr = _Addr,
                                        AcceptUserId = 0,
                                        AcceptUserDate = null,
                                        AcceptRemark = string.Empty,
                                        RefundUserId = 0,
                                        RefundAmount = VariableHelper.SaferequestDecimal(_RefundAmount_Array[t]),
                                        //RefundPoint = VariableHelper.SaferequestDecimal(_RefundPoint_Array[t]),
                                        RefundPoint = 0,
                                        RefundExpress = _Avag_ExpressFee,
                                        RefundSurcharge = _Avag_SurchargeFee,
                                        RefundUserDate = null,
                                        RefundRemark = string.Empty,
                                        IsFromExchange = false,
                                        ManualUserId = 0,
                                        ManualUserDate = null,
                                        ManualRemark = string.Empty
                                    };
                                    //数据加密
                                    EncryptionFactory.Create(objOrderReturn).Encrypt();
                                    db.OrderReturn.Add(objOrderReturn);
                                    db.SaveChanges();
                                    //插入api表
                                    objOrderChangeRecord = new OrderChangeRecord()
                                    {
                                        OrderNo = objOrderDetail.OrderNo,
                                        SubOrderNo = objOrderDetail.SubOrderNo,
                                        Type = (int)OrderChangeType.Return,
                                        DetailTableName = OrderReturnProcessService.TableName,
                                        DetailId = objOrderReturn.Id,
                                        UserId = this.CurrentLoginUser.Userid,
                                        Status = 0,
                                        Remarks = string.Empty,
                                        ApiIsRead = false,
                                        ApiReadDate = null,
                                        ApiReplyDate = null,
                                        ApiReplyMsg = string.Empty,
                                        AddDate = DateTime.Now,
                                        IsDelete = false
                                    };
                                    db.OrderChangeRecord.Add(objOrderChangeRecord);
                                    db.SaveChanges();
                                    //修改子订单退货数量
                                    objOrderDetail.ReturnQuantity = ((objOrderDetail.ReturnQuantity + _Quantity) >= objOrderDetail.Quantity) ? objOrderDetail.Quantity : (objOrderDetail.ReturnQuantity + _Quantity);
                                    //修改子订单状态
                                    objOrderDetail.Status = (int)ProductStatus.Return;
                                    objOrderDetail.EditDate = DateTime.Now;
                                    //添加子订单log
                                    objOrderLog = new OrderLog()
                                    {
                                        Msg = "GoodsReturn Processing",
                                        OrderNo = objOrderDetail.OrderNo,
                                        SubOrderNo = objOrderDetail.SubOrderNo,
                                        UserId = this.CurrentLoginUser.Userid,
                                        OriginStatus = _OrgStatus,
                                        NewStatus = (int)ProductStatus.Return,
                                        CreateDate = DateTime.Now
                                    };
                                    db.OrderLog.Add(objOrderLog);
                                }
                                else
                                {
                                    throw new Exception(string.Format("{0}:{1}", objOrderDetail.SubOrderNo, _LanguagePack["common_data_no_exsit"]));
                                }

                                //如果是Tumi和Micros的订单需要推送ReadyToShip
                                if (objOrder.PlatformType == (int)PlatformType.TUMI_Japan || objOrder.PlatformType == (int)PlatformType.Micros_Japan)
                                {
                                    //由于客户自取或者从店铺去取等方式,可能不需要申请快递号
                                    if (!string.IsNullOrEmpty(_ShippingNo_Array[t]))
                                    {
                                        //取货时间
                                        string[] _clSlot = new string[] { VariableHelper.SaferequestTime(_PickUpTime).ToString("yyyy-MM-dd"), (_PickUpTimeInterval == 1) ? "13:00" : "09:00", (_PickUpTimeInterval == 1) ? "18:00" : "13:00" };
                                        //AssignShipping
                                        SpeedPostExtend objSpeedPostExtend = new SpeedPostExtend();
                                        var resp = objSpeedPostExtend.AssignShipping(new string[] { _ShippingNo_Array[t] }, _clSlot);
                                        if (resp.data != null)
                                        {
                                            //下载快递信息相关文档
                                            objSpeedPostExtend.GetDocument(db.View_OrderDetail.Where(p => p.OrderNo == objOrderDetail.OrderNo && p.SubOrderNo == objOrderDetail.SubOrderNo).SingleOrDefault(), _ShippingNo_Array[t], true, objOrderReturn.Id);
                                        }
                                        else
                                        {
                                            throw new Exception(_LanguagePack["goodsreturn_edit_message_collectionslots_not_available"]);
                                        }
                                    }
                                }
                            }
                            db.SaveChanges();

                            Trans.Commit();
                            //返回信息
                            _result.Data = new
                            {
                                result = true,
                                msg = _LanguagePack["common_data_save_success"]
                            };
                        }
                        else
                        {
                            throw new Exception(_LanguagePack["goodsreturn_edit_message_no_order"]);
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

        /// <summary>
        /// 从speedPost申请快递号
        /// </summary>
        /// <returns></returns>
        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult Add_GenerateTrackingNumber_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            string _OrderNo = VariableHelper.SaferequestStr(Request.Form["OrderNo"]);
            int _Reason = VariableHelper.SaferequestInt(Request.Form["Reason"]);
            string _SelectedSubOrderNo = VariableHelper.SaferequestStr(Request.Form["SelectedSubOrderNo"]);
            ReceiveDto objReceiveDto = new ReceiveDto()
            {
                Receiver = VariableHelper.SaferequestStr(Request.Form["Receiver"]),
                Tel = VariableHelper.SaferequestStr(Request.Form["Tel"]),
                Mobile = VariableHelper.SaferequestStr(Request.Form["Mobile"]),
                ZipCode = VariableHelper.SaferequestStr(Request.Form["Zipcode"]),
                Address = VariableHelper.SaferequestStr(Request.Form["Addr"])
            };

            try
            {
                if (string.IsNullOrEmpty(_SelectedSubOrderNo))
                {
                    throw new Exception(_LanguagePack["goodsreturn_edit_message_at_least_one_product"]);
                }

                using (var db = new ebEntities())
                {
                    var objView_OrderDetail = db.View_OrderDetail.Where(p => p.OrderNo == _OrderNo && p.SubOrderNo == _SelectedSubOrderNo).SingleOrDefault();
                    if (objView_OrderDetail != null)
                    {
                        SpeedPostExtend objSpeedPostExtend = new SpeedPostExtend();
                        var resp = objSpeedPostExtend.CreateShipmentForOrder(objView_OrderDetail, objReceiveDto, db);
                        if (Convert.ToBoolean(resp[0]))
                        {
                            //返回数据
                            _result.Data = new
                            {
                                result = true,
                                shipmentNumber = resp[1].ToString(),
                                msg = resp[1].ToString(),
                            };
                        }
                        else
                        {
                            throw new Exception(resp[2].ToString());
                        }
                    }
                    else
                    {
                        throw new Exception(_LanguagePack["common_data_load_false"]);
                    }
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
                var objView_OrderReturn = db.View_OrderReturn.Where(p => p.Id == _ID && p.Type == (int)OrderChangeType.Return && !p.IsFromExchange).SingleOrDefault();
                if (objView_OrderReturn != null)
                {
                    if (objView_OrderReturn.Status == (int)ProcessStatus.Return)
                    {
                        //数据解密
                        EncryptionFactory.Create(objView_OrderReturn).Decrypt();

                        ViewData["order_detail"] = db.View_OrderDetail.Where(p => p.SubOrderNo == objView_OrderReturn.SubOrderNo).SingleOrDefault();
                        //退货方式
                        ViewData["collecttype_list"] = OrderHelper.CollectTypeObject();
                        //快递公司
                        ViewData["express_list"] = ExpressCompanyService.GetExpressCompanyObject();
                        //理由
                        ViewData["reason_list"] = OrderReturnProcessService.ReasonReflect();

                        return View(objView_OrderReturn);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.Other, Message = _LanguagePack["goodsreturn_edit_message_state_not_allow_edit"] });
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
            int _Quantity = VariableHelper.SaferequestInt(Request.Form["Quantity"]);
            //decimal _RefundPoint = VariableHelper.SaferequestDecimal(Request.Form["RefundPoint"]);
            decimal _RefundAmount = VariableHelper.SaferequestDecimal(Request.Form["RefundAmount"]);
            decimal _ExpressFee = VariableHelper.SaferequestDecimal(Request.Form["ExpressFee"]);
            decimal _ReduceExpressFee = VariableHelper.SaferequestDecimal(Request.Form["ReduceExpressFee"]);
            string _ShippingCompany = VariableHelper.SaferequestStr(Request.Form["ShippingCompany"]);
            string _ShippingNo = VariableHelper.SaferequestStr(Request.Form["ShippingNo"]);
            int _Reason = VariableHelper.SaferequestInt(Request.Form["Reason"]);
            string _Remark = VariableHelper.SaferequestStr(Request.Form["Remark"]);
            string _Receiver = VariableHelper.SaferequestStr(Request.Form["Receiver"]);
            string _Tel = VariableHelper.SaferequestStr(Request.Form["Tel"]);
            string _Mobile = VariableHelper.SaferequestStr(Request.Form["Mobile"]);
            string _Zipcode = VariableHelper.SaferequestStr(Request.Form["Zipcode"]);
            string _Addr = VariableHelper.SaferequestStr(Request.Form["Addr"]);

            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        View_OrderReturn objView_OrderReturn = db.View_OrderReturn.Where(p => p.Id == _ID && p.Type == (int)OrderChangeType.Return && !p.IsFromExchange).SingleOrDefault();
                        if (objView_OrderReturn != null)
                        {
                            View_OrderDetail objOrderDetail = db.View_OrderDetail.Where(p => p.SubOrderNo == objView_OrderReturn.SubOrderNo).SingleOrDefault();
                            if (objOrderDetail != null)
                            {
                                //Tumi/Micros的订单需要上传快递号
                                if (objOrderDetail.PlatformType == (int)PlatformType.TUMI_Japan || objOrderDetail.PlatformType == (int)PlatformType.Micros_Japan)
                                {
                                    if (string.IsNullOrEmpty(_ShippingNo))
                                    {
                                        throw new Exception(_LanguagePack["goodsreturn_edit_message_no_shippingno"]);
                                    }
                                }

                                //计算有效数量
                                int _Effect_Quantity = objOrderDetail.Quantity - objOrderDetail.CancelQuantity - objOrderDetail.ReturnQuantity - objOrderDetail.ExchangeQuantity - objOrderDetail.RejectQuantity + objView_OrderReturn.Quantity;
                                if (_Quantity > _Effect_Quantity)
                                    _Quantity = _Effect_Quantity;
                                //退货数量需要大于零
                                if (_Quantity == 0)
                                {
                                    throw new Exception(string.Format("{0}:{1}", objView_OrderReturn.SubOrderNo, _LanguagePack["goodsreturn_edit_message_quantity_error"]));
                                }
                                else
                                {
                                    //计算剩余可退数量
                                    if (_Quantity > _Effect_Quantity)
                                    {
                                        throw new Exception(string.Format("{0}:{1}", objOrderDetail.SubOrderNo, _LanguagePack["goodsreturn_edit_message_not_enough_quantity"]));
                                    }
                                }

                                if (objView_OrderReturn.Status == (int)ProcessStatus.Return)
                                {
                                    OrderReturn objOrderReturn = db.OrderReturn.Where(p => p.Id == objView_OrderReturn.Id).SingleOrDefault();
                                    if (objOrderReturn != null)
                                    {
                                        objOrderReturn.Quantity = _Quantity;
                                        //objOrderReturn.RefundPoint = _RefundPoint;
                                        objOrderReturn.RefundAmount = _RefundAmount;
                                        objOrderReturn.RefundExpress = _ExpressFee;
                                        objOrderReturn.RefundSurcharge = _ReduceExpressFee;
                                        objOrderReturn.ShippingCompany = _ShippingCompany;
                                        objOrderReturn.ShippingNo = _ShippingNo;
                                        objOrderReturn.CollectionType = 0;
                                        objOrderReturn.CustomerName = _Receiver;
                                        objOrderReturn.Tel = _Tel;
                                        objOrderReturn.Mobile = _Mobile;
                                        objOrderReturn.Zipcode = _Zipcode;
                                        objOrderReturn.Addr = _Addr;
                                        objOrderReturn.Reason = _Reason;
                                        objOrderReturn.Remark = _Remark;
                                        //数据加密
                                        EncryptionFactory.Create(objOrderReturn).Encrypt();
                                        //更新产品退货数量
                                        db.Database.ExecuteSqlCommand("update OrderDetail set ReturnQuantity=ReturnQuantity+{0} where SubOrderNo={1}", (_Quantity - objOrderReturn.Quantity), objView_OrderReturn.SubOrderNo);
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        throw new Exception(_LanguagePack["goodsreturn_edit_message_no_message"]);
                                    }
                                }
                                else
                                {
                                    throw new Exception(_LanguagePack["goodsreturn_edit_message_state_not_allow_edit"]);
                                }
                            }
                            else
                            {
                                throw new Exception(_LanguagePack["common_data_no_exsit"]);
                            }
                        }
                        else
                        {
                            throw new Exception(_LanguagePack["goodsreturn_edit_message_no_message"]);
                        }
                        Trans.Commit();
                        //返回信息
                        _result.Data = new
                        {
                            result = true,
                            msg = _LanguagePack["common_data_save_success"]
                        };
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
                    return _result;
                }
            }
        }
        #endregion

        #region 删除
        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Delete_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            string _IDs = VariableHelper.SaferequestStr(Request.Form["ID"]);
            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_IDs))
                        {
                            throw new Exception(_LanguagePack["common_data_need_one"]);
                        }

                        string[] _IDs_Array = _IDs.Split(',');
                        object[] _r = new object[2];
                        foreach (string _str in _IDs_Array)
                        {
                            _r = OrderReturnProcessService.Delete(VariableHelper.SaferequestInt64(_str), db);
                            if (!Convert.ToBoolean(_r[0]))
                            {
                                throw new Exception(_r[1].ToString());
                            }
                        }
                        Trans.Commit();
                        //返回信息
                        _result.Data = new
                        {
                            result = true,
                            msg = _LanguagePack["common_data_cancel_success"]
                        };
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

        #region 退款确认
        [UserPowerAuthorize]
        public ActionResult Sure()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;
            ViewBag.LanguagePack = _LanguagePack;

            Int64 _ID = VariableHelper.SaferequestInt64(Request.QueryString["ID"]);

            using (var db = new ebEntities())
            {
                var objView_OrderReturn = db.View_OrderReturn.Where(p => p.Id == _ID && p.Type == (int)OrderChangeType.Return && !p.IsFromExchange).SingleOrDefault();
                if (objView_OrderReturn != null)
                {
                    if (objView_OrderReturn.Status == (int)ProcessStatus.ReturnAcceptComfirm)
                    {
                        ViewData["order_detail"] = db.View_OrderDetail.Where(p => p.SubOrderNo == objView_OrderReturn.SubOrderNo).SingleOrDefault();
                        //退货方式
                        ViewData["collecttype_list"] = OrderHelper.CollectTypeObject();

                        //数据解密
                        EncryptionFactory.Create(objView_OrderReturn).Decrypt();

                        return View(objView_OrderReturn);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.Other, Message = _LanguagePack["goodsreturn_sure_message_stock_false"] });
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.NoMessage });
                }
            }
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Sure_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            string _IDs = VariableHelper.SaferequestStr(Request.Form["ID"]);
            decimal? _RefundPoint = VariableHelper.SaferequestDecimal(Request.Form["RefundPoint"]);
            decimal? _RefundAmount = VariableHelper.SaferequestDecimal(Request.Form["RefundAmount"]);
            decimal? _RefundExpress = VariableHelper.SaferequestDecimal(Request.Form["RefundExpress"]);
            decimal? _ReduceExpressFee = VariableHelper.SaferequestDecimal(Request.Form["ReduceExpressFee"]);
            int? _CollectType = VariableHelper.SaferequestInt(Request.Form["CollectType"]);
            string _Remark = VariableHelper.SaferequestEditor(Request.Form["Remark"]);

            List<string> _ErrorList = new List<string>();
            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_IDs))
                    {
                        throw new Exception(_LanguagePack["common_data_need_one"]);
                    }

                    string[] _IDs_Array = _IDs.Split(',');
                    object[] _r = new object[2];
                    foreach (string _str in _IDs_Array)
                    {
                        try
                        {
                            _r = OrderReturnProcessService.RefundSure(VariableHelper.SaferequestInt64(_str), ((Request.Form["RefundPoint"] != null) ? _RefundPoint : null), ((Request.Form["RefundAmount"] != null) ? _RefundAmount : null), ((Request.Form["RefundExpress"] != null) ? _RefundExpress : null), ((Request.Form["ReduceExpressFee"] != null) ? _ReduceExpressFee : null), ((Request.Form["CollectType"] != null) ? _CollectType : null), _Remark, db);
                            if (!Convert.ToBoolean(_r[0]))
                            {
                                throw new Exception(_r[1].ToString());
                            }
                        }
                        catch (Exception ex)
                        {
                            _ErrorList.Add(ex.Message);
                        }
                    }
                    //返回信息
                    if (_ErrorList.Count == 0)
                    {
                        _result.Data = new
                        {
                            result = true,
                            msg = _LanguagePack["common_data_save_success"]
                        };
                    }
                    else
                    {
                        throw new Exception(string.Join("<br/>", _ErrorList));
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
        #endregion

        #region 人工处理
        [UserPowerAuthorize]
        public ActionResult Manual()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;
            ViewBag.LanguagePack = _LanguagePack;

            using (var db = new ebEntities())
            {
                string _IDs = VariableHelper.SaferequestStr(Request.QueryString["ID"]);
                if (string.IsNullOrEmpty(_IDs))
                {
                    return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.Other, Message = _LanguagePack["common_data_need_one"] });
                }
                else
                {
                    var objView_OrderReturn_List = db.Database.SqlQuery<View_OrderReturn>("select * from View_OrderReturn where Id in (" + _IDs + ") and Type={0}", (int)OrderChangeType.Return).ToList();
                    if (objView_OrderReturn_List.Count() > 0)
                    {
                        ViewBag.IDs = string.Join(",", objView_OrderReturn_List.Select(p => p.Id).ToList());
                        foreach (View_OrderReturn objView_OrderReturn in objView_OrderReturn_List)
                        {
                            if (objView_OrderReturn.Status != (int)ProcessStatus.ReturnFail)
                            {
                                return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.Other, Message = string.Format("{0}:{1}", objView_OrderReturn.SubOrderNo, _LanguagePack["goodsreturn_edit_message_error_state"]) });
                            }
                        }

                        return View();
                    }
                    else
                    {
                        return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.NoMessage });
                    }
                }
            }
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Manual_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            string _IDs = VariableHelper.SaferequestStr(Request.Form["ID"]);
            int _Result = VariableHelper.SaferequestInt(Request.Form["Result"]);
            string _Remark = VariableHelper.SaferequestEditor(Request.Form["Remark"]);
            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_IDs))
                        {
                            throw new Exception(_LanguagePack["common_data_need_one"]);
                        }

                        string[] _IDs_Array = _IDs.Split(',');
                        object[] _r = new object[2];
                        foreach (string _str in _IDs_Array)
                        {
                            _r = OrderReturnProcessService.ManualInterference(VariableHelper.SaferequestInt64(_str), (_Result == 1), _Remark, db);
                            if (!Convert.ToBoolean(_r[0]))
                            {
                                throw new Exception(_r[1].ToString());
                            }
                        }
                        Trans.Commit();
                        //返回信息
                        _result.Data = new
                        {
                            result = true,
                            msg = _LanguagePack["common_data_save_success"]
                        };
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

        #region 详情
        [UserPowerAuthorize]
        public ActionResult Detail()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;

            Int64 _ID = VariableHelper.SaferequestInt64(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                var objView_OrderReturn = db.View_OrderReturn.Where(p => p.Id == _ID && p.Type == (int)OrderChangeType.Return).SingleOrDefault();
                if (objView_OrderReturn != null)
                {
                    //数据解密
                    EncryptionFactory.Create(objView_OrderReturn).Decrypt();

                    ViewData["order_detail"] = db.View_OrderDetail.Where(p => p.SubOrderNo == objView_OrderReturn.SubOrderNo).SingleOrDefault();

                    var d = db.OrderReturnDeliverysDocument.Where(p => p.OrderReturnID == _ID && p.DocumentType == (int)ECommerceDocumentType.ShippingDoc).SingleOrDefault();
                    //替换成pdf文件,供下载使用
                    ViewBag.ShippingDoc = (d != null) ? DeliverysDocumentService.ExistDocMapPath(d.DocumentFile.Replace("html", "pdf")) : null;

                    return View(objView_OrderReturn);
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

            JsonResult _result = new JsonResult();
            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            string _orderid = VariableHelper.SaferequestStr(Request.Form["OrderNumber"]);
            string _time1 = VariableHelper.SaferequestStr(Request.Form["Time1"]);
            string _time2 = VariableHelper.SaferequestStr(Request.Form["Time2"]);
            string[] _storeid = VariableHelper.SaferequestStringArray(Request.Form["StoreName"]);
            string _customer = VariableHelper.SaferequestStr(Request.Form["Customer"]);
            string _paytype = VariableHelper.SaferequestStr(Request.Form["PaymentStatus"]);
            int _reason = VariableHelper.SaferequestInt(Request.Form["Reason"]);
            int _process_status = VariableHelper.SaferequestInt(Request.Form["ProcessStatus"]);
            int _stock_status = VariableHelper.SaferequestInt(Request.Form["StockStatus"]);
            using (var db = new DynamicRepository())
            {
                //搜索条件
                if (!string.IsNullOrEmpty(_orderid))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "((vr.OrderNo like {0}) or (vr.SubOrderNo like {0}))", Param = "%" + _orderid + "%" });
                }

                if (!string.IsNullOrEmpty(_time1))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,vr.CreateDate,{0})<=0", Param = VariableHelper.SaferequestTime(_time1) });
                }

                if (!string.IsNullOrEmpty(_time2))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,vr.CreateDate,{0})>=0", Param = VariableHelper.SaferequestTime(_time2) });
                }

                //默认显示当前账号允许看到的店铺订单
                var _UserMalls = new List<string>();
                if (_storeid != null)
                {
                    _UserMalls = _storeid.Where(p => this.CurrentLoginUser.UserMalls.Contains(p)).ToList();
                }
                else
                {
                    _UserMalls = this.CurrentLoginUser.UserMalls;
                }
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vr.MallSapCode in (select item from strToIntTable('" + string.Join(",", _UserMalls) + "',','))", Param = null });

                if (!string.IsNullOrEmpty(_customer))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "(oe.Receive={0} or c.Name={0})", Param = EncryptionBase.EncryptString(_customer) });
                }

                if (!string.IsNullOrEmpty(_paytype))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "od.PaymentType={0}", Param = _paytype });
                }

                if (_reason > -1)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vr.Reason={0}", Param = _reason });
                }

                if (_process_status > 0)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vr.Status={0}", Param = _process_status });
                }

                if (_stock_status > 0)
                {
                    switch (_stock_status)
                    {
                        case 1:
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vr.ApiIsRead={0}", Param = 0 });
                            break;
                        case 2:
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vr.ApiIsRead={0}", Param = 1 });
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vr.ApiStatus={0}", Param = (int)WarehouseStatus.DealSuccessful });
                            break;
                        case 3:
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vr.ApiIsRead={0}", Param = 1 });
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vr.ApiStatus={0}", Param = (int)WarehouseStatus.DealFail });
                            break;
                        case 4:
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vr.ApiIsRead={0}", Param = 1 });
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vr.ApiStatus={0}", Param = (int)WarehouseStatus.Dealing });
                            break;
                        default:
                            break;
                    }
                }

                //不显示换货生成的退款记录
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vr.IsFromExchange={0}", Param = 0 });

                DataTable dt = new DataTable();
                dt.Columns.Add(_LanguagePack["goodsreturn_index_order_number"]);
                dt.Columns.Add(_LanguagePack["goodsreturn_index_sub_order_number"]);
                dt.Columns.Add(_LanguagePack["goodsreturn_index_store"]);
                dt.Columns.Add(_LanguagePack["goodsreturn_index_customer"]);
                dt.Columns.Add(_LanguagePack["goodsreturn_index_receiver"]);
                dt.Columns.Add(_LanguagePack["goodsreturn_index_sku"]);
                dt.Columns.Add(_LanguagePack["goodsreturn_index_productname"]);
                dt.Columns.Add(_LanguagePack["goodsreturn_index_payment_type"]);
                dt.Columns.Add(_LanguagePack["goodsreturn_index_quantity"]);
                dt.Columns.Add(_LanguagePack["goodsreturn_index_refund_delivery"]);
                dt.Columns.Add(_LanguagePack["goodsreturn_index_refund_surcharge"]);
                dt.Columns.Add(_LanguagePack["goodsreturn_index_refund_payment"]);
                dt.Columns.Add(_LanguagePack["goodsreturn_index_state"]);
                dt.Columns.Add(_LanguagePack["goodsreturn_index_oper_message"]);
                dt.Columns.Add(_LanguagePack["goodsreturn_index_stock_reply"]);
                dt.Columns.Add(_LanguagePack["goodsreturn_index_accept_message"]);
                dt.Columns.Add(_LanguagePack["goodsreturn_index_refund_message"]);

                //查询
                DataRow _dr = null;
                var _list = db.Fetch<dynamic>("select vr.Id,vr.OrderNo,vr.SubOrderNo,vr.Quantity,vr.Remark,vr.CreateDate,vr.ApiIsRead,vr.Quantity as ReturnQuantity,vr.[Status],vr.[Type],vr.AcceptUserId,vr.ManualUserID,vr.ApiReplyDate,vr.ApiReplyMsg,vr.AcceptUserDate,vr.AcceptRemark,vr.RefundUserDate,vr.RefundRemark,vr.IsDelete,od.SKU,od.ProductName,vr.[Status] as ProcessStatus,vr.RefundAmount,vr.RefundPoint,vr.RefundExpress,vr.RefundSurcharge,vr.IsFromExchange,vr.ShippingCompany,vr.ShippingNo,vr.ApiStatus,vr.AddUserName,vr.AcceptUserName,vr.RefundUserName,vr.MallName,od.PaymentType,oe.Receive,isnull(c.Name,'')As CustomerName from View_OrderReturn as vr inner join View_OrderDetail as od on vr.SubOrderNo=od.SubOrderNo inner join OrderReceive as oe on vr.SubOrderNo=oe.SubOrderNo left join Customer as c on od.CustomerNo=c.CustomerNo order by vr.Id desc,vr.ChangeID desc", _SqlWhere);

                foreach (var dy in _list)
                {
                    //数据解密并脱敏
                    EncryptionFactory.Create(dy, new string[] { "Receive", "CustomerName" }).HideSensitive();

                    _dr = dt.NewRow();
                    _dr[0] = dy.OrderNo;
                    _dr[1] = dy.SubOrderNo;
                    _dr[2] = dy.MallName;
                    _dr[3] = dy.CustomerName;
                    _dr[4] = dy.Receive;
                    _dr[5] = dy.SKU;
                    _dr[6] = dy.ProductName;
                    _dr[7] = OrderHelper.GetPaymentTypeDisplay(dy.PaymentType);
                    _dr[8] = dy.ReturnQuantity;
                    _dr[9] = VariableHelper.FormateMoney(OrderHelper.MathRound(dy.RefundExpress));
                    _dr[10] = VariableHelper.FormateMoney(OrderHelper.MathRound(dy.RefundSurcharge));
                    _dr[11] = VariableHelper.FormateMoney(OrderHelper.MathRound(dy.RefundAmount + dy.RefundExpress - dy.RefundSurcharge));
                    _dr[12] = OrderHelper.GetProcessStatusDisplay(dy.Status, false);
                    _dr[13] = string.Format("{0} {1}{2}", dy.AddUserName, dy.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"), (!string.IsNullOrEmpty(dy.Remark)) ? " " + dy.Remark : "");
                    _dr[14] = OrderHelper.GetWarehouseStatusDisplay(dy.ApiIsRead, dy.ApiStatus, false) + ((dy.ApiReplyDate != null) ? " " + dy.ApiReplyDate.ToString("yyyy-MM-dd HH:mm:ss") : "") + ((dy.ApiReplyMsg != null) ? " " + dy.ApiReplyMsg : "");
                    _dr[15] = string.Format("{0}", ((dy.AcceptUserDate != null) ? dy.AcceptUserName + " " + VariableHelper.FormateTime(dy.AcceptUserDate, "yyyy-MM-dd HH:mm:ss") : ""));
                    _dr[16] = string.Format("{0}", ((dy.RefundUserDate != null) ? dy.RefundUserName + " " + VariableHelper.FormateTime(dy.RefundUserDate, "yyyy-MM-dd HH:mm:ss") : ""));
                    dt.Rows.Add(_dr);
                }

                string _filepath = $"~{AppGlobalService.UPLOAD_CACHE_PATH}/{DateTime.Now.ToString("yyyy-MM-dd")}";
                if (!Directory.Exists(Server.MapPath(_filepath)))
                {
                    Directory.CreateDirectory(Server.MapPath(_filepath));
                }
                string _fileName = string.Format("OrderReturn_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss"));
                _fileName = System.Web.HttpUtility.UrlEncode(_fileName, Encoding.GetEncoding("UTF-8"));
                string _path = Server.MapPath(string.Format("{0}/{1}", _filepath, _fileName));
                ExcelHelper objExcelHelper = new ExcelHelper(_path);
                objExcelHelper.DataTableToExcel(dt, "Sheet1", true);
                return File(_path, "application/ms-excel", _fileName);
            }
        }
        #endregion
    }
}
