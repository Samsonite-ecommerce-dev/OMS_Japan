using System;
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
using Samsonite.OMS.Service.AppConfig;
using Samsonite.Utility.Common;
using OMS.App.Helper;

namespace OMS.App.Controllers
{
    public class OrderCancelController : BaseController
    {
        //
        // GET: /OrderCancel/

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
            ViewData["proccess_list"] = OrderHelper.ProccessCancelStatusObject();
            //仓库状态
            ViewData["wh_list"] = OrderHelper.WarehouseStatusReflect();
            //理由
            ViewData["reason_list"] = OrderCancelProcessService.ReasonReflect();

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
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "((vc.OrderNo like {0}) or (vc.SubOrderNo like {0}))", Param = "%" + _orderid + "%" });
                }

                if (!string.IsNullOrEmpty(_time1))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,vc.CreateDate,{0})<=0", Param = VariableHelper.SaferequestTime(_time1) });
                }

                if (!string.IsNullOrEmpty(_time2))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,vc.CreateDate,{0})>=0", Param = VariableHelper.SaferequestTime(_time2) });
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
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vc.MallSapCode in (select item from strToIntTable('" + string.Join(",", _UserMalls) + "',','))", Param = null });

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
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vc.Reason={0}", Param = _reason });
                }

                if (_process_status > 0)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vc.Status={0}", Param = _process_status });
                }

                if (_stock_status > 0)
                {
                    switch (_stock_status)
                    {
                        case 1:
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vc.ApiIsRead={0}", Param = 0 });
                            break;
                        case 2:
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vc.ApiIsRead={0}", Param = 1 });
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vc.ApiStatus={0}", Param = (int)WarehouseStatus.DealSuccessful });
                            break;
                        case 3:
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vc.ApiIsRead={0}", Param = 1 });
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vc.ApiStatus={0}", Param = (int)WarehouseStatus.DealFail });
                            break;
                        case 4:
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vc.ApiIsRead={0}", Param = 1 });
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vc.ApiStatus={0}", Param = (int)WarehouseStatus.Dealing });
                            break;
                        default:
                            break;
                    }
                }

                //查询
                var _list = db.GetPage<dynamic>("select vc.Id,vc.OrderNo,vc.SubOrderNo,vc.MallName,vc.RefundAmount,vc.RefundPoint,vc.RefundExpress,vc.Remark,vc.AddUserName,vc.ManualUserID,vc.CreateDate,vc.AcceptUserDate,vc.AcceptUserName,vc.RefundUserName,vc.ApiIsRead,vc.Quantity as CancelQuantity,vc.[Status],vc.[Type],vc.IsSystemCancel,vc.ApiReplyDate,vc.ApiReplyMsg,vc.RefundUserDate,vc.RefundRemark,vc.IsDelete,vc.ApiStatus,od.SKU,od.ProductName,od.PaymentType,oe.Receive,isnull(c.Name,'') As CustomerName from View_OrderCancel as vc inner join View_OrderDetail as od on vc.SubOrderNo=od.SubOrderNo inner join OrderReceive as oe on vc.SubOrderNo=oe.SubOrderNo inner join Customer as c on od.CustomerNo=c.CustomerNo order by vc.Id desc,vc.ChangeID desc", _SqlWhere, VariableHelper.SaferequestInt(Request.Form["rows"]), VariableHelper.SaferequestInt(Request.Form["page"]));
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
                               s1 = string.Format("<a href=\"javascript:void(0);\" onclick=\"artDialogExtend.Dialog.Open('" + Url.Action("Detail", "OrderQuery") + "?ID={0}',{{title:'{0}',width:'100%',height:'100%'}});\">{0}</a>{1}", dy.OrderNo, OrderHelper.GetProcessNatureLabel(new OrderHelper.ProcessNature() { IsSystemCancel = dy.IsSystemCancel, IsManual = (dy.ManualUserID > 0) })),
                               s2 = dy.SubOrderNo,
                               s3 = dy.MallName,
                               s4 = dy.CustomerName,
                               s5 = dy.Receive,
                               s6 = dy.SKU,
                               s7 = dy.ProductName,
                               s8 = OrderHelper.GetPaymentTypeDisplay(dy.PaymentType),
                               s9 = dy.CancelQuantity,
                               s10 = VariableHelper.FormateMoney(OrderHelper.MathRound(dy.RefundPoint)),
                               s11 = VariableHelper.FormateMoney(OrderHelper.MathRound(dy.RefundExpress)),
                               s12 = VariableHelper.FormateMoney(OrderHelper.MathRound(dy.RefundAmount + dy.RefundExpress)),
                               s13 = OrderHelper.GetProcessStatusDisplay(dy.Status, true),
                               s14 = string.Format("{0}<br/>{1}{2}", dy.AddUserName, dy.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"), (!string.IsNullOrEmpty(dy.Remark)) ? "<br/>" + dy.Remark : ""),
                               s15 = (!dy.IsSystemCancel) ? OrderHelper.GetWarehouseStatusDisplay(dy.ApiIsRead, dy.ApiStatus, true) + ((dy.ApiReplyDate != null) ? "<br/>" + dy.ApiReplyDate.ToString("yyyy-MM-dd HH:mm:ss") : "") + ((dy.ApiReplyMsg != null) ? "<br/>" + dy.ApiReplyMsg : "") : "",
                               s16 = string.Format("{0}", ((dy.AcceptUserDate != null) ? dy.AcceptUserName + "<br/>" + VariableHelper.FormateTime(dy.AcceptUserDate, "yyyy-MM-dd HH:mm:ss") : "")),
                               s17 = string.Format("{0}", ((dy.RefundUserDate != null) ? dy.RefundUserName + "<br/>" + VariableHelper.FormateTime(dy.RefundUserDate, "yyyy-MM-dd HH:mm:ss") : ""))
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
                    List<OrderDetail> objOrderDetail_List = db.OrderDetail.Where(p => p.OrderId == objOrder.Id && !p.IsSetOrigin && !p.IsExchangeNew && !p.IsDelete).ToList();
                    //促销活动信息
                    ViewData["adjustment_list"] = db.OrderDetailAdjustment.Where(p => p.OrderNo == objOrder.OrderNo).ToList();
                    //运费活动信息
                    ViewData["shipping_adjustment_list"] = db.OrderShippingAdjustment.Where(p => p.OrderNo == objOrder.OrderNo && p.AdjustmentGrossPrice < 0).ToList();
                    //混合支付
                    ViewData["payment_detail_list"] = db.OrderPaymentDetail.Where(p => p.OrderNo == objOrder.OrderNo).ToList();
                    //理由
                    ViewData["reason_list"] = OrderCancelProcessService.ReasonReflect();
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
            DateTime _CancelTime = VariableHelper.SaferequestTime(Request.Form["CancelTime"]);
            //是否选中ExpressFee
            decimal _ExpressFee = (VariableHelper.SaferequestInt(Request.Form["IsExpressFee"]) == 1) ? VariableHelper.SaferequestDecimal(Request.Form["ExpressFee"]) : 0;
            int _Reason = VariableHelper.SaferequestInt(Request.Form["Reason"]);
            string _Remark = VariableHelper.SaferequestStr(Request.Form["Remark"]);

            string _SelectIDs = Request.Form["SelectID"];
            string _Quantitys = Request.Form["Quantity"];
            //string _RefundPoints = Request.Form["RefundPoint"];
            string _RefundAmounts = Request.Form["RefundAmount"];

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
                                throw new Exception(_LanguagePack["ordercancel_edit_message_at_least_one_product"]);
                            }

                            //读取子订单
                            List<OrderDetail> objOrderDetail_List = db.OrderDetail.Where(p => p.OrderNo == _OrderNo && !p.IsSetOrigin && !p.IsExchangeNew).ToList();
                            OrderDetail objOrderDetail = new OrderDetail();
                            OrderCancel objOrderCancel = new OrderCancel();
                            OrderChangeRecord objOrderChangeRecord = new OrderChangeRecord();
                            OrderWMSReply objOrderWMSReply = new OrderWMSReply();
                            OrderLog objOrderLog = new OrderLog();
                            int _OrgStatus = 0;
                            //要取消的子订单
                            string[] _SelectID_Array = _SelectIDs.Split(',');
                            string[] _Quantity_Array = _Quantitys.Split(',');
                            //string[] _RefundPoint_Array = _RefundPoints.Split(',');
                            string[] _RefundAmount_Array = _RefundAmounts.Split(',');
                            int _Select_Length = _SelectID_Array.Length;
                            int _Quantity = 0;
                            int _Effect_Quantity = 0;
                            decimal _RefundAmount = 0;
                            int _RefundPoint = 0;
                            decimal _Avag_ExpressFee = 0;
                            bool _IsCOD = (objOrder.PaymentType == (int)PayType.CashOnDelivery);
                            bool _IsSystemCancel = false;
                            int _ProcessStatus = 0;
                            int _AcceptUserId = 0;
                            DateTime? _AcceptUserDate = null;
                            string _AcceptRemark = string.Empty;
                            int _RefundUserId = 0;
                            DateTime? _RefundUserDate = null;
                            string _RefundRemark = string.Empty;
                            int _ProductStatus = 0;
                            //套装不允许单个取消
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
                                            throw new Exception(_LanguagePack["ordercancel_edit_message_set_rule"]);
                                        }
                                    }
                                }
                            }
                            for (int t = 0; t < _Select_Length; t++)
                            {
                                Int64 _id = VariableHelper.SaferequestInt64(_SelectID_Array[t]);
                                _Quantity = VariableHelper.SaferequestInt(_Quantity_Array[t]);
                                //如果是COD订单,退款金额和快递费用都是0
                                if (_IsCOD)
                                {
                                    _RefundAmount = 0;
                                    //_RefundPoint = 0;
                                    _Avag_ExpressFee = 0;
                                }
                                else
                                {
                                    _RefundAmount = VariableHelper.SaferequestDecimal(_RefundAmount_Array[t]);
                                    //_RefundPoint= VariableHelper.SaferequestInt(_RefundPoint_Array[t]);
                                    //平摊快递费(按照退货订单数平摊)
                                    if (t == _Select_Length - 1)
                                    {
                                        _Avag_ExpressFee = OrderHelper.MathRound(_ExpressFee - _ExpressFee / _Select_Length * (_Select_Length - 1));
                                    }
                                    else
                                    {
                                        _Avag_ExpressFee = OrderHelper.MathRound(_ExpressFee / _Select_Length);
                                    }
                                }
                                objOrderDetail = objOrderDetail_List.Where(p => p.Id == _id).SingleOrDefault();
                                if (objOrderDetail != null)
                                {
                                    //流程ID
                                    string _RequestId = OrderCancelProcessService.CreateRequestID(objOrderDetail.SubOrderNo);

                                    _OrgStatus = objOrderDetail.Status;
                                    //是否无效信息
                                    if (objOrderDetail.IsDelete || objOrderDetail.IsError)
                                    {
                                        throw new Exception(string.Format("{0}:{1}", objOrderDetail.SubOrderNo, _LanguagePack["common_data_no_avail"]));
                                    }

                                    //判断在发货前和待处理之后才允许取消信息
                                    List<int> objAllowStatus = new List<int>() { (int)ProductStatus.Received, (int)ProductStatus.InDelivery, (int)ProductStatus.ReceivedGoods };
                                    if (!objAllowStatus.Contains(objOrderDetail.Status))
                                    {
                                        throw new Exception(string.Format("{0}:{1}", objOrderDetail.SubOrderNo, _LanguagePack["ordercancel_edit_message_state_no_allow"]));
                                    }

                                    //查询是否已经存在未删除的取消记录,如果存在则不再重复插入
                                    View_OrderCancel objView_OrderCancel = db.View_OrderCancel.Where(p => p.SubOrderNo == objOrderDetail.SubOrderNo && p.Status != (int)ProcessStatus.Delete).FirstOrDefault();
                                    if (objView_OrderCancel != null)
                                    {
                                        throw new Exception(string.Format("{0}:{1}", objOrderDetail.SubOrderNo, _LanguagePack["ordercancel_edit_message_exist_process"]));
                                    }

                                    //计算有效数量
                                    _Effect_Quantity = VariableHelper.SaferequestPositiveInt(objOrderDetail.Quantity - objOrderDetail.CancelQuantity - objOrderDetail.ReturnQuantity - objOrderDetail.ExchangeQuantity - objOrderDetail.RejectQuantity);
                                    if (_Quantity > _Effect_Quantity)
                                        _Quantity = _Effect_Quantity;
                                    //取消数量需要大于零
                                    if (_Quantity <= 0)
                                    {
                                        throw new Exception(string.Format("{0}:{1}", objOrderDetail.SubOrderNo, _LanguagePack["ordercancel_edit_message_quantity_error"]));
                                    }
                                    //判断是否是内部取消
                                    //1.是否处于pending/Received
                                    //2.是否在生成D/N(仓库状态在ToWMS)之前
                                    if (objOrderDetail.Status == (int)ProductStatus.Received)
                                    {
                                        if (objOrderDetail.ShippingStatus < (int)WarehouseProcessStatus.ToWMS)
                                        {
                                            _IsSystemCancel = true;
                                        }
                                        else
                                        {
                                            _IsSystemCancel = false;
                                        }
                                    }
                                    else
                                    {
                                        _IsSystemCancel = false;
                                    }
                                    //如果是内部取消,如果是Demandware/Tumi/Micros的非COD订单需要等待确认付款,不然则直接完成
                                    if (_IsSystemCancel)
                                    {
                                        _AcceptUserId = 0;
                                        _AcceptUserDate = DateTime.Now;
                                        _AcceptRemark = string.Empty;
                                        if (objOrder.PlatformType == (int)PlatformType.TUMI_Japan || objOrder.PlatformType == (int)PlatformType.Micros_Japan)
                                        {
                                            if (_IsCOD)
                                            {
                                                _ProcessStatus = (int)ProcessStatus.CancelComplete;
                                                _RefundUserId = 0;
                                                _RefundUserDate = DateTime.Now;
                                                _RefundRemark = "The system automatically confirms the refund";
                                                _ProductStatus = (int)ProductStatus.CancelComplete;
                                            }
                                            else
                                            {
                                                _ProcessStatus = (int)ProcessStatus.WaitRefund;
                                                _ProductStatus = (int)ProductStatus.Cancel;
                                            }
                                        }
                                        else
                                        {
                                            _ProcessStatus = (int)ProcessStatus.CancelComplete;
                                            _RefundUserId = 0;
                                            _RefundUserDate = DateTime.Now;
                                            _RefundRemark = "The system automatically confirms the refund";
                                            _ProductStatus = (int)ProductStatus.CancelComplete;
                                        }
                                    }
                                    else
                                    {
                                        _ProcessStatus = (int)ProcessStatus.Cancel;
                                        _ProductStatus = (int)ProductStatus.Cancel;
                                    }
                                    //取消流程表
                                    objOrderCancel = new OrderCancel()
                                    {
                                        OrderNo = objOrderDetail.OrderNo,
                                        MallSapCode = objOrder.MallSapCode,
                                        UserId = this.CurrentLoginUser.Userid,
                                        AddDate = DateTime.Now,
                                        CreateDate = _CancelTime,
                                        Reason = _Reason,
                                        Remark = _Remark,
                                        AcceptUserId = _AcceptUserId,
                                        AcceptUserDate = _AcceptUserDate,
                                        AcceptRemark = _AcceptRemark,
                                        FromApi = false,
                                        SubOrderNo = objOrderDetail.SubOrderNo,
                                        Quantity = _Quantity,
                                        Status = _ProcessStatus,
                                        RequestId = _RequestId,
                                        RefundAmount = _RefundAmount,
                                        RefundPoint = _RefundPoint,
                                        RefundExpress = _Avag_ExpressFee,
                                        RefundUserId = _RefundUserId,
                                        RefundUserDate = _RefundUserDate,
                                        RefundRemark = _RefundRemark,
                                        IsSystemCancel = _IsSystemCancel,
                                        ManualUserId = 0,
                                        ManualUserDate = null,
                                        ManualRemark = string.Empty,
                                        IsFromCOD = _IsCOD
                                    };
                                    db.OrderCancel.Add(objOrderCancel);
                                    db.SaveChanges();
                                    //如果是系统内部直接取消
                                    if (_IsSystemCancel)
                                    {
                                        //插入api表,如果是内部取消，则不显示仓库处理情况
                                        objOrderChangeRecord = new OrderChangeRecord()
                                        {
                                            OrderNo = objOrderDetail.OrderNo,
                                            SubOrderNo = objOrderDetail.SubOrderNo,
                                            Type = (int)OrderChangeType.Cancel,
                                            DetailTableName = OrderCancelProcessService.TableName,
                                            DetailId = objOrderCancel.Id,
                                            UserId = this.CurrentLoginUser.Userid,
                                            Status = 1,
                                            Remarks = string.Empty,
                                            ApiIsRead = false,
                                            ApiReadDate = null,
                                            ApiReplyDate = null,
                                            ApiReplyMsg = string.Empty,
                                            AddDate = DateTime.Now,
                                            IsDelete = true
                                        };
                                        db.OrderChangeRecord.Add(objOrderChangeRecord);
                                        db.SaveChanges();
                                        //标识订单为自动取消
                                        objOrderDetail.IsSystemCancel = true;
                                    }
                                    else
                                    {
                                        //插入api表
                                        objOrderChangeRecord = new OrderChangeRecord()
                                        {
                                            OrderNo = objOrderDetail.OrderNo,
                                            SubOrderNo = objOrderDetail.SubOrderNo,
                                            Type = (int)OrderChangeType.Cancel,
                                            DetailTableName = OrderCancelProcessService.TableName,
                                            DetailId = objOrderCancel.Id,
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
                                    }
                                    //修改子订单取消数量
                                    objOrderDetail.CancelQuantity = ((objOrderDetail.CancelQuantity + _Quantity) >= objOrderDetail.Quantity) ? objOrderDetail.Quantity : (objOrderDetail.CancelQuantity + _Quantity);
                                    //修改子订单状态
                                    objOrderDetail.Status = _ProductStatus;
                                    objOrderDetail.EditDate = DateTime.Now;
                                    //如果取消完成,判断是否需要完结主订单
                                    if (_ProductStatus == (int)ProductStatus.CancelComplete)
                                    {
                                        objOrderDetail.CompleteDate = DateTime.Now;
                                    }
                                    //添加子订单log
                                    objOrderLog = new OrderLog()
                                    {
                                        OrderNo = objOrderDetail.OrderNo,
                                        SubOrderNo = objOrderDetail.SubOrderNo,
                                        UserId = this.CurrentLoginUser.Userid,
                                        OriginStatus = _OrgStatus,
                                        NewStatus = _ProductStatus,
                                        Msg = "Cancel Processing",
                                        CreateDate = DateTime.Now
                                    };
                                    db.OrderLog.Add(objOrderLog);
                                    db.SaveChanges();

                                    //如果取消成功
                                    if (objOrderDetail.Status == (int)ProductStatus.CancelComplete)
                                    {
                                        //判断是否需要完结主订单
                                        OrderProcessService.CompleteOrder(objOrderDetail.OrderNo, db);
                                    }
                                }
                                else
                                {
                                    throw new Exception(string.Format("{0}:{1}", objOrderDetail.SubOrderNo, _LanguagePack["common_data_no_exsit"]));
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
                        else
                        {
                            throw new Exception(_LanguagePack["ordercancel_edit_message_no_order"]);
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
                            _r = OrderCancelProcessService.Delete(VariableHelper.SaferequestInt64(_str), db);
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
                View_OrderCancel objView_OrderCancel = db.View_OrderCancel.Where(p => p.Id == _ID && p.Type == (int)OrderChangeType.Cancel).SingleOrDefault();
                if (objView_OrderCancel != null)
                {
                    if (objView_OrderCancel.Status == (int)ProcessStatus.WaitRefund)
                    {
                        ViewData["order_detail"] = db.View_OrderDetail.Where(p => p.SubOrderNo == objView_OrderCancel.SubOrderNo).SingleOrDefault();

                        return View(objView_OrderCancel);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.Other, Message = _LanguagePack["ordercancel_sure_message_not_getby_stock"] });
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
            //decimal? _RefundPoint = VariableHelper.SaferequestDecimal(Request.Form["Refundpoint"]);
            decimal? _RefundAmount = VariableHelper.SaferequestDecimal(Request.Form["RefundAmount"]);
            decimal? _RefundExpress = VariableHelper.SaferequestDecimal(Request.Form["RefundExpress"]);
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
                            _r = OrderCancelProcessService.RefundSure(VariableHelper.SaferequestInt64(_str), 0, ((Request.Form["RefundAmount"] != null) ? _RefundAmount : null), ((Request.Form["RefundExpress"] != null) ? _RefundExpress : null), _Remark, db);
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
                    var objView_OrderCancel_List = db.Database.SqlQuery<View_OrderCancel>("select * from View_OrderCancel where Id in (" + _IDs + ") and Type={0}", (int)OrderChangeType.Cancel).ToList();
                    if (objView_OrderCancel_List.Count() > 0)
                    {
                        ViewBag.IDs = string.Join(",", objView_OrderCancel_List.Select(p => p.Id).ToList());
                        foreach (View_OrderCancel objView_OrderCancel in objView_OrderCancel_List)
                        {
                            if (objView_OrderCancel.Status != (int)ProcessStatus.CancelFail)
                            {
                                return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.Other, Message = string.Format("{0}:{1}", objView_OrderCancel.SubOrderNo, _LanguagePack["ordercancel_edit_message_error_state"]) });
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
                            _r = OrderCancelProcessService.ManualInterference(VariableHelper.SaferequestInt64(_str), (_Result == 1), _Remark, db);
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
                var objView_OrderCancel = db.View_OrderCancel.Where(p => p.Id == _ID).SingleOrDefault();
                if (objView_OrderCancel != null)
                {
                    ViewData["order_detail"] = db.View_OrderDetail.Where(p => p.SubOrderNo == objView_OrderCancel.SubOrderNo).SingleOrDefault();

                    return View(objView_OrderCancel);
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
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "((vc.OrderNo like {0}) or (vc.SubOrderNo like {0}))", Param = "%" + _orderid + "%" });
                }

                if (!string.IsNullOrEmpty(_time1))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,vc.CreateDate,{0})<=0", Param = VariableHelper.SaferequestTime(_time1) });
                }

                if (!string.IsNullOrEmpty(_time2))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,vc.CreateDate,{0})>=0", Param = VariableHelper.SaferequestTime(_time2) });
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
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vc.MallSapCode in (select item from strToIntTable('" + string.Join(",", _UserMalls) + "',','))", Param = null });

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
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vc.Reason={0}", Param = _reason });
                }

                if (_process_status > 0)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vc.Status={0}", Param = _process_status });
                }

                if (_stock_status > 0)
                {
                    switch (_stock_status)
                    {
                        case 1:
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vc.ApiIsRead={0}", Param = 0 });
                            break;
                        case 2:
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vc.ApiIsRead={0}", Param = 1 });
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vc.ApiStatus={0}", Param = (int)WarehouseStatus.DealSuccessful });
                            break;
                        case 3:
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vc.ApiIsRead={0}", Param = 1 });
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vc.ApiStatus={0}", Param = (int)WarehouseStatus.DealFail });
                            break;
                        case 4:
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vc.ApiIsRead={0}", Param = 1 });
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "vc.ApiStatus={0}", Param = (int)WarehouseStatus.Dealing });
                            break;
                        default:
                            break;
                    }
                }

                DataTable dt = new DataTable();
                dt.Columns.Add(_LanguagePack["ordercancel_index_order_number"]);
                dt.Columns.Add(_LanguagePack["ordercancel_index_sub_order_number"]);
                dt.Columns.Add(_LanguagePack["ordercancel_index_store"]);
                dt.Columns.Add(_LanguagePack["ordercancel_index_customer"]);
                dt.Columns.Add(_LanguagePack["ordercancel_index_receiver"]);
                dt.Columns.Add(_LanguagePack["ordercancel_index_sku"]);
                dt.Columns.Add(_LanguagePack["ordercancel_index_productname"]);
                dt.Columns.Add(_LanguagePack["ordercancel_index_payment_type"]);
                dt.Columns.Add(_LanguagePack["ordercancel_index_quantity"]);
                dt.Columns.Add(_LanguagePack["ordercancel_index_refund_delivery"]);
                dt.Columns.Add(_LanguagePack["ordercancel_index_refund_payment"]);
                dt.Columns.Add(_LanguagePack["ordercancel_index_state"]);
                dt.Columns.Add(_LanguagePack["ordercancel_index_oper_message"]);
                dt.Columns.Add(_LanguagePack["ordercancel_index_stock_reply"]);
                dt.Columns.Add(_LanguagePack["ordercancel_index_confirm_message"]);
                dt.Columns.Add(_LanguagePack["ordercancel_index_refund_message"]);

                //查询
                DataRow _dr = null;
                var _list = db.Fetch<dynamic>("select vc.Id,vc.OrderNo,vc.SubOrderNo,vc.MallName,vc.RefundAmount,vc.RefundPoint,vc.RefundExpress,vc.Remark,vc.AddUserName,vc.ManualUserID,vc.CreateDate,vc.AcceptUserDate,vc.AcceptUserName,vc.RefundUserName,vc.ApiIsRead,vc.Quantity as CancelQuantity,vc.[Status],vc.[Type],vc.IsSystemCancel,vc.ApiReplyDate,vc.ApiReplyMsg,vc.RefundUserDate,vc.RefundRemark,vc.IsDelete,vc.ApiStatus,od.SKU,od.ProductName,od.PaymentType,oe.Receive,isnull(c.Name,'') As CustomerName from View_OrderCancel as vc inner join View_OrderDetail as od on vc.SubOrderNo=od.SubOrderNo inner join OrderReceive as oe on vc.SubOrderNo=oe.SubOrderNo left join Customer as c on od.CustomerNo=c.CustomerNo order by vc.Id desc,vc.ChangeID desc", _SqlWhere);

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
                    _dr[8] = dy.CancelQuantity;
                    //_dr[8] = VariableHelper.FormateMoney(OrderHelper.MathRound(dy.RefundPoint));
                    _dr[9] = VariableHelper.FormateMoney(OrderHelper.MathRound(dy.RefundExpress));
                    _dr[10] = VariableHelper.FormateMoney(OrderHelper.MathRound(dy.RefundAmount + dy.RefundExpress));
                    _dr[11] = OrderHelper.GetProcessStatusDisplay(dy.Status, false);
                    _dr[12] = string.Format("{0} {1}{2}", dy.AddUserName, dy.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"), (!string.IsNullOrEmpty(dy.Remark)) ? " " + dy.Remark : "");
                    _dr[13] = (!dy.IsSystemCancel) ? OrderHelper.GetWarehouseStatusDisplay(dy.ApiIsRead, dy.ApiStatus, false) + ((dy.ApiReplyDate != null) ? " " + dy.ApiReplyDate.ToString("yyyy-MM-dd HH:mm:ss") : "") + ((dy.ApiReplyMsg != null) ? " " + dy.ApiReplyMsg : "") : "";
                    _dr[14] = string.Format("{0}", ((dy.AcceptUserDate != null) ? dy.AcceptUserName + " " + VariableHelper.FormateTime(dy.AcceptUserDate, "yyyy-MM-dd HH:mm:ss") : ""));
                    _dr[15] = string.Format("{0}", ((dy.RefundUserDate != null) ? dy.RefundUserName + " " + VariableHelper.FormateTime(dy.RefundUserDate, "yyyy-MM-dd HH:mm:ss") : ""));
                    dt.Rows.Add(_dr);
                }

                string _filepath = $"~{AppGlobalService.UPLOAD_CACHE_PATH}/{DateTime.Now.ToString("yyyy-MM-dd")}";
                if (!Directory.Exists(Server.MapPath(_filepath)))
                {
                    Directory.CreateDirectory(Server.MapPath(_filepath));
                }
                string _fileName = string.Format("OrderCancel_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss"));
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
