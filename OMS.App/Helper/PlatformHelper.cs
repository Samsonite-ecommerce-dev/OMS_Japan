using System;
using System.Collections.Generic;
using System.Reflection;

using Samsonite.OMS.DTO;

namespace OMS.App.Helper
{
    public class PlatformHelper
    {
        private static List<string[]> PlatformServicePowerReflect()
        {
            List<string[]> _result = new List<string[]>();
            PropertyInfo[] _propertyInfos = (new PlatformServicePower()).GetType().GetProperties();
            for (int t = 0; t < _propertyInfos.Length; t++)
            {
                //特性值
                var _attr = _propertyInfos[t].GetCustomAttribute(typeof(CustomPropertyAttribute));
                if (_attr != null)
                {
                    _result.Add(new string[] { _propertyInfos[t].Name, ((CustomPropertyAttribute)_attr).CustomName });
                }
                else
                {
                    _result.Add(new string[] { _propertyInfos[t].Name, "" });
                }
            }
            return _result;
        }

        /// <summary>
        /// 平台服务权限
        /// </summary>
        /// <returns></returns>
        public static List<string[]> PlatformServicePowerOption()
        {
            List<string[]> _result = new List<string[]>();
            foreach (var _o in PlatformServicePowerReflect())
            {
                _result.Add(new string[] { _o[0], _o[1] });
            }
            return _result;
        }
    }
}