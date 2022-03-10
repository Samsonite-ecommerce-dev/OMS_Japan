using System;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Service.AppLanguage;

namespace OMS.App.Helper
{
    public class MallHelper
    {
        #region 店铺类型
        /// <summary>
        /// 店铺类型集合
        /// </summary>
        /// <returns></returns>
        private static List<DefineEnum> MallTypeReflect()
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            List<DefineEnum> _result = new List<DefineEnum>();
            _result.Add(new DefineEnum() { ID = (int)MallType.OnLine, Display = _LanguagePack["stores_index_type_online"], Css = "color_success" });
            _result.Add(new DefineEnum() { ID = (int)MallType.OffLine, Display = _LanguagePack["stores_index_type_offline"], Css = "color_primary" });
            return _result;
        }

        /// <summary>
        /// 店铺类型列表
        /// </summary>
        /// <returns></returns>
        public static List<object[]> MallTypeObject()
        {
            List<object[]> _result = new List<object[]>();
            foreach (var _o in MallTypeReflect())
            {
                _result.Add(new object[] { _o.ID, _o.Display });
            }
            return _result;
        }

        /// <summary>
        /// 店铺类型显示值
        /// </summary>
        /// <param name="objStatus"></param>
        /// <param name="objCss"></param>
        /// <returns></returns>
        public static string GetMallTypeDisplay(int objStatus, bool objCss = false)
        {
            string _result = string.Empty;
            DefineEnum _O = MallTypeReflect().Where(p => p.ID == objStatus).SingleOrDefault();
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

        #region 店铺接口类型
        /// <summary>
        /// 店铺接口类型集合
        /// </summary>
        /// <returns></returns>
        private static List<DefineEnum> MallInterfaceTypeReflect()
        {
            List<DefineEnum> _result = new List<DefineEnum>();
            _result.Add(new DefineEnum() { ID = (int)MallInterfaceType.Http, Display = "HTTP", Css = "color_success" });
            _result.Add(new DefineEnum() { ID = (int)MallInterfaceType.Ftp, Display = "FTP", Css = "color_primary" });
            return _result;
        }

        /// <summary>
        /// 店铺接口类型列表
        /// </summary>
        /// <returns></returns>
        public static List<object[]> MallInterfaceTypeObject()
        {
            List<object[]> _result = new List<object[]>();
            foreach (var _o in MallInterfaceTypeReflect())
            {
                _result.Add(new object[] { _o.ID, _o.Display });
            }
            return _result;
        }

        /// <summary>
        /// 店铺接口类型显示值
        /// </summary>
        /// <param name="objStatus"></param>
        /// <param name="objCss"></param>
        /// <returns></returns>
        public static string GetMallInterfaceTypeDisplay(int objStatus, bool objCss = false)
        {
            string _result = string.Empty;
            DefineEnum _O = MallInterfaceTypeReflect().Where(p => p.ID == objStatus).SingleOrDefault();
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