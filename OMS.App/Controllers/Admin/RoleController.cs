using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service;
using Samsonite.Utility.Common;
using OMS.App.Helper;

namespace OMS.App.Controllers
{
    public class RoleController : BaseController
    {
        //
        // GET: /Role/
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

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult Index_Message()
        {
            JsonResult _result = new JsonResult();
            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            string _keyword = VariableHelper.SaferequestStr(Request.Form["keyword"]);
            using (var db = new DynamicRepository())
            {
                //搜索条件
                if (!string.IsNullOrEmpty(_keyword))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "RoleName like {0}", Param = "%" + _keyword + "%" });
                }
                //查询
                var _list = db.GetPage<SysRole>("select Roleid,RoleName,SeqNumber,RoleMemo from SysRole order by SeqNumber asc", _SqlWhere, VariableHelper.SaferequestInt(Request.Form["rows"]), VariableHelper.SaferequestInt(Request.Form["page"]));
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.Roleid,
                               s1 = dy.RoleName,
                               s2 = dy.SeqNumber,
                               s3 = dy.RoleMemo
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
            var _LanguagePack = this.GetLanguagePack;
            ViewBag.LanguagePack = _LanguagePack;

            //栏目列表
            var objSysFunctionGroup_List = FunctionService.GetFunctionGroupObject();
            //功能列表
            var objSysFunction_List = FunctionService.GetFunctionObject().Where(p => p.IsShow);
            //权限选择框
            StringBuilder _str = new StringBuilder();
            if (objSysFunctionGroup_List.Count() > 0)
            {
                _str.Append("<div class=\"power_list\">");
                _str.Append("<ul>");
                foreach (var _SysFunctionGroup in objSysFunctionGroup_List)
                {
                    _str.Append("<li>");
                    _str.Append("<dl>");
                    _str.AppendFormat("<dt>{0}</dt>", _LanguagePack[string.Format("menu_group_{0}", _SysFunctionGroup.Groupid)]);
                    foreach (var _SysFunction in objSysFunction_List.Where(p => p.Groupid == _SysFunctionGroup.Groupid))
                    {
                        _str.Append("<dd>");
                        _str.AppendFormat("<input id=\"for_{0}_0\" name=\"power_check\" type=\"checkbox\" value=\"{0}\" /><label for=\"for_{0}_0\">{1}</label>", _SysFunction.Funcid, _LanguagePack[string.Format("menu_function_{0}", _SysFunction.Funcid)]);
                        if (!string.IsNullOrEmpty(_SysFunction.FuncPower))
                        {
                            _str.Append("<span class=\"detail_power\">");
                            foreach (var _power in JsonHelper.JsonDeserialize<List<DefineUserPower>>(_SysFunction.FuncPower))
                            {
                                _str.AppendFormat("<input id=\"for_{0}_{2}\" name=\"power_{0}\" type=\"checkbox\" value=\"{2}\" /><label for=\"for_{0}_{2}\">{1}</label>", _SysFunction.Funcid, _power.Name, _power.Value);
                            }
                            _str.Append("</span>");
                        }
                        _str.Append("</dd>");
                    }
                    _str.Append("</dl>");
                    _str.Append("</li>");
                }
                _str.Append("</ul>");
                _str.Append("</div>");
            }
            ViewData["power_list"] = _str;
            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Add_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            string _RoleName = VariableHelper.SaferequestStr(Request.Form["RoleName"]);
            string _RoleMemo = VariableHelper.SaferequestEditor(Request.Form["RoleMemo"]);
            //权限
            string _FunctionIDs = Request.Form["power_check"];

            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_RoleName))
                        {
                            throw new Exception(_LanguagePack["role_edit_message_no_rolename"]);
                        }

                        SysRole objSysRole = db.SysRole.Where(p => p.RoleName == _RoleName).SingleOrDefault();
                        if (objSysRole != null)
                        {
                            throw new Exception(_LanguagePack["role_edit_message_exist_rolename"]);
                        }

                        if (string.IsNullOrEmpty(_FunctionIDs))
                        {
                            throw new Exception(_LanguagePack["role_edit_message_need_one_function"]);
                        }

                        string[] _FunctionID_Array = _FunctionIDs.Split(',');
                        List<object[]> _PowerList = new List<object[]>();
                        foreach (string _str in _FunctionID_Array)
                        {
                            if (Request.Form["power_" + _str] != null)
                            {
                                _PowerList.Add(new object[2] { _str, Request.Form["power_" + _str] });
                            }
                        }

                        int _SeqNumberID = db.Database.SqlQuery<int>("select isnull(Max(SeqNumber),0) as MaxID from SysRole").SingleOrDefault() + 1;
                        SysRole objData = new SysRole()
                        {
                            RoleName = _RoleName,
                            SeqNumber = _SeqNumberID,
                            RoleMemo = _RoleMemo,
                            CreateTime = DateTime.Now
                        };
                        db.SysRole.Add(objData);
                        db.SaveChanges();
                        //添加相关功能
                        SysRoleFunction objSysRoleFunction = new SysRoleFunction();
                        foreach (object[] _O in _PowerList)
                        {
                            objSysRoleFunction = new SysRoleFunction()
                            {
                                Funid = VariableHelper.SaferequestInt(_O[0]),
                                Roleid = objData.Roleid,
                                Powers = _O[1].ToString().ToLower()
                            };
                            db.SysRoleFunction.Add(objSysRoleFunction);
                        }
                        db.SaveChanges();
                        Trans.Commit();
                        //添加日志
                        AppLogService.InsertLog<SysRole>(objData, objData.Roleid.ToString());
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

        #region 编辑
        [UserPowerAuthorize]
        public ActionResult Edit()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;
            ViewBag.LanguagePack = _LanguagePack;

            int _RoleID = VariableHelper.SaferequestInt(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                StringBuilder _str = new StringBuilder();
                SysRole objSysRole = db.SysRole.Where(p => p.Roleid == _RoleID).SingleOrDefault();
                if (objSysRole != null)
                {
                    //栏目列表
                    var objSysFunctionGroup_List = FunctionService.GetFunctionGroupObject();
                    //功能列表
                    var objSysFunction_List = FunctionService.GetFunctionObject().Where(p => p.IsShow);
                    //当前的功能
                    var objSysRoleFunction_List = db.SysRoleFunction.Where(p => p.Roleid == _RoleID);
                    SysRoleFunction objSysRoleFunction = new SysRoleFunction();
                    //权限选择框
                    if (objSysFunctionGroup_List.Count() > 0)
                    {
                        _str.Append("<div class=\"power_list\">");
                        _str.Append("<ul>");
                        foreach (var _SysFunctionGroup in objSysFunctionGroup_List)
                        {
                            _str.Append("<li>");
                            _str.Append("<dl>");
                            _str.AppendFormat("<dt>{0}</dt>", _LanguagePack[string.Format("menu_group_{0}", _SysFunctionGroup.Groupid)]);
                            foreach (var _SysFunction in objSysFunction_List.Where(p => p.Groupid == _SysFunctionGroup.Groupid))
                            {
                                objSysRoleFunction = objSysRoleFunction_List.Where(p => p.Funid == _SysFunction.Funcid).SingleOrDefault();
                                if (objSysRoleFunction != null)
                                {
                                    _str.Append("<dd>");
                                    _str.AppendFormat("<input id=\"for_{0}_0\" name=\"power_check\" type=\"checkbox\" value=\"{0}\" checked=\"checked\" /><label for=\"for_{0}_0\">{1}</label>", _SysFunction.Funcid, _LanguagePack[string.Format("menu_function_{0}", _SysFunction.Funcid)]);
                                    if (!string.IsNullOrEmpty(_SysFunction.FuncPower))
                                    {
                                        _str.Append("<span class=\"detail_power\">");
                                        foreach (var _power in JsonHelper.JsonDeserialize<List<DefineUserPower>>(_SysFunction.FuncPower))
                                        {
                                            if (("," + objSysRoleFunction.Powers.ToLower() + ",").IndexOf("," + _power.Value.ToLower() + ",") > -1)
                                            {
                                                _str.AppendFormat("<input id=\"for_{0}_{2}\" name=\"power_{0}\" type=\"checkbox\" value=\"{2}\" checked=\"checked\" /><label for=\"for_{0}_{2}\">{1}</label>", _SysFunction.Funcid, _power.Name, _power.Value);
                                            }
                                            else
                                            {
                                                _str.AppendFormat("<input id=\"for_{0}_{2}\" name=\"power_{0}\" type=\"checkbox\" value=\"{2}\" /><label for=\"for_{0}_{2}\">{1}</label>", _SysFunction.Funcid, _power.Name, _power.Value);
                                            }
                                        }
                                        _str.Append("</span>");
                                    }
                                    _str.Append("</dd>");
                                }
                                else
                                {
                                    _str.Append("<dd>");
                                    _str.AppendFormat("<input id=\"for_{0}_0\" name=\"power_check\" type=\"checkbox\" value=\"{0}\" /><label for=\"for_{0}_0\">{1}</label>", _SysFunction.Funcid, _LanguagePack[string.Format("menu_function_{0}", _SysFunction.Funcid)]);
                                    if (!string.IsNullOrEmpty(_SysFunction.FuncPower))
                                    {
                                        _str.Append("<span class=\"detail_power\">");
                                        foreach (var _power in JsonHelper.JsonDeserialize<List<DefineUserPower>>(_SysFunction.FuncPower))
                                        {
                                            _str.AppendFormat("<input id=\"for_{0}_{2}\" name=\"power_{0}\" type=\"checkbox\" value=\"{2}\" /><label for=\"for_{0}_{2}\">{1}</label>", _SysFunction.Funcid, _power.Name, _power.Value);
                                        }
                                        _str.Append("</span>");
                                    }
                                    _str.Append("</dd>");
                                }
                            }
                            _str.Append("</dl>");
                            _str.Append("</li>");
                        }
                        _str.Append("</ul>");
                        _str.Append("</div>");
                    }
                    ViewData["power_list"] = _str;

                    return View(objSysRole);
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
            int _RoleID = VariableHelper.SaferequestInt(Request.Form["ID"]);
            string _RoleName = VariableHelper.SaferequestStr(Request.Form["RoleName"]);
            int _SeqNumber = VariableHelper.SaferequestInt(Request.Form["OrderID"]);
            string _RoleMemo = VariableHelper.SaferequestEditor(Request.Form["RoleMemo"]);
            //权限
            string _FunctionIDs = Request.Form["power_check"];

            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_RoleName))
                        {
                            throw new Exception(_LanguagePack["role_edit_message_no_rolename"]);
                        }

                        SysRole objSysRole = db.SysRole.Where(p => p.RoleName == _RoleName && p.Roleid != _RoleID).SingleOrDefault();
                        if (objSysRole != null)
                        {
                            throw new Exception(_LanguagePack["role_edit_message_exist_rolename"]);
                        }

                        if (string.IsNullOrEmpty(_FunctionIDs))
                        {
                            throw new Exception(_LanguagePack["role_edit_message_need_one_function"]);
                        }

                        string[] _FunctionID_Array = _FunctionIDs.Split(',');
                        List<object[]> _PowerList = new List<object[]>();
                        foreach (string _str in _FunctionID_Array)
                        {
                            if (Request.Form["power_" + _str] != null)
                            {
                                _PowerList.Add(new object[2] { _str, Request.Form["power_" + _str] });
                            }
                        }

                        SysRole objData = db.SysRole.Where(p => p.Roleid == _RoleID).SingleOrDefault();
                        if (objData != null)
                        {
                            objData.RoleName = _RoleName;
                            objData.SeqNumber = _SeqNumber;
                            objData.RoleMemo = _RoleMemo;
                            db.SaveChanges();
                            //删除原功能
                            db.Database.ExecuteSqlCommand("delete from SysRoleFunction where Roleid={0}", _RoleID);
                            //添加相关功能
                            SysRoleFunction objSysRoleFunction = new SysRoleFunction();
                            foreach (object[] _O in _PowerList)
                            {
                                objSysRoleFunction = new SysRoleFunction()
                                {
                                    Funid = VariableHelper.SaferequestInt(_O[0]),
                                    Roleid = objData.Roleid,
                                    Powers = _O[1].ToString().ToLower()
                                };
                                db.SysRoleFunction.Add(objSysRoleFunction);
                            }
                            db.SaveChanges();
                            Trans.Commit();
                            //添加日志
                            AppLogService.UpdateLog<SysRole>(objData, objData.Roleid.ToString());
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

                        SysRole objSysRole = new SysRole();
                        foreach (string _str in _IDs.Split(','))
                        {
                            int _ID = VariableHelper.SaferequestInt(_str);
                            objSysRole = db.SysRole.Where(p => p.Roleid == _ID).SingleOrDefault();
                            if (objSysRole != null)
                            {
                                db.Database.ExecuteSqlCommand("delete from SysRoleFunction where Roleid ={0}", _str);
                                db.SysRole.Remove(objSysRole);
                            }
                            else
                            {
                                throw new Exception(string.Format("{0}:{1}", _str, _LanguagePack["common_data_no_exsit"]));
                            }
                        }
                        db.SaveChanges();
                        Trans.Commit();
                        //添加日志
                        AppLogService.DeleteLog("SysRole,SysRoleFunction", _IDs);
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
    }
}
