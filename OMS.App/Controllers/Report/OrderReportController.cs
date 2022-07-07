using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.Database;
using Samsonite.Utility.Common;
using Samsonite.OMS.Service;

using OMS.App.Helper;

namespace OMS.App.Controllers
{
    public class OrderReportController : BaseController
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
            //快速时间选项
            ViewData["quicktime_list"] = QuickTimeHelper.QuickTimeOption();

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public ContentResult Index_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            ContentResult _result = new ContentResult();

            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            string[] _storeid = VariableHelper.SaferequestStringArray(Request.Form["store"]);
            string _time1 = VariableHelper.SaferequestStr(Request.Form["time1"]);
            string _time2 = VariableHelper.SaferequestStr(Request.Form["time2"]);
            int _type = VariableHelper.SaferequestInt(Request.Form["type"]);
            string _showType = VariableHelper.SaferequestStr(Request.Form["show_type"]);
            int _chartType = VariableHelper.SaferequestInt(Request.Form["chart_type"]);
            int _pageSize = VariableHelper.SaferequestInt(Request.Form["rows"]);
            int _page = VariableHelper.SaferequestInt(Request.Form["page"]);
            //排序
            string _sort = VariableHelper.SaferequestStr(Request.Form["sort"]);
            string _order = VariableHelper.SaferequestStr(Request.Form["order"]);
            //计算时间段,默认近一个月
            DateTime _BeginTime = (string.IsNullOrEmpty(_time1)) ? DateTime.Today.AddMonths(-1) : VariableHelper.SaferequestTime(_time1);
            DateTime _EndTime = (string.IsNullOrEmpty(_time2)) ? DateTime.Today.AddDays(-1) : VariableHelper.SaferequestTime(_time2);
            //报表类型
            int _TimeZoon = 0;
            if (_type == 1)
            {
                _TimeZoon = 1;
            }
            else if (_type == 2)
            {
                _TimeZoon = 2;
            }
            else
            {
                _TimeZoon = 0;
            }

