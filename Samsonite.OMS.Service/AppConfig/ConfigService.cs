using System;
using System.Linq;
using System.Collections.Generic;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service.AppLanguage;
using Samsonite.Utility.Common;
using Newtonsoft.Json;

namespace Samsonite.OMS.Service.AppConfig
{
    public class ConfigService
    {
        #region 配置关键字
        /// <summary>
        /// 语言包配置
        /// </summary>
        public const string LANGUAGE_PACK_KEY = "LANGUAGE_PACK";

        /// <summary>
        /// 系统内产品ID配置
        /// </summary>
        public const string PRODUCTID_CONFIG_KEY = "PRODUCTID_CONFIG";

        /// <summary>
        /// 订单支付方式
        /// </summary>
        public const string PAYMENT_TYPE_KEY = "PAYMENT_TYPE";

        /// <summary>
        /// Samsonite库存报警数量
        /// </summary>
        public const string WARNING_INVENTORY_NUM_KEY = "WARNING_INVENTORY_NUM";

        /// <summary>
        /// Tumi库存报警数量
        /// </summary>
        public const string WARNING_INVENTORY_NUM_TUMI_KEY = "WARNING_INVENTORY_NUM_TUMI";

        /// <summary>
        /// 金额小数点配置
        /// </summary>
        public const string AMOUNT_ACCURACY_KEY = "AMOUNT_ACCURACY";

        /// <summary>
        /// 套装审核流程选择
        /// </summary>
        public const string BUNDLE_APPROVAL_KEY = "BUNDLE_APPROVAL";

        /// <summary>
        /// 促销活动审核流程选择
        /// </summary>
        public const string PROMOTION_APPROVAL_KEY = "PROMOTION_APPROVAL";

        /// <summary>
        /// 邮件服务器配置
        /// </summary>
        public const string EMAIL_CONFIG_KEY = "EMAIL_CONFIG";

        /// <summary>
        /// 是否启用API
        /// </summary>
        public const string IS_USE_API_KEY = "IS_USE_API";

        /// <summary>
        /// 皮肤
        /// </summary>
        public const string SKIN_STYLE_KEY = "SKIN_STYLE";
        #endregion 

        /// <summary>
        /// 读取所有配置信息
        /// </summary>
        /// <returns></returns>
        public static ApplicationConfigDto GetConfig()
        {
            ApplicationConfigDto _result = new ApplicationConfigDto();
            using (var db = new ebEntities())
            {
                //读取所有配置信息
                List<SysConfig> objSysConfig_List = db.SysConfig.ToList();
                //语言包配置
                _result.LanguagePacks = GetLanguagePackConfig(objSysConfig_List);
                //系统内产品ID配置
                _result.ProductIDConfig = GetProductIDConfig(objSysConfig_List);
                //订单支付方式配置
                _result.PaymentTypeConfig = GetPaymentTypeConfig(objSysConfig_List);
                //Samsonite库存报警数量配置
                _result.WarningInventoryNumConfig = GetWarningInventoryNumConfig(objSysConfig_List);
                //Tumi库存报警数量配置
                _result.WarningInventoryNumTumiConfig = GetWarningInventoryNumTumiConfig(objSysConfig_List);
                //金额小数点显示配置
                _result.AmountAccuracy = GetAmountAccuracyConfig(objSysConfig_List);
                //套装审核流程
                _result.BundleApproval = GetBundleApprovalConfig(objSysConfig_List);
                //促销活动审核流程
                _result.PromotionApproval = GetPromotionApprovalConfig(objSysConfig_List);
                //邮件配置
                _result.EmailConfig = GetEmailConfig(objSysConfig_List);
                //是否启用API
                _result.IsUseAPI = GetIsUseApiConfig(objSysConfig_List);
                //样式配置
                _result.SkinStyle = GetSkinStyleConfig(objSysConfig_List);
            }
            return _result;
        }

        /// <summary>
        /// 获取语言包配置
        /// </summary>
        /// <param name="objSysConfigList"></param>
        /// <returns></returns>
        public static List<int> GetLanguagePackConfig(List<SysConfig> objSysConfigList = null)
        {
            List<int> _result = new List<int>();
            using (var db = new ebEntities())
            {
                SysConfig objSysConfig = new SysConfig();
                if (objSysConfigList == null)
                {
                    objSysConfig = db.SysConfig.Where(p => p.ConfigKey.ToUpper() == LANGUAGE_PACK_KEY).SingleOrDefault();
                }
                else
                {
                    objSysConfig = objSysConfigList.Where(p => p.ConfigKey.ToUpper() == LANGUAGE_PACK_KEY).SingleOrDefault();
                }

                if (objSysConfig != null)
                {
                    //如果值为空,那么Split之后仍然会生成一个数组
                    if (!string.IsNullOrEmpty(objSysConfig.ConfigValue))
                    {
                        //语言集合
                        var _LanguagePacks = LanguageType.LanguagePackOption();

                        string[] _value = objSysConfig.ConfigValue.Split('|');
                        foreach (string _str in _value)
                        {
                            var _o = _LanguagePacks.Where(p => p.Key == _str).SingleOrDefault();
                            if (_o != null)
                            {
                                _result.Add(_o.ID);
                            }
                        }
                    }
                }
            }
            return _result;
        }

