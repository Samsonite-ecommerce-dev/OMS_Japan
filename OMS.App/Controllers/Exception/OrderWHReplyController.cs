using System;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
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
        // GET: /OrderWHReply/

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
            string _orderid = VariableHelper.SaferequestStr(Request.Form["orderid"]);
            string _storeid = VariableHelper.SaferequestStr(Request.Form["store"]);
            string _time1 = VariableHelper.SaferequestStr(Request.Form["time1"]);
            string _time2 = VariableHelper.SaferequestStr(Request.Form["time2"]);
            string _msg = VariableHelper.SaferequestStr(Request.Form["msg"]);
            int _isdelete = VariableHelper.SaferequestInt(Request.Form["isdelete"]);
            using (var db = new ebEntities())
            {
                var _lambda1 = db.View_OrderDetail.AsQueryable();
                var _lambda2 = db.OrderWMSReply.AsQueryable();

                //搜索条件
                if (!string.IsNullOrEmpty(_orderid))
                {
                    _lambda1 = _lambda1.Where(p => p.OrderNo.Contains(_orderid) || p.SubOrderNo.Contains(_orderid));
                }

                if (!string.IsNullOrEmpty(_storeid))
                {
                    _lambda1 = _lambda1.Where(p => p.MallSapCode == _storeid);
                }
                else
                {
                    //默认显示当前账号允许看到的店铺订单
                    var _UserMalls = this.CurrentLoginUser.UserMalls;
                    _lambda1 = _lambda1.Where(p => _UserMalls.Contains(p.MallSapCode));
                }

                if (!string.IsNullOrEmpty(_time1))
                {
                    var _beginTime = VariableHelper.SaferequestTime(_time1);
                    _lambda1 = _lambda1.Where(p => SqlFunctions.DateDiff("day", p.OrderTime, _beginTime) <= 0);
                }

                if (!string.IsNullOrEmpty(_time2))
                {
                    var _endTime = VariableHelper.SaferequestTime(_time2);
                    _lambda1 = _lambda1.Where(p => SqlFunctions.DateDiff("day", p.OrderTime, _endTime) >= 0);
                }

                if (!string.IsNullOrEmpty(_msg))
                {
                    _lambda2 = _lambda2.Where(p => p.ApiReplyMsg.Contains(_msg));
                }

                if (_isdelete == 0)
                {
                    _lambda2 = _lambda2.Where(p => !p.IsDelete);
                }
                else
                {
                    _lambda2 = _lambda2.Where(p => p.IsDelete);
                }

                //WH接收失败订单
                _lambda2 = _lambda2.Where(p => !p.Status);

                var _lambda = from od in _lambda1
                              join owr in _lambda2 on od.SubOrderNo equals owr.SubOrderNo
                              select new { od, owr };

                //查询
                var _list = this.BaseEntityRepository.GetPage(VariableHelper.SaferequestInt(Request.Form["page"]), VariableHelper.SaferequestInt(Request.Form["rows"]), _lambda.AsNoTracking(), p => p.owr.Id, false);
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.owr.Id,
                               s1 = string.Format("<a href=\"javascript:void(0);\" onclick=\"artDialogExtend.Dialog.Open('" + Url.Action("Detail", "OrderQuery") + "?ID={0}',{{title:'{1}-{0}',width:'100%',height:'100%'}});\">{0}</a>", dy.od.OrderNo, _LanguagePack["ordererror_detail_title"]),
                               s2 = dy.od.SubOrderNo,
                               s3 = dy.od.MallName,
                               s4 = dy.od.SKU,
                               s5 = dy.od.ProductName,
                               s6 = VariableHelper.FormateMoney(dy.od.SellingPrice),
                               s7 = dy.od.Quantity,
                               s8 = VariableHelper.FormateMoney(dy.od.PaymentAmount),
                               s9 = VariableHelper.FormateMoney(OrderHelper.MathRound(dy.od.ActualPaymentAmount)),
                               s10 = OrderHelper.GetProductStatusDisplay(dy.od.ProductStatus, true),
                               s11 = VariableHelper.FormateTime(dy.od.OrderTime, "yyyy-MM-dd HH:mm:ss"),
                               s12 = dy.owr.ApiCount,
                               s13 = VariableHelper.FormateTime(dy.owr.ApiReplyDate, "yyyy-MM-dd HH:mm:ss"),
                               s14 = dy.owr.ApiReplyMsg
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
            int _isdelete = VariableHelper.SaferequestInt(Request.Form["Canceled"]);

            using (var db = new ebEntities())
            {
                var _lambda1 = db.View_OrderDetail.AsQueryable();
                var _lambda2 = db.OrderWMSReply.AsQueryable();

                //搜索条件
                if (!string.IsNullOrEmpty(_orderid))
                {
                    _lambda1 = _lambda1.Where(p => p.OrderNo.Contains(_orderid) || p.SubOrderNo.Contains(_orderid));
                }

                if (!string.IsNullOrEmpty(_storeid))
                {
                    _lambda1 = _lambda1.Where(p => p.MallSapCode == _storeid);
                }
                else
                {
                    //默认显示当前账号允许看到的店铺订单
                    var _UserMalls = this.CurrentLoginUser.UserMalls;
                    _lambda1 = _lambda1.Where(p => _UserMalls.Contains(p.MallSapCode));
                }

                if (!string.IsNullOrEmpty(_time1))
                {
                    var _beginTime = VariableHelper.SaferequestTime(_time1);
                    _lambda1 = _lambda1.Where(p => SqlFunctions.DateDiff("day", p.OrderTime, _beginTime) <= 0);
                }

                if (!string.IsNullOrEmpty(_time2))
                {
                    var _endTime = VariableHelper.SaferequestTime(_time2);
                    _lambda1 = _lambda1.Where(p => SqlFunctions.DateDiff("day", p.OrderTime, _endTime) >= 0);
                }

                if (!string.IsNullOrEmpty(_msg))
                {
                    _lambda2 = _lambda2.Where(p => p.ApiReplyMsg.Contains(_msg));
                }

                if (_isdelete == 0)
                {
                    _lambda2 = _lambda2.Where(p => !p.IsDelete);
                }
                else
                {
                    _lambda2 = _lambda2.Where(p => p.IsDelete);
                }

                //WH接收失败订单
                _lambda2 = _lambda2.Where(p => !p.Status);

                var _lambda = from od in _lambda1
                              join owr in _lambda2 on od.SubOrderNo equals owr.SubOrderNo
                              select new { od, owr };

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
                var _list = _lambda.AsNoTracking().OrderByDescending(p => p.owr.Id).ToList();
                foreach (var _dy in _list)
                {
                    _dr = dt.NewRow();
                    _dr[0] = _dy.od.OrderNo;
                    _dr[1] = _dy.od.SubOrderNo;
                    _dr[2] = _dy.od.MallName;
                    _dr[3] = _dy.od.SKU;
                    _dr[4] = _dy.od.ProductName;
                    _dr[5] = VariableHelper.FormateMoney(_dy.od.SellingPrice);
                    _dr[6] = _dy.od.Quantity;
                    _dr[7] = VariableHelper.FormateMoney(_dy.od.PaymentAmount);
                    _dr[8] = VariableHelper.FormateMoney(_dy.od.ActualPaymentAmount);
                    _dr[9] = OrderHelper.GetProductStatusDisplay(_dy.od.ProductStatus, false);
                    _dr[10] = VariableHelper.FormateTime(_dy.od.OrderTime, "yyyy-MM-dd HH:mm:ss");
                    _dr[11] = _dy.owr.ApiCount;
                    _dr[12] = VariableHelper.FormateTime(_dy.owr.ApiReplyDate, "yyyy-MM-dd HH:mm:ss");
                    _dr[13] = _dy.owr.ApiReplyMsg;
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
