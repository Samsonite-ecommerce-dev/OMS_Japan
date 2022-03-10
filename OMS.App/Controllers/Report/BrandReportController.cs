using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.Utility.Common;
using Samsonite.OMS.Service;
using OMS.App.Helper;

namespace OMS.App.Controllers
{
    public class BrandReportController : BaseController
    {
        //
        // GET: /OrderReport/


        #region 查询
        [UserPowerAuthorize]
        public ActionResult Index()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;
            //菜单栏
            ViewBag.MenuBar = this.MenuBar();
            //功能权限
            ViewBag.FunctionPower = this.FunctionPowers();

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public ContentResult Index_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            ContentResult _result = new ContentResult();

            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            List<string> _SqlOrder = new List<string>();
            string[] _storeid = VariableHelper.SaferequestStringArray(Request.Form["store"]);
            string _time1 = VariableHelper.SaferequestStr(Request.Form["time1"]);
            if (string.IsNullOrEmpty(_time1)) _time1 = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd");
            string _time2 = VariableHelper.SaferequestStr(Request.Form["time2"]);
            if (string.IsNullOrEmpty(_time2)) _time2 = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            string _showType = VariableHelper.SaferequestStr(Request.Form["show_type"]);
            int _chartType = VariableHelper.SaferequestInt(Request.Form["chart_type"]);
            //排序
            string _sort = VariableHelper.SaferequestStr(Request.Form["sort"]);
            string _order = VariableHelper.SaferequestStr(Request.Form["order"]);

            using (var db = new DynamicRepository())
            {
                List<string> _SelectMalls = new List<string>();
                //搜索条件
                List<string> _ALlStores = MallService.GetMallOption().Select(p => p.SapCode).ToList();
                if (_storeid != null)
                {
                    _SelectMalls = _storeid.Where(p => _ALlStores.Contains(p)).ToList();
                }
                else
                {
                    //获取全部店铺
                    _SelectMalls = _ALlStores;
                }
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "ap.MallSapCode in (" + string.Join(",", _SelectMalls) + ")", Param = null });

