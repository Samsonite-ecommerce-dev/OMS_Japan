using System;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Service.AppLanguage;
using Samsonite.OMS.Service.Sap.Poslog.Models;

namespace OMS.App.Helper
{
    public class PoslogHelper
    {
        #region Poslog类型集合
        /// <summary>
        /// Poslog类型集合
        /// </summary>
        /// <returns></returns>
        private static List<object[]> PoslogTypeReflect()
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            List<object[]> _result = new List<object[]>();
            _result.Add(new object[] { (int)SapLogType.KE, _LanguagePack["common_poslog_type_ke"] });
            _result.Add(new object[] { (int)SapLogType.KR, _LanguagePack["common_poslog_type_kr"] });
            return _result;
        }

        /// <summary>
        /// Poslog类型列表
        /// </summary>
        /// <returns></returns>
        public static List<object[]> PoslogTypeObject()
        {
            List<object[]> _result = new List<object[]>();
            foreach (var _o in PoslogTypeReflect())
            {
                _result.Add(new object[] { _o[0], _o[1] });
            }
            return _result;
        }

        /// <summary>
        /// Poslog类型显示值
        /// </summary>
        /// <param name="objStatus"></param>
        /// <returns></returns>
        public static string GetPoslogTypeDisplay(int objStatus)
        {
            string _result = string.Empty;
            foreach (var _O in PoslogTypeReflect())
            {
                if ((int)_O[0] == objStatus)
                {
                    _result = _O[1].ToString();
                    break;
                }
            }
            return _result;
        }
        #endregion

        #region Poslog状态集合
        /// <summary>
        /// Poslog状态集合
        /// </summary>
        /// <returns></returns>
        private static List<DefineEnum> PoslogStatusReflect()
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            List<DefineEnum> _result = new List<DefineEnum>();
            _result.Add(new DefineEnum() { ID = (int)SapState.UnUpload, Display = _LanguagePack["common_poslog_status_0"], Css = "color_default" });
            _result.Add(new DefineEnum() { ID = (int)SapState.ToSap, Display = _LanguagePack["common_poslog_status_1"], Css = "color_primary" });
            _result.Add(new DefineEnum() { ID = (int)SapState.Error, Display = _LanguagePack["common_poslog_status_2"], Css = "color_warning" });
            _result.Add(new DefineEnum() { ID = (int)SapState.Success, Display = _LanguagePack["common_poslog_status_3"], Css = "color_success" });
            return _result;
        }

        /// <summary>
        /// Poslog状态列表
        /// </summary>
        /// <returns></returns>
        public static List<object[]> PoslogStatusObject()
        {
            List<object[]> _result = new List<object[]>();
            foreach (var _o in PoslogStatusReflect())
            {
                _result.Add(new object[] { _o.ID, _o.Display });
            }
            return _result;
        }

        /// <summary>
        /// Poslog状态显示值
        /// </summary>
        /// <param name="objStatus"></param>
        /// <param name="objCss"></param>
        /// <returns></returns>
        public static string GetPoslogStatusDisplay(int objStatus, bool objCss = false)
        {
            string _result = string.Empty;
            DefineEnum _O = PoslogStatusReflect().Where(p => p.ID == objStatus).SingleOrDefault();
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