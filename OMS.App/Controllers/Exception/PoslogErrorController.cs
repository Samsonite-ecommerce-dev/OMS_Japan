using System;
using System.Web;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

using Samsonite.OMS.Database;
using Samsonite.OMS.Service;
using Samsonite.OMS.Service.Sap.Poslog.Models;
using Samsonite.Utility.Common;

using OMS.App.Helper;

namespace OMS.App.Controllers
{
    public class PoslogErrorController : BaseController
    {
        //
        // GET: /PoslogError/

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

            //poslog类型
            ViewData["poslog_type_list"] = PoslogHelper.PoslogTypeObject();

            //poslog状态
            ViewData["poslog_status_list"] = PoslogHelper.PoslogStatusObject();

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
            string _transactionid = VariableHelper.SaferequestStr(Request.Form["transactionid"]);
            string _time1 = VariableHelper.SaferequestStr(Request.Form["time1"]);
            string _time2 = VariableHelper.SaferequestStr(Request.Form["time2"]);
            int _type = VariableHelper.SaferequestInt(Request.Form["type"]);
            int _status = VariableHelper.SaferequestInt(Request.Form["status"]);
            using (var db = new ebEntities())
            {
                var _lambda1 = db.SapUploadLogDetail.AsQueryable();
                var _lambda2 = db.Order.AsQueryable();

                //搜索条件
                if (!string.IsNullOrEmpty(_orderid))
                {
                    _lambda1 = _lambda1.Where(p => p.OrderNo.Contains(_orderid) || p.SubOrderNo.Contains(_orderid));
                }

                if (!string.IsNullOrEmpty(_storeid))
                {
                    _lambda1 = _lambda1.Where(p => p.MallStoreCode == _storeid);
                }
                else
                {
                    //默认显示当前账号允许看到的店铺订单
                    var _UserMalls = this.CurrentLoginUser.UserMalls;
                    _lambda1 = _lambda1.Where(p => _UserMalls.Contains(p.MallStoreCode));
                }

                if (!string.IsNullOrEmpty(_transactionid))
                {
                    _lambda1 = _lambda1.Where(p => p.UploadNo == _transactionid);
                }

                if (!string.IsNullOrEmpty(_time1))
                {
                    var _beginTime = VariableHelper.SaferequestTime(_time1);
                    _lambda1 = _lambda1.Where(p => SqlFunctions.DateDiff("day", p.CreateDate, _beginTime) <= 0);
                }

                if (!string.IsNullOrEmpty(_time2))
                {
                    var _endTime = VariableHelper.SaferequestTime(_time2);
                    _lambda1 = _lambda1.Where(p => SqlFunctions.DateDiff("day", p.CreateDate, _endTime) >= 0);
                }

                if (_type > 0)
                {
                    _lambda1 = _lambda1.Where(p => p.LogType == _type);
                }

                _lambda1 = _lambda1.Where(p => p.Status == _status);

                var _lambda = from sld in _lambda1
                              join o in _lambda2 on sld.OrderNo equals o.OrderNo
                              select new { sld, o.MallName };

                //查询
                var _list = this.BaseEntityRepository.GetPage(VariableHelper.SaferequestInt(Request.Form["page"]), VariableHelper.SaferequestInt(Request.Form["rows"]), _lambda.AsNoTracking(), p => p.sld.Id, false);
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.sld.Id,
                               s1 = dy.sld.UploadNo,
                               s2 = dy.MallName,
                               s3 = string.Format("<a href=\"javascript:void(0);\" onclick=\"artDialogExtend.Dialog.Open('" + Url.Action("Detail", "OrderQuery") + "?ID={0}',{{title:'{1}-{0}',width:'100%',height:'100%'}});\">{0}</a>", dy.sld.OrderNo, _LanguagePack["orderquery_detail_title"]),
                               s4 = dy.sld.SubOrderNo,
                               s5 = PoslogHelper.GetPoslogTypeDisplay(dy.sld.LogType),
                               s6 = PoslogHelper.GetPoslogStatusDisplay(dy.sld.Status, true),
                               s7 = dy.sld.SAPDNNumber,
                               s8 = dy.sld.CreateDate.ToString("yyyy-MM-dd HH:mm:ss")
                           }
                };
            }
            return _result;
        }
        #endregion

        #region 完成
        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Complete_Message()
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

                    SapUploadLogDetail objSapUploadLogDetail = new SapUploadLogDetail();
                    foreach (string _str in _IDs.Split(','))
                    {
                        Int64 _ID = VariableHelper.SaferequestInt64(_str);
                        objSapUploadLogDetail = db.SapUploadLogDetail.Where(p => p.Id == _ID && p.Status < (int)SapState.Success).SingleOrDefault();
                        if (objSapUploadLogDetail != null)
                        {
                            objSapUploadLogDetail.Status = (int)SapState.Success;
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
                        msg = _LanguagePack["common_data_save_success"]
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

        #region 导出Excel
        [UserPowerAuthorize]
        public FileResult ExportExcel()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            string _orderid = VariableHelper.SaferequestStr(Request.Form["OrderNumber"]);
            string _storeid = VariableHelper.SaferequestStr(Request.Form["StoreName"]);
            string _transactionid = VariableHelper.SaferequestStr(Request.Form["TransactionID"]);
            string _time1 = VariableHelper.SaferequestStr(Request.Form["Time1"]);
            string _time2 = VariableHelper.SaferequestStr(Request.Form["Time2"]);
            int _type = VariableHelper.SaferequestInt(Request.Form["Type"]);
            int _status = VariableHelper.SaferequestInt(Request.Form["Status"]);

            using (var db = new ebEntities())
            {
                var _lambda1 = db.SapUploadLogDetail.AsQueryable();
                var _lambda2 = db.Order.AsQueryable();

                if (!string.IsNullOrEmpty(_orderid))
                {
                    _lambda1 = _lambda1.Where(p => p.OrderNo.Contains(_orderid) || p.SubOrderNo.Contains(_orderid));
                }

                if (!string.IsNullOrEmpty(_storeid))
                {
                    _lambda1 = _lambda1.Where(p => p.MallStoreCode == _storeid);
                }
                else
                {
                    //默认显示当前账号允许看到的店铺订单
                    var _UserMalls = this.CurrentLoginUser.UserMalls;
                    _lambda1 = _lambda1.Where(p => _UserMalls.Contains(p.MallStoreCode));
                }

                if (!string.IsNullOrEmpty(_transactionid))
                {
                    _lambda1 = _lambda1.Where(p => p.UploadNo == _transactionid);
                }

                if (!string.IsNullOrEmpty(_time1))
                {
                    var _beginTime = VariableHelper.SaferequestTime(_time1);
                    _lambda1 = _lambda1.Where(p => SqlFunctions.DateDiff("day", p.CreateDate, _beginTime) <= 0);
                }

                if (!string.IsNullOrEmpty(_time2))
                {
                    var _endTime = VariableHelper.SaferequestTime(_time2);
                    _lambda1 = _lambda1.Where(p => SqlFunctions.DateDiff("day", p.CreateDate, _endTime) >= 0);
                }

                if (_type > 0)
                {
                    _lambda1 = _lambda1.Where(p => p.LogType == _type);
                }

                _lambda1 = _lambda1.Where(p => p.Status == _status);

                var _lambda = from sld in _lambda1
                              join o in _lambda2 on sld.OrderNo equals o.OrderNo
                              select new { sld, o.MallName };

                DataTable dt = new DataTable();
                dt.Columns.Add(_LanguagePack["poslogerror_index_transactionid"]);
                dt.Columns.Add(_LanguagePack["poslogerror_index_store"]);
                dt.Columns.Add(_LanguagePack["poslogerror_index_orderno"]);
                dt.Columns.Add(_LanguagePack["poslogerror_index_suborderno"]);
                dt.Columns.Add(_LanguagePack["poslogerror_index_type"]);
                dt.Columns.Add(_LanguagePack["poslogerror_index_status"]);
                dt.Columns.Add(_LanguagePack["poslogerror_dn_number"]);
                dt.Columns.Add(_LanguagePack["poslogerror_index_time"]);

                //读取数据
                DataRow _dr = null;
                var _list = _lambda.AsNoTracking().OrderByDescending(p => p.sld.Id).ToList();
                foreach (var _dy in _list)
                {
                    _dr = dt.NewRow();
                    _dr[0] = _dy.sld.UploadNo;
                    _dr[1] = _dy.MallName;
                    _dr[2] = _dy.sld.OrderNo;
                    _dr[3] = _dy.sld.SubOrderNo;
                    _dr[4] = PoslogHelper.GetPoslogTypeDisplay(_dy.sld.LogType);
                    _dr[5] = PoslogHelper.GetPoslogStatusDisplay(_dy.sld.Status, false);
                    _dr[6] = _dy.sld.SAPDNNumber;
                    _dr[7] = _dy.sld.CreateDate.ToString("yyyy-MM-dd HH:mm:ss");
                    dt.Rows.Add(_dr);
                }

                string _filepath = $"~{AppGlobalService.UPLOAD_CACHE_PATH}/{DateTime.Now.ToString("yyyy-MM-dd")}";
                if (!Directory.Exists(Server.MapPath(_filepath)))
                {
                    Directory.CreateDirectory(Server.MapPath(_filepath));
                }
                string _fileName = string.Format("PoslogError_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss"));
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