                //默认查询最近1个月的数据
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,ap.[Date],{0})<=0", Param = VariableHelper.SaferequestTime(_time1) });
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,ap.[Date],{0})>=0", Param = VariableHelper.SaferequestTime(_time2) });

                //查询
                //只读取能匹配到brand的sku记录,所以用内联
                var _Data = db.Fetch<BrandAnalysisView>("select isnull(p.Name,'')As Brand,ap.OrderQuantity,ap.Quantity,ap.CancelQuantity,ap.ReturnQuantity,ap.ExchangeQuantity,ap.RejectQuantity,ap.TotalOrderAmount,ap.TotalOrderPayment,ap.[Date],ap.MallSapCode from AnalysisDailyProduct as ap inner join Product as p on ap.Sku=p.Sku", _SqlWhere);
                List<BrandAnalysisView> _tempList = new List<BrandAnalysisView>();
                //合并品牌列表
                List<object[]> objBrand_List = BrandService.GetBrands();
                foreach (var _o in objBrand_List)
                {
                    List<string> _Bs = BrandService.GetSons(VariableHelper.SaferequestInt(_o[0]));
                    List<BrandAnalysisView> _d = _Data.Where(p => _Bs.Contains(p.Brand)).ToList();
                    _tempList.Add(new BrandAnalysisView()
                    {
                        Date = VariableHelper.SaferequestTime(_time1),
                        BrandID = VariableHelper.SaferequestInt(_o[0]),
                        Brand = _o[1].ToString(),
                        MallSapCode = ((_storeid != null) ? string.Join(",", _storeid) : ""),
                        OrderQuantity = _d.Sum(p => p.OrderQuantity),
                        Quantity = _d.Sum(p => p.Quantity),
                        CancelQuantity = _d.Sum(p => p.CancelQuantity),
                        ReturnQuantity = _d.Sum(p => p.ReturnQuantity),
                        ExchangeQuantity = _d.Sum(p => p.ExchangeQuantity),
                        RejectQuantity = _d.Sum(p => p.RejectQuantity),
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
                                Value = dy.RejectQuantity.ToString()
                            });
                        }
                        else if (_chartType == 7)
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
                        Paras = objData
                    });
                }
                else
                {
                    //新建页面名称
                    string _tabName = $"{this.MenuName(this.CurrentFunctionID)}_Detail";
                    string _SearchTab = GetSearchOrderTab();
                    //查询
                    var _r = new
                    {
                        rows = from dy in _Data
                               select new
                               {
                                   s1 = $"<a href=\"javascript:void(0);\" onclick=\"window.parent.frames.OpenTab('{_tabName}','{Url.Action("Detail", "BrandReport")}?brand={dy.BrandID}&mall={dy.MallSapCode}',true);\">{dy.Brand}</a>",
                                   s2 = dy.OrderQuantity,
                                   s3 = dy.Quantity,
                                   s4 = dy.CancelQuantity,
                                   s5 = dy.ReturnQuantity,
                                   s6 = dy.ExchangeQuantity,
                                   s7 = dy.RejectQuantity,
                                   s8 = VariableHelper.FormateMoney(dy.TotalOrderPayment),
                                   s9 = (dy.Quantity > 0) ? VariableHelper.FormateMoney(OrderHelper.MathRound(dy.TotalOrderPayment / dy.Quantity)) : "0",
                                   s10 = $"<a href=\"javascript:void(0);\" onclick=\"window.parent.frames.OpenTab('{_SearchTab}','{Url.Action("Index", "OrderQuery")}?store={dy.MallSapCode}&brand={dy.BrandID}&time1={_time1}&time2={_time2}',true);\">{_LanguagePack["common_view_order"]}</a>"
                               }
                    };
                    _result.Content = JsonHelper.JsonSerialize(_r);
                }
                return _result;
            }
        }
        #endregion

        #region 详情
        [UserPowerAuthorize]
        public ActionResult Detail()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;
            //菜单栏
            ViewBag.MenuBar = this.MenuBar();

            int _brand = VariableHelper.SaferequestInt(Request.QueryString["brand"]);
            string _mallSapCode = VariableHelper.SaferequestStr(Request.QueryString["mall"]);

            using (var db = new ebEntities())
            {
                Brand objBrand = db.Brand.Where(p => p.ID == _brand).SingleOrDefault();
                if (objBrand != null)
                {
                    //快速时间选项
                    ViewData["quicktime_list"] = QuickTimeHelper.QuickTimeOption();
                    //品牌信息
                    ViewBag.BrandID = objBrand.ID;
                    ViewBag.BrandName = objBrand.BrandName;
                    //店铺信息
                    ViewBag.MallSapCodes = _mallSapCode;

                    return View();
                }
                else
                {
                    return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.NoMessage });
                }
            }
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public ContentResult Detail_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            ContentResult _result = new ContentResult();

            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            string _store = VariableHelper.SaferequestStr(Request.Form["store"]);
            int _brand = VariableHelper.SaferequestInt(Request.Form["brand"]);
            string _time1 = VariableHelper.SaferequestStr(Request.Form["time1"]);
            string _time2 = VariableHelper.SaferequestStr(Request.Form["time2"]);
            string _showType = VariableHelper.SaferequestStr(Request.Form["show_type"]);
            int _chartType = VariableHelper.SaferequestInt(Request.Form["chart_type"]);
            int _pageSize = VariableHelper.SaferequestInt(Request.Form["rows"]);
            int _page = VariableHelper.SaferequestInt(Request.Form["page"]);

            using (var db = new DynamicRepository())
            {
                Brand objBrand = db.SingleOrDefault<Brand>("select ID,BrandName from Brand where ID=@0", _brand);
                if (objBrand != null)
                {
                    List<string> _SelectMalls = new List<string>();
                    long _TotalTime = 0;
                    int _TotalPage = 0;
                    DateTime _StTimePage = new DateTime();
                    DateTime _EdTimePage = new DateTime();
                    //计算时间段,默认近一个月
                    DateTime _BeginTime = (string.IsNullOrEmpty(_time1)) ? DateTime.Today.AddMonths(-1) : VariableHelper.SaferequestTime(_time1);
                    DateTime _EndTime = (string.IsNullOrEmpty(_time2)) ? DateTime.Today : VariableHelper.SaferequestTime(_time2);

                    if (_showType.ToLower() == "chart")
                    {
                        //如果是图表则显示所有记录
                        _StTimePage = _BeginTime;
                        _EdTimePage = _EndTime;
                    }
                    else
                    {
                        _TotalTime = TimeHelper.DateDiff(TimeHelper.DateInterval.Day, _BeginTime, _EndTime) + 1;
                        _TotalPage = (_TotalTime % _pageSize == 0) ? (int)(_TotalTime / _pageSize) : ((int)(_pageSize / _pageSize) + 1);
                        _StTimePage = _EndTime.AddDays(1 - (_page * _pageSize));
                        if (DateTime.Compare(_StTimePage, _BeginTime) == -1)
                        {
                            _StTimePage = _BeginTime;
                        }
                        _EdTimePage = _EndTime.AddDays(-((_page - 1) * _pageSize));
                    }

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
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "MallSapCode in (" + string.Join(",", _SelectMalls) + ")", Param = null });
                    //获取子品牌
                    List<string> _Brands = BrandService.GetSons(objBrand.ID);
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "charindex(p.Name,{0})>0", Param = string.Join(",", _Brands) });

                    //翻页计算时间
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(DAY,ap.[Date],{0})<=0", Param = _StTimePage.ToString("yyyy-MM-dd") });
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(DAY,ap.[Date],{0})>=0", Param = _EdTimePage.ToString("yyyy-MM-dd") });

                    //查询
                    var _Data = db.Fetch<BrandAnalysisView>("select isnull(p.Name,'')As Brand,ap.OrderQuantity,ap.Quantity,ap.CancelQuantity,ap.ReturnQuantity,ap.ExchangeQuantity,ap.RejectQuantity,ap.TotalOrderAmount,ap.TotalOrderPayment,ap.[Date],ap.MallSapCode from AnalysisDailyProduct as ap inner join Product as p on ap.Sku=p.Sku", _SqlWhere);
                    //如果是多选店铺,合并各店铺合计
                    //合并品牌
                    List<BrandAnalysisView> _tempList = new List<BrandAnalysisView>();
                    List<DateTime> _times = _Data.GroupBy(p => p.Date).Select(o => (DateTime)o.Key).ToList();
                    foreach (var _o in _times)
                    {
                        var _temps = _Data.Where(p => p.Date == _o).ToList();
                        _tempList.Add(new BrandAnalysisView()
                        {
                            Date = _o,
                            BrandID = objBrand.ID,
                            Brand = objBrand.BrandName,
                            OrderQuantity = _temps.Sum(p => p.OrderQuantity),
                            Quantity = _temps.Sum(p => p.Quantity),
                            CancelQuantity = _temps.Sum(p => p.CancelQuantity),
                            ReturnQuantity = _temps.Sum(p => p.ReturnQuantity),
                            ExchangeQuantity = _temps.Sum(p => p.ExchangeQuantity),
                            RejectQuantity = _temps.Sum(p => p.RejectQuantity),
                            TotalOrderAmount = _temps.Sum(p => (decimal)p.TotalOrderAmount),
                            TotalOrderPayment = _temps.Sum(p => (decimal)p.TotalOrderPayment)
                        });
                    }
                    _Data = _tempList;
                    //合并数据,如果没有则添加零数据行
                    for (DateTime t = _EdTimePage; t >= _StTimePage; t = t.AddDays(-1))
                    {
                        if (_Data.Where(p => p.Date.ToString("yyyy-MM-dd") == t.ToString("yyyy-MM-dd")).FirstOrDefault() == null)
                        {
                            _Data.Add(new BrandAnalysisView()
                            {
                                Date = t,
                                BrandID = objBrand.ID,
                                Brand = objBrand.BrandName,
                                OrderQuantity = 0,
                                CancelQuantity = 0,
                                ReturnQuantity = 0,
                                ExchangeQuantity = 0,
                                RejectQuantity = 0,
                                TotalOrderAmount = 0,
                                TotalOrderPayment = 0
                            });
                        }
                    }

                    if (_showType.ToLower() == "chart")
                    {
                        //按照正序显示
                        _Data = _Data.OrderBy(p => p.Date).ToList();
                        //折线图
                        List<EChartsHelper.Line.Config.Para> objData = new List<EChartsHelper.Line.Config.Para>();
                        if (_chartType == 2)
                        {
                            objData.Add(new EChartsHelper.Line.Config.Para()
                            {
                                Legend = _LanguagePack["brandreport_detail_sales_amount"],
                                Name = _LanguagePack["brandreport_detail_sales_amount"],
                                Datas = _Data.Select(p => (object)p.TotalOrderPayment).ToList()
                            });
                        }
                        else
                        {
                            objData.Add(new EChartsHelper.Line.Config.Para()
                            {
                                Legend = _LanguagePack["brandreport_detail_order_quantity"],
                                Name = _LanguagePack["brandreport_detail_order_quantity"],
                                Datas = _Data.Select(p => (object)p.OrderQuantity).ToList()
                            });
                            objData.Add(new EChartsHelper.Line.Config.Para()
                            {
                                Legend = _LanguagePack["brandreport_detail_item_quantity"],
                                Name = _LanguagePack["brandreport_detail_item_quantity"],
                                Datas = _Data.Select(p => (object)p.Quantity).ToList()
                            });
                            objData.Add(new EChartsHelper.Line.Config.Para()
                            {
                                Legend = _LanguagePack["brandreport_detail_cancel_quantity"],
                                Name = _LanguagePack["brandreport_detail_cancel_quantity"],
                                Datas = _Data.Select(p => (object)p.CancelQuantity).ToList()
                            });
                            objData.Add(new EChartsHelper.Line.Config.Para()
                            {
                                Legend = _LanguagePack["brandreport_detail_return_quantity"],
                                Name = _LanguagePack["brandreport_detail_return_quantity"],
                                Datas = _Data.Select(p => (object)p.ReturnQuantity).ToList()
                            });
                            objData.Add(new EChartsHelper.Line.Config.Para()
                            {
                                Legend = _LanguagePack["brandreport_detail_exchange_quantity"],
                                Name = _LanguagePack["brandreport_detail_exchange_quantity"],
                                Datas = _Data.Select(p => (object)p.ExchangeQuantity).ToList()
                            });
                            objData.Add(new EChartsHelper.Line.Config.Para()
                            {
                                Legend = _LanguagePack["brandreport_detail_reject_quantity"],
                                Name = _LanguagePack["brandreport_detail_reject_quantity"],
                                Datas = _Data.Select(p => (object)p.RejectQuantity).ToList()
                            });
                        }

                        _result.Content = EChartsHelper.Line.Set(new EChartsHelper.Line.Config()
                        {
                            Title = "",
                            XAxis = _Data.Select(p => (string)p.Date.ToString("yyyy-MM-dd")).ToList(),
                            YAxis = "",
                            Paras = objData
                        });
                    }
                    else
                    {
                        //合计
                        _SqlWhere[2].Param = _SqlWhere[2].Param.ToString().Replace(_StTimePage.ToString("yyyy-MM-dd"), _BeginTime.ToString("yyyy-MM-dd"));
                        _SqlWhere[3].Param = _SqlWhere[3].Param.ToString().Replace(_EdTimePage.ToString("yyyy-MM-dd"), _EndTime.ToString("yyyy-MM-dd"));
                        //此处统计所有记录集合,所以需要将分页时间替换掉
                        List<AnalysisDailyProduct> _TotalData = db.Fetch<AnalysisDailyProduct>($"select isnull(sum(OrderQuantity),0) as OrderQuantity,isnull(sum(ap.Quantity),0) as Quantity,isnull(sum(ap.CancelQuantity),0) as CancelQuantity,isnull(sum(ap.ReturnQuantity),0) as ReturnQuantity,isnull(sum(ap.ExchangeQuantity),0) as ExchangeQuantity,isnull(sum(ap.RejectQuantity),0) as RejectQuantity,isnull(sum(ap.TotalOrderAmount),0) as TotalOrderAmount,isnull(sum(ap.TotalOrderPayment),0) as TotalOrderPayment from AnalysisDailyProduct as ap inner join Product as p on ap.Sku=p.Sku", _SqlWhere);

                        //新建页面名称
                        string _SearchTab = GetSearchOrderTab();
                        //查询
                        var _r = new
                        {
                            total = _TotalTime,
                            rows = from dy in _Data.OrderByDescending(p => p.Date)
                                   select new
                                   {
                                       s1 = dy.Date.ToString("yyyy-MM-dd"),
                                       s2 = dy.OrderQuantity,
                                       s3 = dy.Quantity,
                                       s4 = dy.CancelQuantity,
                                       s5 = dy.ReturnQuantity,
                                       s6 = dy.ExchangeQuantity,
                                       s7 = dy.RejectQuantity,
                                       s8 = VariableHelper.FormateMoney(dy.TotalOrderPayment),
                                       s9 = (dy.Quantity > 0) ? VariableHelper.FormateMoney(OrderHelper.MathRound(dy.TotalOrderPayment / dy.Quantity)) : "0",
                                       s10 = $"<a href=\"javascript:void(0);\" onclick=\"window.parent.frames.OpenTab('{_SearchTab}','{Url.Action("Index", "OrderQuery")}?store={dy.MallSapCode}&brand={dy.BrandID}&time={dy.Date.ToString("yyyy-MM-dd")}',true);\">{_LanguagePack["common_view_order"]}</a>"
                                   },
                            footer = from dy in _TotalData
                                     select new
                                     {
                                         s1 = string.Format("<label class=\"color_danger font-bold\">{0}</label>", _LanguagePack["common_total"]),
                                         s2 = dy.OrderQuantity,
                                         s3 = dy.Quantity,
                                         s4 = dy.CancelQuantity,
                                         s5 = dy.ReturnQuantity,
                                         s6 = dy.ExchangeQuantity,
                                         s7 = dy.RejectQuantity,
                                         s8 = VariableHelper.FormateMoney(dy.TotalOrderPayment),
                                         s9 = (dy.Quantity > 0) ? VariableHelper.FormateMoney(OrderHelper.MathRound(dy.TotalOrderPayment / dy.Quantity)) : "0",
                                         s10 = ""
                                     }
                        };
                        _result.Content = JsonHelper.JsonSerialize(_r);
                    }
                }
                return _result;
            }
        }
        #endregion
    }
}
