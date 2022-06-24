using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service.AppLanguage;
using Samsonite.OMS.Service.AppConfig;
using Samsonite.Utility.Common;
using Newtonsoft.Json;
using OMS.App.Helper;

namespace OMS.App.Controllers
{
    public class ConfigController : BaseController
    {
        //
        // GET: /Config/

        #region 配置
        [UserPowerAuthorize]
        public ActionResult Index()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;
            //菜单栏
            ViewBag.MenuBar = this.MenuBar();
            //功能权限
            ViewBag.FunctionPower = this.FunctionPowers();
            //语言集合
            ViewData["language_list"] = LanguageType.LanguagePackOption();
            //支付方式集合
            ViewData["paytype_list"] = OrderHelper.PaymentTypeObject();
            //读取配置信息
            ApplicationConfigDto objApplicationConfigDto = ConfigService.GetConfig();

            return View(objApplicationConfigDto);
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Index_Message()
        {
            JsonResult _result = new JsonResult();
            //语言配置
            string _LanguagePack = VariableHelper.SaferequestStr(Request.Form["LanguagePack"]);
            //付款方式
            string _PaymentTypeConfig = VariableHelper.SaferequestStr(Request.Form["PaymentTypeConfig"]);
            //产品配置
            string _ProductIDConfig = VariableHelper.SaferequestStr(Request.Form["ProductIDConfig"]);
            //Samsonite警告库存数量
            int _WarningInventoryNum = VariableHelper.SaferequestInt(Request.Form["WarningInventoryNum"]);
            //Tumi警告库存数量
            int _WarningInventoryNumTumi = VariableHelper.SaferequestInt(Request.Form["WarningInventoryNumTumi"]);
            //金额精确值配置
            int _AmountAccuracy = VariableHelper.SaferequestInt(Request.Form["AmountAccuracy"]);
            //邮件服务器配置
            string _MailStmp = VariableHelper.SaferequestStr(Request.Form["MailStmp"]);
            int _MailPort = VariableHelper.SaferequestInt(Request.Form["MailPort"]);
            string _MailUserName = VariableHelper.SaferequestStr(Request.Form["MailUserName"]);
            string _MailPassword = VariableHelper.SaferequestStr(Request.Form["MailPassword"]);
            //API设置
            int _IsUseAPI = VariableHelper.SaferequestInt(Request.Form["IsUseAPI"]);
            //皮肤配置
            string _SkinStyle = VariableHelper.SaferequestStr(Request.Form["SkinStyle"]);

            using (var db = new ebEntities())
            {
                try
                {
                    //读取配置信息
                    List<SysConfig> objSysConfig_List = db.SysConfig.ToList();

                    //保存语言
                    if (string.IsNullOrEmpty(_LanguagePack))
                    {
                        throw new Exception("请至少配置一种语言");
                    }

                    var _Config_lp = objSysConfig_List.Where(p => p.ConfigKey == ConfigService.LANGUAGE_PACK_KEY).SingleOrDefault();
                    if (_Config_lp != null)
                    {
                        _Config_lp.ConfigValue = _LanguagePack.Replace(",", "|");
                    }

                    //系统产品ID
                    if (string.IsNullOrEmpty(_ProductIDConfig))
                    {
                        throw new Exception("Material和Grid的格式不能为空");
                    }

                    var _Config_pc = objSysConfig_List.Where(p => p.ConfigKey == ConfigService.PRODUCTID_CONFIG_KEY).SingleOrDefault();
                    if (_Config_pc != null)
                    {
                        _Config_pc.ConfigValue = _ProductIDConfig.ToString();
                    }

                    //订单支付方式
                    if (string.IsNullOrEmpty(_PaymentTypeConfig))
                    {
                        throw new Exception("请至少选择一种支付方式");
                    }

                    var _Config_pt = objSysConfig_List.Where(p => p.ConfigKey == ConfigService.PAYMENT_TYPE_KEY).SingleOrDefault();
                    if (_Config_pt != null)
                    {
                        _Config_pt.ConfigValue = _PaymentTypeConfig.Replace(",", "|");
                    }

                    //Samsonite保存警告库存数量
                    var _Config_win = objSysConfig_List.Where(p => p.ConfigKey == ConfigService.WARNING_INVENTORY_NUM_KEY).SingleOrDefault();
                    if (_Config_win != null)
                    {
                        _Config_win.ConfigValue = _WarningInventoryNum.ToString();
                    }

                    //Tumi保存警告库存数量
                    var _Config_win_tumi = objSysConfig_List.Where(p => p.ConfigKey == ConfigService.WARNING_INVENTORY_NUM_TUMI_KEY).SingleOrDefault();
                    if (_Config_win != null)
                    {
                        _Config_win_tumi.ConfigValue = _WarningInventoryNumTumi.ToString();
                    }

                    //保存小数点位数
                    var _Config_aa = objSysConfig_List.Where(p => p.ConfigKey == ConfigService.AMOUNT_ACCURACY_KEY).SingleOrDefault();
                    if (_Config_aa != null)
                    {
                        _Config_aa.ConfigValue = _AmountAccuracy.ToString();
                    }

                    //邮件配置
                    if (string.IsNullOrEmpty(_MailStmp) || _MailPort == 0 || string.IsNullOrEmpty(_MailUserName) || string.IsNullOrEmpty(_MailPassword))
                    {
                        throw new Exception("邮件服务器信息配置不完整");
                    }

                    var _Configec = objSysConfig_List.Where(p => p.ConfigKey == ConfigService.EMAIL_CONFIG_KEY).SingleOrDefault();
                    if (_Configec != null)
                    {
                        EmailDto objEmailDto = new EmailDto()
                        {
                            ServerHost = _MailStmp,
                            Port = _MailPort,
                            MailUsername = _MailUserName,
                            MailPassword = _MailPassword
                        };
                        _Configec.ConfigValue = JsonHelper.JsonSerialize(objEmailDto, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    }

                    //API设置
                    var _Configiu = objSysConfig_List.Where(p => p.ConfigKey == ConfigService.IS_USE_API_KEY).SingleOrDefault();
                    if (_Configiu != null)
                    {
                        _Configiu.ConfigValue = _IsUseAPI.ToString();
                    }

                    //皮肤配置
                    var _Configss = objSysConfig_List.Where(p => p.ConfigKey == ConfigService.SKIN_STYLE_KEY).SingleOrDefault();
                    if (_Configss != null)
                    {
                        _Configss.ConfigValue = _SkinStyle;
                    }
                    db.SaveChanges();

                    //返回信息
                    _result.Data = new
                    {
                        result = true,
                        msg = "配置信息保存成功"
                    };

                }
                catch (Exception ex)
                {
                    _result.Data = new
                    {
                        result = false,
                        msg = ex.Message
                    };
                }
            }

            return _result;
        }
        #endregion

        #region 重置系统配置缓存
        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult Reset_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            try
            {
                ConfigCache.Instance.Reset();
                //返回信息
                _result.Data = new
                {
                    result = true,
                    msg = "系统配置更新成功"
                };
            }
            catch (Exception ex)
            {
                _result.Data = new
                {
                    result = false,
                    msg = ex.Message
                };
            }
            return _result;
        }
        #endregion
    }
}
