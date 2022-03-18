using System;
using System.Collections.Generic;

using Samsonite.OMS.DTO;

namespace OMS.App.Helper
{
    public class APIHelper
    {
        #region API用途
        /// <summary>
        /// API用途集合
        /// </summary>
        /// <returns></returns>
        private static List<object[]> APITypeReflect()
        {
            List<object[]> _result = new List<object[]>();
            _result.Add(new object[] { (int)APIType.Warehouse, APIType.Warehouse.ToString() });
            _result.Add(new object[] { (int)APIType.ClickCollect, APIType.ClickCollect.ToString() });
            _result.Add(new object[] { (int)APIType.Platform, APIType.Platform.ToString() });
            return _result;
        }

        /// <summary>
        /// API用途列表
        /// </summary>
        /// <returns></returns>
        public static List<object[]> APITypeObject()
        {
            List<object[]> _result = new List<object[]>();
            foreach (var _o in APITypeReflect())
            {
                _result.Add(new object[] { _o[0], _o[1] });
            }
            return _result;
        }

        /// <summary>
        /// API用途显示值
        /// </summary>
        /// <param name="objState"></param>
        /// <returns></returns>
        public static string GetAPITypeDisplay(int objStatus)
        {
            string _result = string.Empty;
            foreach (var _O in APITypeReflect())
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
    }
}