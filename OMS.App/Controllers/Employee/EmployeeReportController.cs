using System;
using System.Web;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.Database;
using Samsonite.OMS.Encryption;
using Samsonite.OMS.Service;
using Samsonite.Utility.Common;

namespace OMS.App.Controllers
{
    public class EmployeeReportController : BaseController
    {
        //
        // GET: /EmployeeReport/

        #region 查询
        [UserPowerAuthorize]
        public ActionResult Index()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;

            //菜单栏
            ViewBag.MenuBar = this.MenuBar(this.CurrentFunctionID);
            //功能权限
            ViewBag.FunctionPower = this.FunctionPowers();

            //时间段列表
            ViewData["time_list"] = UserEmployeeService.GetGroupOption();
            //品牌列表
            ViewData["brand_list"] = BrandService.GetBrands();

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public ContentResult Index_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            ContentResult _result = new ContentResult();
            List<DynamicRepository.SQLCondition> _SqlWhere1 = new List<DynamicRepository.SQLCondition>();
            List<DynamicRepository.SQLCondition> _SqlWhere2 = new List<DynamicRepository.SQLCondition>();
            int _groupid = VariableHelper.SaferequestInt(Request.Form["groupid"]);
            string _email = VariableHelper.SaferequestStr(Request.Form["email"]);
            int _is_warning = VariableHelper.SaferequestInt(Request.Form["is_warning"]);
            DataTable objResult = new DataTable();
            using (DynamicRepository db = new DynamicRepository())
            {
                if (_groupid == 0)
                {
                    if (UserEmployeeService.GetGroupOption().Count() > 0)
                    {
                        //取最新的年份记录
                        _groupid = UserEmployeeService.GetGroupOption().LastOrDefault().ID;
                    }
                }
                _SqlWhere1.Add(new DynamicRepository.SQLCondition() { Condition = "a.DataGroupID={0}", Param = _groupid });

                if (!string.IsNullOrEmpty(_email))
                {
                    _SqlWhere1.Add(new DynamicRepository.SQLCondition() { Condition = "ue.EmployeeEmail={0}", Param = EncryptionBase.EncryptString(_email) });
                    _SqlWhere2.Add(new DynamicRepository.SQLCondition() { Condition = "ue.EmployeeEmail={0}", Param = EncryptionBase.EncryptString(_email) });
                }

                //品牌列表
                List<Brand> objBrandList = BrandService.GetBrandLists();
                //时间组
                string _dataGroupValue = GetGroupData(UserEmployeeService.GetGroupOption(), _groupid);
                //新开页标题
                string _SearchTab = GetSearchEmployeeOrderTab();
                //结果集合
                DataTable objData = new DataTable();
                objData.Columns.Add("s1");
                objData.Columns.Add("s2");
                objData.Columns.Add("s3");
                objData.Columns.Add("s4");
                objData.Columns.Add("s5");
                objData.Columns.Add("s6");
                for (int t = 0; t < objBrandList.Count; t++)
                {
                    objData.Columns.Add($"b_{objBrandList[t].ID}");
                }

                //查询
                var objAnalysisEmployeeOrder_List = db.Fetch<AnalysisEmployeeOrder>("select a.EmployeeUserID,a.DataGroupID,a.Brand,a.Quantity,a.TotalOrderPayment from AnalysisEmployeeOrder as a inner join UserEmployee as ue on a.EmployeeUserID=ue.EmployeeID order by id asc", _SqlWhere1);
                //客户列表
                List<string> _sqlWhere2 = db.ConvertSQL(_SqlWhere2);
                var objUserEmployee_List = db.Fetch<View_UserEmployee>("select ue.EmployeeID,ue.EmployeeEmail,ue.EmployeeName,ue.IsAmountLimit,ue.TotalAmount,ue.IsQuantityLimit,ue.TotalQuantity,ue.IsLock,ue.LeaveTime from View_UserEmployee as ue inner join (select DISTINCT EmployeeUserID from AnalysisEmployeeOrder where DataGroupID=" + _groupid + ") as tmp on ue.EmployeeID=tmp.EmployeeUserID " + ((_sqlWhere2.Count > 0) ? " where " + string.Join(" and ", _sqlWhere2) : "") + "");
                //统计数量
                DataRow _dr = null;
                decimal _temp_amount = 0;
                string _temp_amount_str = string.Empty;
                int _temp_quantity = 0;
                string _temp_quantity_str = string.Empty;
                bool _is_err = false;
                foreach (var _o in objUserEmployee_List)
                {
                    //数据解密
                    EncryptionFactory.Create(_o, new string[] { "EmployeeEmail", "EmployeeName" }).Decrypt();

                    _is_err = false;
                    List<AnalysisEmployeeOrder> _t = objAnalysisEmployeeOrder_List.Where(p => p.EmployeeUserID == _o.EmployeeID).ToList();
                    //-----------计算是否需要提醒-----------
                    //金额是否超标
                    _temp_amount = _t.Sum(p => p.TotalOrderPayment);
                    if (_o.IsAmountLimit)
                    {
                        if (_temp_amount > _o.TotalAmount)
                        {
                            _is_err = true;
                            _temp_amount_str = $"<label class=\"color_danger\">{VariableHelper.FormateMoney(_temp_amount)}</label>/{VariableHelper.FormateMoney(_o.TotalAmount)}";
                        }
                        else
                        {
                            _temp_amount_str = $"{VariableHelper.FormateMoney(_temp_amount)}/{VariableHelper.FormateMoney(_o.TotalAmount)}";
                        }
                    }
                    else
                    {
                        _temp_amount_str = $"{VariableHelper.FormateMoney(_temp_amount)}";
                    }
                    //数量是否超标
                    _temp_quantity = _t.Sum(p => p.Quantity);
                    if (_o.IsQuantityLimit)
                    {
                        if (_temp_quantity > _o.TotalQuantity)
                        {
                            _is_err = true;
                            _temp_quantity_str = $"<label class=\"color_danger\">{_temp_quantity}</label>/{_o.TotalQuantity}";
                        }
                        else
                        {
                            _temp_quantity_str = $"{_temp_quantity}/{_o.TotalQuantity}";
                        }
                    }
                    else
                    {
                        _temp_quantity_str = $"{_temp_quantity}";
                    }
                    //离职之后是否有购买记录
                    if (_o.IsLock)
                    {
                        if (!string.IsNullOrEmpty(_o.LeaveTime))
                        {
                            int _c = db.FirstOrDefault<int>("select count(*) from [order] inner join Customer on [order].CustomerNo=Customer.CustomerNo where datediff(day,CreateDate,'2018-01-01')<0 and email = @0", _o.EmployeeEmail);
                            if (_c > 0)
                            {
                                _is_err = true;
                            }
                        }
                    }

                    //-----------填充金额显示列-----------
                    _dr = objData.NewRow();
                    _dr["s1"] = (_is_err) ? $"<label class=\"color_danger\">{_o.EmployeeEmail}</label>" : _o.EmployeeEmail;
                    if (_o.IsLock) _dr["s1"] = $"{_dr["s1"]}<br/><label class=\"label-danger\">{_LanguagePack["employeereport_index_label_dimission"]}</label>";
                    _dr["s2"] = _o.EmployeeName;
                    _dr["s3"] = _dataGroupValue;
                    _dr["s4"] = $"<label class=\"label-info\">{_LanguagePack["employeereport_index_orderamount"]}</label>&nbsp;&nbsp;{_temp_amount_str}";
                    _dr["s5"] = $"<a href=\"javascript:void(0);\" onclick=\"window.parent.frames.OpenTab('{_SearchTab}','{Url.Action("Index", "EmployeeOrder")}?email={_o.EmployeeEmail}',true);\">{_LanguagePack["common_view_order"]}</a>";
                    _dr["s6"] = _is_err ? 1 : 0;
                    //品牌
                    for (int t = 0; t < objBrandList.Count; t++)
                    {
                        _dr[$"b_{objBrandList[t].ID}"] = GetBrandValue(_t, objBrandList[t], "amount");
                    }
                    objData.Rows.Add(_dr);
                    //-----------填充数量显示列-----------
                    _dr = objData.NewRow();
                    _dr["s1"] = (_is_err) ? $"<label class=\"color_danger\">{_o.EmployeeEmail}</label>" : _o.EmployeeEmail;
                    if (_o.IsLock) _dr["s1"] = $"{_dr["s1"]}<br/><label class=\"label-danger\">{_LanguagePack["employeereport_index_label_dimission"]}</label>";
                    _dr["s2"] = _o.EmployeeName;
                    _dr["s3"] = _dataGroupValue;
                    _dr["s4"] = $"<label class=\"label-info\">{_LanguagePack["employeereport_index_quantity"]}</label>&nbsp;&nbsp;{_temp_quantity_str}";
                    _dr["s5"] = $"<a href=\"javascript:void(0);\" onclick=\"window.parent.frames.OpenTab('{_SearchTab}','{Url.Action("Index", "EmployeeOrder")}?email={_o.EmployeeEmail}',true);\">{_LanguagePack["common_view_order"]}</a>";
                    _dr["s6"] = _is_err ? 1 : 0;
                    //品牌
                    for (int t = 0; t < objBrandList.Count; t++)
                    {
                        _dr[$"b_{objBrandList[t].ID}"] = GetBrandValue(_t, objBrandList[t], "quantity");
                    }
                    objData.Rows.Add(_dr);
                    //如果查询警告的员工信息
                    if (_is_warning == 1)
                    {
                        objResult = objData.Clone();
                        foreach (DataRow row in objData.Select("s6=1"))
                        {
                            objResult.Rows.Add(row.ItemArray);
                        }
                    }
                    else
                    {
                        objResult = objData;
                    }
                }
                //生成Json
                string _data_json = JsonHelper.JsonSerialize(objResult);
                _data_json = "{\"rows\":" + _data_json + "}";
                _result.ContentEncoding = System.Text.Encoding.UTF8;
                _result.Content = _data_json;
            }
            return _result;
        }
        #endregion

