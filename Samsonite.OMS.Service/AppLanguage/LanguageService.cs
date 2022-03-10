using System;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Service.AppConfig;
using Samsonite.Utility.Common;

namespace Samsonite.OMS.Service.AppLanguage
{
    public class LanguageService
    {
        private static string _CacheName = $"{AppGlobalService.CACHE_KEY}_LANGUAGE_PACK";
        private static string _CookieName = $"{AppGlobalService.COOKIE_KEY}_LANGUAGE_PACK";

        /// <summary>
        /// 设置当前语言包
        /// </summary>
        /// <param name="objType"></param>
        public static void SetLanguage(int objType)
        {
            Dictionary<string, string> objDictionary = new Dictionary<string, string>();
            objDictionary.Add("LgPack", EncryptHelper.EncryptPassWord(objType.ToString()));
            CookieHelper.InsertCookie(_CookieName, objDictionary, 30);
        }

        /// <summary>
        /// 获取当前语言包类型
        /// </summary>
        public static int CurrentLanguage
        {
            get
            {
                if (CookieHelper.ExistCookie(_CookieName))
                {
                    return VariableHelper.SaferequestInt(EncryptHelper.DecryptPassWord(CookieHelper.GetCookie(_CookieName, "LgPack").ToString()));
                }
                else
                {
                    //如果不存在cookie,则读取用户的默认语言
                    UserSessionInfo objUserSessionInfo = UserLoginService.GetCurrentLoginUser();
                    if (objUserSessionInfo != null)
                    {
                        return objUserSessionInfo.DefaultLanguage;
                    }
                    else
                    {
                        //如果没有设置语言,则获取默认语言包
                        return LanguageType.DefaultLanguagePack().ID;
                    }
                }
            }
        }

        /// <summary>
        /// 获取当前加载语言文件后缀
        /// </summary>
        public static string CurrentLanguageFile
        {
            get
            {
                return LanguageType.LanguagePackOption().Where(p => p.ID == CurrentLanguage).SingleOrDefault().FileName;
            }
        }

        /// <summary>
        /// 获取当前语言包
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> Get()
        {
            Dictionary<string, string> _result = new Dictionary<string, string>();
            try
            {
                object _O = CacheHelper.Get($"{_CacheName}_{CurrentLanguage}");
                if (_O != null)
                {
                    _result = (Dictionary<string, string>)_O;
                }
                else
                {
                    LanguageCache objLanguageCache = new LanguageCache();
                    //重新加载语言包
                    objLanguageCache.Load();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return _result;
        }

        /// <summary>
        /// 根据站点配置获取当前语言列表
        /// </summary>
        /// <returns></returns>
        public static List<AppLanguagePack> CurrentLanguageOption()
        {
            List<AppLanguagePack> _result = new List<AppLanguagePack>();
            List<AppLanguagePack> objAppLanguage_List = LanguageType.LanguagePackOption();
            List<int> _ConfigLanguagePack = ConfigService.GetConfig().LanguagePacks;
            foreach (var _O in objAppLanguage_List)
            {
                if (_ConfigLanguagePack.Contains(_O.ID))
                {
                    _result.Add(_O);
                }
            }
            return _result;
        }
    }
}
