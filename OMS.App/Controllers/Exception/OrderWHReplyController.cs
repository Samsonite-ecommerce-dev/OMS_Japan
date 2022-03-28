using System;
using System.Web;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.Database;
using Samsonite.OMS.Service;
using Samsonite.Utility.Common;
using OMS.App.Helper;
using System.IO;
using System.Text;

namespace OMS.App.Controllers
{
    public class OrderWHReplyController : BaseController
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
            string _msg = VariableHelper.SaferequestStr(Request.Form["msg"]);
            int _isdelete = VariableHelper.SaferequestInt(Request.Form["isdelete"]);
            using (var db = new DynamicRepository())
            {
                //搜索条件
                if (!string.IsNullOrEmpty(_orderid))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "((od.OrderNo like {0}) or (od.SubOrderNo like {0}))", Param = "%" + _orderid + "%" });
                }

                if (!string.IsNullOrEmpty(_storeid))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "od.MallSapCode={0}", Param = _storeid });
                }
                else
                {
                    //默认显示当前账号允许看到的店铺订单
                    var _UserMalls = this.CurrentLoginUser.UserMalls;
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "od.MallSapCode in (select item from strToIntTable('" + string.Join(",", _UserMalls) + "',','))", Param = null });
                }

                if (!string.IsNullOrEmpty(_time1))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,od.OrderTime,{0})<=0", Param = VariableHelper.SaferequestTime(_time1) });
                }

                if (!string.IsNullOrEmpty(_time2))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,od.OrderTime,{0})>=0", Param = VariableHelper.SaferequestTime(_time2) });
                }

                if (!string.IsNullOrEmpty(_msg))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "owr.ApiReplyMsg like {0}", Param = "%" + _msg + "%" });
                }

                if (_isdelete == 0)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "owr.IsDelete=0", Param = null });
                }
                else
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "owr.IsDelete=1", Param = null });
                }

                //WH接收失败订单
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "owr.Status=0", Param = null });
                //查询
                var _list = db.GetPage<dynamic>("select owr.Id,od.OrderNo,od.SubOrderNo,od.MallName,od.OrderTime,od.SKU,od.ProductName,od.Quantity,od.SellingPrice,od.ProductStatus,od.PaymentAmount,od.ActualPaymentAmount,owr.ApiReplyDate,owr.ApiReplyMsg,owr.ApiCount from View_OrderDetail as od inner join OrderWMSReply as owr on od.SubOrderNo=owr.SubOrderNo order by owr.Id desc", _SqlWhere, VariableHelper.SaferequestInt(Request.Form["rows"]), VariableHelper.SaferequestInt(Request.Form["page"]));
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.Id,
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
                               s12 = dy.ApiCount,
                               s13 = VariableHelper.FormateTime(dy.ApiReplyDate, "yyyy-MM-dd HH:mm:ss"),
                               s14 = dy.ApiReplyMsg
                           }
                };
            }
            return _result;
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
                try
                {
                    if (string.IsNullOrEmpty(_IDs))
                    {
                        throw new Exception(_LanguagePack["common_data_need_one"]);
                    }

                    OrderWMSReply objOrderWMSReply = new OrderWMSReply();
                    foreach (string _str in _IDs.Split(','))
                    {
                        Int64 _ID = VariableHelper.SaferequestInt64(_str);
                        objOrderWMSReply = db.OrderWMSReply.Where(p => p.Id == _ID).SingleOrDefault();
                        if (objOrderWMSReply != null)
                        {
                            objOrderWMSReply.IsDelete = true;
                        }
                        else
                        {
                            throw new Exception(string.Format("{0}:{1}", _str, _LanguagePack["common_data_no_exsit"]));
                        }
                    }
                    db.SaveChanges();
                    //返回信息
                    _result.Data = new
                    {
                        result = true,
                        msg = _LanguagePack["common_data_delete_success"]
                    };
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
        }
        #endregion

        #region 恢复
        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Restore_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            string _IDs = VariableHelper.SaferequestStr(Request.Form["ID"]);
            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_IDs))
                    {
                        throw new Exception(_LanguagePack["common_data_need_one"]);
                    }

                    OrderWMSReply objOrderWMSReply = new OrderWMSReply();
                    foreach (string _str in _IDs.Split(','))
                    {
                        Int64 _ID = VariableHelper.SaferequestInt64(_str);
                        objOrderWMSReply = db.OrderWMSReply.Where(p => p.Id == _ID).SingleOrDefault();
                        if (objOrderWMSReply != null)
                        {
                            objOrderWMSReply.IsDelete = false;
                        }
                        else
                        {
                            throw new Exception(string.Format("{0}:{1}", _str, _LanguagePack["common_data_no_exsit"]));
                        }
                    }
                    db.SaveChanges();
                    //返回信息
                    _result.Data = new
                    {
                        result = true,
                        msg = _LanguagePack["common_data_recover_success"]
                    };
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
        }
        #endregion

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
            string _msg = VariableHelper.SaferequestStr(Request.Form["PostReplyMessage"]);

            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            using (var db = new DynamicRepository())
            {
                //搜索条件
                if (!string.IsNullOrEmpty(_orderid))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "((od.OrderNo like {0}) or (od.SubOrderNo like {0}))", Param = "%" + _orderid + "%" });
                }

                if (!string.IsNullOrEmpty(_storeid))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "od.MallSapCode={0}", Param = _storeid });
                }
                else
                {
                    //默认显示当前账号允许看到的店铺订单
                    var _UserMalls = this.CurrentLoginUser.UserMalls;
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "od.MallSapCode in (select item from strToIntTable('" + string.Join(",", _UserMalls) + "',','))", Param = null });
                }

                if (!string.IsNullOrEmpty(_time1))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,od.OrderTime,{0})<=0", Param = VariableHelper.SaferequestTime(_time1) });
                }

                if (!string.IsNullOrEmpty(_time2))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,od.OrderTime,{0})>=0", Param = VariableHelper.SaferequestTime(_time2) });
                }

                if (!string.IsNullOrEmpty(_msg))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "owr.ApiReplyMsg like {0}", Param = "%" + _msg + "%" });
                }

                //WH接收失败订单
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "owr.Status=0", Param = null });

                DataTable dt = new DataTable();
                dt.Columns.Add(_LanguagePack["orderwhreply_index_order_no"]);
                dt.Columns.Add(_LanguagePack["orderwhreply_index_sub_order_no"]);
                dt.Columns.Add(_LanguagePack["orderwhreply_index_store"]);
                dt.Columns.Add(_LanguagePack["orderwhreply_index_sku"]);
                dt.Columns.Add(_LanguagePack["orderwhreply_index_productname"]);
                dt.Columns.Add(_LanguagePack["orderwhreply_index_price"]);
                dt.Columns.Add(_LanguagePack["orderwhreply_index_quantity"]);
                dt.Columns.Add(_LanguagePack["orderwhreply_index_payment"]);
                dt.Columns.Add(_LanguagePack["orderwhreply_index_actual_pay"]);
                dt.Columns.Add(_LanguagePack["orderwhreply_index_product_status"]);
                dt.Columns.Add(_LanguagePack["orderwhreply_index_ordertime"]);
                dt.Columns.Add(_LanguagePack["orderwhreply_index_count"]);
                dt.Columns.Add(_LanguagePack["orderwhreply_index_reply_time"]);
                dt.Columns.Add(_LanguagePack["orderwhreply_index_reply_error"]);

                //读取数据
                DataRow _dr = null;
                //默认显示当前账号允许看到的店铺订单
                List<dynamic> _List = db.Fetch<dynamic>($"select od.Id,od.OrderNo,od.SubOrderNo,od.MallName,od.OrderTime,od.SKU,od.ProductName,od.Quantity,od.SellingPrice,od.ProductStatus,od.PaymentAmount,od.ActualPaymentAmount,owr.ApiReplyDate,owr.ApiReplyMsg,owr.ApiCount from View_OrderDetail as od inner join OrderWMSReply as owr on od.SubOrderNo=owr.SubOrderNo order by od.OrderTime desc", _SqlWhere);
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
                    _dr[8] = VariableHelper.FormateMoney(_dy.ActualPaymentAmount);
                    _dr[9] = OrderHelper.GetProductStatusDisplay(_dy.ProductStatus, false);
                    _dr[10] = VariableHelper.FormateTime(_dy.OrderTime, "yyyy-MM-dd HH:mm:ss");
                    _dr[11] = _dy.ApiCount;
                    _dr[12] = VariableHelper.FormateTime(_dy.ApiReplyDate, "yyyy-MM-dd HH:mm:ss");
                    _dr[13] = _dy.ApiReplyMsg;
                    dt.Rows.Add(_dr);
                }

                string _filepath = $"~{AppGlobalService.UPLOAD_CACHE_PATH}/{DateTime.Now.ToString("yyyy-MM-dd")}";
                if (!Directory.Exists(Server.MapPath(_filepath)))
                {
                    Directory.CreateDirectory(Server.MapPath(_filepath));
                }
                string _fileName = string.Format("PostReplyError_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss"));
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
