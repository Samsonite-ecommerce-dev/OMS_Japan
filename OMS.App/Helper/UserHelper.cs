using System;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Service.AppLanguage;

namespace OMS.App.Helper
{
    public class UserHelper
    {
        #region 账号类型
        /// <summary>
        /// 账号类型集合
        /// </summary>
        /// <returns></returns>
        private static List<DefineEnum> UserTypeReflect()
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            List<DefineEnum> _result = new List<DefineEnum>();
            _result.Add(new DefineEnum() { ID = (int)UserType.InternalStaff, Display = _LanguagePack["users_index_type_1"], Css = "color_primary" });
            _result.Add(new DefineEnum() { ID = (int)UserType.WarehouseStaff, Display = _LanguagePack["users_index_type_2"], Css = "color_warning" });
            return _result;
        }

        /// <summary>
        /// 账号类型列表
        /// </summary>
        /// <returns></returns>
        public static List<object[]> UserTypeObject()
        {
            List<object[]> _result = new List<object[]>();
            foreach (var _o in UserTypeReflect())
            {
                _result.Add(new object[] { _o.ID, _o.Display });
            }
            return _result;
        }

        /// <summary>
        /// 账号类型显示值
        /// </summary>
        /// <param name="objStatus"></param>
        /// <param name="objCss"></param>
        /// <returns></returns>
        public static string GetUserTypeDisplay(int objStatus, bool objCss = false)
        {
            string _result = string.Empty;
            DefineEnum _O = UserTypeReflect().Where(p => p.ID == objStatus).SingleOrDefault();
            if (_O != null)
            {
                if (objCss)
                {
                    _result = string.Format("<label class=\"{0}\">{1}</label>", _O.Css, _O.Display);
                }
                else
                {
                    _result = _O.Display;
                }
            }
            return _result;
        }
        #endregion
    }
}