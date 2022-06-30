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
    public class GoodsExchangeController : BaseController
    {
        //
        // GET: /GoodsExchange/

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
            ViewData["proccess_list"] = OrderHelper.ProccessExchangeStatusObject();
            //仓库状态
            ViewData["wh_list"] = OrderHelper.WarehouseStatusReflect();
            //理由
            ViewData["reason_list"] = OrderExchangeProcessService.ReasonReflect();

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult Index_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            List<EntityRepository.SqlQueryCondition> _sqlWhere = new List<EntityRepository.SqlQueryCondition>();
            string _orderid = VariableHelper.SaferequestStr(Request.Form["orderid"]);
            string _time1 = VariableHelper.SaferequestStr(Request.Form["time1"]);
            string _time2 = VariableHelper.SaferequestStr(Request.Form["time2"]);
            string[] _storeid = VariableHelper.SaferequestStringArray(Request.Form["store"]);
            string _customer = VariableHelper.SaferequestStr(Request.Form["customer"]);
            string _paytype = VariableHelper.SaferequestStr(Request.Form["paytype"]);
            int _reason = VariableHelper.SaferequestInt(Request.Form["reason"]);
            int _process_status = VariableHelper.SaferequestInt(Request.Form["proccess_status"]);
            int _stock_status = VariableHelper.SaferequestInt(Request.Form["stock_status"]);
            using (var db = new ebEntities())
            {
                //搜索条件
                if (!string.IsNullOrEmpty(_orderid))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "((ve.OrderNo like {0}) or (ve.SubOrderNo like {0}))", Param = "%" + _orderid + "%" });
                }

                if (!string.IsNullOrEmpty(_time1))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "datediff(day,ve.CreateDate,{0})<=0", Param = VariableHelper.SaferequestTime(_time1) });
                }

                if (!string.IsNullOrEmpty(_time2))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "datediff(day,ve.CreateDate,{0})>=0", Param = VariableHelper.SaferequestTime(_time2) });
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
                _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "ve.MallSapCode in (select item from strToIntTable('" + string.Join(",", _UserMalls) + "',','))", Param = null });

                if (!string.IsNullOrEmpty(_customer))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "(oe.Receive={0} or c.Name={0})", Param = EncryptionBase.EncryptString(_customer) });
                }

                if (!string.IsNullOrEmpty(_paytype))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "o.PaymentType={0}", Param = _paytype });
                }

                if (_reason > -1)
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "ve.Reason={0}", Param = _reason });
                }

                if (_process_status > 0)
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "ve.Status={0}", Param = _process_status });
                }

                if (_stock_status > 0)
                {
                    switch (_stock_status)
                    {
                        case 1:
                            _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "ve.ApiIsRead={0}", Param = 0 });
                            break;
                        case 2:
                            _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "ve.ApiIsRead={0}", Param = 1 });
                            _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "ve.ApiStatus={0}", Param = (int)WarehouseStatus.DealSuccessful });
                            break;
                        case 3:
                            _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "ve.ApiIsRead={0}", Param = 1 });
                            _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "ve.ApiStatus={0}", Param = (int)WarehouseStatus.DealFail });
                            break;
                        case 4:
                            _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "ve.ApiIsRead={0}", Param = 1 });
                            _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "ve.ApiStatus={0}", Param = (int)WarehouseStatus.Dealing });
                            break;
                        default:
                            break;
                    }
                }

                //查询
                var _list = this.BaseEntityRepository.SqlQueryGetPage<ExchangeOrderQuery>(db, "select ve.Id,ve.OrderNo,ve.SubOrderNo,ve.MallName,ve.NewSku,ve.ShippingNo,ve.Status,ve.Quantity,ve.AcceptUserId,ve.AcceptUserName,ve.AcceptUserDate,ve.AcceptRemark,ve.ManualUserID,ve.AddUserName,ve.CreateDate,ve.Remark,ve.ApiIsRead,ve.ApiStatus,ve.ApiReplyDate,ve.ApiReplyMsg,o.PaymentType,isnull((select SKU from OrderDetail where OrderDetail.SubOrderNo=ve.SubOrderNo),'') As SKU,oe.Receive,isnull(c.Name,'')As CustomerName from View_OrderExchange as ve inner join [Order] as o on ve.OrderNo = o.OrderNo inner join OrderReceive as oe on ve.SubOrderNo=oe.SubOrderNo inner join Customer as c on o.CustomerNo=c.CustomerNo order by ve.Id desc", _sqlWhere, VariableHelper.SaferequestInt(Request.Form["page"]), VariableHelper.SaferequestInt(Request.Form["rows"]));
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
                               s7 = dy.NewSku,
                               s8 = dy.Quantity,
                               s9 = dy.ShippingNo,
                               s10 = OrderHelper.GetProcessStatusDisplay(dy.Status, true),
                               s11 = string.Format("{0}<br/>{1}{2}", dy.AddUserName, dy.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"), (!string.IsNullOrEmpty(dy.Remark)) ? "<br/>" + dy.Remark : ""),
                               s12 = OrderHelper.GetWarehouseStatusDisplay(dy.ApiIsRead, dy.ApiStatus, true) + ((dy.ApiReplyDate != null) ? "<br/>" + dy.ApiReplyDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "") + ((dy.ApiReplyMsg != null) ? "<br/>" + dy.ApiReplyMsg : ""),
                               s13 = string.Format("{0}", ((dy.AcceptUserDate != null) ? dy.AcceptUserName + "<br/>" + VariableHelper.FormateTime(dy.AcceptUserDate, "yyyy-MM-dd HH:mm:ss") : ""))
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
                List<OrderDetail> objOrderDetail_List = new List<OrderDetail>();
                Order objOrder = db.Order.Where(p => p.Id == _ID).SingleOrDefault();
                if (objOrder != null)
                {
                    ViewBag.OrderNo = objOrder.OrderNo;
                    objOrderDetail_List = db.OrderDetail.Where(p => p.OrderId == objOrder.Id && !p.IsSetOrigin && !p.IsDelete && !p.IsExchangeNew).ToList();
                    //快递公司
                    ViewData["express_list"] = ExpressCompanyService.GetExpressCompanyObject();
                    //最新收货信息集合
                    List<ReceiveDto> objReceiveDto_List = new List<ReceiveDto>();
                    foreach (var _o in objOrderDetail_List)
                    {
                        objReceiveDto_List.Add(OrderReceiveService.GetNewestReceive(_o.OrderNo, _o.SubOrderNo));
                    }
                    ViewData["receive_list"] = objReceiveDto_List;
                    //理由
                    ViewData["reason_list"] = OrderExchangeProcessService.ReasonReflect();

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
            DateTime _ExchangeTime = VariableHelper.SaferequestTime(Request.Form["ExchangeTime"]);
            string _ShippingCompany = VariableHelper.SaferequestStr(Request.Form["ShippingCompany"]);
            string _ShippingNo = VariableHelper.SaferequestStr(Request.Form["ShippingNo"]);
            int _Reason = VariableHelper.SaferequestInt(Request.Form["Reason"]);
            string _Remark = VariableHelper.SaferequestStr(Request.Form["Remark"]);
            string _Receiver = VariableHelper.SaferequestStr(Request.Form["Receiver"]);
            string _Tel = VariableHelper.SaferequestStr(Request.Form["Tel"]);
            string _Mobile = VariableHelper.SaferequestStr(Request.Form["Mobile"]);
            string _Zipcode = VariableHelper.SaferequestStr(Request.Form["Zipcode"]);
            string _Addr = VariableHelper.SaferequestStr(Request.Form["Addr"]);

            string _SelectIDs = Request.Form["SelectID"];
            string _NewSkus = Request.Form["NewSku"];
            string _Quantitys = Request.Form["Quantity"];

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
                                throw new Exception(_LanguagePack["goodsexchange_edit_message_at_least_one_product"]);
                            }

                            //默认使用Ninja Van
                            ExpressCompany objExpressCompany = ExpressCompanyService.GetExpressCompany(AppGlobalService.DEFAULT_EXPRESS_COMPANY_ID);
                            //读取子订单
                            List<OrderDetail> objOrderDetail_List = db.OrderDetail.Where(p => p.OrderNo == _OrderNo && !p.IsSetOrigin && !p.IsExchangeNew).ToList();
                            OrderDetail objOrderDetail = new OrderDetail();
                            OrderExchange objOrderExchange = new OrderExchange();
                            OrderChangeRecord objOrderChangeRecord = new OrderChangeRecord();
                            OrderLog objOrderLog = new OrderLog();
                            int _OrgStatus = 0;
                            string _New_SubOrderNo = string.Empty;
                            //要换货的子订单
                            string[] _SelectID_Array = _SelectIDs.Split(',');
                            string[] _NewSku_Array = _NewSkus.Split(',');
                            string[] _Quantity_Array = _Quantitys.Split(',');
                            int _Quantity = 0;
                            int _Effect_Quantity = 0;

                            for (int t = 0; t < _SelectIDs.Split(',').Length; t++)
                            {
                                Int64 _id = VariableHelper.SaferequestInt64(_SelectID_Array[t]);
                                _Quantity = VariableHelper.SaferequestPage(_Quantity_Array[t]);
                                objOrderDetail = objOrderDetail_List.Where(p => p.Id == _id).SingleOrDefault();
                                if (objOrderDetail != null)
                                {
                                    //流程ID
                                    string _RequestId = OrderExchangeProcessService.CreateRequestID(objOrderDetail.SubOrderNo);

                                    _New_SubOrderNo = OrderService.CreateExchangeSubOrderNo(objOrderDetail.SubOrderNo);
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
                                        throw new Exception(string.Format("{0}:{1}", objOrderDetail.SubOrderNo, _LanguagePack["goodsexchange_edit_message_state_no_allow"]));
                                    }

                                    //有效数量
                                    _Effect_Quantity = VariableHelper.SaferequestPositiveInt(objOrderDetail.Quantity - objOrderDetail.CancelQuantity - objOrderDetail.ReturnQuantity - objOrderDetail.ExchangeQuantity - objOrderDetail.RejectQuantity);
                                    if (_Quantity > _Effect_Quantity)
                                        _Quantity = _Effect_Quantity;
                                    //退货数量需要大于零
                                    if (_Quantity <= 0)
                                    {
                                        throw new Exception(string.Format("{0}:{1}", objOrderDetail.SubOrderNo, _LanguagePack["goodsexchange_edit_message_quantity_error"]));
                                    }
                                    //添加换货记录
                                    objOrderExchange = new OrderExchange()
                                    {
                                        OrderNo = objOrder.OrderNo,
                                        MallSapCode = objOrder.MallSapCode,
                                        UserId = this.CurrentLoginUser.Userid,
                                        AddDate = DateTime.Now,
                                        CreateDate = _ExchangeTime,
                                        Reason = _Reason,
                                        Remark = _Remark,
                                        SubOrderNo = objOrderDetail.SubOrderNo,
                                        NewSKU = _NewSku_Array[t],
                                        Quantity = _Quantity,
                                        DifferenceAmount = 0,
                                        ExpressAmount = 0,
                                        FromApi = false,
                                        Status = (int)ProcessStatus.Exchange,
                                        ShippingCompany = string.Empty,
                                        ShippingNo = string.Empty,
                                        RequestId = _RequestId,
                                        CustomerName = _Receiver,
                                        Addr = _Addr,
                                        Tel = _Tel,
                                        Mobile = _Mobile,
                                        Zipcode = _Zipcode,
                                        AcceptUserId = 0,
                                        AcceptUserDate = null,
                                        AcceptRemark = string.Empty,
                                        SendUserId = 0,
                                        SendUserDate = null,
                                        SendRemark = string.Empty,
                                        ManualUserId = 0,
                                        ManualUserDate = null,
                                        ManualRemark = string.Empty
                                    };
                                    //数据加密
                                    EncryptionFactory.Create(objOrderExchange).Encrypt();
                                    db.OrderExchange.Add(objOrderExchange);
                                    db.SaveChanges();
                                    //插入api表
                                    objOrderChangeRecord = new OrderChangeRecord()
                                    {
                                        OrderNo = objOrderDetail.OrderNo,
                                        SubOrderNo = objOrderDetail.SubOrderNo,
                                        Type = (int)OrderChangeType.Exchange,
                                        DetailTableName = OrderExchangeProcessService.TableName,
                                        DetailId = objOrderExchange.Id,
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
                                    //修改子订单换货数量
                                    objOrderDetail.ExchangeQuantity = ((objOrderDetail.ExchangeQuantity + _Quantity) >= objOrderDetail.Quantity) ? objOrderDetail.Quantity : (objOrderDetail.ExchangeQuantity + _Quantity);
                                    //修改子订单状态
                                    objOrderDetail.Status = (int)ProductStatus.Exchange;
                                    objOrderDetail.EditDate = DateTime.Now;
                                    //添加子订单log
                                    objOrderLog = new OrderLog()
                                    {
                                        Msg = "Goods exchange processing",
                                        OrderNo = objOrderDetail.OrderNo,
                                        SubOrderNo = objOrderDetail.SubOrderNo,
                                        UserId = this.CurrentLoginUser.Userid,
                                        OriginStatus = _OrgStatus,
                                        NewStatus = (int)ProductStatus.Exchange,
                                        CreateDate = DateTime.Now
                                    };
                                    db.OrderLog.Add(objOrderLog);
                                    db.SaveChanges();
                                }
                                else
                                {
                                    throw new Exception(string.Format("{0}:{1}", objOrderDetail.SubOrderNo, _LanguagePack["common_data_no_exsit"]));
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
                            throw new Exception(_LanguagePack["goodsexchange_edit_message_no_order"]);
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
                View_OrderExchange objView_OrderExchange = db.View_OrderExchange.Where(p => p.Id == _ID).SingleOrDefault();
                if (objView_OrderExchange != null)
                {
                    if (objView_OrderExchange.Status == (int)ProcessStatus.Exchange)
                    {
                        //数据解密
                        EncryptionFactory.Create(objView_OrderExchange).Decrypt();

                        ViewData["order_detail"] = db.OrderDetail.Where(p => p.SubOrderNo == objView_OrderExchange.SubOrderNo).SingleOrDefault();
                        //快递公司
                        ViewData["express_list"] = ExpressCompanyService.GetExpressCompanyObject();
                        //理由
                        ViewData["reason_list"] = OrderExchangeProcessService.ReasonReflect();

                        return View(objView_OrderExchange);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.Other, Message = _LanguagePack["goodsexchange_edit_message_state_not_allow_edit"] });
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
            int _ID = VariableHelper.SaferequestInt(Request.Form["ID"]);
            string _NewSku = VariableHelper.SaferequestStr(Request.Form["NewSku"]);
            int _Quantity = VariableHelper.SaferequestInt(Request.Form["Quantity"]);
            string _ShippingCompany = VariableHelper.SaferequestStr(Request.Form["ShippingCompany"]);
            string _ShippingNo = VariableHelper.SaferequestStr(Request.Form["ShippingNo"]);
            string _Remark = VariableHelper.SaferequestStr(Request.Form["Remark"]);
            int _Reason = VariableHelper.SaferequestInt(Request.Form["Reason"]);
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
                        OrderExchange objOrderExchange = db.OrderExchange.Where(p => p.Id == _ID).SingleOrDefault();
                        if (objOrderExchange != null)
                        {
                            OrderDetail objOrderDetail = db.OrderDetail.Where(p => p.SubOrderNo == objOrderExchange.SubOrderNo).SingleOrDefault();
                            if (objOrderDetail != null)
                            {
                                //计算有效数量
                                int _Effect_Quantity = objOrderDetail.Quantity - objOrderDetail.CancelQuantity - objOrderDetail.ReturnQuantity - objOrderDetail.ExchangeQuantity - objOrderDetail.RejectQuantity + objOrderExchange.Quantity;
                                if (_Quantity > _Effect_Quantity)
                                    _Quantity = _Effect_Quantity;
                                //换货数量需要大于零
                                if (_Quantity == 0)
                                {
                                    throw new Exception(string.Format("{0}:{1}", objOrderExchange.SubOrderNo, _LanguagePack["goodsexchange_edit_message_quantity_error"]));
                                }
                                else
                                {
                                    //计算剩余可换数量
                                    if (_Quantity > _Effect_Quantity)
                                    {
                                        throw new Exception(string.Format("{0}:{1}", objOrderDetail.SubOrderNo, _LanguagePack["goodsexchange_edit_message_not_enough_quantity"]));
                                    }
                                }

                                if (objOrderExchange.Status == (int)ProcessStatus.Exchange)
                                {
                                    //更新换货信息
                                    objOrderExchange.NewSKU = _NewSku;
                                    objOrderExchange.Quantity = _Quantity;
                                    objOrderExchange.ShippingCompany = _ShippingCompany;
                                    objOrderExchange.ShippingNo = _ShippingNo;
                                    objOrderExchange.CustomerName = _Receiver;
                                    objOrderExchange.Tel = _Tel;
                                    objOrderExchange.Mobile = _Mobile;
                                    objOrderExchange.Zipcode = _Zipcode;
                                    objOrderExchange.Addr = _Addr;
                                    objOrderExchange.Reason = _Reason;
                                    objOrderExchange.Remark = _Remark;
                                    //数据加密
                                    EncryptionFactory.Create(objOrderExchange).Encrypt();
                                    //更新换货数量
                                    objOrderDetail.ExchangeQuantity += _Quantity - objOrderExchange.Quantity;
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
                                    throw new Exception(_LanguagePack["goodsexchange_edit_message_state_not_allow_edit"]);
                                }
                            }
                            else
                            {
                                throw new Exception(_LanguagePack["common_data_no_exsit"]);
                            }
                        }
                        else
                        {
                            throw new Exception(_LanguagePack["goodsexchange_edit_message_no_message"]);
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
                            _r = OrderExchangeProcessService.Delete(VariableHelper.SaferequestInt64(_str), db);
                            if (!Convert.ToBoolean(_r[0]))
                            {
                                throw new Exception(_r[1].ToString());
                            }
                        }
                        Trans.Commit();
                        //添加日志
                        AppLogService.DeleteLog("OrderExchange", _IDs);
                        //返回信息
                        _result.Data = new
                        {
                            result = true,
                            msg = _LanguagePack["common_data_delete_success"]
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
                    var objOrderExchange_List = db.Database.SqlQuery<OrderExchange>("select * from OrderExchange where Id in (" + _IDs + ")").ToList();
                    if (objOrderExchange_List.Count > 0)
                    {
                        ViewBag.IDs = string.Join(",", objOrderExchange_List.Select(p => p.Id).ToList());
                        foreach (OrderExchange objOrderExchange in objOrderExchange_List)
                        {
                            if (objOrderExchange.Status != (int)ProcessStatus.ExchangeFail)
                            {
                                return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.Other, Message = string.Format("{0}:{1}", objOrderExchange.SubOrderNo, _LanguagePack["goodsexchange_edit_message_error_state"]) });
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
                            _r = OrderExchangeProcessService.ManualInterference(VariableHelper.SaferequestInt64(_str), (_Result == 1), _Remark, db);
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
                var objView_OrderExchange = db.View_OrderExchange.Where(p => p.Id == _ID).SingleOrDefault();
                if (objView_OrderExchange != null)
                {
                    //数据解密
                    EncryptionFactory.Create(objView_OrderExchange).Decrypt();

                    ViewData["order_detail"] = db.OrderDetail.Where(p => p.SubOrderNo == objView_OrderExchange.SubOrderNo).SingleOrDefault();

                    return View(objView_OrderExchange);
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
            List<EntityRepository.SqlQueryCondition> _sqlWhere = new List<EntityRepository.SqlQueryCondition>();
            string _orderid = VariableHelper.SaferequestStr(Request.Form["OrderNumber"]);
            string _time1 = VariableHelper.SaferequestStr(Request.Form["Time1"]);
            string _time2 = VariableHelper.SaferequestStr(Request.Form["Time2"]);
            string[] _storeid = VariableHelper.SaferequestStringArray(Request.Form["StoreName"]);
            string _customer = VariableHelper.SaferequestStr(Request.Form["Customer"]);
            string _paytype = VariableHelper.SaferequestStr(Request.Form["PaymentStatus"]);
            int _reason = VariableHelper.SaferequestInt(Request.Form["Reason"]);
            int _process_status = VariableHelper.SaferequestInt(Request.Form["ProcessStatus"]);
            int _stock_status = VariableHelper.SaferequestInt(Request.Form["StockStatus"]);
            using (var db = new ebEntities())
            {
                //搜索条件
                if (!string.IsNullOrEmpty(_orderid))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "((ve.OrderNo like {0}) or (ve.SubOrderNo like {0}))", Param = "%" + _orderid + "%" });
                }

                if (!string.IsNullOrEmpty(_time1))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "datediff(day,ve.CreateDate,{0})<=0", Param = VariableHelper.SaferequestTime(_time1) });
                }

                if (!string.IsNullOrEmpty(_time2))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "datediff(day,ve.CreateDate,{0})>=0", Param = VariableHelper.SaferequestTime(_time2) });
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
                _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "ve.MallSapCode in (select item from strToIntTable('" + string.Join(",", _UserMalls) + "',','))", Param = null });

                if (!string.IsNullOrEmpty(_customer))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "(oe.Receive={0} or c.Name={0})", Param = EncryptionBase.EncryptString(_customer) });
                }

                if (!string.IsNullOrEmpty(_paytype))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "o.PaymentType={0}", Param = _paytype });
                }

                if (_reason > -1)
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "ve.Reason={0}", Param = _reason });
                }

                if (_process_status > 0)
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "ve.Status={0}", Param = _process_status });
                }

                if (_stock_status > 0)
                {
                    switch (_stock_status)
                    {
                        case 1:
                            _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "ve.ApiIsRead={0}", Param = 0 });
                            break;
                        case 2:
                            _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "ve.ApiIsRead={0}", Param = 1 });
                            _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "ve.ApiStatus={0}", Param = (int)WarehouseStatus.DealSuccessful });
                            break;
                        case 3:
                            _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "ve.ApiIsRead={0}", Param = 1 });
                            _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "ve.ApiStatus={0}", Param = (int)WarehouseStatus.DealFail });
                            break;
                        case 4:
                            _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "ve.ApiIsRead={0}", Param = 1 });
                            _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "ve.ApiStatus={0}", Param = (int)WarehouseStatus.Dealing });
                            break;
                        default:
                            break;
                    }
                }

                DataTable dt = new DataTable();
                dt.Columns.Add(_LanguagePack["goodsexchange_index_order_number"]);
                dt.Columns.Add(_LanguagePack["goodsexchange_index_sub_order_number"]);
                dt.Columns.Add(_LanguagePack["goodsexchange_index_store"]);
                dt.Columns.Add(_LanguagePack["goodsexchange_index_customer"]);
                dt.Columns.Add(_LanguagePack["goodsexchange_index_receiver"]);
                dt.Columns.Add(_LanguagePack["goodsexchange_index_sku"]);
                dt.Columns.Add(_LanguagePack["goodsexchange_index_quantity"]);
                dt.Columns.Add(_LanguagePack["goodsexchange_index_new_sku"]);
                dt.Columns.Add(_LanguagePack["goodsexchange_index_logistics_message"]);
                dt.Columns.Add(_LanguagePack["goodsexchange_index_state"]);
                dt.Columns.Add(_LanguagePack["goodsexchange_index_oper_message"]);
                dt.Columns.Add(_LanguagePack["goodsexchange_index_stock_reply"]);
                dt.Columns.Add(_LanguagePack["goodsexchange_index_accept_message"]);

                //查询
                DataRow _dr = null;
                var _list = this.BaseEntityRepository.SqlQueryGetList<ExchangeOrderQuery>(db, "select ve.Id,ve.OrderNo,ve.SubOrderNo,ve.MallName,ve.NewSku,ve.ShippingNo,ve.Status,ve.Quantity,ve.AcceptUserId,ve.AcceptUserName,ve.AcceptUserDate,ve.AcceptRemark,ve.ManualUserID,ve.AddUserName,ve.CreateDate,ve.Remark,ve.ApiIsRead,ve.ApiStatus,ve.ApiReplyDate,ve.ApiReplyMsg,o.PaymentType,isnull((select SKU from OrderDetail where OrderDetail.SubOrderNo=ve.SubOrderNo),'') As SKU,oe.Receive,isnull(c.Name,'')As CustomerName from View_OrderExchange as ve inner join [Order] as o on ve.OrderNo = o.OrderNo inner join OrderReceive as oe on ve.SubOrderNo=oe.SubOrderNo inner join Customer as c on o.CustomerNo=c.CustomerNo order by ve.Id desc", _sqlWhere);

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
                    _dr[6] = dy.NewSku;
                    _dr[7] = dy.Quantity;
                    _dr[8] = dy.ShippingNo;
                    _dr[9] = OrderHelper.GetProcessStatusDisplay(dy.Status, false);
                    _dr[10] = string.Format("{0} {1}{2}", dy.AddUserName, dy.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"), (!string.IsNullOrEmpty(dy.Remark)) ? " " + dy.Remark : "");
                    _dr[11] = OrderHelper.GetWarehouseStatusDisplay(dy.ApiIsRead, dy.ApiStatus, false) + ((dy.ApiReplyDate != null) ? " " + dy.ApiReplyDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "") + ((dy.ApiReplyMsg != null) ? " " + dy.ApiReplyMsg : "");
                    _dr[12] = string.Format("{0}", ((dy.AcceptUserDate != null) ? dy.AcceptUserName + " " + VariableHelper.FormateTime(dy.AcceptUserDate, "yyyy-MM-dd HH:mm:ss") : ""));
                    dt.Rows.Add(_dr);
                }

                string _filepath = $"~{AppGlobalService.UPLOAD_CACHE_PATH}/{DateTime.Now.ToString("yyyy-MM-dd")}";
                if (!Directory.Exists(Server.MapPath(_filepath)))
                {
                    Directory.CreateDirectory(Server.MapPath(_filepath));
                }
                string _fileName = string.Format("OrderExchange_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss"));
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
