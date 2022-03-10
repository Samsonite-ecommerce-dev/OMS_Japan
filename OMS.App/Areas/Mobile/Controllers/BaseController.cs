using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service;
using Samsonite.OMS.Service.AppConfig;
using Samsonite.OMS.Service.AppLanguage;

namespace OMS.App.Areas.Mobile.Controllers
{
    public class BaseController : Controller
    {
        private int _CurrentFunctionID = 0;
        public BaseController()
        {
            _CurrentFunctionID = this.CurrentFunctionID;
        }

        public string GetAreaName
        {
            get
            {
                return "MOBILE";
            }
        }

        /// <summary>
        /// 获取语言包
        /// </summary>
        public Dictionary<string, string> GetLanguagePack
        {
            get
            {
                return LanguageService.Get();
            }
        }

        /// <summary>
        /// 获取站点配置信息
        /// </summary>
        public ApplicationConfigDto GetApplicationConfig
        {
            get
            {
                return ConfigCache.Instance.Get();
            }
        }

        /// <summary>
        /// 获取当前登录信息
        /// </summary>
        public UserSessionInfo CurrentLoginUser
        {
            get
            {
                return UserLoginService.GetCurrentLoginUser();
            }
        }

        /// <summary>
        /// 当前页面ID
        /// </summary>
        public int CurrentFunctionID
        {
            get
            {
                return UserRoleService.GetCurrentFunctionID(this.GetAreaName);
            }
        }

        /// <summary>
        /// 获取当前功能栏权限
        /// </summary>
        /// <returns></returns>
        public string FunctionPowers()
        {
            string _result = string.Empty;
            UserSessionInfo objUserSessionInfo = UserLoginService.GetCurrentLoginUser();
            UserSessionInfo.UserPower objUserPower = objUserSessionInfo.UserPowers.Where(p => p.FunctionID == _CurrentFunctionID).FirstOrDefault();
            _result = string.Join(",", objUserPower.FunctionPower);
            return _result;
        }

        /// <summary>
        /// 获取当前功能栏权限
        /// </summary>
        /// <param name="objFunctionID"></param>
        /// <returns></returns>
        public string FunctionPowers(int objFunctionID)
        {
            string _result = string.Empty;
            UserSessionInfo objUserSessionInfo = UserLoginService.GetCurrentLoginUser();
            UserSessionInfo.UserPower objUserPower = objUserSessionInfo.UserPowers.Where(p => p.FunctionID == objFunctionID).FirstOrDefault();
            _result = string.Join(",", objUserPower.FunctionPower);
            return _result;
        }

        /// <summary>
        /// 下拉菜单栏
        /// </summary>
        /// <param name="objMenuList"></param>
        /// <returns></returns>
        public string MenuBar(List<object[]> objMenuList = null)
        {
            var _LanguagePack = this.GetLanguagePack;

            string _result = string.Empty;
            //默认菜单栏
            List<object[]> _defaultList = new List<object[]>() {
                new object[] { "glyphicon-home", _LanguagePack["home_index_index"],Url.Action("Index","Home") },
                new object[] { "glyphicon-share", _LanguagePack["home_index_logout"],Url.Action("LoginOut", "Login") }
            };
            if (objMenuList != null)
                _defaultList = objMenuList;
            _result += "<button class=\"btn dropdown-toggle\" type=\"button\" id=\"userMenu\" data-toggle=\"dropdown\"><i class=\"glyphicon glyphicon-th-list\"></i></button>";
            _result += "<ul class=\"dropdown-menu dropdown-menu-right\" role=\"menu\" aria-labelledby=\"userMenu\">";
            foreach (var _o in _defaultList)
            {
                _result += $"<li role=\"presentation\"><a role=\"menuitem\" tabindex=\" - 1\" href=\"{_o[2]}\"><i class=\"glyphicon {_o[0]}\" ></i>{_o[1]}</a></li>";
            }
            _result += "</ul>";
            return _result;
        }

        /// <summary>
        /// 获取功能名称
        /// </summary>
        /// <param name="objFunctionID"></param>
        /// <returns></returns>
        public string MenuName()
        {
            //加载语言包
            var _LanguagePack = GetLanguagePack;
            string _result = string.Empty;
            using (var db = new ebEntities())
            {
                SysFunction objSysFunction = db.SysFunction.Where(p => p.Funcid == _CurrentFunctionID).SingleOrDefault();
                if (objSysFunction != null)
                {
                    _result = _LanguagePack[$"menu_function_{objSysFunction.Funcid}"];
                }
            }
            return _result;
        }
    }
}
