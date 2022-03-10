using System;

namespace Samsonite.OMS.DTO
{
    public class ProductImportDto
    {
        public string Name { get; set; }
        public string MatlGroup { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string GroupDesc { get; set; }
        public string Material { get; set; }
        public string GdVal { get; set; }
        public string EAN { get; set; }
        public string SKU { get; set; }
        public float Length { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float Volume { get; set; }
        public float Weight { get; set; }
        public decimal MarketPrice { get; set; }
        public decimal SalesPrice { get; set; }
        public decimal SupplyPrice { get; set; }
        public bool IsCommon { get; set; }
        public bool IsGift { get; set; }
        public bool IsSet { get; set; }
        public bool IsUsed { get; set; }
        public bool Result { get; set; }
        public string ResultMsg { get; set; }
    }

    public class ProductInventoryImportDto
    {
        public string MallSapCode { get; set; }
        public string MallProductName { get; set; }
        public string ProductType { get; set; }
        public string SKU { get; set; }
        public string OuterProduct { get; set; }
        public string OuterSku { get; set; }
        public bool IsOnSale { get; set; }
        public bool IsUsed { get; set; }
        public bool Result { get; set; }
        public string ResultMsg { get; set; }
    }

    public class ProductPriceImportDto
    {
        public string MallSapCode { get; set; }
        public string SKU { get; set; }
        public string OuterProduct { get; set; }
        public string OuterSku { get; set; }
        public decimal SalesPrice { get; set; }
        public string SalesPriceBeginTime { get; set; }
        public string SalesPriceEndTime { get; set; }
        public bool Result { get; set; }
        public string ResultMsg { get; set; }
    }
}
