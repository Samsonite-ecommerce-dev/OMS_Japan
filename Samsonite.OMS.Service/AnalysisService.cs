using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Encryption;

namespace Samsonite.OMS.Service
{
    public class AnalysisService
    {
        #region 每日订单统计
        /// <summary>
        /// 每日订单销售统计
        /// </summary>
        /// <param name="objTime"></param>
        /// <returns></returns>
        public int OrderDailyStatistics(DateTime objTime)
        {
            int _result = 0;
            string _sql = string.Empty;
            using (var db = new ebEntities())
            {
                try
                {
                    //有效店铺
                    List<Mall> objMall_List = MallService.GetMallOption();
                    //删除当日记录重新统计
                    db.Database.ExecuteSqlCommand($"delete from AnalysisDailyOrder where Datediff(DAY,[date],'{objTime.ToString("yyyy-MM-dd")}')=0 and TimeZoon=0;");
                    foreach (Mall _m in objMall_List.Where(p => p.IsOpenService))
                    {
                        //日
                        OrderReport objOrderReport = db.Database.SqlQuery<OrderReport>("Exec Proc_Report_Order {0},{1},{2}", _m.SapCode, objTime.ToString("yyyy-MM-dd"), "DAY").SingleOrDefault();
                        if (objOrderReport != null)
                        {
                            _sql += $"Insert into AnalysisDailyOrder Values('{_m.SapCode}',{objOrderReport.OrderNum},{objOrderReport.ItemNum},'{objOrderReport.TotalOrderAmount}','{objOrderReport.TotalPaymentAmount}',{objOrderReport.CancelNum},{objOrderReport.ReturnNum},{objOrderReport.ExchangeNum},{objOrderReport.RejectNum},0,'{objTime.ToString("yyyy-MM-dd")}','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}');";
                            _result++;
                        }
                    }
                    if (!string.IsNullOrEmpty(_sql)) db.Database.ExecuteSqlCommand(_sql);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return _result;
        }

        /// <summary>
        /// 每月订单销售统计
        /// </summary>
        /// <param name="objTime"></param>
        /// <returns></returns>
        public int OrderMonthStatistics(DateTime objTime)
        {
            int _result = 0;
            string _sql = string.Empty;
            using (var db = new ebEntities())
            {
                try
                {
                    //有效店铺
                    List<Mall> objMall_List = MallService.GetMallOption();
                    //删除当月记录重新统计
                    db.Database.ExecuteSqlCommand($"delete from AnalysisDailyOrder where Datediff(MONTH,[date],'{objTime.ToString("yyyy-MM-dd")}')=0 and TimeZoon=1;");
                    foreach (Mall _m in objMall_List)
                    {
                        //月
                        OrderReport objOrderReport = db.Database.SqlQuery<OrderReport>("Exec Proc_Report_Order {0},{1},{2}", _m.SapCode, objTime.ToString("yyyy-MM-01"), "MONTH").SingleOrDefault();
                        if (objOrderReport != null)
                        {
                            _sql += $"Insert into AnalysisDailyOrder Values('{_m.SapCode}',{objOrderReport.OrderNum},{objOrderReport.ItemNum},'{objOrderReport.TotalOrderAmount}','{objOrderReport.TotalPaymentAmount}',{objOrderReport.CancelNum},{objOrderReport.ReturnNum},{objOrderReport.ExchangeNum},{objOrderReport.RejectNum},1,'{objTime.ToString("yyyy-MM-01")}','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}');";
                            _result++;
                        }
                    }
                    if (!string.IsNullOrEmpty(_sql)) db.Database.ExecuteSqlCommand(_sql);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return _result;
        }

        /// <summary>
        /// 每年订单销售统计
        /// </summary>
        /// <param name="objTime"></param>
        /// <returns></returns>
        public int OrderYearStatistics(DateTime objTime)
        {
            int _result = 0;
            string _sql = string.Empty;
            using (var db = new ebEntities())
            {
                try
                {
                    //有效店铺
                    List<Mall> objMall_List = MallService.GetMallOption();
                    //删除当年记录重新统计
                    db.Database.ExecuteSqlCommand($"delete from AnalysisDailyOrder where Datediff(YEAR,[date],'{objTime.ToString("yyyy-MM-dd")}')=0 and TimeZoon=2;");
                    foreach (Mall _m in objMall_List)
                    {
                        //月
                        OrderReport objOrderReport = db.Database.SqlQuery<OrderReport>("Exec Proc_Report_Order {0},{1},{2}", _m.SapCode, objTime.ToString("yyyy-01-01"), "YEAR").SingleOrDefault();
                        if (objOrderReport != null)
                        {
                            _sql += $"Insert into AnalysisDailyOrder Values('{_m.SapCode}',{objOrderReport.OrderNum},{objOrderReport.ItemNum},'{objOrderReport.TotalOrderAmount}','{objOrderReport.TotalPaymentAmount}',{objOrderReport.CancelNum},{objOrderReport.ReturnNum},{objOrderReport.ExchangeNum},{objOrderReport.RejectNum},2,'{objTime.ToString("yyyy-01-01")}','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}');";
                            _result++;
                        }
                    }
                    if (!string.IsNullOrEmpty(_sql)) db.Database.ExecuteSqlCommand(_sql);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return _result;
        }
        #endregion

        #region 每日产品销售统计
        /// <summary>
        /// 每日产品销售统计
        /// </summary>
        /// <param name="objTime"></param>
        /// <returns></returns>
        public int ProductDailyStatistics(DateTime objTime)
        {
            int _result = 0;
            string _sql = string.Empty;
            using (var db = new ebEntities())
            {
                try
                {
                    //当天订单(过滤已关闭订单,赠品,套餐子订单，只统计整个套装)
                    List<View_OrderDetail> objOrderDetail_List = db.Database.SqlQuery<View_OrderDetail>("select * from View_OrderDetail where Datediff(DAY,OrderTime,{0})=0 and ProductStatus>=-1 and IsDelete=0 and not (IsSet=1 and IsSetOrigin=0)", objTime.ToString("yyyy-MM-dd")).ToList();
                    //有效店铺
                    List<Mall> objMall_List = MallService.GetMallOption();
                    //删除当天记录重新统计
                    db.Database.ExecuteSqlCommand($"delete from AnalysisDailyProduct where Datediff(DAY,[date],'{objTime.ToString("yyyy-MM-dd")}')=0");
                    //计算各店铺统计
                    _sql = string.Empty;
                    foreach (Mall objMall in objMall_List)
                    {
                        List<View_OrderDetail> _TempOrderDetail_List = objOrderDetail_List.Where(p => p.MallSapCode == objMall.SapCode).ToList();
                        foreach (string _sku in _TempOrderDetail_List.GroupBy(p => p.SKU).Select(o => o.Key))
                        {
                            if (!string.IsNullOrEmpty(_sku))
                            {
                                ProductReport objProductReport = db.Database.SqlQuery<ProductReport>("Exec Proc_Report_Product {0},{1},{2}", objMall.SapCode, _sku, objTime.ToString("yyyy-MM-dd")).SingleOrDefault();
                                if (objProductReport != null)
                                {
                                    _sql += $"Insert into AnalysisDailyProduct Values('{objMall.SapCode}','{_sku}',{objProductReport.OrderNum},{objProductReport.ItemNum},'{objProductReport.TotalOrderAmount}','{objProductReport.TotalPaymentAmount}',{objProductReport.CancelNum},{objProductReport.ReturnNum},{objProductReport.ExchangeNum},{objProductReport.RejectNum},0,'{objTime.ToString("yyyy-MM-dd")}','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}');";
                                    _result++;
                                }
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(_sql)) db.Database.ExecuteSqlCommand(_sql);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return _result;
        }
        #endregion

        #region 每日员工订单统计
        /// <summary>
        /// 每日员工订单销售统计
        /// </summary>
        /// <returns></returns>
        public int EmployeeOrderStatistics()
        {
            int _result = 0;
            using (var db = new ebEntities())
            {
                try
                {
                    //获取员工账号(被锁定的也继续统计，只是在每年更新时间段时不更新被锁定的员工)
                    List<UserEmployee> objUserEmployee_List = db.UserEmployee.ToList();
                    //获取品牌列表
                    List<string> objBrand_List = db.Database.SqlQuery<string>("select BrandName from Brand where IsLock=0").ToList();
                    foreach (var _O in objUserEmployee_List)
                    {
                        foreach (var _brand in objBrand_List)
                        {
                            try
                            {
                                EmployeeOrderReport objEmployeeOrderReport = db.Database.SqlQuery<EmployeeOrderReport>("Exec Proc_Report_Employee {0},{1},{2}", _O.EmployeeEmail, _O.DataGroupID, _brand).SingleOrDefault();
                                if (objEmployeeOrderReport != null)
                                {
                                    //查询该年度该员工记录是否存在
                                    AnalysisEmployeeOrder objAnalysisEmployeeOrder = db.AnalysisEmployeeOrder.Where(p => p.EmployeeUserID == _O.EmployeeID && p.Brand == objEmployeeOrderReport.BrandName && p.DataGroupID == _O.DataGroupID).SingleOrDefault();
                                    if (objAnalysisEmployeeOrder != null)
                                    {
                                        objAnalysisEmployeeOrder.Quantity = objEmployeeOrderReport.ItemQuantity;
                                        objAnalysisEmployeeOrder.TotalOrderPayment = objEmployeeOrderReport.TotalPaymentAmount;
                                        objAnalysisEmployeeOrder.EditTime = DateTime.Now;
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        db.AnalysisEmployeeOrder.Add(new AnalysisEmployeeOrder()
                                        {
                                            EmployeeUserID = _O.EmployeeID,
                                            DataGroupID = objEmployeeOrderReport.TimeGroupID,
                                            Brand = objEmployeeOrderReport.BrandName,
                                            Quantity = objEmployeeOrderReport.ItemQuantity,
                                            TotalOrderPayment = objEmployeeOrderReport.TotalPaymentAmount,
                                            AddTime = DateTime.Now,
                                            EditTime = null
                                        });
                                    }
                                    db.SaveChanges();
                                }
                            }
                            catch
                            {

                            }
                        }
                        _result++;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return _result;
        }

        /// <summary>
        /// 每年一次重置员工报表的时间段
        /// </summary>
        /// <param name="objDateTime"></param>
        public void EmployeeOrderYearDateReset(DateTime objDateTime)
        {
            using (var db = new ebEntities())
            {
                //匹配时间段
                int _DataGroupID = 0;
                DateTime _BeginDate = Convert.ToDateTime(objDateTime.ToString("yyyy-01-01"));
                DateTime _EndDate = Convert.ToDateTime(objDateTime.ToString("yyyy-12-31"));
                UserEmployeeGroup objUserEmployeeGroup = db.UserEmployeeGroup.Where(p => p.BeginDate == _BeginDate && p.EndDate == _EndDate).SingleOrDefault();
                if (objUserEmployeeGroup != null)
                {
                    _DataGroupID = objUserEmployeeGroup.ID;
                }
                else
                {
                    objUserEmployeeGroup = new UserEmployeeGroup()
                    {
                        EmployeeGroup = $"{_BeginDate.ToString("yyyy/MM/dd")}-{_EndDate.ToString("yyyy/MM/dd")}",
                        BeginDate = _BeginDate,
                        EndDate = _EndDate,
                        Remark = string.Empty
                    };
                    db.UserEmployeeGroup.Add(objUserEmployeeGroup);
                    db.SaveChanges();
                    _DataGroupID = objUserEmployeeGroup.ID;
                }

                //更新所有非锁定员工的时间段指向
                db.Database.ExecuteSqlCommand("Update UserEmployee set DataGroupID={0} where IsLock=0", _DataGroupID);
            }
        }
        #endregion
    }
}