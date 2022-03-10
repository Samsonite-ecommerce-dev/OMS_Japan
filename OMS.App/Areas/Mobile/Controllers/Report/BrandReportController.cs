using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service;
using Samsonite.Utility.Common;
using OMS.App.Helper;

namespace OMS.App.Areas.Mobile.Controllers
{
    public class BrandReportController : BaseController
    {
        // GET: Mobile/BrandReportController

        [UserLoginAuthorize]
        public ActionResult Index()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;
            //菜单栏
            ViewBag.MenuName = this.MenuName();
            //下拉菜单栏
            ViewBag.MenuBar = this.MenuBar();
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
            ViewBag.Time1 = VariableHelper.SaferequestStr(Request.Form["Time1"]);
            ViewBag.Time2 = VariableHelper.SaferequestStr(Request.Form["Time2"]);

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
            string _time1 = VariableHelper.SaferequestStr(Request.Form["time1"]);
            if (string.IsNullOrEmpty(_time1)) _time1 = DateTime.Now.ToString("yyyy-MM-dd");
            string _time2 = VariableHelper.SaferequestStr(Request.Form["time2"]);
            if (string.IsNullOrEmpty(_time2)) _time2 = DateTime.Now.ToString("yyyy-MM-dd");
            string _showType = VariableHelper.SaferequestStr(Request.Form["show_type"]);
            int _chartType = VariableHelper.SaferequestInt(Request.Form["chart_type"]);
            //排序
            string _sort = VariableHelper.SaferequestStr(Request.Form["sort"]);
            string _order = VariableHelper.SaferequestStr(Request.Form["order"]);

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