            //店铺列表
            List<Mall> objMall_List = MallService.GetMallOption();
            long _TotalTime = 0;
            int _TotalPage = 0;
            DateTime _StTimePage = new DateTime();
            DateTime _EdTimePage = new DateTime();

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
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "MallSapCode in (" + string.Join(",", _SelectMalls) + ")", Param = null });

                //报表时间类型
                if (_TimeZoon == 1)
                {
                    if (_showType.ToLower() == "chart")
                    {
                        //如果是图表则显示所有记录
                        _StTimePage = _BeginTime;
                        _EdTimePage = _EndTime;
                    }
                    else
                    {
                        _TotalTime = TimeHelper.DateDiff(TimeHelper.DateInterval.Month, _BeginTime, _EndTime) + 1;
                        _TotalPage = (_TotalTime % _pageSize == 0) ? (int)(_TotalTime / _pageSize) : ((int)(_pageSize / _pageSize) + 1);
                        _StTimePage = _EndTime.AddMonths(1 - (_page * _pageSize));
                        if (DateTime.Compare(_StTimePage, _BeginTime) == -1)
                        {
                            _StTimePage = _BeginTime;
                        }
                        _EdTimePage = _EndTime.AddMonths(-((_page - 1) * _pageSize));
                    }

                    //类型
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "TimeZoon={0}", Param = _TimeZoon });
                    //翻页计算时间
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(MONTH,[Date],{0})<=0", Param = _StTimePage.ToString("yyyy-MM-dd") });
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(MONTH,[Date],{0})>=0", Param = _EdTimePage.ToString("yyyy-MM-dd") });
                }
                else if (_TimeZoon == 2)
                {
                    if (_showType.ToLower() == "chart")
                    {
                        _StTimePage = _BeginTime;
                        _EdTimePage = _EndTime;
                    }
                    else
                    {
                        _TotalTime = TimeHelper.DateDiff(TimeHelper.DateInterval.Year, _BeginTime, _EndTime) + 1;
                        _TotalPage = (_TotalTime % _pageSize == 0) ? (int)(_TotalTime / _pageSize) : ((int)(_pageSize / _pageSize) + 1);
                        _StTimePage = _EndTime.AddYears(1 - (_page * _pageSize));
                        if (DateTime.Compare(_StTimePage, _BeginTime) == -1)
                        {
                            _StTimePage = _BeginTime;
                        }
                        _EdTimePage = _EndTime.AddYears(-((_page - 1) * _pageSize));
                    }

                    //类型
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "TimeZoon={0}", Param = _TimeZoon });
                    //翻页计算时间
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(YEAR,[Date],{0})<=0", Param = _StTimePage.ToString("yyyy-MM-dd") });
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(YEAR,[Date],{0})>=0", Param = _EdTimePage.ToString("yyyy-MM-dd") });
                }
                else
                {
                    if (_showType.ToLower() == "chart")
                    {
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

                    //类型
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "TimeZoon={0}", Param = _TimeZoon });
                    //翻页计算时间
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(DAY,[Date],{0})<=0", Param = _StTimePage.ToString("yyyy-MM-dd") });
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(DAY,[Date],{0})>=0", Param = _EdTimePage.ToString("yyyy-MM-dd") });
                }

                //查询
                List<AnalysisDailyOrder> _Data = db.Fetch<AnalysisDailyOrder>("select * from AnalysisDailyOrder", _SqlWhere);
                //如果是多选店铺,合并各店铺合计
                if (_SelectMalls.Count > 1)
                {
                    List<AnalysisDailyOrder> _tempList = new List<AnalysisDailyOrder>();
                    List<DateTime> _times = _Data.GroupBy(p => p.Date).Select(o => o.Key).ToList();
                    foreach (var _o in _times)
                    {
                        var _temps = _Data.Where(p => p.Date == _o).ToList();
                        _tempList.Add(new AnalysisDailyOrder()
                        {
                            Date = _o,
                            OrderQuantity = _temps.Sum(p => p.OrderQuantity),
                            Quantity = _temps.Sum(p => p.Quantity),
                            CancelQuantity = _temps.Sum(p => p.CancelQuantity),
                            ReturnQuantity = _temps.Sum(p => p.ReturnQuantity),
                            ExchangeQuantity = _temps.Sum(p => p.ExchangeQuantity),
                            RejectQuantity = _temps.Sum(p => p.RejectQuantity),
                            TotalOrderAmount = _temps.Sum(p => p.TotalOrderAmount),
                            TotalOrderPayment = _temps.Sum(p => p.TotalOrderPayment),
                            TimeZoon = _TimeZoon
                        });
                    }
                    _Data = _tempList;
                }

                //合并数据,如果没有则添加零数据行
                if (_TimeZoon == 1)
                {
                    for (DateTime t = _EdTimePage; t >= _StTimePage; t = t.AddMonths(-1))
                    {
                        if (_Data.Where(p => p.Date.ToString("yyyy-MM") == t.ToString("yyyy-MM")).FirstOrDefault() == null)
                        {
                            _Data.Add(new AnalysisDailyOrder()
                            {
                                Date = t,
                                OrderQuantity = 0,
                                Quantity = 0,
                                TotalOrderAmount = 0,
                                TotalOrderPayment = 0,
                                TimeZoon = _TimeZoon
                            });
                        }
                    }
                }
                else if (_TimeZoon == 2)
                {
                    for (DateTime t = _EdTimePage; t >= _StTimePage; t = t.AddYears(-1))
                    {
                        if (_Data.Where(p => p.Date.ToString("yyyy") == t.ToString("yyyy")).FirstOrDefault() == null)
                        {
                            _Data.Add(new AnalysisDailyOrder()
                            {
                                Date = t,
                                OrderQuantity = 0,
                                Quantity = 0,
                                TotalOrderAmount = 0,
                                TotalOrderPayment = 0,
                                TimeZoon = _TimeZoon
                            });
                        }
                    }
                }
                else
                {
                    for (DateTime t = _EdTimePage; t >= _StTimePage; t = t.AddDays(-1))
                    {
                        if (_Data.Where(p => p.Date.ToString("yyyy-MM-dd") == t.ToString("yyyy-MM-dd")).FirstOrDefault() == null)
                        {
                            _Data.Add(new AnalysisDailyOrder()
                            {
                                Date = t,
                                OrderQuantity = 0,
                                Quantity = 0,
                                TotalOrderAmount = 0,
                                TotalOrderPayment = 0,
                                TimeZoon = _TimeZoon
                            });
                        }
                    }
                }

                //返回数据
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
                            Legend = _LanguagePack["orderreport_index_sales_amount"],
                            Name = _LanguagePack["orderreport_index_sales_amount"],
                            Datas = _Data.Select(p => (object)p.TotalOrderPayment).ToList()
                        });
                    }
                    else
                    {
                        objData.Add(new EChartsHelper.Line.Config.Para()
                        {
                            Legend = _LanguagePack["orderreport_index_order_quantity"],
                            Name = _LanguagePack["orderreport_index_order_quantity"],
                            Datas = _Data.Select(p => (object)p.OrderQuantity).ToList()
                        });
                        objData.Add(new EChartsHelper.Line.Config.Para()
                        {
                            Legend = _LanguagePack["orderreport_index_item_quantity"],
                            Name = _LanguagePack["orderreport_index_item_quantity"],
                            Datas = _Data.Select(p => (object)p.Quantity).ToList()
                        });
                        objData.Add(new EChartsHelper.Line.Config.Para()
                        {
                            Legend = _LanguagePack["orderreport_index_cancel_quantity"],
                            Name = _LanguagePack["orderreport_index_cancel_quantity"],
                            Datas = _Data.Select(p => (object)p.CancelQuantity).ToList()
                        });
                        objData.Add(new EChartsHelper.Line.Config.Para()
                        {
                            Legend = _LanguagePack["orderreport_index_return_quantity"],
                            Name = _LanguagePack["orderreport_index_return_quantity"],
                            Datas = _Data.Select(p => (object)p.ReturnQuantity).ToList()
                        });
                        objData.Add(new EChartsHelper.Line.Config.Para()
                        {
                            Legend = _LanguagePack["orderreport_index_exchange_quantity"],
                            Name = _LanguagePack["orderreport_index_exchange_quantity"],
                            Datas = _Data.Select(p => (object)p.ReturnQuantity).ToList()
                        });
                        objData.Add(new EChartsHelper.Line.Config.Para()
                        {
                            Legend = _LanguagePack["orderreport_index_reject_quantity"],
                            Name = _LanguagePack["orderreport_index_reject_quantity"],
                            Datas = _Data.Select(p => (object)p.ReturnQuantity).ToList()
                        });
                    }

                    _result.Content = EChartsHelper.Line.Set(new EChartsHelper.Line.Config()
                    {
                        Title = "",
                        XAxis = _Data.Select(p => FormatTime(p.Date, p.TimeZoon)).ToList(),
                        YAxis = "",
                        Paras = objData
                    });
                }
                else
                {
                    //替换分页时间段为查询时间段
                    _SqlWhere[2].Param = _SqlWhere[2].Param.ToString().Replace(_StTimePage.ToString("yyyy-MM-dd"), _BeginTime.ToString("yyyy-MM-dd"));
                    _SqlWhere[3].Param = _SqlWhere[3].Param.ToString().Replace(_EdTimePage.ToString("yyyy-MM-dd"), _EndTime.ToString("yyyy-MM-dd"));
                    //查询总合计
                    List<AnalysisDailyOrder> _TotalData = db.Fetch<AnalysisDailyOrder>("select isnull(sum(OrderQuantity),0) as OrderQuantity,isnull(sum(Quantity),0) as Quantity,isnull(sum(TotalOrderAmount),0) as TotalOrderAmount,isnull(sum(TotalOrderPayment),0) as TotalOrderPayment,isnull(sum(CancelQuantity),0) as CancelQuantity,isnull(sum(ReturnQuantity),0) as ReturnQuantity,isnull(sum(ExchangeQuantity),0) as ExchangeQuantity,isnull(sum(RejectQuantity),0) as RejectQuantity from AnalysisDailyOrder", _SqlWhere);

                    //排序
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
                            else if (_sort_array[t] == "s4")
                            {
                                if (_order_array[t] == "desc")
                                {
                                    _Data = _Data.OrderByDescending(p => p.CancelQuantity).ToList();
                                }
                                else
                                {
                                    _Data = _Data.OrderBy(p => p.CancelQuantity).ToList();
                                }
                            }
                            else if (_sort_array[t] == "s5")
                            {
                                if (_order_array[t] == "desc")
                                {
                                    _Data = _Data.OrderByDescending(p => p.ReturnQuantity).ToList();
                                }
                                else
                                {
                                    _Data = _Data.OrderBy(p => p.ReturnQuantity).ToList();
                                }
                            }
                            else if (_sort_array[t] == "s6")
                            {
                                if (_order_array[t] == "desc")
                                {
                                    _Data = _Data.OrderByDescending(p => p.ExchangeQuantity).ToList();
                                }
                                else
                                {
                                    _Data = _Data.OrderBy(p => p.ExchangeQuantity).ToList();
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
                                _Data = _Data.OrderByDescending(p => p.Date).ToList();
                            }
                        }
                    }
                    else
                    {
                        _Data = _Data.OrderByDescending(p => p.Date).ToList();
                    }
                    //新建页面名称
                    string _tabName = $"{this.MenuName(this.CurrentFunctionID)}_Detail";
                    string _SearchTab = GetSearchOrderTab();
                    var r = new
                    {
                        total = _TotalTime,
                        rows = from dy in _Data
                               select new
                               {
                                   s1 = $"<a href=\"javascript:void(0);\" onclick=\"window.parent.frames.OpenTab('{_tabName}','{Url.Action("Detail", "OrderReport")}?mall={((_storeid != null) ? string.Join(",", _storeid) : "")}&tm={dy.Date.ToString("yyyy-MM-dd")}&ty={dy.TimeZoon}',true);\">{FormatTime(dy.Date, dy.TimeZoon)}</a>",
                                   s2 = dy.OrderQuantity,
                                   s3 = dy.Quantity,
                                   s4 = dy.CancelQuantity,
                                   s5 = dy.ReturnQuantity,
                                   s6 = dy.ExchangeQuantity,
                                   s7 = dy.RejectQuantity,
                                   s8 = VariableHelper.FormateMoney(dy.TotalOrderPayment),
                                   s9 = (dy.OrderQuantity > 0) ? VariableHelper.FormateMoney(OrderHelper.MathRound(dy.TotalOrderPayment / dy.OrderQuantity)) : "0",
                                   s10 = $"<a href=\"javascript:void(0);\" onclick=\"window.parent.frames.OpenTab('{_SearchTab}','{Url.Action("Index", "OrderQuery")}?store={string.Join(",", _SelectMalls)}&time={FormatTime(dy.Date, dy.TimeZoon)}',true);\">{_LanguagePack["common_view_order"]}</a>"
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
                                     s9 = (dy.OrderQuantity > 0) ? VariableHelper.FormateMoney(OrderHelper.MathRound(dy.TotalOrderPayment / dy.OrderQuantity)) : "0",
                                     s10 = ""
                                 }
                    };
                    _result.Content = JsonHelper.JsonSerialize(r);
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

            string _mallSapCode = VariableHelper.SaferequestStr(Request.QueryString["mall"]);
            ViewBag.Type = VariableHelper.SaferequestInt(Request.QueryString["ty"]);
            ViewBag.Time = VariableHelper.SaferequestStr(Request.QueryString["tm"]);
            //店铺信息
            ViewBag.MallSapCodes = _mallSapCode;

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public ContentResult Detail_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            ContentResult _result = new ContentResult();

            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            string _store = VariableHelper.SaferequestStr(Request.Form["store"]);
            string _time = VariableHelper.SaferequestStr(Request.Form["time"]);
            int _type = VariableHelper.SaferequestInt(Request.Form["type"]);
            string _showType = VariableHelper.SaferequestStr(Request.Form["show_type"]);
            int _chartType = VariableHelper.SaferequestInt(Request.Form["chart_type"]);
            List<Mall> objMall_List = MallService.GetMallOption();
            string[] _store_array = _store.Split(',');
            DateTime _currentTime = VariableHelper.SaferequestTime(_time);
            //报表类型
            int _TimeZoon = 0;
            if (_type == 1)
            {
                _TimeZoon = 1;
            }
            else if (_type == 2)
            {
                _TimeZoon = 2;
            }
            else
            {
                _TimeZoon = 0;
            }
            //如果为空，则默认表示全部店铺
            if (!string.IsNullOrEmpty(_store))
            {
                foreach (Mall _m in objMall_List)
                {
                    if (!_store_array.Contains(_m.SapCode, StringComparison.CurrentCulture))
                    {
                        _m.IsUsed = false;
                    }
                }
                //剔除没有选中的店铺
                objMall_List.RemoveAll(p => !p.IsUsed);
            }

            //搜索条件
            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "TimeZoon={0}", Param = _TimeZoon });
            if (_TimeZoon == 1)
            {
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(MONTH,[Date],{0})=0", Param = _currentTime.ToString("yyyy-01-01") });
            }
            else if (_TimeZoon == 2)
            {
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(YEAR,[Date],{0})=0", Param = _currentTime.ToString("yyyy-MM-01") });
            }
            else
            {
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(DAY,[Date],{0})=0", Param = _currentTime.ToString("yyyy-MM-dd") });
            }

            if (objMall_List.Count == 0)
            {
                //限制查询出数据
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "MallSapCode in (0)", Param = "" });
            }
            else
            {
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = $"MallSapCode in ({string.Join(",", objMall_List.Select(p => p.SapCode))})", Param = "" });
            }

            using (var db = new DynamicRepository())
            {
                List<AnalysisDailyOrder> _Data = new List<AnalysisDailyOrder>();
                //查询
                _Data = db.Fetch<AnalysisDailyOrder>("select * from AnalysisDailyOrder", _SqlWhere);
                //合并数据,如果没有则添加零数据行
                foreach (var _m in objMall_List)
                {
                    if (_Data.Where(p => p.MallSapCode == _m.SapCode).FirstOrDefault() == null)
                    {
                        _Data.Add(new AnalysisDailyOrder()
                        {
                            Date = _currentTime,
                            MallSapCode = _m.SapCode,
                            OrderQuantity = 0,
                            Quantity = 0,
                            TotalOrderAmount = 0,
                            TotalOrderPayment = 0,
                            TimeZoon = _TimeZoon
                        });
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
                                Name = MallService.GetMallName(objMall_List, dy.MallSapCode),
                                Value = dy.Quantity.ToString()
                            });
                        }
                        else if (_chartType == 3)
                        {
                            _pies.Add(new EChartsHelper.Pie.Config.Para.Data()
                            {
                                Name = MallService.GetMallName(objMall_List, dy.MallSapCode),
                                Value = dy.CancelQuantity.ToString()
                            });
                        }
                        else if (_chartType == 4)
                        {
                            _pies.Add(new EChartsHelper.Pie.Config.Para.Data()
                            {
                                Name = MallService.GetMallName(objMall_List, dy.MallSapCode),
                                Value = dy.ReturnQuantity.ToString()
                            });
                        }
                        else if (_chartType == 5)
                        {
                            _pies.Add(new EChartsHelper.Pie.Config.Para.Data()
                            {
                                Name = MallService.GetMallName(objMall_List, dy.MallSapCode),
                                Value = dy.ExchangeQuantity.ToString()
                            });
                        }
                        else if (_chartType == 6)
                        {
                            _pies.Add(new EChartsHelper.Pie.Config.Para.Data()
                            {
                                Name = MallService.GetMallName(objMall_List, dy.MallSapCode),
                                Value = dy.RejectQuantity.ToString()
                            });
                        }
                        else if (_chartType == 7)
                        {
                            _pies.Add(new EChartsHelper.Pie.Config.Para.Data()
                            {
                                Name = MallService.GetMallName(objMall_List, dy.MallSapCode),
                                Value = dy.TotalOrderPayment.ToString()
                            });
                        }
                        else
                        {
                            _pies.Add(new EChartsHelper.Pie.Config.Para.Data()
                            {
                                Name = MallService.GetMallName(objMall_List, dy.MallSapCode),
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
                    //合计
                    List<AnalysisDailyOrder> _TotalData = db.Fetch<AnalysisDailyOrder>("select '{_store}' As MallSapCode,isnull(sum(OrderQuantity),0) as OrderQuantity,isnull(sum(Quantity),0) as Quantity,isnull(sum(TotalOrderAmount),0) as TotalOrderAmount,isnull(sum(TotalOrderPayment),0) as TotalOrderPayment,isnull(sum(CancelQuantity),0) as CancelQuantity,isnull(sum(ReturnQuantity),0) as ReturnQuantity,isnull(sum(ExchangeQuantity),0) as ExchangeQuantity from AnalysisDailyOrder", _SqlWhere);
                    //新建页面名称
                    string _SearchTab = GetSearchOrderTab();
                    //查询
                    var r = new
                    {
                        rows = from dy in _Data
                               select new
                               {
                                   s1 = MallService.GetMallName(objMall_List, dy.MallSapCode),
                                   s2 = dy.OrderQuantity,
                                   s3 = dy.Quantity,
                                   s4 = dy.CancelQuantity,
                                   s5 = dy.ReturnQuantity,
                                   s6 = dy.ExchangeQuantity,
                                   s7 = dy.RejectQuantity,
                                   s8 = VariableHelper.FormateMoney(dy.TotalOrderPayment),
                                   s9 = (dy.OrderQuantity > 0) ? VariableHelper.FormateMoney(OrderHelper.MathRound(dy.TotalOrderPayment / dy.OrderQuantity)) : "0",
                                   s10 = $"<a href=\"javascript:void(0);\" onclick=\"window.parent.frames.OpenTab('{_SearchTab}','{Url.Action("Index", "OrderQuery")}?store={dy.MallSapCode}&time={FormatTime(dy.Date, dy.TimeZoon)}',true);\">{_LanguagePack["common_view_order"]}</a>"
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
                                     s9 = (dy.OrderQuantity > 0) ? VariableHelper.FormateMoney(OrderHelper.MathRound(dy.TotalOrderPayment / dy.OrderQuantity)) : "0",
                                     s10 = ""
                                 }
                    };
                    _result.Content = JsonHelper.JsonSerialize(r);
                }
                return _result;
            }
        }
        #endregion

        /// <summary>
        /// 格式化显示时间
        /// </summary>
        /// <param name="objTime"></param>
        /// <param name="objType"></param>
        /// <returns></returns>
        private string FormatTime(DateTime objTime, int objType)
        {
            string _result = string.Empty;
            if (objType == 1)
            {
                _result = objTime.ToString("yyyy-MM");
            }
            else if (objType == 2)
            {
                _result = objTime.ToString("yyyy");
            }
            else
            {
                _result = objTime.ToString("yyyy-MM-dd");
            }
            return _result;
        }
    }
}