        /// <summary>
        /// 系统内产品ID配置
        /// </summary>
        /// <param name="objSysConfigList"></param>
        public static string GetProductIDConfig(List<SysConfig> objSysConfigList = null)
        {
            string _result = string.Empty;
            using (var db = new ebEntities())
            {
                SysConfig objSysConfig = new SysConfig();
                if (objSysConfigList == null)
                {
                    objSysConfig = db.SysConfig.Where(p => p.ConfigKey.ToUpper() == PRODUCTID_CONFIG_KEY).SingleOrDefault();
                }
                else
                {
                    objSysConfig = objSysConfigList.Where(p => p.ConfigKey.ToUpper() == PRODUCTID_CONFIG_KEY).SingleOrDefault();
                }

                if (objSysConfig != null)
                {
                    _result = objSysConfig.ConfigValue.ToString();
                }
            }
            return _result;
        }

        /// <summary>
        /// 订单支付方式配置
        /// </summary>
        /// <param name="objSysConfigList"></param>
        public static List<int> GetPaymentTypeConfig(List<SysConfig> objSysConfigList = null)
        {
            List<int> _result = new List<int>();
            using (var db = new ebEntities())
            {
                SysConfig objSysConfig = new SysConfig();
                if (objSysConfigList == null)
                {
                    objSysConfig = db.SysConfig.Where(p => p.ConfigKey.ToUpper() == PAYMENT_TYPE_KEY).SingleOrDefault();
                }
                else
                {
                    objSysConfig = objSysConfigList.Where(p => p.ConfigKey.ToUpper() == PAYMENT_TYPE_KEY).SingleOrDefault();
                }

                if (objSysConfig != null)
                {
                    if (!string.IsNullOrEmpty(objSysConfig.ConfigValue))
                    {
                        string[] _value = objSysConfig.ConfigValue.Split('|');

                        foreach (string _str in _value)
                        {
                            _result.Add(VariableHelper.SaferequestInt(_str));
                        }
                    }
                }
            }
            return _result;
        }

        /// <summary>
        /// 获取Samsonite库存报警数量配置
        /// </summary>
        /// <param name="objSysConfigList"></param>
        public static int GetWarningInventoryNumConfig(List<SysConfig> objSysConfigList = null)
        {
            int _result = 0;
            using (var db = new ebEntities())
            {
                SysConfig objSysConfig = new SysConfig();
                if (objSysConfigList == null)
                {
                    objSysConfig = db.SysConfig.Where(p => p.ConfigKey.ToUpper() == WARNING_INVENTORY_NUM_KEY).SingleOrDefault();
                }
                else
                {
                    objSysConfig = objSysConfigList.Where(p => p.ConfigKey.ToUpper() == WARNING_INVENTORY_NUM_KEY).SingleOrDefault();
                }

                if (objSysConfig != null)
                {
                    _result = VariableHelper.SaferequestInt(objSysConfig.ConfigValue);
                }
            }
            return _result;
        }

        /// <summary>
        /// 获取Tumi库存报警数量配置
        /// </summary>
        /// <param name="objSysConfigList"></param>
        public static int GetWarningInventoryNumTumiConfig(List<SysConfig> objSysConfigList = null)
        {
            int _result = 0;
            using (var db = new ebEntities())
            {
                SysConfig objSysConfig = new SysConfig();
                if (objSysConfigList == null)
                {
                    objSysConfig = db.SysConfig.Where(p => p.ConfigKey.ToUpper() == WARNING_INVENTORY_NUM_TUMI_KEY).SingleOrDefault();
                }
                else
                {
                    objSysConfig = objSysConfigList.Where(p => p.ConfigKey.ToUpper() == WARNING_INVENTORY_NUM_TUMI_KEY).SingleOrDefault();
                }

                if (objSysConfig != null)
                {
                    _result = VariableHelper.SaferequestInt(objSysConfig.ConfigValue);
                }
            }
            return _result;
        }

        /// <summary>
        /// 获取金额精确度配置
        /// </summary>
        /// <param name="objSysConfigList"></param>
        public static int GetAmountAccuracyConfig(List<SysConfig> objSysConfigList = null)
        {
            int _result = 0;
            using (var db = new ebEntities())
            {
                SysConfig objSysConfig = new SysConfig();
                if (objSysConfigList == null)
                {
                    objSysConfig = db.SysConfig.Where(p => p.ConfigKey.ToUpper() == AMOUNT_ACCURACY_KEY).SingleOrDefault();
                }
                else
                {
                    objSysConfig = objSysConfigList.Where(p => p.ConfigKey.ToUpper() == AMOUNT_ACCURACY_KEY).SingleOrDefault();
                }

                if (objSysConfig != null)
                {
                    _result = VariableHelper.SaferequestInt(objSysConfig.ConfigValue);
                }
            }
            return _result;
        }

