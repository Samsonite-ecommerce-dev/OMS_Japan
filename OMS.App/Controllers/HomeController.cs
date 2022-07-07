using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service;
using Samsonite.OMS.Service.AppLanguage;
using Samsonite.Utility.Common;

namespace OMS.App.Controllers
{
    public class HomeController : BaseController
    {
        //
        // GET: /Home/
        [UserLoginAuthorize]
        public ActionResult Index()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;
            ViewBag.LanguagePack = _LanguagePack;
            //登录信息
            UserSessionInfo _UserSessionInfo = this.CurrentLoginUser;
            //权限功能列表
            List<int> _powers = new List<int>();
            if (_UserSessionInfo != null)
            {
                ViewBag.UserName = _UserSessionInfo.UserName;
                ViewBag.UserType = _UserSessionInfo.UserType;
                _powers = _UserSessionInfo.UserPowers.Select(p => p.FunctionID).ToList();
            }
            else
            {
                ViewBag.UserName = string.Empty;
            }

            //菜单栏
            using (var db = new ebEntities())
            {
                List<DefineMenu> _menu_list = new List<DefineMenu>();
                List<DefineMenu.MenuChild> _children = new List<DefineMenu.MenuChild>();
                List<SysFunctionGroup> _SysFunctionGroup_List = db.SysFunctionGroup.Where(p => p.Parentid == 0).OrderBy(p => p.Rootid).ToList();
                List<SysFunction> _SysFunction_List = db.SysFunction.Where(p => p.FuncType == 1 && p.IsShow && _powers.Contains(p.Funcid)).OrderBy(p => p.SeqNumber).ToList();
                List<SysFunction> _SysFunction_Next_List = new List<SysFunction>();
                foreach (SysFunctionGroup _SysFunctionGroup in _SysFunctionGroup_List)
                {
                    //是否存在子功能
                    _SysFunction_Next_List = _SysFunction_List.Where<SysFunction>(p => p.Groupid == _SysFunctionGroup.Groupid && p.FuncType == 1 && p.IsShow).OrderBy(p => p.SeqNumber).ToList();
                    if (_SysFunction_Next_List.Count > 0)
                    {
                        _children = new List<DefineMenu.MenuChild>();
                        foreach (SysFunction _SysFunction in _SysFunction_Next_List)
                        {
                            _children.Add(new DefineMenu.MenuChild() { ID = _SysFunction.Funcid, Name = _LanguagePack[string.Format("menu_function_{0}", _SysFunction.Funcid)], Url = _SysFunction.FuncUrl, Target = _SysFunction.FuncTarget });
                        }
                        _menu_list.Add(new DefineMenu() { ID = _SysFunctionGroup.Groupid, Name = _LanguagePack[string.Format("menu_group_{0}", _SysFunctionGroup.Groupid)], Icon = _SysFunctionGroup.GroupIcon, Children = _children });
                    }
                }
                return View(_menu_list);
            }
        }

        #region Main
        [UserLoginAuthorize]
        public ActionResult Main()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;

            //信息对象
            HomeSummary objHomeSummary = new HomeSummary();

