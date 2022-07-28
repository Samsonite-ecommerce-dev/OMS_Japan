using Samsonite.OMS.ECommerce;
using System;

namespace OMS.ToolWPF.Service
{
    public class WCFService : ECommerceBaseService
    {
        public static new void SetItemsOffSale(string objMallSapCode)
        {
            ECommerceBaseService.SetItemsOffSale(objMallSapCode);
        }

        public static new void CalculateMallSkuSalesPrice(string objMallSapCode)
        {
            ECommerceBaseService.CalculateMallSkuSalesPrice(objMallSapCode);
        }
    }
}
