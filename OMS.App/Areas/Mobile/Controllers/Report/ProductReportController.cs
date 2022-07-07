using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service;
using Samsonite.Utility.Common;

using OMS.App.Helper;

namespace OMS.App.Areas.Mobile.Controllers
{
    public class ProductReportController : BaseController
    {
        // GET: Mobile/ProductReport

        [UserLoginAuthorize]
        public ActionResult Index()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;
            //菜单栏
            ViewBag.MenuName = this.MenuName();
            //下拉菜单栏
            ViewBag.MenuBar = this.MenuBar();
            //品牌列表
            ViewData["brand_list"] = BrandService.GetBrands();
            //店铺列表
            ViewData["store_list"] = MallService.GetMallOption();

            return View();
        }

        [UserLoginAuthorize]
        public ActionResult Report()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;

            ViewBag.Stores = VariableHelper.SaferequestStr(Request.Form["Store"]);
            ViewBag.Brand = VariableHelper.SaferequestStr(Request.Form["Brand"]);
            ViewBag.Time1 = VariableHelper.SaferequestStr(Request.Form["Time1"]);
            ViewBag.Time2 = VariableHelper.SaferequestStr(Request.Form["Time2"]);
            ViewBag.Sku = VariableHelper.SaferequestStr(Request.Form["Sku"]);

