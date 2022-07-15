using System;
using System.Configuration;
using System.Linq;

using Samsonite.Utility.Common;

namespace Samsonite.OMS.Service
{
    public class AppGlobalService
    {
        private static AppGlobalService instance = null;
        /// <summary>
        /// Session前缀
        /// </summary>
        public static string SESSION_KEY
        {
            get
            {
                return $"{VariableHelper.SaferequestAppSettingValue("basePrefix")}_SESSION_SAMSONITE_OMS";
            }
        }

        /// <summary>
        /// Cookie前缀
        /// </summary>
        public static string COOKIE_KEY
        {
            get
            {
                return $"{VariableHelper.SaferequestAppSettingValue("basePrefix")}_COOKIE_SAMSONITE_OMS";
            }
        }
        /// <summary>
        /// Cache前缀
        /// </summary>
        public static string CACHE_KEY
        {
            get
            {
                return $"{VariableHelper.SaferequestAppSettingValue("basePrefix")}_CACHE_SAMSONITE_OMS";
            }
        }
        /// <summary>
        /// 文件上传目录
        /// </summary>
        public const string UPLOAD_FILE_PATH = "/UploadFile/Temporary";
        /// <summary>
        /// 文件缓存目录
        /// </summary>
        public const string UPLOAD_CACHE_PATH = "/UploadFile/CacheFile";
        /// <summary>
        /// 域名地址
        /// </summary>
        public static string HTTP_URL
        {
            get
            {
                return VariableHelper.SaferequestAppSettingValue("httpURL");
            }
        }
        /// <summary>
        /// 站点物理地址
        /// </summary>
        public static string SITE_PHYSICAL_PATH
        {
            get
            {
                return VariableHelper.SaferequestAppSettingValue("sitePhysicalPATH");
            }
        }
        /// <summary>
        /// 国家前缀
        /// </summary>
        public const string COUNTRY_PREFIX = "sg_";
        /// <summary>
        /// 最近N次修改密码不能重复
        /// </summary>
        public const int PWD_PAST_NUM = 5;
        /// <summary>
        /// 密码过期时间
        /// </summary>
        public const int PWD_VALIDITY_TIME = 90;
        /// <summary>
        /// 连续密码错误次数锁定
        /// </summary>
        public const int PWDERROR_LOCK_NUM = 5;
        /// <summary>
        /// 默认快递公司ID(Speed Post)
        /// </summary>
        public const int DEFAULT_EXPRESS_COMPANY_ID = 1;
        /// <summary>
        /// 物流方式
        /// </summary>
        public const string SHIPPING_METHOD_CODE = "2542675";
        /// <summary>
        /// SAM组织架构编号
        /// </summary>
        public const string SAM_SALES_ORGANIZATION = "5710";
        /// <summary>
        /// Tumi组织架构编号
        /// </summary>
        public const string TUMI_SALES_ORGANIZATION = "5010";
        /// <summary>
        /// 仓库自取快递号
        /// </summary>
        public const string ExpressTakenByCustomer = "Collected by Customer";

        private AppGlobalService()
        {
        }

        public static AppGlobalService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AppGlobalService();
                }
                return instance;
            }
        }
    }
}