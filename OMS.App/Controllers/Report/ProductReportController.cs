using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.Utility.Common;
using Samsonite.OMS.Service;
using OMS.App.Helper;

namespace OMS.App.Controllers
{
    public class ProductReportController : BaseController
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
            //品牌列表
            ViewData["brand_list"] = BrandService.GetBrands();

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
            int _brand = VariableHelper.SaferequestInt(Request.Form["brand"]);
            string _sku = VariableHelper.SaferequestStr(Request.Form["sku"]);
            string _time = VariableHelper.SaferequestStr(Request.Form["time"]);
            if (string.IsNullOrEmpty(_time)) _time = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            //昨天日期
            string _time_lastday = VariableHelper.SaferequestTime(_time).AddDays(-1).ToString("yyyy-MM-dd");
            string _showType = VariableHelper.SaferequestStr(Request.Form["show_type"]);
            int _chartType = VariableHelper.SaferequestInt(Request.Form["chart_type"]);
            int _pageSize = VariableHelper.SaferequestInt(Request.Form["rows"]);
            int _page = VariableHelper.SaferequestInt(Request.Form["page"]);
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

                if (_brand > 0)
                {
                    string _Brands = string.Join(",", BrandService.GetSons(_brand));
                    if (!string.IsNullOrEmpty(_Brands))
                    {
                        _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "charindex(p.Name, {0}) > 0", Param = _Brands });
                    }
                }

                if (!string.IsNullOrEmpty(_sku))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "ap.Sku like {0}", Param = '%' + _sku + '%' });
                }

                //默认查询昨天的产品信息(需要读取当天和昨天的记录)
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,ap.[Date],{0})>=0", Param = VariableHelper.SaferequestTime(_time) });
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,ap.[Date],{0})<=0", Param = VariableHelper.SaferequestTime(_time_lastday) });

                //查询
                var _TotalData = db.Fetch<ProductAnalysisView>("select isnull(p.[Description],'')As ProductName,isnull(p.Name,'')As Brand,isnull(p.GroupDesc,'') as GroupDesc,isnull(p.MarketPrice,0) as MarketPrice,ap.Sku,ap.OrderQuantity,ap.Quantity,ap.CancelQuantity,ap.ReturnQuantity,ap.ExchangeQuantity,ap.RejectQuantity,ap.TotalOrderAmount,ap.TotalOrderPayment,ap.[Date],ap.MallSapCode from AnalysisDailyProduct as ap left join Product as p on ap.Sku=p.Sku", _SqlWhere);
                //昨天数据
                var _Data_LastDay = _TotalData.Where(p => p.Date.ToString("yyyy-MM-dd") == _time_lastday).ToList();
                //当天数据
                var _Data = _TotalData.Where(p => p.Date.ToString("yyyy-MM-dd") == _time).ToList();
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
                            Date = VariableHelper.SaferequestTime(_time),
                            ProductName = _temp_single.ProductName,
                            Brand = _temp_single.Brand,
                            GroupDesc = _temp_single.GroupDesc,
                            Sku = _temp_single.Sku,
                            MarketPrice = _temp_single.MarketPrice,
                            OrderQuantity = _temps.Sum(p => p.OrderQuantity),
                            Quantity = _temps.Sum(p => p.Quantity),
                            CancelQuantity = _temps.Sum(p => p.CancelQuantity),
                            ReturnQuantity = _temps.Sum(p => p.ReturnQuantity),
                            ExchangeQuantity = _temps.Sum(p => p.ExchangeQuantity),
                            RejectQuantity = _temps.Sum(p => p.RejectQuantity),
                            TotalOrderAmount = _temps.Sum(p => p.TotalOrderAmount),
                            TotalOrderPayment = _temps.Sum(p => p.TotalOrderPayment)
                        });
                    }
                    _Data = _tempList;
                }
                //记录总数
                int _TotalCount = _Data.Count;
                //排序条件
                if (!string.IsNullOrEmpty(_sort))
                {
                    string[] _sort_array = _sort.Split(',');
                    string[] _order_array = _order.Split(',');
                    for (int t = 0; t < _sort_array.Length; t++)
                    {
                        if (_sort_array[t] == "s5")
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
                        else if (_sort_array[t] == "s6")
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
                        else if (_sort_array[t] == "s10")
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
                            _Data = _Data.OrderByDescending(p => p.Quantity).ToList();
                        }
                    }
                }
                else
                {
                    _Data = _Data.OrderByDescending(p => p.Quantity).ToList();
                }
                //翻页
                _Data = _Data.Skip((_page - 1) * _pageSize).Take(_pageSize).ToList();

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
                            Legend = _LanguagePack["productreport_index_reject_quantity"],
                            Name = _LanguagePack["productreport_index_reject_quantity"],
                            Datas = _Data.Select(p => (object)p.RejectQuantity).ToList()
                        });
                    }
                    else if (_chartType == 7)
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
                        total = _TotalCount,
                        rows = from dy in _Data
                               select new
                               {
                                   s1 = $"<a href=\"javascript:void(0);\" onclick=\"window.parent.frames.OpenTab('{_tabName}','{Url.Action("Detail", "ProductReport")}?sku={dy.Sku}&mall={((_storeid != null) ? string.Join(",", _storeid) : "")}',true);\">{dy.Sku}</a>",
                                   s2 = dy.ProductName,
                                   s3 = dy.Brand,
                                   s4 = dy.GroupDesc,
                                   s5 = VariableHelper.FormateMoney(dy.MarketPrice),
                                   s6 = FormateText(dy.OrderQuantity, _Data_LastDay.Where(p => p.Sku == dy.Sku).ToList().Sum(o => (decimal)o.OrderQuantity)),
                                   s7 = FormateText(dy.Quantity, _Data_LastDay.Where(p => p.Sku == dy.Sku).ToList().Sum(o => (decimal)o.Quantity)),
                                   s8 = dy.CancelQuantity,
                                   s9 = dy.ReturnQuantity,
                                   s10 = dy.ExchangeQuantity,
                                   s11 = dy.RejectQuantity,
                                   s12 = FormateText(dy.TotalOrderPayment, _Data_LastDay.Where(p => p.Sku == dy.Sku).ToList().Sum(o => (decimal)o.TotalOrderPayment)),
                                   s13 = (dy.Quantity > 0) ? VariableHelper.FormateMoney(OrderHelper.MathRound(dy.TotalOrderPayment / dy.Quantity)) : "0",
                                   s14 = $"<a href=\"javascript:void(0);\" onclick=\"window.parent.frames.OpenTab('{_SearchTab}','{Url.Action("Index", "OrderQuery")}?store={dy.MallSapCode}&brand={dy.Brand}&sku={dy.Sku}&time={_time}',true);\">{_LanguagePack["common_view_order"]}</a>"
                               }
                    };
                    _result.Content = JsonHelper.JsonSerialize(_r);
                }
                return _result;
            }
        }

        private string FormateText(decimal objNew, decimal _objOld)
        {
            string _result = string.Empty;
            decimal _dValue = objNew - _objOld;
            if (_dValue > 0)
            {
                _result = $"{ VariableHelper.FormateMoney(objNew)}<label class=\"color_danger\">(<i class=\"fontSize-10 fa fa-arrow-up\"></i>{VariableHelper.FormateMoney(Math.Abs(_dValue))})</label>";
            }
            else if (_dValue < 0)
            {
                _result = $"{ VariableHelper.FormateMoney(objNew)}<label class=\"color_success\">(<i class=\"fontSize-10 fa fa-arrow-down\"></i>{VariableHelper.FormateMoney(Math.Abs(_dValue))})</label>";
            }
            else
            {
                _result = VariableHelper.FormateMoney(objNew);
            }
            return _result;
        }
        #endregion

        #region 导出Excel
        [UserPowerAuthorize]
        public FileResult ExportExcel()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            string[] _storeid = VariableHelper.SaferequestStringArray(Request.Form["StoreName"]);
            int _brand = VariableHelper.SaferequestInt(Request.Form["BrandName"]);
            string _sku = VariableHelper.SaferequestStr(Request.Form["Sku"]);
            string _time = VariableHelper.SaferequestStr(Request.Form["Time"]);
            if (string.IsNullOrEmpty(_time)) _time = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");

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

                if (_brand > 0)
                {
                    string _Brands = string.Join(",", BrandService.GetSons(_brand));
                    if (!string.IsNullOrEmpty(_Brands))
                    {
                        _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "charindex(p.Name, {0}) > 0", Param = _Brands });
                    }
                }

                if (!string.IsNullOrEmpty(_sku))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "ap.Sku like {0}", Param = '%' + _sku + '%' });
                }

                //默认查询当天的产品信息(需要读取当天和昨天的记录)
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,ap.[Date],{0})=0", Param = VariableHelper.SaferequestTime(_time) });

                DataRow _dr = null;
                //读取数据
                var _Data = db.Fetch<ProductAnalysisView>("select isnull(p.[Description],'')As ProductName,isnull(p.Name,'')As Brand,isnull(p.GroupDesc,'') as GroupDesc,isnull(p.MarketPrice,0) as MarketPrice,ap.Sku,ap.OrderQuantity,ap.Quantity,ap.CancelQuantity,ap.ReturnQuantity,ap.ExchangeQuantity,ap.RejectQuantity,ap.TotalOrderAmount,ap.TotalOrderPayment,ap.[Date],ap.MallSapCode from AnalysisDailyProduct as ap left join Product as p on ap.Sku=p.Sku", _SqlWhere);
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
                            Date = VariableHelper.SaferequestTime(_time),
                            ProductName = _temp_single.ProductName,
                            Brand = _temp_single.Brand,
                            GroupDesc = _temp_single.GroupDesc,
                            Sku = _temp_single.Sku,
                            MarketPrice = _temp_single.MarketPrice,
                            OrderQuantity = _temps.Sum(p => p.OrderQuantity),
                            Quantity = _temps.Sum(p => p.Quantity),
                            CancelQuantity = _temps.Sum(p => p.CancelQuantity),
                            ReturnQuantity = _temps.Sum(p => p.ReturnQuantity),
                            ExchangeQuantity = _temps.Sum(p => p.ExchangeQuantity),
                            RejectQuantity = _temps.Sum(p => p.RejectQuantity),
                            TotalOrderAmount = _temps.Sum(p => p.TotalOrderAmount),
                            TotalOrderPayment = _temps.Sum(p => p.TotalOrderPayment)
                        });
                    }
                    _Data = _tempList;
                }
                //默认排序条件(form无法取到DataGrid排序字段)
                _Data = _Data.OrderByDescending(p => p.Quantity).ToList();

                DataTable dt = new DataTable();
                dt.Columns.Add(_LanguagePack["productreport_index_sku"]);
                dt.Columns.Add(_LanguagePack["productreport_index_product"]);
                dt.Columns.Add(_LanguagePack["productreport_index_brand"]);
                dt.Columns.Add(_LanguagePack["productreport_index_groupdesc"]);
                dt.Columns.Add(_LanguagePack["productreport_index_price"]);
                dt.Columns.Add(_LanguagePack["productreport_index_order_quantity"]);
                dt.Columns.Add(_LanguagePack["productreport_index_item_quantity"]);
                dt.Columns.Add(_LanguagePack["productreport_index_cancel_quantity"]);
                dt.Columns.Add(_LanguagePack["productreport_index_return_quantity"]);
                dt.Columns.Add(_LanguagePack["productreport_index_exchange_quantity"]);
                dt.Columns.Add(_LanguagePack["productreport_index_reject_quantity"]);
                dt.Columns.Add(_LanguagePack["productreport_index_sales_amount"]);
                dt.Columns.Add(_LanguagePack["productreport_index_per_value"]);

                foreach (var _dy in _Data)
                {
                    _dr = dt.NewRow();
                    _dr[0] = _dy.Sku;
                    _dr[1] = _dy.ProductName;
                    _dr[2] = _dy.Brand;
                    _dr[3] = _dy.GroupDesc;
                    _dr[4] = VariableHelper.FormateMoney(_dy.MarketPrice);
                    _dr[5] = _dy.OrderQuantity;
                    _dr[6] = _dy.Quantity;
                    _dr[7] = _dy.CancelQuantity;
                    _dr[8] = _dy.ReturnQuantity;
                    _dr[9] = _dy.ExchangeQuantity;
                    _dr[10] = _dy.RejectQuantity;
                    _dr[11] = VariableHelper.FormateMoney(_dy.TotalOrderPayment);
                    _dr[12] = (_dy.Quantity > 0) ? VariableHelper.FormateMoney(OrderHelper.MathRound(_dy.TotalOrderPayment / _dy.Quantity)) : "0";
                    dt.Rows.Add(_dr);
                }

                string _filepath = $"~{AppGlobalService.UPLOAD_CACHE_PATH}/{DateTime.Now.ToString("yyyy-MM-dd")}";
                if (!Directory.Exists(Server.MapPath(_filepath)))
                {
                    Directory.CreateDirectory(Server.MapPath(_filepath));
                }
                string _fileName = string.Format("ProductReport_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss"));
                _fileName = HttpUtility.UrlEncode(_fileName, Encoding.GetEncoding("UTF-8"));
                string _path = Server.MapPath(string.Format("{0}/{1}", _filepath, _fileName));
                ExcelHelper objExcelHelper = new ExcelHelper(_path);
                objExcelHelper.DataTableToExcel(dt, "Sheet1", true);
                return File(_path, "application/ms-excel", _fileName);
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

            string _sku = VariableHelper.SaferequestStr(Request.QueryString["sku"]);
            string _mallSapCode = VariableHelper.SaferequestStr(Request.QueryString["mall"]);

            using (var db = new ebEntities())
            {
                Product objProduct = db.Product.Where(p => p.SKU == _sku).FirstOrDefault();
                if (objProduct != null)
                {
                    //Sku信息
                    ViewBag.Sku = objProduct.SKU;
                }
                else
                {
                    //Sku信息
                    ViewBag.Sku = _sku;
                }
                //快速时间选项
                ViewData["quicktime_list"] = QuickTimeHelper.QuickTimeOption();
                //店铺信息
                ViewBag.MallSapCodes = _mallSapCode;

                return View(objProduct);
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
            string _sku = VariableHelper.SaferequestStr(Request.Form["sku"]);
            string _time1 = VariableHelper.SaferequestStr(Request.Form["time1"]);
            string _time2 = VariableHelper.SaferequestStr(Request.Form["time2"]);
            string _showType = VariableHelper.SaferequestStr(Request.Form["show_type"]);
            int _chartType = VariableHelper.SaferequestInt(Request.Form["chart_type"]);
            int _pageSize = VariableHelper.SaferequestInt(Request.Form["rows"]);
            int _page = VariableHelper.SaferequestInt(Request.Form["page"]);

            using (var db = new DynamicRepository())
            {
                List<AnalysisDailyProduct> _Data = new List<AnalysisDailyProduct>();
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

                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "Sku={0}", Param = _sku });

                //翻页计算时间
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(DAY,[Date],{0})<=0", Param = _StTimePage.ToString("yyyy-MM-dd") });
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(DAY,[Date],{0})>=0", Param = _EdTimePage.ToString("yyyy-MM-dd") });

                //查询
                _Data = db.Fetch<AnalysisDailyProduct>("select * from AnalysisDailyProduct order by Date desc", _SqlWhere);
                //如果是多选店铺,合并各店铺合计
                if (_SelectMalls.Count > 1)
                {
                    List<AnalysisDailyProduct> _tempList = new List<AnalysisDailyProduct>();
                    List<DateTime> _times = _Data.GroupBy(p => p.Date).Select(o => o.Key).ToList();
                    foreach (var _o in _times)
                    {
                        var _temps = _Data.Where(p => p.Date == _o).ToList();
                        _tempList.Add(new AnalysisDailyProduct()
                        {
                            Date = _o,
                            Sku = _sku,
                            OrderQuantity = _temps.Sum(p => p.OrderQuantity),
                            Quantity = _temps.Sum(p => p.Quantity),
                            CancelQuantity = 0,
                            ReturnQuantity = 0,
                            ExchangeQuantity = 0,
                            RejectQuantity = 0,
                            TotalOrderAmount = _temps.Sum(p => p.TotalOrderAmount),
                            TotalOrderPayment = _temps.Sum(p => p.TotalOrderPayment)
                        });
                    }
                    _Data = _tempList;
                }
                //合并数据,如果没有则添加零数据行
                for (DateTime t = _EdTimePage; t >= _StTimePage; t = t.AddDays(-1))
                {
                    if (_Data.Where(p => p.Date.ToString("yyyy-MM-dd") == t.ToString("yyyy-MM-dd")).FirstOrDefault() == null)
                    {
                        _Data.Add(new AnalysisDailyProduct()
                        {
                            Date = t,
                            Sku = _sku,
                            OrderQuantity = 0,
                            Quantity = 0,
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
                            Legend = _LanguagePack["productreport_detail_sales_amount"],
                            Name = _LanguagePack["productreport_detail_sales_amount"],
                            Datas = _Data.Select(p => (object)p.TotalOrderPayment).ToList()
                        });
                    }
                    else
                    {
                        objData.Add(new EChartsHelper.Line.Config.Para()
                        {
                            Legend = _LanguagePack["productreport_detail_order_quantity"],
                            Name = _LanguagePack["productreport_detail_order_quantity"],
                            Datas = _Data.Select(p => (object)p.OrderQuantity).ToList()
                        });
                        objData.Add(new EChartsHelper.Line.Config.Para()
                        {
                            Legend = _LanguagePack["productreport_detail_item_quantity"],
                            Name = _LanguagePack["productreport_detail_item_quantity"],
                            Datas = _Data.Select(p => (object)p.Quantity).ToList()
                        });
                        objData.Add(new EChartsHelper.Line.Config.Para()
                        {
                            Legend = _LanguagePack["productreport_detail_cancel_quantity"],
                            Name = _LanguagePack["productreport_detail_cancel_quantity"],
                            Datas = _Data.Select(p => (object)p.CancelQuantity).ToList()
                        });
                        objData.Add(new EChartsHelper.Line.Config.Para()
                        {
                            Legend = _LanguagePack["productreport_detail_return_quantity"],
                            Name = _LanguagePack["productreport_detail_return_quantity"],
                            Datas = _Data.Select(p => (object)p.ReturnQuantity).ToList()
                        });
                        objData.Add(new EChartsHelper.Line.Config.Para()
                        {
                            Legend = _LanguagePack["productreport_detail_exchange_quantity"],
                            Name = _LanguagePack["productreport_detail_exchange_quantity"],
                            Datas = _Data.Select(p => (object)p.ExchangeQuantity).ToList()
                        });
                        objData.Add(new EChartsHelper.Line.Config.Para()
                        {
                            Legend = _LanguagePack["productreport_detail_reject_quantity"],
                            Name = _LanguagePack["productreport_detail_reject_quantity"],
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
                    List<AnalysisDailyProduct> _TotalData = db.Fetch<AnalysisDailyProduct>("select isnull(sum(OrderQuantity),0) as OrderQuantity,isnull(sum(Quantity),0) as Quantity,isnull(sum(CancelQuantity),0) as CancelQuantity,isnull(sum(ReturnQuantity),0) as ReturnQuantity,isnull(sum(ExchangeQuantity),0) as ExchangeQuantity,isnull(sum(RejectQuantity),0) as RejectQuantity,isnull(sum(TotalOrderAmount),0) as TotalOrderAmount,isnull(sum(TotalOrderPayment),0) as TotalOrderPayment from AnalysisDailyProduct", _SqlWhere);

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
                                   s10 = $"<a href=\"javascript:void(0);\" onclick=\"window.parent.frames.OpenTab('{_SearchTab}','{Url.Action("Index", "OrderQuery")}?store={dy.MallSapCode}&sku={dy.Sku}&time={dy.Date.ToString("yyyy-MM-dd")}',true);\">{_LanguagePack["common_view_order"]}</a>"
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
                return _result;
            }
        }
        #endregion
    }
}