                //默认查询昨天的产品信息
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,ap.[Date],{0})<=0", Param =VariableHelper.SaferequestTime(_time1)  });
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,ap.[Date],{0})>=0", Param = VariableHelper.SaferequestTime(_time2) });

                //查询
                //只读取能匹配到brand的sku记录,所以用内联
                var _Data = db.Fetch<BrandAnalysisView>("select isnull(p.Name,'')As Brand,ap.OrderQuantity,ap.Quantity,ap.CancelQuantity,ap.ReturnQuantity,ap.ExchangeQuantity,ap.TotalOrderAmount,ap.TotalOrderPayment,ap.[Date],ap.MallSapCode from AnalysisDailyProduct as ap inner join Product as p on ap.Sku=p.Sku", _SqlWhere);
                List<BrandAnalysisView> _tempList = new List<BrandAnalysisView>();
                //合并品牌列表
                List<object[]> objBrand_List = BrandService.GetBrands();
                foreach (var _o in objBrand_List)
                {
                    List<string> _Bs = BrandService.GetSons(VariableHelper.SaferequestInt(_o[0]));
                    List<BrandAnalysisView> _d = _Data.Where(p => _Bs.Contains(p.Brand)).ToList();
                    _tempList.Add(new BrandAnalysisView()
                    {
                        BrandID = VariableHelper.SaferequestInt(_o[0]),
                        Brand = _o[1].ToString(),
                        MallSapCode = _store,
                        OrderQuantity = _d.Sum(p => p.OrderQuantity),
                        Quantity = _d.Sum(p => p.Quantity),
                        CancelQuantity = _d.Sum(p => p.CancelQuantity),
                        ReturnQuantity = _d.Sum(p => p.ReturnQuantity),
                        ExchangeQuantity = _d.Sum(p => p.ExchangeQuantity),
                        TotalOrderAmount = _d.Sum(p => (decimal)p.TotalOrderAmount),
                        TotalOrderPayment = _d.Sum(p => (decimal)p.TotalOrderPayment)
                    });
                }
                _Data = _tempList;

                //排序条件
                if (!string.IsNullOrEmpty(_sort))
                {
                    string[] _sort_array = _sort.Split(',');
                    string[] _order_array = _order.Split(',');
                    for (int t = 0; t < _sort_array.Length; t++)
                    {
                        if (_sort_array[t] == "s2")
                        {
                            if (_order_array[t] == "desc")
                            {
                                _Data = _Data.OrderByDescending(p => p.OrderQuantity).ToList();
                            }
                            else
                            {
                                _Data = _Data.OrderBy(p => p.OrderQuantity).ToList();
                            }
                        }
                        else if (_sort_array[t] == "s3")
                        {
                            if (_order_array[t] == "desc")
                            {
                                _Data = _Data.OrderByDescending(p => p.Quantity).ToList();
                            }
                            else
                            {
                                _Data = _Data.OrderBy(p => p.Quantity).ToList();
                            }
                        }
                        else if (_sort_array[t] == "s7")
                        {
                            if (_order_array[t] == "desc")
                            {
                                _Data = _Data.OrderByDescending(p => p.TotalOrderPayment).ToList();
                            }
                            else
                            {
                                _Data = _Data.OrderBy(p => p.TotalOrderPayment).ToList();
                            }
                        }
                        else
                        {

                        }
                    }
                }

                if (_showType.ToLower() == "chart")
                {
                    //排除无销售的品牌
                    _Data = _Data.Where(p => p.OrderQuantity > 0).ToList();
                    //数据
                    List<EChartsHelper.Pie.Config.Para.Data> _pies = new List<EChartsHelper.Pie.Config.Para.Data>();
                    foreach (var dy in _Data)
                    {
                        if (_chartType == 2)
                        {
                            _pies.Add(new EChartsHelper.Pie.Config.Para.Data()
                            {
                                Name = dy.Brand,
                                Value = dy.Quantity.ToString()
                            });
                        }
                        else if (_chartType == 3)
                        {
                            _pies.Add(new EChartsHelper.Pie.Config.Para.Data()
                            {
                                Name = dy.Brand,
                                Value = dy.CancelQuantity.ToString()
                            });
                        }
                        else if (_chartType == 4)
                        {
                            _pies.Add(new EChartsHelper.Pie.Config.Para.Data()
                            {
                                Name = dy.Brand,
                                Value = dy.ReturnQuantity.ToString()
                            });
                        }
                        else if (_chartType == 5)
                        {
                            _pies.Add(new EChartsHelper.Pie.Config.Para.Data()
                            {
                                Name = dy.Brand,
                                Value = dy.ExchangeQuantity.ToString()
                            });
                        }
                        else if (_chartType == 6)
                        {
                            _pies.Add(new EChartsHelper.Pie.Config.Para.Data()
                            {
                                Name = dy.Brand,
                                Value = dy.TotalOrderPayment.ToString()
                            });
                        }
                        else
                        {
                            _pies.Add(new EChartsHelper.Pie.Config.Para.Data()
                            {
                                Name = dy.Brand,
                                Value = dy.OrderQuantity.ToString()
                            });
                        }
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
                        IsMagicType = false,
                        IsRestore = false,
                        IsSaveAsImage = false
                    });
                }
                else
                {
                    //按销量排行
                    _Data = _Data.OrderByDescending(p => p.TotalOrderPayment).ToList();
                    //总销量
                    decimal _TotalPayments = _Data.Sum(p => p.TotalOrderPayment);
                    //查询
                    var _r = new
                    {
                        rows = from dy in _Data
                               select new
                               {
                                   s1 = dy.Brand,
                                   s2 = dy.OrderQuantity,
                                   s3 = dy.Quantity,
                                   s4 = dy.CancelQuantity,
                                   s5 = dy.ReturnQuantity,
                                   s6 = dy.ExchangeQuantity,
                                   s7 = VariableHelper.FormateMoney(Math.Round(dy.TotalOrderPayment, 0)),
                                   s8 = (_TotalPayments > 0) ? $"{Math.Round(dy.TotalOrderPayment / _TotalPayments * 100, 2)}%" : "0%",
                                   s9 = (dy.Quantity > 0) ? VariableHelper.FormateMoney(OrderHelper.MathRound(Math.Round(dy.TotalOrderPayment / dy.Quantity, 0))) : "0"
                               }
                    };
                    _result.Content = JsonHelper.JsonSerialize(_r);

                }
                return _result;
            }
        }
    }
}