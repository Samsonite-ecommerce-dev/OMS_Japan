using System;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service.AppConfig;
using Samsonite.Utility.Common;

namespace Samsonite.OMS.Service.AppLanguage
{
    public class LanguageCache
    {
        private static LanguageCache instance = null;
        public static LanguageCache Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new LanguageCache();
                }
                return instance;
            }
        }
        //缓存名称前缀
        public string _CacheName = $"{AppGlobalService.CACHE_KEY}_LANGUAGE_PACK";
        //默认365天
        public int _CacheTime = 31536000;

        /// <summary>
        /// 需要加载的语言集合
        /// </summary>
        public List<int> _LoadLanguages = new List<int>();

        /// <summary>
        /// 初始化
        /// </summary>
        public LanguageCache()
        {
            //读取需要加载的语言
            _LoadLanguages = ConfigService.GetConfig().LanguagePacks;
        }

        /// <summary>
        /// 初始化语言缓存
        /// </summary>
        public void Load()
        {
            using (var db = new ebEntities())
            {
                //从数据库读取语言包
                List<LanguagePack> objLanguagePack_List = db.LanguagePack.Where(p => !p.IsDelete).ToList();
                foreach (var _O in LanguageType.LanguagePackOption())
                {
                    if (_LoadLanguages.Contains(_O.ID))
                    {
                        //单个语言包缓存格式为_CacheName+'_'+语言包ID
                        object _object = CacheHelper.Get(string.Format("{0}_{1}", _CacheName, _O.ID));
                        if (_object == null)
                        {
                            InsertCache(objLanguagePack_List, _O.ID);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 重置语言缓存
        /// </summary>
        public void Reset()
        {
            using (var db = new ebEntities())
            {
                //从数据库读取语言包
                List<LanguagePack> objLanguagePack_List = db.LanguagePack.Where(p => !p.IsDelete).ToList();
                foreach (var _O in LanguageType.LanguagePackOption())
                {
                    if (_LoadLanguages.Contains(_O.ID))
                    {
                        //单个语言包缓存格式为_CacheName+'_'+语言包ID
                        object _object = CacheHelper.Get(string.Format("{0}_{1}", _CacheName, _O.ID));
                        if (_object != null)
                        {
                            CacheHelper.Remove(string.Format("{0}_{1}", _CacheName, _O.ID));
                        }
                        //重新插入缓存
                        InsertCache(objLanguagePack_List, _O.ID);
                    }
                }
            }
        }

        /// <summary>
        /// 写入语言包缓存
        /// </summary>
        /// <param name="objPack"></param>
        /// <param name="objType"></param>
        private void InsertCache(List<LanguagePack> objPack, int objType)
        {
            AppLanguagePack _AppLanguagePack = LanguageType.LanguagePackOption().Where(p => p.ID == objType).SingleOrDefault();
            if (_AppLanguagePack != null)
            {
                try
                {
                    Dictionary<string, string> _pack = new Dictionary<string, string>();
                    switch (objType)
                    {
                        case (int)LanguagePackType.Simplified_Chinese:
                            //存放到字典中
                            foreach (LanguagePack _n in objPack)
                            {
                                _pack.Add(_n.PackKey, _n.PackChinese);
                            }
                            break;
                        case (int)LanguagePackType.English:
                            //存放到字典中
                            foreach (LanguagePack _n in objPack)
                            {
                                _pack.Add(_n.PackKey, _n.PackEnglish);
                            }
                            break;
                        case (int)LanguagePackType.Korean:
                            //存放到字典中
                            foreach (LanguagePack _n in objPack)
                            {
                                _pack.Add(_n.PackKey, _n.PackKorean);
                            }
                            break;
                        case (int)LanguagePackType.Traditional_Chinese:
                            //存放到字典中
                            foreach (LanguagePack _n in objPack)
                            {
                                _pack.Add(_n.PackKey, _n.PackTraditionalChinese);
                            }
                            break;
                        case (int)LanguagePackType.Thai:
                            //存放到字典中
                            foreach (LanguagePack _n in objPack)
                            {
                                _pack.Add(_n.PackKey, _n.PackThai);
                            }
                            break;
                        default:
                            break;
                    }
                    CacheHelper.Insert(string.Format("{0}_{1}", _CacheName, _AppLanguagePack.ID), _pack, _CacheTime);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}