using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.Utility.Common;
using Samsonite.OMS.Service;
using Samsonite.OMS.Encryption;
using OMS.App.Helper;

namespace OMS.App.Controllers
{
    public class OrderReserveController : BaseController
    {
        //
        // GET: /OrderReserve/

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
            //订单状态
            ViewData["productstate_list"] = OrderHelper.ProductStatusObject();
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
            List<EntityRepository.SqlQueryCondition> _sqlWhere = new List<EntityRepository.SqlQueryCondition>();
            string _orderid = VariableHelper.SaferequestStr(Request.Form["orderid"]);
            string[] _storeid = VariableHelper.SaferequestStringArray(Request.Form["store"]);
            string _time1 = VariableHelper.SaferequestStr(Request.Form["time1"]);
            string _time2 = VariableHelper.SaferequestStr(Request.Form["time2"]);
            string _product_status = VariableHelper.SaferequestStr(Request.Form["product_status"]);
            using (var db = new ebEntities())
            {
                //搜索条件
                if (!string.IsNullOrEmpty(_orderid))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "((od.OrderNo like {0}) or (od.SubOrderNo like {0}))", Param = "%" + _orderid + "%" });
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
                _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "o.MallSapCode in (select item from strToIntTable('" + string.Join(",", _UserMalls) + "',','))", Param = null });

                if (!string.IsNullOrEmpty(_time1))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "datediff(day,od.ReservationDate,{0})<=0", Param = VariableHelper.SaferequestTime(_time1) });
                }

                if (!string.IsNullOrEmpty(_time2))
                {
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "datediff(day,od.ReservationDate,{0})>=0", Param = VariableHelper.SaferequestTime(_time2) });
                }

                if (!string.IsNullOrEmpty(_product_status))
                {
                    if (("," + _product_status + ",").IndexOf("," + ((int)ProductStatus.Cancel).ToString() + ",") > -1)
                    {
                        _product_status += "," + ((int)ProductStatus.CancelComplete).ToString();
                    }
                    if (("," + _product_status + ",").IndexOf("," + ((int)ProductStatus.Return).ToString() + ",") > -1)
                    {
                        _product_status += "," + ((int)ProductStatus.ReturnComplete).ToString();
                    }
                    if (("," + _product_status + ",").IndexOf("," + ((int)ProductStatus.Exchange).ToString() + ",") > -1)
                    {
                        _product_status += "," + ((int)ProductStatus.ExchangeComplete).ToString();
                    }
                    _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "od.[Status] in (" + _product_status + ")", Param = null });
                }

                //预售订单
                _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "od.IsReservation={0}", Param = 1 });
                //过滤套装主订单
                _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "od.IsSetOrigin={0}", Param = 0 });
                //不显示无效的订单
                _sqlWhere.Add(new EntityRepository.SqlQueryCondition() { Condition = "od.IsDelete={0}", Param = 0 });
                //查询
                var _list = this.BaseEntityRepository.SqlQueryGetPage<ReserveOrderQuery>(db, "select od.Id,od.OrderNo,od.SubOrderNo,o.MallSapCode,o.MallName,o.PaymentDate,o.CreateDate,od.SKU,od.ProductName,od.Quantity,od.Status,od.SellingPrice,od.PaymentAmount,od.ActualPaymentAmount,od.IsReservation,od.ReservationDate,od.ReservationRemark,od.ShippingStatus,od.IsError,isnull((select Name from Customer where Customer.CustomerNo=o.CustomerNo),'')As CustomerName,r.[Receive],r.ReceiveTel,r.ReceiveCel,r.ReceiveAddr from OrderDetail as od inner join [Order] as o on od.OrderNo=o.OrderNo inner join OrderReceive as r on r.SubOrderNo =od.SubOrderNo order by od.Id desc", _sqlWhere, VariableHelper.SaferequestInt(Request.Form["page"]), VariableHelper.SaferequestInt(Request.Form["rows"]));
                //数据解密并脱敏
                foreach (var item in _list.Items)
                {
                    EncryptionFactory.Create(item, new string[] { "CustomerName", "Receive", "ReceiveTel", "ReceiveAddr" }).HideSensitive();
                }
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.Id,
                               s1 = string.Format("{2}<a href=\"javascript:void(0);\" onclick=\"artDialogExtend.Dialog.Open('" + Url.Action("Detail", "OrderQuery") + "?ID={0}',{{title:'{1}-{0}',width:'100%',height:'100%'}});\">{0}</a>", dy.OrderNo, _LanguagePack["orderquery_detail_title"], (dy.IsError) ? "<i class=\"fa fa-exclamation-circle color_warning\"></i>" : ""),
                               s2 = dy.SubOrderNo,
                               s3 = dy.MallName,
                               s4 = dy.SKU,
                               s5 = dy.ProductName,
                               s6 = VariableHelper.FormateMoney(dy.SellingPrice),
                               s7 = dy.Quantity,
                               s8 = VariableHelper.FormateMoney(dy.PaymentAmount),
                               s9 = OrderHelper.GetProductStatusDisplay(dy.Status, true),
                               s10 = dy.CustomerName,
                               s11 = dy.Receive,
                               s12 = dy.ReceiveTel,
                               s13 = dy.ReceiveAddr,
                               s14 = VariableHelper.FormateTime(dy.ReservationDate, "yyyy-MM-dd"),
                               s15 = OrderHelper.GetWarehouseProcessStatusDisplay(dy.ShippingStatus, true),
                               s16 = VariableHelper.FormateTime(dy.PaymentDate, "yyyy-MM-dd HH:mm:ss"),
                               s17 = VariableHelper.FormateTime(dy.CreateDate, "yyyy-MM-dd HH:mm:ss"),
                               s18 = dy.ReservationRemark
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

            using (var db = new ebEntities())
            {
                string _IDs = VariableHelper.SaferequestStr(Request.QueryString["ID"]);
                if (string.IsNullOrEmpty(_IDs))
                {
                    return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.Other, Message = _LanguagePack["common_data_need_one"] });
                }
                else
                {
                    var _IdArrays = VariableHelper.SaferequestInt64Array(_IDs);
                    var objOrderDetail_List = db.OrderDetail.Where(p => _IdArrays.Contains(p.Id) && p.IsReservation).ToList();
                    if (objOrderDetail_List.Count > 0)
                    {
                        ViewBag.IDs = string.Join(",", objOrderDetail_List.Select(p => p.Id).ToList());
                        ViewBag.ReservationDate = objOrderDetail_List.FirstOrDefault().ReservationDate;

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
        public JsonResult Edit_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            string _IDs = VariableHelper.SaferequestStr(Request.Form["ID"]);
            DateTime _ReservationDate = VariableHelper.SaferequestTime(Request.Form["ReservationDate"]);
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
                        OrderDetail objOrderDetail = new OrderDetail();
                        foreach (string _str in _IDs_Array)
                        {
                            Int64 _ID = VariableHelper.SaferequestInt64(_str);
                            //防止解析出现问题,设定修改时间只能是晚于今天
                            if (DateTime.Compare(_ReservationDate, DateTime.Now) <= 0)
                            {
                                throw new Exception(_LanguagePack["orderbooking_edit_message_error_date"]);
                            }

                            objOrderDetail = db.OrderDetail.Where(p => p.Id == _ID && p.IsReservation).SingleOrDefault();
                            if (objOrderDetail != null)
                            {
                                //是否已经被生成记录传递给wms
                                if (objOrderDetail.Status > (int)ProductStatus.InDelivery)
                                {
                                    throw new Exception(string.Format("{0}:{1}", objOrderDetail.SubOrderNo, _LanguagePack["orderbooking_edit_message_not_allow_edit"]));
                                }
                                else
                                {
                                    objOrderDetail.ReservationDate = _ReservationDate;
                                    objOrderDetail.EditDate = DateTime.Now;
                                    objOrderDetail.ReservationRemark = _Remark;
                                }
                            }
                            else
                            {
                                throw new Exception(_LanguagePack["common_data_load_false"]);
                            }
                        }
                        db.SaveChanges();
                        Trans.Commit();
                        //添加日志
                        AppLogService.UpdateLog<OrderDetail>(objOrderDetail, _IDs);
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
            }
            return _result;
        }
        #endregion
    }
}

