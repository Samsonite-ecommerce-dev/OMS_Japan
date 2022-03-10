using System;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Service.AppLanguage;

namespace OMS.App.Helper
{
    public class ProductHelper
    {
        #region 产品类型
        /// <summary>
        /// 产品类型集合
        /// </summary>
        /// <returns></returns>
        private static List<DefineEnum> ProductTypeReflect()
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            List<DefineEnum> _result = new List<DefineEnum>();
            _result.Add(new DefineEnum() { ID = (int)ProductType.Common, Display = _LanguagePack["producttype_common"], Css = "color_default" });
            _result.Add(new DefineEnum() { ID = (int)ProductType.Bundle, Display = _LanguagePack["producttype_set"], Css = "color_success" });
            _result.Add(new DefineEnum() { ID = (int)ProductType.Gift, Display = _LanguagePack["producttype_gift"], Css = "color_warning" });
            return _result;
        }

        /// <summary>
        /// 产品类型列表
        /// </summary>
        /// <returns></returns>
        public static List<object[]> ProductTypeObject()
        {
            List<object[]> _result = new List<object[]>();
            foreach (var _o in ProductTypeReflect())
            {
                _result.Add(new object[] { _o.ID, _o.Display });
            }
            return _result;
        }

        /// <summary>
        /// 产品类型显示值
        /// </summary>
        /// <param name="objStatus"></param>
        /// <param name="objCss"></param>
        /// <returns></returns>
        public static string GetProductTypeDisplay(int objStatus, bool objCss = false)
        {
            string _result = string.Empty;
            DefineEnum _O = ProductTypeReflect().Where(p => p.ID == objStatus).SingleOrDefault();
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

        /// <summary>
        /// 产品类型枚举值
        /// </summary>
        /// <param name="objValue"></param>
        /// <returns></returns>
        public static int GetProductTypeEnum(string objValue)
        {
            int _result = 0;
            DefineEnum _O = ProductTypeReflect().Where(p => p.Display == objValue).SingleOrDefault();
            if (_O != null)
            {
                _result = _O.ID;
            }
            return _result;
        }
        #endregion
    }
}