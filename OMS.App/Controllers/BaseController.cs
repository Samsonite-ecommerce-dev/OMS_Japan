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

namespace OMS.App.Controllers
{
    public class BaseController : Controller
    {
        private int _CurrentFunctionID = 0;
        public BaseController()
        {

            _CurrentFunctionID = this.CurrentFunctionID;
        }

        public EntityRepository BaseEntityRepository
        {
            get
            {
                return new EntityRepository(); ;
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
                return UserRoleService.GetCurrentFunctionID();
            }
        }

        /// <summary>
        /// 获取当前功能栏权限
        /// </summary>
        /// <returns></returns>
        public string FunctionPowers()
        {
            return UserRoleService.GetFunctionPowers(_CurrentFunctionID);
        }

        /// <summary>
        /// 获取当前功能栏权限
        /// </summary>
        /// <param name="objFunctionID"></param>
        /// <returns></returns>
        public string FunctionPowers(int objFunctionID)
        {
            return UserRoleService.GetFunctionPowers(objFunctionID);
        }

        /// <summary>
        /// 获取当前导航栏
        /// </summary>
        /// <param name="objQuery"></param>
        /// <returns></returns>
        public string MenuBar(string objQuery = "")
        {
            //加载语言包
            var _LanguagePack = GetLanguagePack;
            return UserRoleService.GetMenuBar(_CurrentFunctionID, _LanguagePack, objQuery);
        }

        /// <summary>
        /// 获取当前导航栏
        /// </summary>
        /// <param name="objFunctionID"></param>
        /// <param name="objQuery"></param>
        /// <returns></returns>
        public string MenuBar(int objFunctionID, string objQuery = "")
        {
            //加载语言包
            var _LanguagePack = GetLanguagePack;
            return UserRoleService.GetMenuBar(objFunctionID, _LanguagePack, objQuery);
        }

        /// <summary>
        /// 获取功能名称
        /// </summary>
        /// <param name="objFunctionID"></param>
        /// <returns></returns>
        public string MenuName(int objFunctionID)
        {
            //加载语言包
            var _LanguagePack = GetLanguagePack;
            return UserRoleService.GetMenuName(objFunctionID, _LanguagePack);
        }

        /// <summary>
        /// 获取订单查询页面菜单Tab
        /// </summary>
        /// <returns></returns>
        public string GetSearchOrderTab()
        {
            //加载语言包
            var _LanguagePack = GetLanguagePack;
            return UserRoleService.GetSearchOrderTab(_LanguagePack);
        }

        /// <summary>
        /// 获取订单查询页面菜单Tab
        /// </summary>
        /// <returns></returns>
        public string GetSearchEmployeeOrderTab()
        {
            //加载语言包
            var _LanguagePack = GetLanguagePack;
            return UserRoleService.GetSearchEmployeeOrderTab(_LanguagePack);
        }
    }
}