        #region 导出Excel
        [UserPowerAuthorize]
        public FileResult ExportExcel()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            int _groupid = VariableHelper.SaferequestInt(Request.Form["GroupTime"]);
            string _email = VariableHelper.SaferequestStr(Request.Form["CustomerEmail"]);
            int _is_warning = VariableHelper.SaferequestInt(Request.Form["IsWarning"]);

            DataTable objResult = new DataTable();
            List<DynamicRepository.SQLCondition> _SqlWhere1 = new List<DynamicRepository.SQLCondition>();
            List<DynamicRepository.SQLCondition> _SqlWhere2 = new List<DynamicRepository.SQLCondition>();
            using (DynamicRepository db = new DynamicRepository())
            {
                if (_groupid == 0)
                {
                    //取最新的年份记录
                    _groupid = UserEmployeeService.GetGroupOption().LastOrDefault().ID;
                }
                _SqlWhere1.Add(new DynamicRepository.SQLCondition() { Condition = "a.DataGroupID={0}", Param = _groupid });

                if (!string.IsNullOrEmpty(_email))
                {
                    _SqlWhere1.Add(new DynamicRepository.SQLCondition() { Condition = "ue.EmployeeEmail={0}", Param = EncryptionBase.EncryptString(_email) });
                    _SqlWhere2.Add(new DynamicRepository.SQLCondition() { Condition = "ue.EmployeeEmail={0}", Param = EncryptionBase.EncryptString(_email) });
                }

                //查询
                DataTable dt = new DataTable();
                dt.Columns.Add(_LanguagePack["employeereport_index_email"]);
                dt.Columns.Add(_LanguagePack["employeereport_index_name"]);
                dt.Columns.Add(_LanguagePack["employeereport_index_time"]);
                dt.Columns.Add(_LanguagePack["employeereport_index_orderamount"]);
                dt.Columns.Add(_LanguagePack["employeereport_index_quantity"]);
                dt.Columns.Add(_LanguagePack["employeereport_index_is_dimission"]);
                dt.Columns.Add(_LanguagePack["employeereport_index_dimission_time"]);
                dt.Columns.Add(_LanguagePack["employeereport_index_is_warning"]);

                //时间组
                string _dataGroupValue = GetGroupData(UserEmployeeService.GetGroupOption(), _groupid);
                //查询
                var objAnalysisEmployeeOrder_List = db.Fetch<AnalysisEmployeeOrder>("select a.EmployeeUserID,a.DataGroupID,a.Brand,a.Quantity,a.TotalOrderPayment from AnalysisEmployeeOrder as a inner join UserEmployee as ue on a.EmployeeUserID=ue.EmployeeID order by id asc", _SqlWhere1);
                //客户列表
                List<string> _sqlWhere2 = db.ConvertSQL(_SqlWhere2);
                var objUserEmployee_List = db.Fetch<View_UserEmployee>("select ue.EmployeeID,ue.EmployeeEmail,ue.EmployeeName,ue.IsAmountLimit,ue.TotalAmount,ue.IsQuantityLimit,ue.TotalQuantity,ue.IsLock,ue.LeaveTime from View_UserEmployee as ue inner join (select DISTINCT EmployeeUserID from AnalysisEmployeeOrder where DataGroupID=" + _groupid + ") as tmp on ue.EmployeeID=tmp.EmployeeUserID " + ((_sqlWhere2.Count > 0) ? " where " + string.Join(" and ", _sqlWhere2) : "") + "");
                DataRow _dr = null;
                decimal _temp_amount = 0;
                string _temp_amount_str = string.Empty;
                int _temp_quantity = 0;
                string _temp_quantity_str = string.Empty;
                bool _is_err = false;
                foreach (var _o in objUserEmployee_List)
                {
                    //数据解密
                    EncryptionFactory.Create(_o, new string[] { "EmployeeEmail", "EmployeeName" }).Decrypt();

                    _is_err = false;
                    List<AnalysisEmployeeOrder> _t = objAnalysisEmployeeOrder_List.Where(p => p.EmployeeUserID == _o.EmployeeID).ToList();
                    //-----------计算是否需要提醒-----------
                    //金额是否超标
                    _temp_amount = _t.Sum(p => p.TotalOrderPayment);
                    if (_o.IsAmountLimit)
                    {
                        if (_temp_amount > _o.TotalAmount)
                        {
                            _is_err = true;
                            _temp_amount_str = $"<label class=\"color_danger\">{VariableHelper.FormateMoney(_temp_amount)}</label>/{VariableHelper.FormateMoney(_o.TotalAmount)}";
                        }
                        else
                        {
                            _temp_amount_str = $"{VariableHelper.FormateMoney(_temp_amount)}/{VariableHelper.FormateMoney(_o.TotalAmount)}";
                        }
                    }
                    else
                    {
                        _temp_amount_str = $"{VariableHelper.FormateMoney(_temp_amount)}";
                    }
                    //数量是否超标
                    _temp_quantity = _t.Sum(p => p.Quantity);
                    if (_o.IsQuantityLimit)
                    {
                        if (_temp_quantity > _o.TotalQuantity)
                        {
                            _is_err = true;
                            _temp_quantity_str = $"<label class=\"color_danger\">{_temp_quantity}</label>/{_o.TotalQuantity}";
                        }
                        else
                        {
                            _temp_quantity_str = $"{_temp_quantity}/{_o.TotalQuantity}";
                        }
                    }
                    else
                    {
                        _temp_quantity_str = $"{_temp_quantity}";
                    }
                    //离职之后是否有购买记录
                    if (_o.IsLock)
                    {
                        if (!string.IsNullOrEmpty(_o.LeaveTime))
                        {
                            int _c = db.FirstOrDefault<int>("select count(*) from [order] inner join Customer on [order].CustomerNo=Customer.CustomerNo where datediff(day,CreateDate,'2018-01-01')<0 and email = @0", _o.EmployeeEmail);
                            if (_c > 0)
                            {
                                _is_err = true;
                            }
                        }
                    }

                    _dr = dt.NewRow();
                    _dr[0] = _o.EmployeeEmail;
                    _dr[1] = _o.EmployeeName;
                    _dr[2] = _dataGroupValue;
                    _dr[3] = VariableHelper.FormateMoney(_t.Sum(p => p.TotalOrderPayment));
                    _dr[4] = _t.Sum(p => p.Quantity);
                    _dr[5] = _o.IsLock ? 1 : 0;
                    _dr[6] = _o.LeaveTime;
                    _dr[7] = _is_err ? 1 : 0;
                    dt.Rows.Add(_dr);
                }

                //如果查询警告的员工信息
                if (_is_warning == 1)
                {
                    objResult = dt.Clone();
                    foreach (DataRow row in dt.Select("" + _LanguagePack["employeereport_index_is_warning"] + "=1"))
                    {
                        objResult.Rows.Add(row.ItemArray);
                    }
                }
                else
                {
                    objResult = dt;
                }

                string _filepath = string.Format("~/UploadFile/CacheFile/{0}", DateTime.Now.ToString("yyyy-MM-dd"));
                if (!Directory.Exists(Server.MapPath(_filepath)))
                {
                    Directory.CreateDirectory(Server.MapPath(_filepath));
                }
                string _fileName = string.Format("EmployeeReport_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss"));
                _fileName = HttpUtility.UrlEncode(_fileName, Encoding.GetEncoding("UTF-8"));
                string _path = Server.MapPath(string.Format("{0}/{1}", _filepath, _fileName));
                ExcelHelper objExcelHelper = new ExcelHelper(_path);
                objExcelHelper.DataTableToExcel(objResult, "Sheet1", true);
                return File(_path, "application/ms-excel", _fileName);
            }
        }
        #endregion

        #region 函数
        private string GetGroupData(List<UserEmployeeGroup> objUserEmployeeGroupList, int objDataGroupID)
        {
            string _result = string.Empty;
            UserEmployeeGroup objUserEmployeeGroup = objUserEmployeeGroupList.Where(p => p.ID == objDataGroupID).SingleOrDefault();
            if (objUserEmployeeGroup != null)
            {
                _result = $"{objUserEmployeeGroup.BeginDate.ToString("yyyy/MM/dd")}-{objUserEmployeeGroup.EndDate.ToString("yyyy/MM/dd")}";
            }
            return _result;
        }

        private string GetBrandValue(List<AnalysisEmployeeOrder> objAnalysisEmployeeOrderList, Brand objBrand, string objType)
        {
            string _result = string.Empty;
            List<string> objSons = BrandService.GetSons(objBrand.ID);

            var objAnalysisEmployeeOrder_List = objAnalysisEmployeeOrderList.Where(p => objSons.Contains(p.Brand)).ToList();
            if (objAnalysisEmployeeOrder_List.Count > 0)
            {
                if (objType == "amount")
                {
                    _result = VariableHelper.FormateMoney(objAnalysisEmployeeOrder_List.Sum(p => p.TotalOrderPayment));
                }
                else if (objType == "quantity")
                {
                    _result = VariableHelper.FormateMoney(objAnalysisEmployeeOrder_List.Sum(p => p.Quantity));
                }
                else
                {
                    _result = "0";
                }
            }
            else
            {
                _result = "0";
            }
            return _result;
        }
        #endregion
    }
}

