using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;

namespace Samsonite.OMS.Service
{
    public class UserRoleService
    {
        /// <summary>
        /// 根据获取当前页面的FuncID
        /// </summary>
        /// <param name="objAreas">如果是Area目录在的,此处需要增加Area目录名</param>
        /// <returns></returns>
        public static int GetCurrentFunctionID(string objAreas = "")
        {
            string _currentUrl = System.Web.HttpContext.Current.Request.Url.AbsolutePath;
            int t = _currentUrl.IndexOf("?");
            if (t > -1) _currentUrl = _currentUrl.Substring(0, t);
            using (var db = new ebEntities())
            {
                int _FunID = 0;
                if (!string.IsNullOrEmpty(_currentUrl))
                {
                    //取功能标识
                    if (_currentUrl.IndexOf("/") > -1)
                    {
                        string[] _urlArray = _currentUrl.Split('/');
                        string _func_sign = string.Empty;
                        //因为有时候会省略index,所以需要加以分别判断
                        if (!string.IsNullOrEmpty(objAreas))
                        {
                            if (objAreas.ToUpper() == _urlArray[1].ToUpper())
                            {
                                _func_sign = _urlArray[2];
                            }
                            else
                            {
                                _func_sign = _urlArray[1];
                            }
                        }
                        else
                        {
                            _func_sign = _urlArray[1];
                        }

                        SysFunction objSysFunction = db.SysFunction.Where(p => p.FuncSign.ToLower() == _func_sign.ToLower()).SingleOrDefault();
                        if (objSysFunction != null)
                        {
                            _FunID = objSysFunction.Funcid;
                        }
                    }
                }
                return _FunID;
            }
        }

        /// <summary>
        /// 获取功能权限
        /// </summary>
        /// <param name="objFunctionID"></param>
        /// <returns></returns>
        public static string GetFunctionPowers(int objFunctionID)
        {
            string _result = string.Empty;
            UserSessionInfo objUserSessionInfo = UserLoginService.GetCurrentLoginUser();
            UserSessionInfo.UserPower objUserPower = objUserSessionInfo.UserPowers.Where(p => p.FunctionID == objFunctionID).FirstOrDefault();
            if (objUserPower != null)
            {
                _result = string.Join(",", objUserPower.FunctionPower);
            }
            return _result;
        }

        /// <summary>
        /// 获取当前导航栏
        /// </summary>
        /// <param name="objFunctionID"></param>
        /// <param name="objLanguagePack"></param>
        /// <param name="objQuery"></param>
        /// <returns></returns>
        public static string GetMenuBar(int objFunctionID, Dictionary<string, string> objLanguagePack, string objQuery = "")
        {
            string _result = string.Empty;
            using (var db = new ebEntities())
            {
                SysFunction objSysFunction = db.SysFunction.Where(p => p.Funcid == objFunctionID).SingleOrDefault();
                if (objSysFunction != null)
                {
                    string _url = $"/{objSysFunction.FuncUrl}";
                    if (!string.IsNullOrEmpty(objQuery))
                    {
                        _url += "?" + objQuery;
                    }
                    _result += string.Format("<a href=\"{0}\">{1}</a>", _url, objLanguagePack[$"menu_function_{objSysFunction.Funcid}"]);
                    SysFunctionGroup objSysFunctionGroup = db.SysFunctionGroup.Where(p => p.Groupid == objSysFunction.Groupid).SingleOrDefault();
                    if (objSysFunctionGroup != null)
                    {
                        _result = string.Format("<i class=\"fa fa-home\"></i>{0}", objLanguagePack[$"menu_group_{objSysFunctionGroup.Groupid}"]) + _result;
                    }
                }
            }
            return _result;
        }

        /// <summary>
        /// 获取功能名称
        /// </summary>
        /// <param name="objFunctionID"></param>
        /// <param name="objLanguagePack"></param>
        /// <returns></returns>
        public static string GetMenuName(int objFunctionID, Dictionary<string, string> objLanguagePack)
        {
            string _result = string.Empty;
            using (var db = new ebEntities())
            {
                SysFunction objSysFunction = db.SysFunction.Where(p => p.Funcid == objFunctionID).SingleOrDefault();
                if (objSysFunction != null)
                {
                    _result = objLanguagePack[$"menu_function_{objSysFunction.Funcid}"];
                }
            }
            return _result;
        }

        /// <summary>
        /// 获取订单查询页面菜单Tab
        /// </summary>
        /// <param name="objLanguagePack"></param>
        /// <returns></returns>
        public static string GetSearchOrderTab(Dictionary<string, string> objLanguagePack)
        {
            string _result = string.Empty;
            using (var db = new ebEntities())
            {
                SysFunction objSysFunction = db.SysFunction.Where(p => p.Funcid == 1).SingleOrDefault();
                if (objSysFunction != null)
                {
                    _result = objLanguagePack[$"menu_function_{objSysFunction.Funcid}"];
                }
            }
            return _result;
        }

        /// <summary>
        /// 获取订单查询页面菜单Tab
        /// </summary>
        /// <param name="objLanguagePack"></param>
        /// <returns></returns>
        public static string GetSearchEmployeeOrderTab(Dictionary<string, string> objLanguagePack)
        {
            string _result = string.Empty;
            using (var db = new ebEntities())
            {
                SysFunction objSysFunction = db.SysFunction.Where(p => p.Funcid == 66).SingleOrDefault();
                if (objSysFunction != null)
                {
                    _result = objLanguagePack[$"menu_function_{objSysFunction.Funcid}"];
                }
            }
            return _result;
        }
    }
}