            using (var db = new ebEntities())
            {
                //今日信息提示
                var _summaryToday = db.Database.SqlQuery<HomeSummaryToday>("exec Proc_HomeSummary_Today {0}", DateTime.Now).SingleOrDefault();
                if (_summaryToday != null)
                {
                    objHomeSummary.SummaryTodayInfo = _summaryToday;
                }
                //最新订单
                objHomeSummary.NewOrders = db.Database.SqlQuery<HomeSummaryNewOrder>("select top 10 OrderNo,MallName,CreateDate from [Order] order by CreateDate desc").ToList();
                //最新错误订单
                objHomeSummary.ExceptionOrders = db.Database.SqlQuery<HomeSummaryExceptionOrder>("select top 10 OrderNo,SubOrderNo,MallName,OrderTime,ErrorMsg from View_OrderDetail where IsError=1 and ProductStatus>={0} and IsDelete=0 order by OrderTime desc", (int)ProductStatus.Received).ToList();
                //最新仓库回复
                objHomeSummary.WMSPostreplys = db.Database.SqlQuery<HomeSummaryWMSPostreply>("select top 10 od.OrderNo,od.SubOrderNo,od.MallName,ows.ApiReplyMsg from View_OrderDetail as od inner join OrderWMSReply as ows on od.SubOrderNo=ows.SubOrderNo where ows.Status=0 order by od.OrderTime desc").ToList();
                //最新请求列表
                objHomeSummary.Claims = db.Database.SqlQuery<HomeSummaryClaim>("select top 10 oc.OrderNo,oc.SubOrderNo,oc.ClaimType,oc.ErrorMessage,isnull(m.Name,'') as MallName from OrderClaimCache as oc left join Mall as m on oc.MallSapCode=m.SapCode where Status=0 order by oc.ClaimDate desc").ToList();
                //最新快递获取列表
                objHomeSummary.DeliveryRequires = db.Database.SqlQuery<HomeSummaryDeliveryRequire>("select top 10 od.OrderNo,od.SubOrderNo,od.MallName,epr.PushResultMessage as ErrorMessage from View_OrderDetail as od inner join ECommercePushRecord as epr on epr.RelatedId=od.DetailId and epr.PushType={0} where epr.PushResult=0 and od.ProductStatus={1} and epr.IsDelete=0 order by epr.EditTime desc", (int)ECommercePushType.RequireTrackingCode, (int)ProductStatus.Received).ToList();
                //最新快递推送列表
                objHomeSummary.PushRequires = db.Database.SqlQuery<HomeSummaryPushRequire>("select top 10 od.OrderNo,od.SubOrderNo,od.MallName,ds.InvoiceNo,epr.PushResultMessage as ErrorMessage from Deliverys as ds inner join View_OrderDetail as od on ds.SubOrderNo=od.SubOrderNo inner join ECommercePushRecord as epr on epr.RelatedId=ds.Id and epr.PushType={0} where epr.PushResult=0 and od.ProductStatus={1} and epr.IsDelete=0 order by epr.EditTime desc", (int)ECommercePushType.PushTrackingCode, (int)ProductStatus.Processing).ToList();
                //最近30天统计
                var _summaryThirtyDay = db.Database.SqlQuery<HomeSummaryThirtyDay>("exec Proc_HomeSummary_ThirtyDay {0},{1}", DateTime.Now.AddDays(-29), DateTime.Now).SingleOrDefault();
                if (_summaryThirtyDay != null)
                {
                    objHomeSummary.SummaryThirtyDayInfo = _summaryThirtyDay;
                }
            }

