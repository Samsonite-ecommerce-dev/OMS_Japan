using System;
using System.Collections.Generic;
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
            //读取最新10条订单信息
            using (var db = new DynamicRepository())
            {
                //今日信息提示
                string _sql = string.Empty;
                _sql += "select IsNull((select count(*) from (select OrderNo from View_OrderDetail where datediff(day, OrderTime, @0) = 0 and Isdelete=0 group by OrderNo) as tmp), 0) AS NewOrder";
                _sql += ",IsNull((select count(*) from OrderModify where datediff(day, AddDate, @0) = 0),0) AS NewModifyOrder";
                _sql += ",IsNull((select count(*) from OrderCancel where datediff(day, CreateDate, @0) = 0),0) AS NewCancelOrder";
                _sql += ",IsNull((select count(*) from OrderReturn where datediff(day, CreateDate, @0) = 0 and IsFromExchange = 0),0) AS NewReturnOrder";
                _sql += ",IsNull((select count(*) from OrderExchange where datediff(day, CreateDate, @0) = 0),0) AS NewExchangeOrder";
                _sql += ",IsNull((select count(*) from OrderReject where datediff(day, CreateDate, @0) = 0),0) AS NewRejectOrder";
                _sql += ",IsNull((select count(*) from OrderDetail where datediff(day, CreateDate, @0) = 0 and IsSetOrigin=0 and IsExchangeNew = 0 and IsError = 1 and IsDelete=0),0) AS ExceptionOrder";
                _sql += ",IsNull((select count(*) from OrderWMSReply where datediff(day, ApiReplyDate, @0) = 0 and [Status] = 0),0) AS WMSPostReply";
                _sql += ",IsNull((select count(*) from OrderClaimCache where datediff(day, ClaimDate, @0) = 0 and [Status] = 0),0) AS ExceptionClaim";
                _sql += ",IsNull((select count(*) from View_Orderdetail as od inner join ECommercePushRecord as epr on epr.PushType=" + (int)ECommercePushType.RequireTrackingCode + " and epr.RelatedId=od.Id where datediff(day, epr.EditTime, @0) = 0 and epr.PushResult = 0 and od.ProductStatus=" + (int)ProductStatus.Pending + " and epr.IsDelete=0),0) AS ExceptionRequireDelivery";
                _sql += ",IsNull((select count(*) from Deliverys as ds inner join View_OrderDetail as od on ds.SubOrderNo=od.SubOrderNo inner join ECommercePushRecord as epr on epr.PushType=" + (int)ECommercePushType.PushTrackingCode + " and epr.RelatedId=ds.Id where datediff(day, epr.EditTime, @0) = 0 and epr.PushResult = 0 and od.ProductStatus=" + (int)ProductStatus.InDelivery + " and epr.IsDelete=0),0) AS ExceptionPushDelivery";
                _sql += ",IsNull((select count(*) from MallProduct as mp inner join ECommercePushInventoryRecord as epr on epr.PushType in (" + (int)ECommercePushType.PushInventory + "," + (int)ECommercePushType.PushWarningInventory + ") and epr.RelatedId=mp.Id where datediff(day, epr.AddTime, @0) = 0 and epr.PushResult=0 and epr.IsDelete=0),0) AS ExceptionInventory";
                _sql += ",IsNull((select count(*) from MallProduct as mp inner join ECommercePushPriceRecord as epr on epr.PushType=" + (int)ECommercePushType.PushPrice + " and epr.RelatedId=mp.Id where datediff(day, epr.AddTime, @0) = 0 and epr.PushResult=0 and epr.IsDelete=0),0) AS ExceptionPrice";
                var _AlertMessage = db.SingleOrDefault<dynamic>(_sql, DateTime.Now.ToString("yyyy-MM-dd"));
                if (_AlertMessage != null)
                {
                    ViewBag.NewOrder = _AlertMessage.NewOrder;
                    ViewBag.NewModifyOrder = _AlertMessage.NewModifyOrder;
                    ViewBag.NewCancelOrder = _AlertMessage.NewCancelOrder;
                    ViewBag.NewReturnOrder = _AlertMessage.NewReturnOrder;
                    ViewBag.NewExchangeOrder = _AlertMessage.NewExchangeOrder;
                    ViewBag.NewRejectOrder = _AlertMessage.NewRejectOrder;
                    ViewBag.ExceptionOrder = _AlertMessage.ExceptionOrder;
                    ViewBag.WMSPostReply = _AlertMessage.WMSPostReply;
                    ViewBag.ExceptionClaim = _AlertMessage.ExceptionClaim;
                    ViewBag.ExceptionRequireDelivery = _AlertMessage.ExceptionRequireDelivery;
                    ViewBag.ExceptionPushDelivery = _AlertMessage.ExceptionPushDelivery;
                    ViewBag.ExceptionInventory = _AlertMessage.ExceptionInventory;
                    ViewBag.ExceptionPrice = _AlertMessage.ExceptionPrice;
                }
                //最新订单
                ViewData["order_list"] = db.Fetch<Order>("select top 10 OrderNo,MallName,CreateDate from [Order] order by CreateDate desc");
                //最新错误订单
                ViewData["exception_order_list"] = db.Fetch<dynamic>("select top 10 OrderNo,SubOrderNo,ErrorMsg,MallName,OrderTime from View_OrderDetail where IsError=1 and ProductStatus>=@0 and IsDelete=0 order by OrderTime desc", (int)ProductStatus.Received);
                //最新仓库回复
                ViewData["wms_postreply_list"] = db.Fetch<dynamic>("select top 10 od.OrderNo,od.SubOrderNo,ows.ApiReplyMsg,o.MallName from OrderDetail as od inner join [Order] as o on od.OrderNo=o.OrderNo inner join OrderWMSReply as ows on od.SubOrderNo=ows.SubOrderNo where ows.Status=0 order by od.CreateDate desc");
                //最新请求列表
                ViewData["claim_list"] = db.Fetch<dynamic>("select top 10 oc.OrderNo,oc.SubOrderNo,oc.ClaimType,oc.ErrorMessage,isnull(m.Name,'') as MallName from OrderClaimCache as oc left join Mall as m on oc.MallSapCode=m.SapCode where Status=0 order by oc.ClaimDate desc");
                //最新快递获取列表
                ViewData["delivery_require_list"] = db.Fetch<dynamic>("select top 10 od.OrderNo,od.SubOrderNo,od.MallName,epr.PushResultMessage as ErrorMessage from View_OrderDetail as od inner join ECommercePushRecord as epr on epr.RelatedId=od.DetailId and epr.PushType=" + (int)ECommercePushType.RequireTrackingCode + " where epr.PushResult=0 and od.ProductStatus=" + (int)ProductStatus.Pending + " and epr.IsDelete=0 order by epr.EditTime desc");
                //最新快递推送列表
                ViewData["delivery_push_list"] = db.Fetch<dynamic>("select top 10 od.OrderNo,od.SubOrderNo,od.MallName,ds.InvoiceNo,epr.PushResultMessage as ErrorMessage from Deliverys as ds inner join View_OrderDetail as od on ds.SubOrderNo=od.SubOrderNo inner join ECommercePushRecord as epr on epr.RelatedId=ds.Id and epr.PushType=" + (int)ECommercePushType.PushTrackingCode + " where epr.PushResult=0 and od.ProductStatus=" + (int)ProductStatus.InDelivery + " and epr.IsDelete=0 order by epr.EditTime desc");
                //最近30天统计
                ViewBag.OrderQuantity = 0;
                ViewBag.ItemQuantity = 0;
                ViewBag.CancelQuantity = 0;
                ViewBag.ReturnQuantity = 0;
                ViewBag.ExchangeQuantity = 0;
                ViewBag.TotalOrderPayment = 0;
                var _TotalMessage = db.SingleOrDefault<dynamic>("select isnull(SUM(OrderQuantity),0) As OrderQuantity,isnull(SUM(Quantity),0) As Quantity,isnull(SUM(CancelQuantity),0) As CancelQuantity,isnull(SUM(ReturnQuantity),0) As ReturnQuantity,isnull(SUM(ExchangeQuantity),0) As ExchangeQuantity,isnull(SUM(RejectQuantity),0) As RejectQuantity,isnull(SUM(TotalOrderPayment),0) As TotalOrderPayment from AnalysisDailyOrder where datediff(day,[Date],@0)<= 0 and datediff(day,[Date],@1)>= 0 and TimeZoon=0", DateTime.Now.AddDays(-29).ToString("yyyy-MM-dd"), DateTime.Now.ToString("yyyy-MM-dd"));
                if (_TotalMessage != null)
                {
                    ViewBag.OrderQuantity = _TotalMessage.OrderQuantity;
                    ViewBag.ItemQuantity = _TotalMessage.Quantity;
                    ViewBag.CancelQuantity = _TotalMessage.CancelQuantity;
                    ViewBag.ReturnQuantity = _TotalMessage.ReturnQuantity;
                    ViewBag.ExchangeQuantity = _TotalMessage.ExchangeQuantity;
                    ViewBag.RejectQuantity = _TotalMessage.RejectQuantity;
                    ViewBag.TotalOrderPayment = _TotalMessage.TotalOrderPayment;
                }
            }

            return View();
        }

        [UserLoginAuthorize(Type = UserLoginAuthorize.ResultType.Json)]
        public ContentResult Main_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            ContentResult _result = new ContentResult();
            int _Type = VariableHelper.SaferequestInt(Request.Form["type"]);
            using (var db = new DynamicRepository())
            {
                switch (_Type)
                {
                    case 1:
                        //店铺列表
                        List<Mall> objMall_List = MallService.GetMallOption();
                        //最近30天各店铺销售统计
                        var objStoreSales_List = db.Fetch<AnalysisDailyOrder>("select MallSapCode,Quantity from AnalysisDailyOrder where datediff(day,[Date],@0)<= 0 and datediff(day,[Date],@1)>= 0 and timezoon=0", DateTime.Now.AddDays(-29).ToString("yyyy-MM-dd"), DateTime.Now.ToString("yyyy-MM-dd"));
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
                        var objMicrosMalls = db.Fetch<Mall>("select SapCode from Mall where PlatformCode=@0", (int)PlatformType.Micros_Japan).Select(p => p.SapCode).ToList();
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
                        var objProductSales_List = db.Fetch<AnalysisDailyProduct>("select Sku,Quantity from AnalysisDailyProduct where datediff(day,[Date],@0)<= 0 and datediff(day,[Date],@1)>= 0", DateTime.Now.AddDays(-29).ToString("yyyy-MM-dd"), DateTime.Now.ToString("yyyy-MM-dd"));
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
                        var objBrandSales_List = db.Fetch<dynamic>("select ap.Quantity,p.Name as Brand from AnalysisDailyProduct as ap inner join Product as p on ap.Sku=p.Sku where datediff(day,ap.[Date],@0)<= 0 and datediff(day,ap.[Date],@1)>= 0", DateTime.Now.AddDays(-29).ToString("yyyy-MM-dd"), DateTime.Now.ToString("yyyy-MM-dd"));
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
