using System;
using System.IO;
using System.Web;
using System.Data;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using Samsonite.OMS.Service;
using Samsonite.OMS.Encryption;
using Samsonite.Utility.Common;

namespace OMS.App.Controllers
{
    public class EmployeesController : BaseController
    {
        //
        // GET: /Employees/

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
            //等级分组
            ViewData["level_group"] = UserEmployeeService.GetLevelOption();

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult Index_Message()
        {
            JsonResult _result = new JsonResult();
            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            string _keyword = VariableHelper.SaferequestStr(Request.Form["keyword"]);
            int _levelid = VariableHelper.SaferequestInt(Request.Form["levelid"]);
            int _isdelete = VariableHelper.SaferequestInt(Request.Form["isdelete"]);
            using (DynamicRepository db = new DynamicRepository())
            {
                //搜索条件
                if (!string.IsNullOrEmpty(_keyword))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "((EmployeeEmail={0}) or (EmployeeName={0}))", Param = EncryptionBase.EncryptString(_keyword) });
                }

                if (_levelid > 0)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "LevelID={0}", Param = _levelid });
                }

                if (_isdelete == 1)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "IsLock=1", Param = null });
                }
                else
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "IsLock=0", Param = null });
                }

                //查询
                var _list = db.GetPage<View_UserEmployee>("select * from View_UserEmployee order by EmployeeID desc", _SqlWhere, VariableHelper.SaferequestInt(Request.Form["rows"]), VariableHelper.SaferequestInt(Request.Form["page"]));
                //数据解密
                foreach (var item in _list.Items)
                {
                    EncryptionFactory.Create(item).Decrypt();
                }
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.EmployeeID,
                               s1 = dy.EmployeeName,
                               s2 = dy.EmployeeEmail,
                               s3 = dy.LevelName,
                               s4 = $"<label class=\"label-info\">Amount</label>&nbsp;{((dy.IsAmountLimit) ? VariableHelper.FormateMoney(dy.TotalAmount) : "--")}<br/><label class=\"label-info\">Quantity</label>&nbsp;{((dy.IsQuantityLimit) ? dy.TotalQuantity.ToString() : "--")}",
                               s5 = $"{dy.BeginDate.ToString("yyyy/MM/dd")}-{dy.EndDate.ToString("yyyy/MM/dd")}",
                               s6 = dy.LeaveTime,
                               s7 = dy.Remark,
                               s8 = (!dy.IsLock) ? "<label class=\"fa fa-check color_primary\"></label>" : "<label class=\"fa fa-close color_danger\"></label>",
                           }
                };
                return _result;
            }
        }
        #endregion

        #region 添加
        [UserPowerAuthorize]
        public ActionResult Add()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;

            ViewData["level_group"] = UserEmployeeService.GetLevelOption();
            ViewData["group_list"] = UserEmployeeService.GetGroupOption();

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Add_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            string _Name = VariableHelper.SaferequestStr(Request.Form["Name"]);
            string _Email = VariableHelper.SaferequestStr(Request.Form["Email"]);
            int _LevelID = VariableHelper.SaferequestInt(Request.Form["LevelID"]);
            string _LeaveTime = VariableHelper.SaferequestStr(Request.Form["LeaveTime"]);
            string _Memo = VariableHelper.SaferequestEditor(Request.Form["Memo"]);
            int _Status = VariableHelper.SaferequestInt(Request.Form["Status"]);
            int _DataGroupID = 0;

            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {

                    try
                    {
                        if (string.IsNullOrEmpty(_Name))
                        {
                            throw new Exception(_LanguagePack["employees_edit_message_no_name"]);
                        }

                        if (string.IsNullOrEmpty(_Email))
                        {
                            throw new Exception(_LanguagePack["employees_edit_message_no_email"]);
                        }
                        else
                        {
                            string _ept_Email = EncryptionBase.EncryptString(_Email);
                            UserEmployee objUserEmployee = db.UserEmployee.Where(p => p.EmployeeEmail == _ept_Email).SingleOrDefault();
                            if (objUserEmployee != null)
                            {
                                throw new Exception(_LanguagePack["employees_edit_message_exist_email"]);
                            }
                        }

                        if (_LevelID == 0)
                        {
                            throw new Exception(_LanguagePack["employees_edit_message_no_limit"]);
                        }

                        //匹配时间段
                        _DataGroupID = UserEmployeeService.GetDataGroupID(DateTime.Today, db);

                        UserEmployee objData = new UserEmployee()
                        {
                            EmployeeName = _Name,
                            EmployeeEmail = _Email,
                            LevelID = _LevelID,
                            CurrentAmount = 0,
                            LeaveTime = _LeaveTime,
                            CurrentQuantity = 0,
                            DataGroupID = _DataGroupID,
                            Remark = _Memo,
                            IsLock = (_Status == 1),
                            AddTime = DateTime.Now,
                            EditTime = null
                        };
                        //数据加密
                        EncryptionFactory.Create(objData).Encrypt();
                        db.UserEmployee.Add(objData);
                        db.SaveChanges();
                        Trans.Commit();
                        //添加日志
                        AppLogService.InsertLog<UserEmployee>(objData, objData.EmployeeID.ToString());
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

        #region 编辑
        [UserPowerAuthorize]
        public ActionResult Edit()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;

            int _EmployeeID = VariableHelper.SaferequestInt(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                View_UserEmployee objView_UserEmployee = db.View_UserEmployee.Where(p => p.EmployeeID == _EmployeeID).SingleOrDefault();
                if (objView_UserEmployee != null)
                {
                    //数据解密
                    EncryptionFactory.Create(objView_UserEmployee).Decrypt();
                    ViewData["level_group"] = UserEmployeeService.GetLevelOption();
                    ViewData["group_list"] = UserEmployeeService.GetGroupOption();

                    return View(objView_UserEmployee);
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
            int _EmployeeID = VariableHelper.SaferequestInt(Request.Form["ID"]);
            string _Name = VariableHelper.SaferequestStr(Request.Form["Name"]);
            string _Email = VariableHelper.SaferequestStr(Request.Form["Email"]);
            int _LevelID = VariableHelper.SaferequestInt(Request.Form["LevelID"]);
            string _LeaveTime = VariableHelper.SaferequestStr(Request.Form["LeaveTime"]);
            string _Memo = VariableHelper.SaferequestEditor(Request.Form["Memo"]);
            int _Status = VariableHelper.SaferequestInt(Request.Form["Status"]);

            _Name = EncryptionBase.EncryptString(_Name);
            _Email = EncryptionBase.EncryptString(_Email);

            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_Name))
                    {
                        throw new Exception(_LanguagePack["employees_edit_message_no_name"]);
                    }

                    if (string.IsNullOrEmpty(_Email))
                    {
                        throw new Exception(_LanguagePack["employees_edit_message_no_email"]);
                    }
                    else
                    {
                        UserEmployee objUserEmployee = db.UserEmployee.Where(p => p.EmployeeEmail == _Email && p.EmployeeID != _EmployeeID).SingleOrDefault();
                        if (objUserEmployee != null)
                        {
                            throw new Exception(_LanguagePack["users_edit_message_exist_email"]);
                        }
                    }

                    if (_LevelID == 0)
                    {
                        throw new Exception(_LanguagePack["users_edit_message_no_limit"]);
                    }

                    UserEmployee objData = db.UserEmployee.Where(p => p.EmployeeID == _EmployeeID).SingleOrDefault();
                    if (objData != null)
                    {
                        objData.EmployeeName = _Name;
                        objData.EmployeeEmail = _Email;
                        objData.LevelID = _LevelID;
                        objData.LeaveTime = _LeaveTime;
                        objData.Remark = _Memo;
                        objData.IsLock = (_Status == 1);
                        objData.EditTime = DateTime.Now;
                        db.SaveChanges();
                        //添加日志
                        AppLogService.UpdateLog<UserEmployee>(objData, objData.EmployeeID.ToString());
                        //返回信息
                        _result.Data = new
                        {
                            result = true,
                            msg = _LanguagePack["common_data_save_success"]
                        };
                    }
                    else
                    {
                        throw new Exception(_LanguagePack["common_data_load_false"]);
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

                    UserEmployee objUserEmployee = new UserEmployee();
                    foreach (string _str in _IDs.Split(','))
                    {
                        Int64 _ID = VariableHelper.SaferequestInt64(_str);
                        objUserEmployee = db.UserEmployee.Where(p => p.EmployeeID == _ID).SingleOrDefault();
                        if (objUserEmployee != null)
                        {
                            objUserEmployee.IsLock = true;
                        }
                        else
                        {
                            throw new Exception(string.Format("{0}:{1}", _str, _LanguagePack["common_data_no_exsit"]));
                        }
                    }
                    db.SaveChanges();
                    //添加日志
                    AppLogService.DeleteLog("UserEmployee", _IDs);
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

                    UserEmployee objUserEmployee = new UserEmployee();
                    foreach (string _str in _IDs.Split(','))
                    {
                        Int64 _ID = VariableHelper.SaferequestInt64(_str);
                        objUserEmployee = db.UserEmployee.Where(p => p.EmployeeID == _ID).SingleOrDefault();
                        if (objUserEmployee != null)
                        {
                            objUserEmployee.IsLock = false;
                        }
                        else
                        {
                            throw new Exception(string.Format("{0}:{1}", _str, _LanguagePack["common_data_no_exsit"]));
                        }
                    }
                    db.SaveChanges();
                    //添加日志
                    AppLogService.DeleteLog("UserEmployee", _IDs);
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

        #region 导入Excel
        [UserPowerAuthorize]
        public ActionResult ImportExcel()
        {
            //加载语言包
            ViewBag.LanguagePack = GetLanguagePack;
            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult ImportExcel_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            string _filePath = VariableHelper.SaferequestNull(Request.Form["filepath"]);

            try
            {
                if (!string.IsNullOrEmpty(_filePath))
                {
                    if (System.IO.File.Exists(Server.MapPath(_filePath)))
                    {
                        int perPage = VariableHelper.SaferequestInt(Request.Form["rows"]);
                        int page = VariableHelper.SaferequestInt(Request.Form["page"]);
                        List<EmployeeImportDto> list = UserEmployeeService.ConvertToUserEmployees(Server.MapPath(_filePath)).ToList();
                        _result.Data = new
                        {
                            total = list.Count,
                            rows = list.Skip((page - 1) * perPage).Take(perPage).ToList()
                        };
                    }
                    else
                    {
                        throw new Exception(_LanguagePack["common_uploadfile_file_not_exist"]);
                    }
                }
                else
                {
                    throw new Exception(_LanguagePack["common_uploadfile_no_file"]);
                }
            }
            catch
            {
                //返回信息
                _result.Data = new
                {
                    total = 0,
                    rows = new List<EmployeeImportDto>()
                };
            }
            return _result;
        }

        [HttpPost]
        [UserLoginAuthorize(Type = BaseAuthorize.ResultType.Json)]
        public ActionResult ImportExcel_SaveUpload()
        {
            //加载语言包
            var _LanguagePack = GetLanguagePack;
            JsonResult _result = new JsonResult();

            //错误信息
            List<EmployeeImportDto> _errorList = new List<EmployeeImportDto>();

            //文件路径
            string _filePath = VariableHelper.SaferequestNull(Request.Form["filepath"]);
            //限制时间列表
            List<UserEmployeeLevel> objUserEmployeeLevel_List = UserEmployeeService.GetLevelOption();

            try
            {
                if (System.IO.File.Exists(Server.MapPath(_filePath)))
                {
                    //限制组
                    List<EmployeeImportDto> list = UserEmployeeService.ConvertToUserEmployees(Server.MapPath(_filePath)).ToList();
                    //时间组
                    int _DataGroupID = UserEmployeeService.GetDataGroupID(DateTime.Now);
                    foreach (var item in list)
                    {
                        ItemResult itemResult = UserEmployeeService.SaveUserEmployees(objUserEmployeeLevel_List, _DataGroupID, item);
                        if (!itemResult.Result)
                        {
                            //写入错误
                            item.Email = $"<span class=\"color_danger\">{item.Email}</span>";
                            item.Result = false;
                            item.ResultMsg = $"<span class=\"color_danger\">{itemResult.Message}</span>";
                            _errorList.Add(item);
                        }
                    }

                    //返回信息
                    if (_errorList.Count == 0)
                    {
                        _result.Data = new
                        {
                            result = true,
                            msg = _LanguagePack["common_data_save_success"],
                            rows = _errorList
                        };
                    }
                    else
                    {
                        _result.Data = new
                        {
                            result = true,
                            msg = _LanguagePack["common_partial_data_save_success"],
                            rows = _errorList
                        };
                    }
                }
                else
                {
                    throw new Exception(_LanguagePack["common_uploadfile_file_not_exist"]);
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

        #region 导出Excel
        [UserPowerAuthorize]
        public FileResult ExportExcel()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            string _keyword = VariableHelper.SaferequestStr(Request.Form["SearchKey"]);
            int _levelid = VariableHelper.SaferequestInt(Request.Form["LevelID"]);
            int _isdelete = VariableHelper.SaferequestInt(Request.Form["Deleted"]);

            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            using (DynamicRepository db = new DynamicRepository())
            {
                //搜索条件
                if (!string.IsNullOrEmpty(_keyword))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "(EmployeeEmail like {0} or EmployeeName like {0})", Param = "%" + _keyword + "%" });
                }

                if (_levelid > 0)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "LevelID={0}", Param = _levelid });
                }

                if (_isdelete == 1)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "IsLock=1", Param = null });
                }
                else
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "IsLock=0", Param = null });
                }

                //查询
                DataTable dt = new DataTable();
                dt.Columns.Add(_LanguagePack["employees_index_name"]);
                dt.Columns.Add(_LanguagePack["employees_index_email"]);
                dt.Columns.Add(_LanguagePack["employees_index_level"]);
                dt.Columns.Add(_LanguagePack["employees_index_limit"]);
                dt.Columns.Add(_LanguagePack["employees_index_leavetime"]);
                dt.Columns.Add(_LanguagePack["employees_index_remark"]);
                dt.Columns.Add(_LanguagePack["employees_index_effect"]);

                //读取数据
                DataRow _dr = null;
                //查询
                var _list = db.Fetch<View_UserEmployee>("select * from View_UserEmployee order by EmployeeID desc", _SqlWhere);
                foreach (var _dy in _list)
                {
                    //数据解
                    EncryptionFactory.Create(_dy).Decrypt();

                    _dr = dt.NewRow();
                    _dr[0] = _dy.EmployeeName;
                    _dr[1] = _dy.EmployeeEmail;
                    _dr[2] = _dy.LevelName;
                    _dr[3] = _dy.LevelKey;
                    _dr[4] = _dy.LeaveTime;
                    _dr[5] = _dy.Remark;
                    _dr[6] = _dy.IsLock ? 0 : 1;
                    dt.Rows.Add(_dr);
                }

                string _filepath = string.Format("~/UploadFile/CacheFile/{0}", DateTime.Now.ToString("yyyy-MM-dd"));
                if (!Directory.Exists(Server.MapPath(_filepath)))
                {
                    Directory.CreateDirectory(Server.MapPath(_filepath));
                }
                string _fileName = string.Format("Employees_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss"));
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
