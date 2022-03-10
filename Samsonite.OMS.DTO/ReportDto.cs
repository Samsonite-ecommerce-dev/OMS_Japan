using System;

namespace Samsonite.OMS.DTO
{
    #region 存储过程对象
    /// <summary>
    /// 订单统计结果
    /// </summary>
    public class OrderReport
    {
        public string DTime { get; set; }

        public string MallSapCode { get; set; }

        public int OrderNum { get; set; }

        public int ItemNum { get; set; }

        public decimal TotalOrderAmount { get; set; }

        public decimal TotalPaymentAmount { get; set; }

        public int CancelNum { get; set; }

        public int ReturnNum { get; set; }

        public int ExchangeNum { get; set; }

        public int RejectNum { get; set; }
    }

    /// <summary>
    /// 产品统计结果
    /// </summary>
    public class ProductReport
    {
        public int OrderNum { get; set; }

        public int ItemNum { get; set; }

        public decimal TotalOrderAmount { get; set; }

        public decimal TotalPaymentAmount { get; set; }

        public int CancelNum { get; set; }

        public int ReturnNum { get; set; }

        public int ExchangeNum { get; set; }

        public int RejectNum { get; set; }
    }

    /// <summary>
    /// 品牌统计结果
    /// </summary>
    public class BrandReport
    {
        public DateTime Date { get; set; }

        public string BrandName { get; set; }

        public string MallSapCode { get; set; }

        public int OrderNum { get; set; }

        public int ItemNum { get; set; }

        public int CancelNum { get; set; }

        public int ReturnNum { get; set; }

        public int ExchangeNum { get; set; }

        public int RejectNum { get; set; }

        public decimal TotalOrderAmount { get; set; }

        public decimal TotalPaymentAmount { get; set; }
    }

    /// <summary>
    /// 客户统计结果
    /// </summary>
    public class CustomerReport
    {
        public string CustomerNo { get; set; }

        public int OrderNum { get; set; }

        public int ItemNum { get; set; }

        public decimal TotalOrderAmount { get; set; }

        public decimal TotalPaymentAmount { get; set; }

        public int CancelNum { get; set; }

        public int ReturnNum { get; set; }

        public int ExchangeNum { get; set; }

        public int RejectNum { get; set; }
    }

    /// <summary>
    /// 员工订单统计结果
    /// </summary>
    public class EmployeeOrderReport
    {
        public int EmployeeUserID { get; set; }
        public string CustomerEmail { get; set; }

        public int TimeGroupID { get; set; }

        public string BrandName { get; set; }

        public int ItemQuantity { get; set; }

        public decimal TotalPaymentAmount { get; set; }
    }

    /// <summary>
    /// 通用统计
    /// </summary>
    public class CommonReport
    {
        public string Key { get; set; }

        public object Value { get; set; }
    }
    #endregion

    #region 页面对象
    public class ProductAnalysisView
    {
        public string MallSapCode { get; set; }
        public string ProductName { get; set; }
        public string Brand { get; set; }
        public string GroupDesc { get; set; }
        public string Sku { get; set; }
        public decimal MarketPrice { get; set; }
        public int OrderQuantity { get; set; }
        public int Quantity { get; set; }
        public int CancelQuantity { get; set; }
        public int ReturnQuantity { get; set; }
        public int ExchangeQuantity { get; set; }
        public int RejectQuantity { get; set; }
        public decimal TotalOrderAmount { get; set; }
        public decimal TotalOrderPayment { get; set; }
        public DateTime Date { get; set; }
    }

    public class BrandAnalysisView
    {
        public string MallSapCode { get; set; }
        public int BrandID { get; set; }
        public string Brand { get; set; }
        public int OrderQuantity { get; set; }
        public int Quantity { get; set; }
        public int CancelQuantity { get; set; }
        public int ReturnQuantity { get; set; }
        public int ExchangeQuantity { get; set; }
        public int RejectQuantity { get; set; }
        public decimal TotalOrderAmount { get; set; }
        public decimal TotalOrderPayment { get; set; }
        public DateTime Date { get; set; }
    }
    #endregion
}