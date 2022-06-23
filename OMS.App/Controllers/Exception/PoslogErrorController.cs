using System;
using System.Web;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

using Samsonite.OMS.DTO.Sap;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service;
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
            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            string _orderid = VariableHelper.SaferequestStr(Request.Form["orderid"]);
            string _storeid = VariableHelper.SaferequestStr(Request.Form["store"]);
            string _transactionid = VariableHelper.SaferequestStr(Request.Form["transactionid"]);
            string _time1 = VariableHelper.SaferequestStr(Request.Form["time1"]);
            string _time2 = VariableHelper.SaferequestStr(Request.Form["time2"]);
            int _type = VariableHelper.SaferequestInt(Request.Form["type"]);
            int _status = VariableHelper.SaferequestInt(Request.Form["status"]);
            using (var db = new DynamicRepository())
            {
                //搜索条件
                if (!string.IsNullOrEmpty(_orderid))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "((sld.OrderNo like {0}) or (sld.SubOrderNo like {0}))", Param = "%" + _orderid + "%" });
                }

                if (!string.IsNullOrEmpty(_storeid))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "sld.MallStoreCode={0}", Param = _storeid });
                }
                else
                {
                    //默认显示当前账号允许看到的店铺订单
                    var _UserMalls = this.CurrentLoginUser.UserMalls;
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "sld.MallStoreCode in (select item from strToIntTable('" + string.Join(",", _UserMalls) + "',','))", Param = null });
                }

                if (!string.IsNullOrEmpty(_transactionid))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "sld.UploadNo={0}", Param = _transactionid });
                }

                if (!string.IsNullOrEmpty(_time1))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,sld.CreateDate,{0})<=0", Param = VariableHelper.SaferequestTime(_time1) });
                }

                if (!string.IsNullOrEmpty(_time2))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,sld.CreateDate,{0})>=0", Param = VariableHelper.SaferequestTime(_time2) });
                }

                if (_type > 0)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "sld.LogType={0}", Param = _type });
                }

                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "sld.Status={0}", Param = _status });

                //查询
                var _list = db.GetPage<dynamic>("select sld.Id,sld.OrderNo,sld.SubOrderNo,sld.UploadNo,sld.CreateDate,sld.LogType,sld.Status,sld.MallStoreCode,sld.SAPDNNumber,o.MallName from SapUploadLogDetail as sld inner join [Order] as o on sld.OrderNo=o.OrderNo order by sld.Id desc", _SqlWhere, VariableHelper.SaferequestInt(Request.Form["rows"]), VariableHelper.SaferequestInt(Request.Form["page"]));
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.Id,
                               s1 = dy.UploadNo,
                               s2 = dy.MallName,
                               s3 = string.Format("<a href=\"javascript:void(0);\" onclick=\"artDialogExtend.Dialog.Open('" + Url.Action("Detail", "OrderQuery") + "?ID={0}',{{title:'{1}-{0}',width:'100%',height:'100%'}});\">{0}</a>", dy.OrderNo, _LanguagePack["orderquery_detail_title"]),
                               s4 = dy.SubOrderNo,
                               s5 = PoslogHelper.GetPoslogTypeDisplay(dy.LogType),
                               s6 = PoslogHelper.GetPoslogStatusDisplay(dy.Status, true),
                               s7 = dy.SAPDNNumber,
                               s8 = dy.CreateDate.ToString("yyyy-MM-dd HH:mm:ss")
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
            string _time = VariableHelper.SaferequestStr(Request.Form["Time"]);
            int _type = VariableHelper.SaferequestInt(Request.Form["Type"]);
            int _status = VariableHelper.SaferequestInt(Request.Form["Status"]);

            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            using (var db = new DynamicRepository())
            {
                //搜索条件
                if (!string.IsNullOrEmpty(_orderid))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "((sld.OrderNo like {0}) or (sld.SubOrderNo like {0}))", Param = "%" + _orderid + "%" });
                }

                if (!string.IsNullOrEmpty(_storeid))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "sld.MallStoreCode={0}", Param = _storeid });
                }
                else
                {
                    //默认显示当前账号允许看到的店铺订单
                    var _UserMalls = this.CurrentLoginUser.UserMalls;
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "sld.MallStoreCode in (select item from strToIntTable('" + string.Join(",", _UserMalls) + "',','))", Param = null });
                }

                if (!string.IsNullOrEmpty(_transactionid))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "sld.UploadNo={0}", Param = _transactionid });
                }

                if (!string.IsNullOrEmpty(_time))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,sld.CreateDate,{0})=0", Param = VariableHelper.SaferequestTime(_time) });
                }

                if (_type > 0)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "sld.LogType={0}", Param = _type });
                }

                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "sld.Status={0}", Param = _status });

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
                List<dynamic> _List = db.Fetch<dynamic>("select sld.Id,sld.OrderNo,sld.SubOrderNo,sld.UploadNo,sld.CreateDate,sld.LogType,sld.Status,sld.MallStoreCode,sld.SAPDNNumber,o.MallName from SapUploadLogDetail as sld inner join [Order] as o on sld.OrderNo=o.OrderNo order by sld.Id desc", _SqlWhere);
                foreach (var _dy in _List)
                {
                    _dr = dt.NewRow();
                    _dr[0] = _dy.UploadNo;
                    _dr[1] = _dy.MallName;
                    _dr[2] = _dy.OrderNo;
                    _dr[3] = _dy.SubOrderNo;
                    _dr[4] = PoslogHelper.GetPoslogTypeDisplay(_dy.LogType);
                    _dr[5] = PoslogHelper.GetPoslogStatusDisplay(_dy.Status, false);
                    _dr[6] = _dy.SAPDNNumber;
                    _dr[7] = _dy.CreateDate.ToString("yyyy-MM-dd HH:mm:ss");
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
