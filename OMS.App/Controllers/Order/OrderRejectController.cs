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
using Samsonite.Utility.Common;

using OMS.App.Helper;

namespace OMS.App.Controllers
{
    public class OrderRejectController : BaseController
    {
        //
        // GET: /OrderReject/

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
            //快速时间选项
            ViewData["quicktime_list"] = QuickTimeHelper.QuickTimeOption();
            //流程状态
            ViewData["proccess_list"] = OrderHelper.ProccessRejectStatusObject();

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
            int _process_status = VariableHelper.SaferequestInt(Request.Form["proccess_status"]);
            using (var db = new ebEntities())
            {
                //搜索条件
                if (!string.IsNullOrEmpty(_orderid))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "((vr.OrderNo like {0}) or (vr.SubOrderNo like {0}))", Param = "%" + _orderid + "%" });
                }

                if (!string.IsNullOrEmpty(_time1))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "datediff(day,vr.CreateDate,{0})<=0", Param = VariableHelper.SaferequestTime(_time1) });
                }

                if (!string.IsNullOrEmpty(_time2))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "datediff(day,vr.CreateDate,{0})>=0", Param = VariableHelper.SaferequestTime(_time2) });
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
                _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "vr.MallSapCode in (select item from strToIntTable('" + string.Join(",", _UserMalls) + "',','))", Param = null });

                if (!string.IsNullOrEmpty(_customer))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "(oe.Receive={0} or c.Name={0})", Param = EncryptionBase.EncryptString(_customer) });
                }

                if (_process_status > 0)
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "vr.Status={0}", Param = _process_status });
                }

                //查询
                var _list = this.BaseEntityRepository.SqlQueryGetPage<RejectOrderQuery>(db, "select vr.Id,vr.OrderNo,vr.SubOrderNo,vr.MallName,vr.Quantity,vr.Status,vr.AddUserName,vr.Remark,vr.AcceptUserName,vr.AcceptUserDate,vr.AcceptRemark,vr.CreateDate,vr.Quantity as RejectQuantity,od.SKU,od.ProductName,od.PaymentType,oe.Receive,isnull(c.Name,'')As CustomerName from View_OrderReject as vr inner join View_OrderDetail as od on vr.SubOrderNo=od.SubOrderNo inner join OrderReceive as oe on vr.SubOrderNo=oe.SubOrderNo inner join Customer as c on od.CustomerNo=c.CustomerNo order by vr.Id desc", _sqlWhere, VariableHelper.SaferequestInt(Request.Form["page"]), VariableHelper.SaferequestInt(Request.Form["rows"]));
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
                               s1 = string.Format("<a href=\"javascript:void(0);\" onclick=\"artDialogExtend.Dialog.Open('" + Url.Action("Detail", "OrderQuery") + "?ID={0}',{{title:'{0}',width:'100%',height:'100%'}});\">{0}</a>", dy.OrderNo),
                               s2 = dy.SubOrderNo,
                               s3 = dy.MallName,
                               s4 = dy.CustomerName,
                               s5 = dy.Receive,
                               s6 = dy.SKU,
                               s7 = dy.ProductName,
                               s8 = dy.RejectQuantity,
                               s9 = OrderHelper.GetProcessStatusDisplay(dy.Status, true),
                               s10 = string.Format("{0}<br/>{1}{2}", dy.AddUserName, dy.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"), (!string.IsNullOrEmpty(dy.Remark)) ? "<br/>" + dy.Remark : ""),
                               s11 = string.Format("{0}", ((dy.AcceptUserDate != null) ? dy.AcceptUserName + "<br/>" + VariableHelper.FormateTime(dy.AcceptUserDate, "yyyy-MM-dd HH:mm:ss") : ""))
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
            DateTime _RejectTime = VariableHelper.SaferequestTime(Request.Form["CancelTime"]);
            string _Remark = VariableHelper.SaferequestStr(Request.Form["Remark"]);

            string _SelectIDs = Request.Form["SelectID"];
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
                                throw new Exception(_LanguagePack["orderreject_edit_message_at_least_one_product"]);
                            }

                            //读取子订单
                            List<OrderDetail> objOrderDetail_List = db.OrderDetail.Where(p => p.OrderNo == _OrderNo && !p.IsSetOrigin && !p.IsExchangeNew).ToList();
                            OrderDetail objOrderDetail = new OrderDetail();
                            OrderReject objOrderReject = new OrderReject();
                            OrderLog objOrderLog = new OrderLog();
                            int _OrgStatus = 0;
                            //要取消的子订单
                            string[] _SelectID_Array = _SelectIDs.Split(',');
                            string[] _Quantity_Array = _Quantitys.Split(',');
                            int _Select_Length = _SelectID_Array.Length;
                            int _Quantity = 0;
                            int _Effect_Quantity = 0;
                            //只能对整个订单进行拒收
                            if (_SelectID_Array.Count() < objOrderDetail_List.Count)
                            {
                                throw new Exception(_LanguagePack["orderreject_edit_message_reject_rule"]);
                            }
                            for (int t = 0; t < _Select_Length; t++)
                            {
                                Int64 _id = VariableHelper.SaferequestInt64(_SelectID_Array[t]);
                                _Quantity = VariableHelper.SaferequestInt(_Quantity_Array[t]);
                                objOrderDetail = objOrderDetail_List.Where(p => p.Id == _id).SingleOrDefault();
                                if (objOrderDetail != null)
                                {
                                    //流程ID
                                    string _RequestId = OrderRejectProcessService.CreateRequestID(objOrderDetail.SubOrderNo);

                                    _OrgStatus = objOrderDetail.Status;
                                    //是否无效信息
                                    if (objOrderDetail.IsDelete || objOrderDetail.IsError)
                                    {
                                        throw new Exception(string.Format("{0}:{1}", objOrderDetail.SubOrderNo, _LanguagePack["common_data_no_avail"]));
                                    }

                                    //货到付款订单才能进行拒收
                                    if (objOrder.PaymentType != (int)PayType.CashOnDelivery)
                                    {
                                        throw new Exception(string.Format("{0}:{1}", objOrderDetail.SubOrderNo, _LanguagePack["orderreject_edit_message_cod_no_allow"]));
                                    }

                                    //判断在发货前和待处理之后才能进行拒收
                                    List<int> objAllowStatus = new List<int>() { (int)ProductStatus.Received, (int)ProductStatus.Processing };
                                    if (!objAllowStatus.Contains(objOrderDetail.Status))
                                    {
                                        throw new Exception(string.Format("{0}:{1}", objOrderDetail.SubOrderNo, _LanguagePack["orderreject_edit_message_state_no_allow"]));
                                    }

                                    //查询是否已经存在未删除的拒收记录,如果存在则不再重复插入
                                    View_OrderReject objView_OrderReject = db.View_OrderReject.Where(p => p.SubOrderNo == objOrderDetail.SubOrderNo && p.Status != (int)ProcessStatus.Delete).FirstOrDefault();
                                    if (objView_OrderReject != null)
                                    {
                                        throw new Exception(string.Format("{0}:{1}", objOrderDetail.SubOrderNo, _LanguagePack["orderreject_edit_message_exist_process"]));
                                    }

                                    //计算有效数量
                                    _Effect_Quantity = VariableHelper.SaferequestPositiveInt(objOrderDetail.Quantity - objOrderDetail.CancelQuantity - objOrderDetail.ReturnQuantity - objOrderDetail.ExchangeQuantity - objOrderDetail.RejectQuantity);
                                    if (_Quantity > _Effect_Quantity)
                                        _Quantity = _Effect_Quantity;
                                    //拒收数量需要大于零
                                    if (_Quantity <= 0)
                                    {
                                        throw new Exception(string.Format("{0}:{1}", objOrderDetail.SubOrderNo, _LanguagePack["orderreject_edit_message_quantity_error"]));
                                    }
                                    //拒收流程表
                                    objOrderReject = new OrderReject()
                                    {
                                        OrderNo = objOrderDetail.OrderNo,
                                        MallSapCode = objOrder.MallSapCode,
                                        UserId = this.CurrentLoginUser.Userid,
                                        AddDate = DateTime.Now,
                                        CreateDate = _RejectTime,
                                        Reason = 0,
                                        Remark = _Remark,
                                        AcceptUserId = this.CurrentLoginUser.Userid,
                                        AcceptUserDate = DateTime.Now,
                                        AcceptRemark = string.Empty,
                                        FromApi = false,
                                        SubOrderNo = objOrderDetail.SubOrderNo,
                                        Quantity = _Quantity,
                                        Status = (int)ProcessStatus.Reject,
                                        RequestId = _RequestId,
                                        IsSystemReject = false,
                                    };
                                    db.OrderReject.Add(objOrderReject);
                                    db.SaveChanges();
                                    //修改子订单拒收数量
                                    objOrderDetail.RejectQuantity = ((objOrderDetail.RejectQuantity + _Quantity) >= objOrderDetail.Quantity) ? objOrderDetail.Quantity : (objOrderDetail.RejectQuantity + _Quantity);
                                    //修改子订单状态
                                    objOrderDetail.Status = (int)ProductStatus.Reject;
                                    objOrderDetail.EditDate = DateTime.Now;
                                    //添加子订单log
                                    objOrderLog = new OrderLog()
                                    {
                                        OrderNo = objOrderDetail.OrderNo,
                                        SubOrderNo = objOrderDetail.SubOrderNo,
                                        UserId = this.CurrentLoginUser.Userid,
                                        OriginStatus = _OrgStatus,
                                        NewStatus = (int)ProductStatus.Reject,
                                        Msg = "Reject Processing",
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
                            throw new Exception(_LanguagePack["orderreject_edit_message_no_order"]);
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

        #region 详情
        [UserPowerAuthorize]
        public ActionResult Detail()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;

            Int64 _ID = VariableHelper.SaferequestInt64(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                var objView_OrderReject = db.View_OrderReject.Where(p => p.Id == _ID).SingleOrDefault();
                if (objView_OrderReject != null)
                {
                    ViewData["order_detail"] = db.View_OrderDetail.Where(p => p.SubOrderNo == objView_OrderReject.SubOrderNo).SingleOrDefault();

                    return View(objView_OrderReject);
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
            int _process_status = VariableHelper.SaferequestInt(Request.Form["ProcessStatus"]);
            using (var db = new ebEntities())
            {
                //搜索条件
                if (!string.IsNullOrEmpty(_orderid))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "((vr.OrderNo like {0}) or (vr.SubOrderNo like {0}))", Param = "%" + _orderid + "%" });
                }

                if (!string.IsNullOrEmpty(_time1))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "datediff(day,vr.CreateDate,{0})<=0", Param = VariableHelper.SaferequestTime(_time1) });
                }

                if (!string.IsNullOrEmpty(_time2))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "datediff(day,vr.CreateDate,{0})>=0", Param = VariableHelper.SaferequestTime(_time2) });
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
                _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "vr.MallSapCode in (select item from strToIntTable('" + string.Join(",", _UserMalls) + "',','))", Param = null });

                if (!string.IsNullOrEmpty(_customer))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "(oe.Receive={0} or c.Name={0})", Param = EncryptionBase.EncryptString(_customer) });
                }

                if (_process_status > 0)
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "vr.Status={0}", Param = _process_status });
                }

                DataTable dt = new DataTable();
                dt.Columns.Add(_LanguagePack["orderreject_index_order_number"]);
                dt.Columns.Add(_LanguagePack["orderreject_index_sub_order_number"]);
                dt.Columns.Add(_LanguagePack["orderreject_index_store"]);
                dt.Columns.Add(_LanguagePack["orderreject_index_customer"]);
                dt.Columns.Add(_LanguagePack["orderreject_index_receiver"]);
                dt.Columns.Add(_LanguagePack["orderreject_index_sku"]);
                dt.Columns.Add(_LanguagePack["orderreject_index_productname"]);
                dt.Columns.Add(_LanguagePack["orderreject_index_quantity"]);
                dt.Columns.Add(_LanguagePack["orderreject_index_state"]);
                dt.Columns.Add(_LanguagePack["orderreject_index_oper_message"]);
                dt.Columns.Add(_LanguagePack["orderreject_index_confirm_message"]);

                //查询
                DataRow _dr = null;
                var _list = this.BaseEntityRepository.SqlQueryGetList<RejectOrderQuery>(db, "select vr.Id,vr.OrderNo,vr.MallName,vr.SubOrderNo,vr.Quantity,vr.Status,vr.AddUserName,vr.Remark,vr.AcceptUserName,vr.AcceptUserDate,vr.AcceptRemark,vr.CreateDate,vr.Quantity as RejectQuantity,od.SKU,od.ProductName,od.PaymentType,oe.Receive,isnull(c.Name,'')As CustomerName from View_OrderReject as vr inner join View_OrderDetail as od on vr.SubOrderNo=od.SubOrderNo inner join OrderReceive as oe on vr.SubOrderNo=oe.SubOrderNo left join Customer as c on od.CustomerNo=c.CustomerNo order by vr.Id desc", _sqlWhere);
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
                    _dr[7] = dy.RejectQuantity;
                    _dr[8] = OrderHelper.GetProcessStatusDisplay(dy.Status, false);
                    _dr[9] = string.Format("{0} {1}{2}", dy.AddUserName, dy.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"), (!string.IsNullOrEmpty(dy.Remark)) ? " " + dy.Remark : "");
                    _dr[10] = string.Format("{0}", ((dy.AcceptUserDate != null) ? dy.AcceptUserName + " " + VariableHelper.FormateTime(dy.AcceptUserDate, "yyyy-MM-dd HH:mm:ss") : ""));
                    dt.Rows.Add(_dr);
                }

                string _filepath = $"~{AppGlobalService.UPLOAD_CACHE_PATH}/{DateTime.Now.ToString("yyyy-MM-dd")}";
                if (!Directory.Exists(Server.MapPath(_filepath)))
                {
                    Directory.CreateDirectory(Server.MapPath(_filepath));
                }
                string _fileName = string.Format("OrderReject_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss"));
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
