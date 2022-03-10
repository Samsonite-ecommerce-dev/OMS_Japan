using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;

namespace Samsonite.OMS.Service
{
    /// <summary>
    /// 库存相关服务
    /// </summary>
    public class InventoryService
    {
        /// <summary>
        /// 计算当前在售套装库存数
        /// </summary>
        public static void CalculateBundleInventory_OnSale()
        {
            using (var db = new ebEntities())
            {
                //读取所有在售套装产品
                List<MallProduct> objMallProductSetList = db.MallProduct.Where(p => p.ProductType == (int)ProductType.Bundle && p.IsOnSale).ToList();
                //去重处理
                List<string> objBundleSkuLists = objMallProductSetList.GroupBy(p => p.SKU).Select(o => o.Key).ToList();
                //计算库存数
                CalculateBundleMethod(objBundleSkuLists, db);
            }
        }

        /// <summary>
        /// 计算当前在售套装库存数
        /// </summary>
        public static void CalculateBundleInventory_OnSale(string objMallSapCode)
        {
            using (var db = new ebEntities())
            {
                //读取所有在售套装产品
                List<MallProduct> objMallProductSetList = db.MallProduct.Where(p => p.MallSapCode == objMallSapCode && p.ProductType == (int)ProductType.Bundle && p.IsOnSale).ToList();
                //去重处理
                List<string> objBundleSkuLists = objMallProductSetList.GroupBy(p => p.SKU).Select(o => o.Key).ToList();
                //计算库存数
                CalculateBundleMethod(objBundleSkuLists, db);
            }
        }

        /// <summary>
        /// 计算套装库存数
        /// </summary>
        /// <param name="objBundles"></param>
        /// <param name="db"></param>
        private static void CalculateBundleMethod(List<string> objBundles, ebEntities db = null)
        {
            if (db == null)
                db = new ebEntities();
            //读取套装信息
            List<ProductSet> objProductSetList = db.ProductSet.ToList();
            var objProductSetDetailList = (from psd in db.ProductSetDetail
                                           join p in db.Product on psd.SKU equals p.SKU into tmp
                                           from p in tmp.DefaultIfEmpty()
                                           select new
                                           {
                                               ProductSetId = psd.ProductSetId,
                                               Sku = psd.SKU,
                                               PerQty = psd.Quantity,
                                               Inventory = (int?)p.Quantity ?? 0
                                           }).ToList();

            foreach (string _bundleSkus in objBundles)
            {
                int _setInventory = 0;
                var objCurrentProductSet = objProductSetList.Where(p => p.SetCode == _bundleSkus).FirstOrDefault();
                if (objCurrentProductSet != null)
                {
                    var objCurrentProductSetDetail = objProductSetDetailList.Where(p => p.ProductSetId == objCurrentProductSet.Id).ToList();
                    //构成该套装的子产品数可能大于1
                    int _minInventory = objCurrentProductSetDetail.Min(p => p.Inventory / p.PerQty);
                    //套装配成限制
                    if (_minInventory <= objCurrentProductSet.Inventory)
                    {
                        _setInventory = _minInventory;
                    }
                    else
                    {
                        _setInventory = objCurrentProductSet.Inventory;
                    }
                    if (_setInventory < 0)
                        _setInventory = 0;
                    //更新套装库存
                    ProductService.SetBundleProductInventory(_bundleSkus, _setInventory);
                }
            }
        }
    }
}
