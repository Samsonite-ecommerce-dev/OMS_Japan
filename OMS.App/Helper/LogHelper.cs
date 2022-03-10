using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Service;

namespace OMS.App.Helper
{
    public class LogHelper
    {
        #region 日志级别
        /// <summary>
        /// 日志等级集合
        /// </summary>
        /// <returns></returns>
        private static List<object[]> LogLevelReflect()
        {
            List<object[]> _result = new List<object[]>();
            _result.Add(new object[] { (int)LogLevel.Info, "Info" });
            _result.Add(new object[] { (int)LogLevel.Warning, "Warning" });
            _result.Add(new object[] { (int)LogLevel.Error, "Error" });
            _result.Add(new object[] { (int)LogLevel.Debug, "Debug" });
            return _result;
        }

        /// <summary>
        /// 日志等级列表
        /// </summary>
        /// <returns></returns>
        public static List<object[]> LogLevelObject()
        {
            List<object[]> _result = new List<object[]>();
            foreach (var _o in LogLevelReflect())
            {
                _result.Add(new object[] { _o[0], _o[1] });
            }
            return _result;
        }

        /// <summary>
        /// 日志等级显示值
        /// </summary>
        /// <param name="objState"></param>
        /// <returns></returns>
        public static string GetLogTypeDisplay(int objStatus)
        {
            string _result = string.Empty;
            foreach (var _O in LogLevelReflect())
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