            return View();
        }

        [UserLoginAuthorize(Type = UserLoginAuthorize.ResultType.Json)]
        public ContentResult Report_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            ContentResult _result = new ContentResult();

            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            List<string> _SqlOrder = new List<string>();
            string _store = VariableHelper.SaferequestStr(Request.Form["store"]);
            int _brand = VariableHelper.SaferequestInt(Request.Form["brand"]);
            string _sku = VariableHelper.SaferequestStr(Request.Form["sku"]);
            string _time1 = VariableHelper.SaferequestStr(Request.Form["time1"]);
            if (string.IsNullOrEmpty(_time1)) _time1 = DateTime.Now.ToString("yyyy-MM-dd");
            string _time2 = VariableHelper.SaferequestStr(Request.Form["time2"]);
            if (string.IsNullOrEmpty(_time2)) _time2 = DateTime.Now.ToString("yyyy-MM-dd");
            string _showType = VariableHelper.SaferequestStr(Request.Form["show_type"]);
            int _chartType = VariableHelper.SaferequestInt(Request.Form["chart_type"]);

            using (var db = new DynamicRepository())
            {
                List<string> _SelectMalls = new List<string>();

                //搜索条件
                if (!string.IsNullOrEmpty(_store))
                {
                    string[] _SelectMalls_Array = _store.Split(',');
                    foreach (string str in _SelectMalls_Array)
                    {
                        _SelectMalls.Add(str);
                    }
                }
                else
                {
                    //获取全部店铺
                    _SelectMalls = MallService.GetMallOption().Select(p => p.SapCode).ToList();
                }
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "ap.MallSapCode in (" + string.Join(",", _SelectMalls) + ")", Param = null });

                if (_brand > 0)
                {
                    string _Brands = string.Join(",", BrandService.GetSons(_brand));
                    if (!string.IsNullOrEmpty(_Brands))
                    {
                        _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "charindex(p.Name,{0})>0", Param = _Brands });
                    }
                }

                if (!string.IsNullOrEmpty(_sku))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "ap.Sku like {0}", Param = '%' + _sku + '%' });
                }

                //默认查询昨天的产品信息
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,ap.[Date],{0})<=0", Param = VariableHelper.SaferequestTime(_time1) });
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,ap.[Date],{0})>=0", Param = VariableHelper.SaferequestTime(_time2) });

                //查询
                var _Data = db.Fetch<ProductAnalysisView>("select isnull(p.[Description],'')As ProductName,isnull(p.Name,'')As Brand,isnull(p.SalesPrice,0) as SalesPrice,ap.Sku,ap.OrderQuantity,ap.Quantity,ap.CancelQuantity,ap.ReturnQuantity,ap.ExchangeQuantity,ap.TotalOrderAmount,ap.TotalOrderPayment,ap.[Date],ap.MallSapCode from AnalysisDailyProduct as ap left join Product as p on ap.Sku=p.Sku", _SqlWhere);
                //如果是多选店铺,合并各店铺合计
                if (_SelectMalls.Count > 1)
                {
                    List<ProductAnalysisView> _tempList = new List<ProductAnalysisView>();
                    List<string> _skus = _Data.GroupBy(p => p.Sku).Select(o => o.Key).ToList();
                    foreach (var _o in _skus)
                    {
                        var _temps = _Data.Where(p => p.Sku == _o).ToList();
                        var _temp_single = _temps.FirstOrDefault();
                        _tempList.Add(new ProductAnalysisView()
                        {
                            ProductName = _temp_single.ProductName,
                            Brand = _temp_single.Brand,
                            Sku = _temp_single.Sku,
                            MarketPrice = _temp_single.MarketPrice,
                            OrderQuantity = _temps.Sum(p => p.OrderQuantity),
                            Quantity = _temps.Sum(p => p.Quantity),
                            CancelQuantity = _temps.Sum(p => p.CancelQuantity),
                            ReturnQuantity = _temps.Sum(p => p.ReturnQuantity),
                            ExchangeQuantity = _temps.Sum(p => p.ExchangeQuantity),
                            TotalOrderAmount = _temps.Sum(p => p.TotalOrderAmount),
                            TotalOrderPayment = _temps.Sum(p => p.TotalOrderPayment)
                        });
                    }
                    _Data = _tempList;
                }
                //限制只取前20
                _Data = _Data.OrderByDescending(p => p.Quantity).Take(20).ToList();
                if (_showType.ToLower() == "chart")
                {
                    //柱状图
                    List<EChartsHelper.Bar.Config.Para> objData = new List<EChartsHelper.Bar.Config.Para>();
                    if (_chartType == 2)
                    {
                        objData.Add(new EChartsHelper.Bar.Config.Para()
                        {
                            Legend = _LanguagePack["productreport_index_item_quantity"],
                            Name = _LanguagePack["productreport_index_item_quantity"],
                            Datas = _Data.Select(p => (object)p.Quantity).ToList()
                        });
                    }
                    else if (_chartType == 3)
                    {
                        objData.Add(new EChartsHelper.Bar.Config.Para()
                        {
                            Legend = _LanguagePack["productreport_index_cancel_quantity"],
                            Name = _LanguagePack["productreport_index_cancel_quantity"],
                            Datas = _Data.Select(p => (object)p.CancelQuantity).ToList()
                        });
                    }
                    else if (_chartType == 4)
                    {
                        objData.Add(new EChartsHelper.Bar.Config.Para()
                        {
                            Legend = _LanguagePack["productreport_index_return_quantity"],
                            Name = _LanguagePack["productreport_index_return_quantity"],
                            Datas = _Data.Select(p => (object)p.ReturnQuantity).ToList()
                        });
                    }
                    else if (_chartType == 5)
                    {
                        objData.Add(new EChartsHelper.Bar.Config.Para()
                        {
                            Legend = _LanguagePack["productreport_index_exchange_quantity"],
                            Name = _LanguagePack["productreport_index_exchange_quantity"],
                            Datas = _Data.Select(p => (object)p.ExchangeQuantity).ToList()
                        });
                    }
                    else if (_chartType == 6)
                    {
                        objData.Add(new EChartsHelper.Bar.Config.Para()
                        {
                            Legend = _LanguagePack["productreport_index_sales_amount"],
                            Name = _LanguagePack["productreport_index_sales_amount"],
                            Datas = _Data.Select(p => (object)p.TotalOrderPayment).ToList()
                        });
                    }
                    else
                    {
                        objData.Add(new EChartsHelper.Bar.Config.Para()
                        {
                            Legend = _LanguagePack["productreport_index_order_quantity"],
                            Name = _LanguagePack["productreport_index_order_quantity"],
                            Datas = _Data.Select(p => (object)p.Quantity).ToList()
                        });
                    }

                    _result.Content = EChartsHelper.Bar.Set(new EChartsHelper.Bar.Config()
                    {
                        Title = "",
                        XAxis = _Data.Select(p => (string)p.Sku).ToList(),
                        YAxis = "",
                        Paras = objData,
                        IsMagicType = false,
                        IsRestore = false,
                        IsSaveAsImage = false
                    });
                }
                else
                {
                    //查询
                    var _r = new
                    {
                        rows = from dy in _Data
                               select new
                               {
                                   s1 = dy.Sku,
                                   s2 = dy.ProductName,
                                   s3 = dy.Brand,
                                   s4 = VariableHelper.FormateMoney(dy.MarketPrice),
                                   s5 = dy.OrderQuantity,
                                   s6 = dy.Quantity,
                                   s7 = dy.CancelQuantity,
                                   s8 = dy.ReturnQuantity,
                                   s9 = dy.ExchangeQuantity,
                                   s10 = VariableHelper.FormateMoney(Math.Round(dy.TotalOrderPayment, 0)),
                                   s11 = (dy.Quantity > 0) ? VariableHelper.FormateMoney(OrderHelper.MathRound(Math.Round(dy.TotalOrderPayment / dy.Quantity, 0))) : "0"
                               }
                    };
                    _result.Content = JsonHelper.JsonSerialize(_r);
                }
            }

            return _result;
        }
    }
}