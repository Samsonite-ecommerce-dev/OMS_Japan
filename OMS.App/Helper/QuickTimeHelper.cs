using System;
using System.Collections.Generic;

using Samsonite.OMS.Service.AppLanguage;

using Samsonite.OMS.Service;

namespace OMS.App.Helper
{
    public class QuickTimeHelper
    {
        /// <summary>
        /// 快速选择时间
        /// </summary>
        /// <returns>List</returns>
        public static List<object[]> QuickTimeOption()
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();

            List<object[]> _result = new List<object[]>();
            _result.Add(new object[] { 1, _LanguagePack["common_qiucktime_1"] });
            _result.Add(new object[] { 2, _LanguagePack["common_qiucktime_2"] });
            _result.Add(new object[] { 3, _LanguagePack["common_qiucktime_3"] });
            _result.Add(new object[] { 4, _LanguagePack["common_qiucktime_4"] });
            _result.Add(new object[] { 5, _LanguagePack["common_qiucktime_5"] });
            _result.Add(new object[] { 6, _LanguagePack["common_qiucktime_6"] });
            _result.Add(new object[] { 7, _LanguagePack["common_qiucktime_7"] });
            _result.Add(new object[] { 8, _LanguagePack["common_qiucktime_8"] });
            return _result;
        }

        /// <summary>
        /// 返回对应的SQL查询语句
        /// </summary>
        /// <param name="objType">时间类型</param>
        /// <param name="objParaTime">查询的时间字段</param>
        /// <returns></returns>
        public static string[] GetQuickTime(int objType)
        {
            string[] _result = new string[2];
            switch (objType)
            {
                case 1:
                    _result[0] = DateTime.Now.ToString("yyyy-MM-dd 00:00:00");
                    _result[1] = DateTime.Now.ToString("yyyy-MM-dd 23:59:59");
                    break;
                case 2:
                    _result[0] = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd 00:00:00");
                    _result[1] = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd 23:59:59");
                    break;
                case 3:
                    _result[0] = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd 00:00:00");
                    _result[1] = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd 23:59:59");
                    break;
                case 4:
                    _result[0] = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd 00:00:00");
                    _result[1] = DateTime.Now.ToString("yyyy-MM-dd 23:59:59");
                    break;
                case 5:
                    int _week = (int)DateTime.Now.DayOfWeek;
                    _result[0] = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd 00:00:00");
                    _result[1] = DateTime.Now.ToString("yyyy-MM-dd 23:59:59");
                    break;
                case 6:
                    _result[0] = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd 00:00:00");
                    _result[1] = DateTime.Now.ToString("yyyy-MM-dd 23:59:59");
                    break;
                case 7:
                    _result[0] = DateTime.Now.AddYears(-1).ToString("yyyy-MM-dd 00:00:00");
                    _result[1] = DateTime.Now.ToString("yyyy-MM-dd 23:59:59");
                    break;
                case 8:
                    _result[0] = DateTime.Now.AddYears(-2).ToString("yyyy-MM-dd 00:00:00");
                    _result[1] = DateTime.Now.ToString("yyyy-MM-dd 23:59:59");
                    break;
                default:
                    _result[0] = string.Empty;
                    _result[1] = string.Empty;
                    break;
            }
            return _result;
        }
    }
}