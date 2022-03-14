using System;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.DTO;


namespace Samsonite.OMS.Service.AppLanguage
{
    public class LanguageType
    {
        public static List<AppLanguagePack> LanguagePackOption()
        {
            //默认语言为英文
            //js文件名称默认值有中文简体,中文繁体和英文
            List<AppLanguagePack> _result = new List<AppLanguagePack>();
            _result.Add(new AppLanguagePack() { ID = (int)LanguagePackType.Simplified_Chinese, Key = "CN", Name = "Simplified Chinese", FileName = "zh_cn", IsDefault = false });
            _result.Add(new AppLanguagePack() { ID = (int)LanguagePackType.Traditional_Chinese, Key = "CN_TW", Name = "Traditional Chinese", FileName = "zh_tw", IsDefault = false });
            _result.Add(new AppLanguagePack() { ID = (int)LanguagePackType.English, Key = "EN", Name = "English", FileName = "en", IsDefault = true });
            _result.Add(new AppLanguagePack() { ID = (int)LanguagePackType.Korean, Key = "KO", Name = "Korean", FileName = "en", IsDefault = false });
            _result.Add(new AppLanguagePack() { ID = (int)LanguagePackType.Thai, Key = "TH", Name = "Thai", FileName = "en", IsDefault = false });
            _result.Add(new AppLanguagePack() { ID = (int)LanguagePackType.Japan, Key = "JPN", Name = "Japan", FileName = "en", IsDefault = false });
            return _result;
        }

        /// <summary>
        /// 获取默认语言
        /// </summary>
        /// <returns></returns>
        public static AppLanguagePack DefaultLanguagePack()
        {
            return LanguagePackOption().Where(p => p.IsDefault).SingleOrDefault();
        }
    }

    public enum LanguagePackType
    {
        /// <summary>
        /// 中文简体
        /// </summary>
        Simplified_Chinese = 1,
        /// <summary>
        /// 中文繁体
        /// </summary>
        Traditional_Chinese = 2,
        /// <summary>
        /// 英文
        /// </summary>
        English = 3,
        /// <summary>
        /// 韩文
        /// </summary>
        Korean = 4,
        /// <summary>
        /// 泰文
        /// </summary>
        Thai = 5,
        /// <summary>
        /// 日文
        /// </summary>
        Japan = 6
    }
}