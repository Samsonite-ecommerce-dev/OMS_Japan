using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service;
using Samsonite.OMS.Service.AppLanguage;
using Samsonite.Utility.Common;
using OMS.App.Helper;

namespace OMS.App.Controllers
{
    public class UsersController : BaseController
    {
        //
        // GET: /Users/

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
            int _isdelete = VariableHelper.SaferequestInt(Request.Form["isdelete"]);
            using (var db = new DynamicRepository())
            {
                //搜索条件
                if (!string.IsNullOrEmpty(_keyword))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "(UserName like {0} or RealName like {0})", Param = "%" + _keyword + "%" });
                }

                if (_isdelete == 1)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "Status={0}", Param = (int)UserStatus.Locked });
                }
                else
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "Status in (" + (int)UserStatus.Normal + "," + (int)UserStatus.ExpiredPwd + ")", Param = null });
                }

                //查询
                var _list = db.GetPage<UserInfo>("select UserID,UserName,RealName,Remark,[Type],[Status] from UserInfo order by UserID desc", _SqlWhere, VariableHelper.SaferequestInt(Request.Form["rows"]), VariableHelper.SaferequestInt(Request.Form["page"]));
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.UserID,
                               s1 = dy.UserName,
                               s2 = dy.RealName,
                               s3 = dy.Remark,
                               s4 = UserHelper.GetUserTypeDisplay(dy.Type),
                               s5 = (dy.Status == (int)UserStatus.Locked) ? "<label class=\"fa fa-lock color_danger\"></label>" : "<label class=\"fa fa-check color_primary\"></label>",
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

            //角色组列表
            ViewData["role_list"] = FunctionService.GetRoleObject();
            //账号类型
            ViewData["usertype_list"] = UserHelper.UserTypeObject();
            //店铺选择框
            StringBuilder _str = new StringBuilder();
            var objMall_List = MallService.GetAllMallOption();
            if (objMall_List.Count > 0)
            {
                foreach (var _m in new List<int>() { (int)MallType.OnLine, (int)MallType.OffLine })
                {
                    var _list = objMall_List.Where(p => p.MallType == _m).ToList();
                    if (_list.Count > 0)
                    {
                        _str.AppendFormat("<h3>{0}</h3>", MallHelper.GetMallTypeDisplay(_m));
                        _str.Append("<ul>");
                        foreach (var _o in _list)
                        {
                            _str.AppendFormat("<li><input id=\"mall_{0}\" name=\"Mall\" type=\"checkbox\" value=\"{2}\" /><label for=\"mall_{0}\">{1}</label></li>", _o.Id, _o.Name, _o.SapCode);
                        }
                        _str.Append("</ul>");
                    }
                }
            }
            ViewData["mall_list"] = _str;
            //语言包集合
            ViewData["language_list"] = LanguageService.CurrentLanguageOption();
            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Add_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            string _UserName = VariableHelper.SaferequestStr(Request.Form["UserName"]);
            string _RealName = VariableHelper.SaferequestStr(Request.Form["RealName"]);
            string _Password = Request.Form["Password"];
            string _RoleIDs = Request.Form["RoleID"];
            string _MallIDs = Request.Form["Mall"];
            int _DefaultLanguage = VariableHelper.SaferequestInt(Request.Form["DefaultLanguage"]);
            string _Memo = VariableHelper.SaferequestEditor(Request.Form["Memo"]);
            int _UserType = VariableHelper.SaferequestInt(Request.Form["UserType"]);

            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_UserName))
                        {
                            throw new Exception(_LanguagePack["users_edit_message_no_username"]);
                        }

                        UserInfo objUserInfo = db.UserInfo.Where(p => p.UserName == _UserName).SingleOrDefault();
                        if (objUserInfo != null)
                        {
                            throw new Exception(_LanguagePack["users_edit_message_exist_username"]);
                        }

                        if (string.IsNullOrEmpty(_Password))
                        {
                            throw new Exception(_LanguagePack["users_edit_message_no_password"]);
                        }

                        if (!CheckHelper.ValidPassword(_Password))
                        {
                            throw new Exception(_LanguagePack["users_edit_message_password_error"]);
                        }

                        if (string.IsNullOrEmpty(_RoleIDs))
                        {
                            throw new Exception(_LanguagePack["users_edit_message_select_role"]);
                        }

                        if (string.IsNullOrEmpty(_MallIDs))
                        {
                            throw new Exception(_LanguagePack["users_edit_message_select_mall"]);
                        }

                        UserInfo objData = new UserInfo()
                        {
                            UserName = _UserName,
                            RealName = _RealName,
                            Pwd = UserLoginService.EncryptPassword(_Password),
                            Remark = _Memo,
                            DefaultLanguage = _DefaultLanguage,
                            Status = (int)UserStatus.ExpiredPwd,
                            Type = _UserType,
                            LastPwdEditTime = DateTime.Now,
                            AddTime = DateTime.Now
                        };
                        db.UserInfo.Add(objData);
                        db.SaveChanges();
                        //添加相关角色组
                        UserRoles objUserRoles = new UserRoles();
                        foreach (string _str in _RoleIDs.Split(','))
                        {
                            objUserRoles = new UserRoles()
                            {
                                Userid = (int)objData.UserID,
                                Roleid = VariableHelper.SaferequestInt(_str),
                            };
                            db.UserRoles.Add(objUserRoles);
                        }
                        db.SaveChanges();
                        //添加店铺权限
                        UserMalls objUserMalls = new UserMalls();
                        foreach (string _str in _MallIDs.Split(','))
                        {
                            objUserMalls = new UserMalls()
                            {
                                Userid = (int)objData.UserID,
                                MallSapCode = _str,
                            };
                            db.UserMalls.Add(objUserMalls);
                        }
                        db.SaveChanges();
                        Trans.Commit();
                        //添加密码日志
                        AppLogService.PasswordLog(new WebAppPasswordLog()
                        {
                            Account = _UserName,
                            Password = UserLoginService.EncryptPassword(_Password),
                            UserID = objData.UserID,
                            IP = Samsonite.Utility.Common.UrlHelper.GetRequestIP(),
                            Remark = string.Empty,
                            AddTime = DateTime.Now
                        });
                        //添加日志
                        AppLogService.InsertLog<UserInfo>(objData, objData.UserID.ToString());
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
            ViewBag.LanguagePack = this.GetLanguagePack;
            //角色组列表
            ViewData["role_list"] = FunctionService.GetRoleObject();
            //账号类型
            ViewData["usertype_list"] = UserHelper.UserTypeObject();
            //语言包集合
            ViewData["language_list"] = LanguageService.CurrentLanguageOption();

            int _UserID = VariableHelper.SaferequestInt(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                UserInfo objUser = db.UserInfo.Where(p => p.UserID == _UserID).SingleOrDefault();
                if (objUser != null)
                {
                    //拥有的角色组
                    ViewData["user_role"] = db.UserRoles.Where(p => p.Userid == _UserID).Select(p => p.Roleid).ToList();
                    //店铺选择框
                    List<UserMalls> objUserMalls_List = db.UserMalls.Where(p => p.Userid == objUser.UserID).ToList();
                    StringBuilder _str = new StringBuilder();
                    var objMall_List = MallService.GetAllMallOption();
                    if (objMall_List.Count > 0)
                    {
                        foreach (var _m in new List<int>() { (int)MallType.OnLine, (int)MallType.OffLine })
                        {
                            var _list = objMall_List.Where(p => p.MallType == _m).ToList();
                            if (_list.Count > 0)
                            {
                                _str.AppendFormat("<h3>{0}</h3>", MallHelper.GetMallTypeDisplay(_m));
                                _str.Append("<ul>");
                                foreach (var _o in _list)
                                {
                                    if (objUserMalls_List.Select(p => p.MallSapCode).Contains(_o.SapCode))
                                    {
                                        _str.AppendFormat("<li><input id=\"mall_{0}\" name=\"Mall\" type=\"checkbox\" value=\"{2}\" checked=\"checked\" /><label for=\"mall_{0}\">{1}</label></li>", _o.Id, _o.Name, _o.SapCode);
                                    }
                                    else
                                    {
                                        _str.AppendFormat("<li><input id=\"mall_{0}\" name=\"Mall\" type=\"checkbox\" value=\"{2}\" /><label for=\"mall_{0}\">{1}</label></li>", _o.Id, _o.Name, _o.SapCode);
                                    }
                                }
                                _str.Append("</ul>");
                            }
                        }
                    }
                    ViewData["mall_list"] = _str;

                    return View(objUser);
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
            int _UserID = VariableHelper.SaferequestInt(Request.Form["ID"]);
            string _RealName = VariableHelper.SaferequestStr(Request.Form["RealName"]);
            string _RoleIDs = Request.Form["RoleID"];
            string _MallIDs = Request.Form["Mall"];
            int _DefaultLanguage = VariableHelper.SaferequestInt(Request.Form["DefaultLanguage"]);
            string _Memo = VariableHelper.SaferequestEditor(Request.Form["Memo"]);
            int _UserType = VariableHelper.SaferequestInt(Request.Form["UserType"]);
            int _Status = VariableHelper.SaferequestInt(Request.Form["Status"]);

            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_RoleIDs))
                        {
                            throw new Exception(_LanguagePack["users_edit_message_select_role"]);
                        }

                        if (string.IsNullOrEmpty(_MallIDs))
                        {
                            throw new Exception(_LanguagePack["users_edit_message_select_mall"]);
                        }

                        UserInfo objData = db.UserInfo.Where(p => p.UserID == _UserID).SingleOrDefault();
                        if (objData != null)
                        {
                            objData.RealName = _RealName;
                            objData.DefaultLanguage = _DefaultLanguage;
                            objData.Remark = _Memo;
                            if (objData.Status == (int)UserStatus.ExpiredPwd)
                            {
                                objData.Status = (_Status == 0) ? (int)UserStatus.Locked : (int)UserStatus.ExpiredPwd;
                            }
                            else
                            {
                                objData.Status = (_Status == 0) ? (int)UserStatus.Locked : (int)UserStatus.Normal;
                            }

                            objData.Type = _UserType;
                            db.SaveChanges();
                            //删除原角色组
                            db.Database.ExecuteSqlCommand("delete from UserRoles where Userid={0}", objData.UserID);
                            //添加相关角色组
                            UserRoles objUserRoles = new UserRoles();
                            foreach (string _str in _RoleIDs.Split(','))
                            {
                                objUserRoles = new UserRoles()
                                {
                                    Userid = (int)objData.UserID,
                                    Roleid = VariableHelper.SaferequestInt(_str),
                                };
                                db.UserRoles.Add(objUserRoles);
                            }
                            db.SaveChanges();
                            //删除原店铺权限
                            db.Database.ExecuteSqlCommand("delete from UserMalls where Userid={0}", objData.UserID);
                            //添加店铺权限
                            UserMalls objUserMalls = new UserMalls();
                            foreach (string _str in _MallIDs.Split(','))
                            {
                                objUserMalls = new UserMalls()
                                {
                                    Userid = (int)objData.UserID,
                                    MallSapCode = _str,
                                };
                                db.UserMalls.Add(objUserMalls);
                            }
                            db.SaveChanges();
                            Trans.Commit();
                            //添加日志
                            AppLogService.UpdateLog<UserInfo>(objData, objData.UserID.ToString());
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
                try
                {
                    if (string.IsNullOrEmpty(_IDs))
                    {
                        throw new Exception(_LanguagePack["common_data_need_one"]);
                    }

                    UserInfo objUserInfo = new UserInfo();
                    foreach (string _str in _IDs.Split(','))
                    {
                        Int64 _ID = VariableHelper.SaferequestInt64(_str);
                        objUserInfo = db.UserInfo.Where(p => p.UserID == _ID).SingleOrDefault();
                        if (objUserInfo != null)
                        {
                            objUserInfo.Status = 1;
                        }
                        else
                        {
                            throw new Exception(string.Format("{0}:{1}", _str, _LanguagePack["common_data_no_exsit"]));
                        }
                    }
                    db.SaveChanges();
                    //添加日志
                    AppLogService.DeleteLog("UserInfo", _IDs);
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

                    UserInfo objUserInfo = new UserInfo();
                    foreach (string _str in _IDs.Split(','))
                    {
                        Int64 _ID = VariableHelper.SaferequestInt64(_str);
                        objUserInfo = db.UserInfo.Where(p => p.UserID == _ID).SingleOrDefault();
                        if (objUserInfo != null)
                        {
                            objUserInfo.Status = 0;
                        }
                        else
                        {
                            throw new Exception(string.Format("{0}:{1}", _str, _LanguagePack["common_data_no_exsit"]));
                        }
                    }
                    db.SaveChanges();
                    //添加日志
                    AppLogService.DeleteLog("UserInfo", _IDs);
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

        #region 编辑密码
        [UserPowerAuthorize]
        public ActionResult EditPassword()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;

            int _UserID = VariableHelper.SaferequestInt(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                UserInfo objUserInfo = db.UserInfo.Where(p => p.UserID == _UserID).SingleOrDefault();
                if (objUserInfo != null)
                {
                    return View(objUserInfo);
                }
                else
                {
                    return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.NoMessage });
                }
            }
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult EditPassword_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            int _UserID = VariableHelper.SaferequestInt(Request.Form["ID"]);
            string _Password = Request.Form["Password"];
            string _SurePassword = Request.Form["SurePassword"];

            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_Password))
                    {
                        throw new Exception(_LanguagePack["users_editpassword_message_no_password"]);
                    }

                    if (!CheckHelper.ValidPassword(_Password))
                    {
                        throw new Exception(_LanguagePack["users_editpassword_message_password_error"]);
                    }

                    if (string.IsNullOrEmpty(_SurePassword))
                    {
                        throw new Exception(_LanguagePack["users_editpassword_message_no_reset_password"]);
                    }

                    if (_Password != _SurePassword)
                    {
                        throw new Exception(_LanguagePack["users_editpassword_message_not_same"]);
                    }

                    UserInfo objData = db.UserInfo.Where(p => p.UserID == _UserID).SingleOrDefault();
                    if (objData != null)
                    {
                        //检查是否存在N次密码修改存在重复
                        using (var db1 = new logEntities())
                        {
                            string _encryptPWD = UserLoginService.EncryptPassword(_Password);
                            List<string> objWebAppPasswordLog_List = db1.WebAppPasswordLog.Where(p => p.UserID == objData.UserID).OrderByDescending(p => p.LogID).Select(p => p.Password).Take(AppGlobalService.PWD_PAST_NUM).ToList();
                            if (objWebAppPasswordLog_List.Contains(_encryptPWD))
                            {
                                throw new Exception(_LanguagePack["users_editpassword_message_password_repeat_error"]);
                            }
                        }

                        objData.Pwd = UserLoginService.EncryptPassword(_Password);
                        objData.LastPwdEditTime = DateTime.Now;
                        db.SaveChanges();
                        //添加密码日志
                        AppLogService.PasswordLog(new WebAppPasswordLog()
                        {
                            Account = objData.UserName,
                            Password = UserLoginService.EncryptPassword(_Password),
                            UserID = objData.UserID,
                            IP = Samsonite.Utility.Common.UrlHelper.GetRequestIP(),
                            Remark = string.Empty,
                            AddTime = DateTime.Now
                        });
                        //添加日志
                        AppLogService.UpdateLog<UserInfo>(objData, objData.UserID.ToString());
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
                return _result;
            }
        }
        #endregion
    }
}
