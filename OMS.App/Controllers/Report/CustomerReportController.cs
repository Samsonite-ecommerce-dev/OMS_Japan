using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Encryption;
using Samsonite.Utility.Common;
using OMS.App.Helper;

namespace OMS.App.Controllers
{
    public class CustomerReportController : BaseController
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
            string _SqlOrderWhere = string.Empty;
            List<string> _SqlOrder = new List<string>();
            string _customer = VariableHelper.SaferequestStr(Request.Form["customer"]);
            string _time1 = VariableHelper.SaferequestStr(Request.Form["time1"]);
            string _time2 = VariableHelper.SaferequestStr(Request.Form["time2"]);
            string _showType = VariableHelper.SaferequestStr(Request.Form["show_type"]);
            int _chartType = VariableHelper.SaferequestInt(Request.Form["chart_type"]);
            int _pageSize = VariableHelper.SaferequestInt(Request.Form["rows"]);
            int _page = VariableHelper.SaferequestInt(Request.Form["page"]);
            //排序
            string _sort = VariableHelper.SaferequestStr(Request.Form["sort"]);
            string _order = VariableHelper.SaferequestStr(Request.Form["order"]);

            using (var db = new DynamicRepository())
            {
                //搜索条件
                if (!string.IsNullOrEmpty(_customer))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "Name={0}", Param = EncryptionBase.EncryptString(_customer) });
                }

                if (!string.IsNullOrEmpty(_time1))
                {
                    _SqlOrderWhere += " and datediff(DAY,[order].CreateDate,'" + VariableHelper.SaferequestTime(_time1).ToString("yyyy-MM-dd") + "')<=0";
                }

                if (!string.IsNullOrEmpty(_time2))
                {
                    _SqlOrderWhere += " and datediff(DAY,[order].CreateDate,'" + VariableHelper.SaferequestTime(_time1).ToString("yyyy-MM-dd") + "')>=0";
                }

                //排序条件
                if (!string.IsNullOrEmpty(_sort))
                {
                    string[] _sort_array = _sort.Split(',');
                    string[] _order_array = _order.Split(',');
                    for (int t = 0; t < _sort_array.Length; t++)
                    {
                        if (_sort_array[t] == "s3")
                        {
                            _SqlOrder.Add(string.Format("{0} {1}", "IsNull((select count(*) from [order] where CustomerNo=Customer.CustomerNo " + _SqlOrderWhere + "),0)", (_order_array[t] == "desc") ? "desc" : "asc"));
                        }
                        else if (_sort_array[t] == "s4")
                        {
                            _SqlOrder.Add(string.Format("{0} {1}", "IsNull((select sum(Quantity) from OrderDetail inner join [order] on OrderDetail.OrderNo=[order].OrderNo where CustomerNo=Customer.CustomerNo and IsExchangeNew=0 " + _SqlOrderWhere + "),0)", (_order_array[t] == "desc") ? "desc" : "asc"));
                        }
                        else if (_sort_array[t] == "s5")
                        {
                            _SqlOrder.Add(string.Format("{0} {1}", "IsNull((select sum(Quantity) from OrderCancel inner join [order] on OrderCancel.OrderNo=[order].OrderNo where CustomerNo=Customer.CustomerNo and OrderCancel.[Status]!=" + (int)ProcessStatus.Delete + " " + _SqlOrderWhere + "),0)", (_order_array[t] == "desc") ? "desc" : "asc"));
                        }
                        else if (_sort_array[t] == "s6")
                        {
                            _SqlOrder.Add(string.Format("{0} {1}", "IsNull((select sum(Quantity) from OrderReturn inner join [order] on OrderReturn.OrderNo=[order].OrderNo where CustomerNo=Customer.CustomerNo and OrderReturn.[Status]!=" + (int)ProcessStatus.Delete + " " + _SqlOrderWhere + "),0)", (_order_array[t] == "desc") ? "desc" : "asc"));
                        }
                        else if (_sort_array[t] == "s7")
                        {
                            _SqlOrder.Add(string.Format("{0} {1}", "IsNull((select sum(Quantity) from OrderExchange inner join [order] on OrderExchange.OrderNo=[order].OrderNo where CustomerNo=Customer.CustomerNo and OrderExchange.[Status]!=" + (int)ProcessStatus.Delete + " " + _SqlOrderWhere + "),0)", (_order_array[t] == "desc") ? "desc" : "asc"));
                        }
                        else if (_sort_array[t] == "s8")
                        {
                            _SqlOrder.Add(string.Format("{0} {1}", "IsNull((select sum(Quantity) from OrderReject inner join [order] on OrderReject.OrderNo=[order].OrderNo where CustomerNo=Customer.CustomerNo and OrderReject.[Status]!=" + (int)ProcessStatus.Delete + " " + _SqlOrderWhere + "),0)", (_order_array[t] == "desc") ? "desc" : "asc"));
                        }
                        else if (_sort_array[t] == "s9")
                        {
                            _SqlOrder.Add(string.Format("{0} {1}", "IsNull((select sum(PaymentAmount-DeliveryFee) from [order] where CustomerNo=Customer.CustomerNo " + _SqlOrderWhere + "),0)", (_order_array[t] == "desc") ? "desc" : "asc"));
                        }
                        else
                        {
                            _SqlOrder.Add("CustomerNo desc");
                        }
                    }
                }
                else
                {
                    _SqlOrder.Add("CustomerNo desc");
                }

                //查询
                var _list = db.GetPage<Customer>($"select CustomerNo,Name,Tel,Mobile,Email,Addr,Zipcode,City from Customer order by {string.Join(", ", _SqlOrder)}", _SqlWhere, VariableHelper.SaferequestInt(Request.Form["rows"]), VariableHelper.SaferequestInt(Request.Form["page"]));
                //数据解密
                foreach (var item in _list.Items)
                {
                    EncryptionFactory.Create(item).Decrypt();
                }

                List<string> _Customers = _list.Items.Select(p => (string)p.CustomerNo).ToList();
                List<CustomerReport> _Data = new List<CustomerReport>();
                if (_Customers.Count > 0)
                {
                    _Data = db.ExecStoredProcedure<CustomerReport>("Proc_Report_Customer @0,@1,@2", string.Join(",", _Customers), _time1, _time2);
                }
                //重新按照规则排序(这边用in查询客户信息会打乱之前的排序)
                if (!string.IsNullOrEmpty(_sort))
                {
                    string[] _sort_array = _sort.Split(',');
                    string[] _order_array = _order.Split(',');
                    for (int t = 0; t < _sort_array.Length; t++)
                    {
                        if (_sort_array[t] == "s3")
                        {
                            if (_order_array[t] == "desc")
                            {
                                _Data = _Data.OrderByDescending(p => p.OrderNum).ToList();
                            }
                            else
                            {
                                _Data = _Data.OrderBy(p => p.OrderNum).ToList();
                            }
                        }
                        else if (_sort_array[t] == "s4")
                        {
                            if (_order_array[t] == "desc")
                            {
                                _Data = _Data.OrderByDescending(p => p.ItemNum).ToList();
                            }
                            else
                            {
                                _Data = _Data.OrderBy(p => p.ItemNum).ToList();
                            }
                        }
                        else if (_sort_array[t] == "s5")
                        {
                            if (_order_array[t] == "desc")
                            {
                                _Data = _Data.OrderByDescending(p => p.CancelNum).ToList();
                            }
                            else
                            {
                                _Data = _Data.OrderBy(p => p.CancelNum).ToList();
                            }
                        }
                        else if (_sort_array[t] == "s6")
                        {
                            if (_order_array[t] == "desc")
                            {
                                _Data = _Data.OrderByDescending(p => p.ReturnNum).ToList();
                            }
                            else
                            {
                                _Data = _Data.OrderBy(p => p.ReturnNum).ToList();
                            }
                        }
                        else if (_sort_array[t] == "s7")
                        {
                            if (_order_array[t] == "desc")
                            {
                                _Data = _Data.OrderByDescending(p => p.ExchangeNum).ToList();
                            }
                            else
                            {
                                _Data = _Data.OrderBy(p => p.ExchangeNum).ToList();
                            }
                        }
                        else if (_sort_array[t] == "s8")
                        {
                            if (_order_array[t] == "desc")
                            {
                                _Data = _Data.OrderByDescending(p => p.ExchangeNum).ToList();
                            }
                            else
                            {
                                _Data = _Data.OrderBy(p => p.RejectNum).ToList();
                            }
                        }
                        else if (_sort_array[t] == "s9")
                        {
                            if (_order_array[t] == "desc")
                            {
                                _Data = _Data.OrderByDescending(p => p.TotalPaymentAmount).ToList();
                            }
                            else
                            {
                                _Data = _Data.OrderBy(p => p.TotalPaymentAmount).ToList();
                            }
                        }
                        else
                        {
                            _SqlOrder.Add("CustomerNo desc");
                        }
                    }
                }
                else
                {
                    _SqlOrder.Add("CustomerNo desc");
                }

                if (_showType.ToLower() == "chart")
                {
                    //数据
                    List<EChartsHelper.Pie.Config.Para.Data> _pies = new List<EChartsHelper.Pie.Config.Para.Data>();
                    foreach (var dy in _Data)
                    {
                        if (_chartType == 1)
                        {
                            _pies.Add(new EChartsHelper.Pie.Config.Para.Data()
                            {
                                Name = _list.Items.Where(p => p.CustomerNo == dy.CustomerNo).SingleOrDefault().Name,
                                Value = dy.OrderNum.ToString()
                            });
                        }
                        else if (_chartType == 2)
                        {
                            _pies.Add(new EChartsHelper.Pie.Config.Para.Data()
                            {
                                Name = _list.Items.Where(p => p.CustomerNo == dy.CustomerNo).SingleOrDefault().Name,
                                Value = dy.ItemNum.ToString()
                            });
                        }
                        else if (_chartType == 3)
                        {
                            _pies.Add(new EChartsHelper.Pie.Config.Para.Data()
                            {
                                Name = _list.Items.Where(p => p.CustomerNo == dy.CustomerNo).SingleOrDefault().Name,
                                Value = dy.CancelNum.ToString()
                            });
                        }
                        else if (_chartType == 4)
                        {
                            _pies.Add(new EChartsHelper.Pie.Config.Para.Data()
                            {
                                Name = _list.Items.Where(p => p.CustomerNo == dy.CustomerNo).SingleOrDefault().Name,
                                Value = dy.ReturnNum.ToString()
                            });
                        }
                        else if (_chartType == 5)
                        {
                            _pies.Add(new EChartsHelper.Pie.Config.Para.Data()
                            {
                                Name = _list.Items.Where(p => p.CustomerNo == dy.CustomerNo).SingleOrDefault().Name,
                                Value = dy.ExchangeNum.ToString()
                            });
                        }
                        else if (_chartType == 6)
                        {
                            _pies.Add(new EChartsHelper.Pie.Config.Para.Data()
                            {
                                Name = _list.Items.Where(p => p.CustomerNo == dy.CustomerNo).SingleOrDefault().Name,
                                Value = dy.RejectNum.ToString()
                            });
                        }
                        else
                        {
                            _pies.Add(new EChartsHelper.Pie.Config.Para.Data()
                            {
                                Name = _list.Items.Where(p => p.CustomerNo == dy.CustomerNo).SingleOrDefault().Name,
                                Value = dy.TotalPaymentAmount.ToString()
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
                    string _SearchTab = GetSearchOrderTab();
                    //查询
                    var r = new
                    {
                        total = _list.TotalItems,
                        rows = from dy in _Data
                               select new
                               {
                                   s1 = string.Format("<a href=\"javascript:void(0);\" onclick=\"artDialogExtend.Dialog.Open('" + Url.Action("Detail", "Customer") + "?ID={0}',{{title:'{1}-{0}',width:'100%',height:'100%'}});\">{0}</a>", dy.CustomerNo, _LanguagePack["customer_index_detail"]),
                                   s2 = _list.Items.Where(p => p.CustomerNo == dy.CustomerNo).SingleOrDefault().Name,
                                   s3 = dy.OrderNum,
                                   s4 = dy.ItemNum,
                                   s5 = dy.CancelNum,
                                   s6 = dy.ReturnNum,
                                   s7 = dy.ExchangeNum,
                                   s8 = dy.RejectNum,
                                   s9 = VariableHelper.FormateMoney(dy.TotalPaymentAmount),
                                   s10 = $"<a href=\"javascript:void(0);\" onclick=\"window.parent.frames.OpenTab('{_SearchTab}','{Url.Action("Index", "OrderQuery")}?customer={_list.Items.Where(p => p.CustomerNo == dy.CustomerNo).SingleOrDefault().Name}&time1={_time1}&time2={_time2}',true);\">{_LanguagePack["common_view_order"]}</a>"
                               }
                    };
                    _result.Content = JsonHelper.JsonSerialize(r);
                }
                return _result;
            }
        }
        #endregion
    }
}
