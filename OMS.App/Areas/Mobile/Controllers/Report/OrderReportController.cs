using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.Database;
using Samsonite.OMS.Service;
using Samsonite.Utility.Common;

using OMS.App.Helper;

namespace OMS.App.Areas.Mobile.Controllers
{
    public class OrderReportController : BaseController
    {
        // GET: Mobile/OrderReport

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
            string _store = VariableHelper.SaferequestStr(Request.Form["store"]);
            string _time1 = VariableHelper.SaferequestStr(Request.Form["time1"]);
            string _time2 = VariableHelper.SaferequestStr(Request.Form["time2"]);
            string _showType = VariableHelper.SaferequestStr(Request.Form["show_type"]);
            int _chartType = VariableHelper.SaferequestInt(Request.Form["chart_type"]);
            int _TimeZoon = 0;
            //计算时间段,默认近七天
            DateTime _BeginTime = (string.IsNullOrEmpty(_time1)) ? DateTime.Today.AddDays(-6) : VariableHelper.SaferequestTime(_time1);
            DateTime _EndTime = (string.IsNullOrEmpty(_time2)) ? DateTime.Today : VariableHelper.SaferequestTime(_time2);
            //店铺列表
            List<Mall> objMall_List = MallService.GetMallOption();
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
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "MallSapCode in (" + string.Join(",", _SelectMalls) + ")", Param = null });
                //默认日报表
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "TimeZoon={0}", Param = _TimeZoon });
                //时间段
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(DAY,[Date],{0})<=0", Param = _BeginTime.ToString("yyyy-MM-dd") });
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(DAY,[Date],{0})>=0", Param = _EndTime.ToString("yyyy-MM-dd") });

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
                            TotalOrderAmount = _temps.Sum(p => p.TotalOrderAmount),
                            TotalOrderPayment = _temps.Sum(p => p.TotalOrderPayment),
                            TimeZoon = _TimeZoon
                        });
                    }
                    _Data = _tempList;
                }
                //合并数据,如果没有则添加零数据行
                for (DateTime t = _EndTime; t >= _BeginTime; t = t.AddDays(-1))
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

                //返回数据
                if (_showType.ToLower() == "chart")
                {
                    //排序
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
                    }

                    _result.Content = EChartsHelper.Line.Set(new EChartsHelper.Line.Config()
                    {
                        Title = "",
                        XAxis = _Data.Select(p => FormatTime(p.Date, p.TimeZoon)).ToList(),
                        YAxis = "",
                        Paras = objData,
                        IsMagicType = false,
                        IsRestore = false,
                        IsSaveAsImage = false
                    });
                }
                else
                {
                    //排序
                    _Data = _Data.OrderByDescending(p => p.Date).ToList();
                    //合计(ID=-1表示合计数据)
                    _Data.Add(new AnalysisDailyOrder()
                    {
                        ID = -1,
                        OrderQuantity = _Data.Sum(p => p.OrderQuantity),
                        Quantity = _Data.Sum(p => p.Quantity),
                        CancelQuantity = _Data.Sum(p => p.CancelQuantity),
                        ReturnQuantity = _Data.Sum(p => p.ReturnQuantity),
                        ExchangeQuantity = _Data.Sum(p => p.ExchangeQuantity),
                        TotalOrderAmount = _Data.Sum(p => p.TotalOrderAmount),
                        TotalOrderPayment = _Data.Sum(p => p.TotalOrderPayment)
                    });

                    var r = new
                    {
                        rows = from dy in _Data
                               select new
                               {
                                   s0 = (dy.ID == -1),
                                   s1 = (dy.ID == -1) ? "合计" : dy.Date.ToString("yyyy-MM-dd"),
                                   s2 = dy.OrderQuantity,
                                   s3 = dy.Quantity,
                                   s4 = dy.CancelQuantity,
                                   s5 = dy.ReturnQuantity,
                                   s6 = dy.ExchangeQuantity,
                                   s7 = VariableHelper.FormateMoney(Math.Round(dy.TotalOrderPayment, 0)),
                                   s8 = (dy.OrderQuantity > 0) ? VariableHelper.FormateMoney(OrderHelper.MathRound(Math.Round(dy.TotalOrderPayment / dy.OrderQuantity, 0))) : "0"
                               }
                    };
                    _result.Content = JsonHelper.JsonSerialize(r);
                }
            }

            return _result;
        }

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