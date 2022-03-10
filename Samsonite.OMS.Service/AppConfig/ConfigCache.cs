using System;
using System.Linq;
using System.Collections.Generic;

using Samsonite.OMS.DTO;
using Samsonite.Utility.Common;

namespace Samsonite.OMS.Service.AppConfig
{
    public class ConfigCache
    {
        private static ConfigCache instance = null;
        public static ConfigCache Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ConfigCache();
                }
                return instance;
            }
        }

        /// <summary>
        /// 缓存名
        /// </summary>
        private string ConfigCacheName = $"{AppGlobalService.CACHE_KEY}_SYSCONFIG";

        //缓存默认365天
        private int CacheTime = 31536000;

        /// <summary>
        /// 初始化配置信息
        /// </summary>
        /// <returns></returns>
        public void Load()
        {
            object _object = CacheHelper.Get(this.ConfigCacheName);
            if (_object == null)
            {
                ApplicationConfigDto objConfig = ConfigService.GetConfig();
                //写入缓存
                CacheHelper.Insert(this.ConfigCacheName, objConfig, CacheTime);
            }
        }

        /// <summary>
        /// 获取配置信息(从缓存中读取)
        /// </summary>
        /// <returns></returns>
        public ApplicationConfigDto Get()
        {
            ApplicationConfigDto _result = new ApplicationConfigDto();
            object _object = CacheHelper.Get(this.ConfigCacheName);
            if (_object != null)
            {
                _result = (ApplicationConfigDto)_object;
            }
            else
            {
                _result = ConfigService.GetConfig();
                //写入缓存
                CacheHelper.Insert(this.ConfigCacheName, _result, CacheTime);
            }
            return _result;
        }

        /// <summary>
        /// 重置系统配置缓存
        /// </summary>
        public void Reset()
        {
            object _object = CacheHelper.Get(this.ConfigCacheName);
            if (_object != null)
            {
                CacheHelper.Remove(this.ConfigCacheName);
            }
            //重新插入缓存
            CacheHelper.Insert(this.ConfigCacheName, ConfigService.GetConfig(), CacheTime);
        }
    }
}