            return View(objHomeSummary);
        }

        [UserLoginAuthorize(Type = UserLoginAuthorize.ResultType.Json)]
        public ContentResult Main_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            ContentResult _result = new ContentResult();
            int _Type = VariableHelper.SaferequestInt(Request.Form["type"]);
            var _beginTime = DateTime.Now.AddDays(-29);
            var _endTime = DateTime.Now;
            using (var db = new ebEntities())
            {
                switch (_Type)
                {
                    case 1:
                        //店铺列表
                        List<Mall> objMall_List = MallService.GetMallOption();
                        //最近30天各店铺销售统计
                        var objStoreSales_List = db.AnalysisDailyOrder.Where(p => SqlFunctions.DateDiff("day", p.Date, _beginTime) <= 0 && SqlFunctions.DateDiff("day", p.Date, _endTime) >= 0 && p.TimeZoon == 0).AsNoTracking().ToList();
                        //数据
                        List<EChartsHelper.Pie.Config.Para.Data> _pies = new List<EChartsHelper.Pie.Config.Para.Data>();
                        foreach (var dy in objMall_List)
                        {
                            if (dy.PlatformCode != (int)PlatformType.Micros_Japan)
                            {
                                _pies.Add(new EChartsHelper.Pie.Config.Para.Data()
                                {
                                    Name = dy.Name,
                                    Value = objStoreSales_List.Where(p => p.MallSapCode == dy.SapCode).ToList().Sum(o => o.Quantity).ToString()
                                });
                            }
                        }
                        //读取micros店铺
                        var objMicrosMalls = db.Mall.Where(p => p.PlatformCode == (int)PlatformType.Micros_Japan).Select(o => o.SapCode).ToList();
                        //mircos数据合并成一个店铺
                        if (objMicrosMalls.Count > 0)
                        {
                            _pies.Add(new EChartsHelper.Pie.Config.Para.Data()
                            {
                                Name = "Micros",
                                Value = objStoreSales_List.Where(p => objMicrosMalls.Contains(p.MallSapCode)).ToList().Sum(o => o.Quantity).ToString()
                            });
                        }
                        //饼图
                        EChartsHelper.Pie.Config.Para objData = new EChartsHelper.Pie.Config.Para()
                        {
                            Name = "",
                            Center = new List<string>() { "50%", "60%" },
                            Radius = "55%",
                            Datas = _pies
                        };
                        _result.Content = EChartsHelper.Pie.Set(new EChartsHelper.Pie.Config()
                        {
                            Title = "",
                            Paras = objData,
                            IsShowTab = false,
                            IsMagicType = false,
                            IsRestore = false,
                            IsSaveAsImage = false
                        });
                        break;
                    case 2:
                        //最近30天各产品销售统计
                        List<CommonReport> _list = new List<CommonReport>();
                        var objProductSales_List = db.AnalysisDailyProduct.Where(p => SqlFunctions.DateDiff("day", p.Date, _beginTime) <= 0 && SqlFunctions.DateDiff("day", p.Date, _endTime) >= 0).Select(o => new { o.Sku, o.Quantity }).AsNoTracking().ToList();
                        List<string> _Skus = objProductSales_List.GroupBy(p => p.Sku).Select(o => o.Key).ToList();
                        foreach (string _str in _Skus)
                        {
                            _list.Add(new CommonReport()
                            {
                                Key = _str,
                                Value = objProductSales_List.Where(p => p.Sku == _str).ToList().Sum(p => p.Quantity),
                            });
                        }
                        //取前10的统计数据
                        _list = _list.OrderByDescending(p => (int)p.Value).Take(10).ToList();
                        //柱状图
                        List<EChartsHelper.Bar.Config.Para> objData1 = new List<EChartsHelper.Bar.Config.Para>();
                        objData1.Add(new EChartsHelper.Bar.Config.Para()
                        {
                            Legend = _LanguagePack["home_main_last_sales_item_quantity"],
                            Name = _LanguagePack["home_main_last_sales_item_quantity"],
                            Datas = _list.Select(p => p.Value).ToList()
                        });
                        _result.Content = EChartsHelper.Bar.Set(new EChartsHelper.Bar.Config()
                        {
                            Title = "",
                            XAxis = _list.Select(p => p.Key).ToList(),
                            YAxis = "",
                            Paras = objData1,
                            IsMagicType = false,
                            IsRestore = false,
                            IsSaveAsImage = false
                        });
                        break;
                    case 3:
                        //品牌列表
                        List<string> objBrands = BrandService.GetBrandOption();
                        //最近30天各产品销售统计
                        List<CommonReport> _list1 = new List<CommonReport>();

                        var objBrandSales_List = (from adp in db.AnalysisDailyProduct.Where(p => SqlFunctions.DateDiff("day", p.Date, _beginTime) <= 0 && SqlFunctions.DateDiff("day", p.Date, _endTime) >= 0)
                                                  join p in db.Product on adp.Sku equals p.SKU
                                                  select new
                                                  {
                                                      Quantity = adp.Quantity,
                                                      Brand = p.Name
                                                  }).AsNoTracking().ToList();

                        foreach (string _str in objBrands)
                        {
                            _list1.Add(new CommonReport()
                            {
                                Key = _str,
                                Value = objBrandSales_List.Where(p => p.Brand == _str).ToList().Sum(p => p.Quantity),
                            });
                        }
                        //取前10的统计数据
                        _list1 = _list1.OrderByDescending(p => (int)p.Value).Take(10).ToList();
                        //柱状图
                        List<EChartsHelper.Bar.Config.Para> objData2 = new List<EChartsHelper.Bar.Config.Para>();
                        objData2.Add(new EChartsHelper.Bar.Config.Para()
                        {
                            Legend = _LanguagePack["home_main_last_sales_item_quantity"],
                            Name = _LanguagePack["home_main_last_sales_item_quantity"],
                            Datas = _list1.Select(p => p.Value).ToList()
                        });
                        _result.Content = EChartsHelper.Bar.Set(new EChartsHelper.Bar.Config()
                        {
                            Title = "",
                            XAxis = _list1.Select(p => p.Key).ToList(),
                            YAxis = "",
                            Paras = objData2,
                            IsMagicType = false,
                            IsRestore = false,
                            IsSaveAsImage = false
                        });
                        break;
                    default:
                        break;
                }
            }
            return _result;
        }
        #endregion

        #region 选择语言
        [UserLoginAuthorize]
        public ActionResult LanguageConfig()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;

            //语言包集合
            ViewData["language_list"] = LanguageService.CurrentLanguageOption();

            //当前默认语言
            ViewBag.LanguageType = LanguageService.CurrentLanguage;
            return View();
        }

        [UserLoginAuthorize(Type = UserLoginAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult LanguageConfig_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            int _Type = VariableHelper.SaferequestInt(Request.Form["Language"]);
            using (var db = new ebEntities())
            {
                try
                {
                    //是否存在该语言
                    AppLanguagePack objAppLanguagePack = LanguageType.LanguagePackOption().Where(p => p.ID == _Type).SingleOrDefault();
                    if (objAppLanguagePack != null)
                    {
                        //设置语言包
                        LanguageService.SetLanguage(_Type);
                        //返回信息
                        _result.Data = new
                        {
                            result = true,
                            msg = _LanguagePack["home_languageconfig_message_config_success"]
                        };
                    }
                    else
                    {
                        throw new Exception(_LanguagePack["common_data_load_false"]);
                    }
                }
                catch
                {
                    _result.Data = new
                    {
                        result = false,
                        msg = _LanguagePack["home_languageconfig_message_config_false"]
                    };
                }
            }
            return _result;
        }
        #endregion

        #region 修改密码
        [UserLoginAuthorize]
        public ActionResult EditPassword()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;

            return View();
        }

        [UserLoginAuthorize(Type = UserLoginAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult EditPassword_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            int _UserID = this.CurrentLoginUser.Userid;
            string _OldPassword = Request.Form["OldPassword"];
            string _Password = Request.Form["Password"];
            string _SurePassword = Request.Form["SurePassword"];

            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_OldPassword))
                    {
                        throw new Exception(_LanguagePack["home_editpassword_message_no_old_password"]);
                    }

                    if (string.IsNullOrEmpty(_Password))
                    {
                        throw new Exception(_LanguagePack["home_editpassword_message_no_password"]);
                    }
                    else
                    {

                        if (!CheckHelper.ValidPassword(_Password))
                        {
                            throw new Exception(_LanguagePack["home_editpassword_message_password_error"]);
                        }
                        else
                        {
                            //检查是否存在N次密码修改存在重复
                            using (var db1 = new logEntities())
                            {
                                string _encryptPWD = UserLoginService.EncryptPassword(_Password);
                                List<string> objWebAppPasswordLog_List = db1.WebAppPasswordLog.Where(p => p.UserID == _UserID).OrderByDescending(p => p.LogID).Select(p => p.Password).Take(AppGlobalService.PWD_PAST_NUM).ToList();
                                if (objWebAppPasswordLog_List.Contains(_encryptPWD))
                                {
                                    throw new Exception(_LanguagePack["home_editpassword_message_password_repeat_error"]);
                                }
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(_SurePassword))
                    {
                        throw new Exception(_LanguagePack["home_editpassword_message_no_reset_password"]);
                    }
                    else
                    {
                        if (_Password != _SurePassword)
                        {
                            throw new Exception(_LanguagePack["home_editpassword_message_not_same"]);
                        }
                    }

                    UserInfo objData = db.UserInfo.Where(p => p.UserID == _UserID).SingleOrDefault();
                    if (objData != null)
                    {
                        if (objData.Pwd.ToLower() != UserLoginService.EncryptPassword(_OldPassword).ToLower())
                        {
                            throw new Exception(_LanguagePack["home_editpassword_message_error_old_password"]);
                        }

                        objData.Pwd = UserLoginService.EncryptPassword(_Password);
                        objData.LastPwdEditTime = DateTime.Now;
                        //如果密码过期
                        if (objData.Status == (int)UserStatus.ExpiredPwd)
                        {
                            objData.Status = (int)UserStatus.Normal;
                        }
                        db.SaveChanges();
                        //添加密码日志
                        AppLogService.PasswordLog(new WebAppPasswordLog()
                        {
                            Account = objData.UserName,
                            Password = UserLoginService.EncryptPassword(_Password),
                            UserID = objData.UserID,
                            IP = Samsonite.Utility.Common.UrlHelper.GetRequestIP(),
                            Remark = string.Empty,
                            AddTime = DateTime.Now
                        });
                        //添加日志
                        AppLogService.UpdateLog<UserInfo>(objData, objData.UserID.ToString());
                        //返回信息
                        _result.Data = new
                        {
                            result = true,
                            msg = _LanguagePack["common_data_save_success"]
                        };
                    }
                    else
                    {
                        throw new Exception(_LanguagePack["common_data_load_false"]);
                    }
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
        }
        #endregion
    }
}