        /// <summary>
        /// 获取套装审核流程配置
        /// </summary>
        /// <param name="objSysConfigList"></param>
        /// <returns></returns>
        public static List<string> GetBundleApprovalConfig(List<SysConfig> objSysConfigList = null)
        {
            List<string> _result = new List<string>();
            using (var db = new ebEntities())
            {
                SysConfig objSysConfig = new SysConfig();
                if (objSysConfigList == null)
                {
                    objSysConfig = db.SysConfig.Where(p => p.ConfigKey.ToUpper() == BUNDLE_APPROVAL_KEY).SingleOrDefault();
                }
                else
                {
                    objSysConfig = objSysConfigList.Where(p => p.ConfigKey.ToUpper() == BUNDLE_APPROVAL_KEY).SingleOrDefault();
                }

                if (objSysConfig != null)
                {
                    if (!string.IsNullOrEmpty(objSysConfig.ConfigValue))
                    {
                        string[] _value = objSysConfig.ConfigValue.Split('|');

                        foreach (string _str in _value)
                        {
                            _result.Add(_str);
                        }
                    }
                }
            }
            return _result;
        }

        /// <summary>
        /// 获取促销活动审核流程配置
        /// </summary>
        /// <param name="objSysConfigList"></param>
        /// <returns></returns>
        public static List<string> GetPromotionApprovalConfig(List<SysConfig> objSysConfigList = null)
        {
            List<string> _result = new List<string>();
            using (var db = new ebEntities())
            {
                SysConfig objSysConfig = new SysConfig();
                if (objSysConfigList == null)
                {
                    objSysConfig = db.SysConfig.Where(p => p.ConfigKey.ToUpper() == PROMOTION_APPROVAL_KEY).SingleOrDefault();
                }
                else
                {
                    objSysConfig = objSysConfigList.Where(p => p.ConfigKey.ToUpper() == PROMOTION_APPROVAL_KEY).SingleOrDefault();
                }

                if (objSysConfig != null)
                {
                    if (!string.IsNullOrEmpty(objSysConfig.ConfigValue))
                    {
                        string[] _value = objSysConfig.ConfigValue.Split('|');
                        foreach (string _str in _value)
                        {
                            _result.Add(_str);
                        }
                    }
                }
            }
            return _result;
        }

        /// <summary>
        /// 邮件配置
        /// </summary>
        /// <param name="objSysConfigList"></param>
        public static EmailDto GetEmailConfig(List<SysConfig> objSysConfigList = null)
        {
            EmailDto _result = new EmailDto();
            using (var db = new ebEntities())
            {
                SysConfig objSysConfig = new SysConfig();
                if (objSysConfigList == null)
                {
                    objSysConfig = db.SysConfig.Where(p => p.ConfigKey.ToUpper() == EMAIL_CONFIG_KEY).SingleOrDefault();
                }
                else
                {
                    objSysConfig = objSysConfigList.Where(p => p.ConfigKey.ToUpper() == EMAIL_CONFIG_KEY).SingleOrDefault();
                }

                if (objSysConfig != null)
                {
                    _result = JsonHelper.JsonDeserialize<EmailDto>(objSysConfig.ConfigValue);
                }
            }
            return _result;
        }

        /// <summary>
        /// 是否启用API
        /// </summary>
        /// <param name="objSysConfigList"></param>
        public static bool GetIsUseApiConfig(List<SysConfig> objSysConfigList = null)
        {
            bool _result = false;
            using (var db = new ebEntities())
            {
                SysConfig objSysConfig = new SysConfig();
                if (objSysConfigList == null)
                {
                    objSysConfig = db.SysConfig.Where(p => p.ConfigKey.ToUpper() == IS_USE_API_KEY).SingleOrDefault();
                }
                else
                {
                    objSysConfig = objSysConfigList.Where(p => p.ConfigKey.ToUpper() == IS_USE_API_KEY).SingleOrDefault();
                }

                if (objSysConfig != null)
                {
                    _result = (objSysConfig.ConfigValue == "1");
                }
            }
            return _result;
        }

        /// <summary>
        /// 皮肤配置
        /// </summary>
        /// <param name="objSysConfigList"></param>
        public static string GetSkinStyleConfig(List<SysConfig> objSysConfigList = null)
        {
            string _result = string.Empty;
            using (var db = new ebEntities())
            {
                SysConfig objSysConfig = new SysConfig();
                if (objSysConfigList == null)
                {
                    objSysConfig = db.SysConfig.Where(p => p.ConfigKey.ToUpper() == SKIN_STYLE_KEY).SingleOrDefault();
                }
                else
                {
                    objSysConfig = objSysConfigList.Where(p => p.ConfigKey.ToUpper() == SKIN_STYLE_KEY).SingleOrDefault();
                }

                if (objSysConfig != null)
                {
                    _result = objSysConfig.ConfigValue;
                }
            }
            return _result;
        }
    }
}