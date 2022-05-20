using System;
using System.Linq;
using System.Collections.Generic;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;

namespace Samsonite.OMS.Service
{
    public class PromotionService
    {
        /// <summary>
        /// 获取促销信息
        /// </summary>
        /// <returns></returns>
        public static List<ProductPromotionDto> GetProductPromotions()
        {
            //读取有效的促销信息
            List<ProductPromotionDto> _result = new List<ProductPromotionDto>();
            using (var db = new ebEntities())
            {
                //过滤未审核和已删除的促销信息
                List<Promotion> objPromotion_List = db.Promotion.Where(p => p.IsApproval && !p.IsDelete).OrderBy(p => p.GiftType).ToList();
                List<long> PromotionIDList = objPromotion_List.Select(p => p.Id).ToList();
                string PromotionIDs = string.Join(",", PromotionIDList);
                if (string.IsNullOrEmpty(PromotionIDs)) PromotionIDs = "0";
                List<PromotionMall> objPromotionMall_List = db.PromotionMall.Where(p => PromotionIDList.Contains(p.PromotionId)).ToList();
                List<PromotionProduct> objPromotionProduct_List = db.PromotionProduct.Where(p => PromotionIDList.Contains(p.PromotionId)).ToList();
                List<ProductPromotionDto.PromotionGift> objPromotionGift_List = db.Database.SqlQuery<ProductPromotionDto.PromotionGift>("select pg.PromotionId,pg.SKU,pg.Quantity,isnull(p.SalesPrice,0)as SalesPrice,isnull(p.[Description],'')as ProductName,isnull(p.ProductID,'')as ProductID from PromotionGift as pg left join Product as p on pg.sku = p.sku where pg.PromotionId in (" + PromotionIDs + ")").ToList();
                List<PromotionProductInventory> objPromotionProductInventory_List = db.PromotionProductInventory.Where(p => PromotionIDList.Contains(p.PromotionId)).ToList();
                foreach (Promotion objPromotion in objPromotion_List)
                {
                    _result.Add(new ProductPromotionDto()
                    {
                        PromotionID = objPromotion.Id,
                        PromotionName = objPromotion.PromotionName,
                        RuleType = objPromotion.RuleType,
                        GiftRule = objPromotion.GiftRule,
                        GiftType = objPromotion.GiftType,
                        TotalAmount = objPromotion.TotalAmount,
                        BeginDate = objPromotion.BeginDate,
                        EndDate = objPromotion.EndDate,
                        Remark = objPromotion.Remark,
                        Malls = objPromotionMall_List.Where(p => p.PromotionId == objPromotion.Id).Select(p => p.MallSapCode).ToList(),
                        PromotionProducts = objPromotionProduct_List.Where(p => p.PromotionId == objPromotion.Id).ToList(),
                        PromotionGifts = objPromotionGift_List.Where(p => p.PromotionID == objPromotion.Id).ToList()
                    });
                }
            }
            return _result;
        }

        /// <summary>
        /// 解析促销信息
        /// </summary>
        /// <param name="tradeDto"></param>
        /// <param name="productPromotion"></param>
        /// <param name="GiftSubOrderNo"></param>
        public static TradeDto ParseProductPromotion(TradeDto tradeDto, ProductPromotionDto productPromotion, string GiftSubOrderNo = null)
        {
            try
            {
                using (var db = new ebEntities())
                {
                    //买特定产品送
                    if (productPromotion.RuleType == 1)
                    {
                        foreach (var detail in tradeDto.OrderDetails)
                        {
                            //赠品附加的子订单号,如果没有传递,则默认为当前子订单号
                            string _GiftSubOrderNo = (string.IsNullOrEmpty(GiftSubOrderNo)) ? detail.SubOrderNo : GiftSubOrderNo;
                            //判断是否满足条件(不允许设置买2件以上SKU才送赠品的情况)
                            var _p = productPromotion.PromotionProducts.Where(p => p.SKU == detail.SKU && detail.Quantity >= p.Quantity).FirstOrDefault();
                            if (_p != null)
                            {
                                //附加赠品
                                foreach (var sd in productPromotion.PromotionGifts)
                                {
                                    var existGift = tradeDto.OrderGifts.Where(p => p.Sku == sd.SKU && p.IsSystemGift).FirstOrDefault();
                                    if (existGift != null)
                                    {
                                        //取较大数量
                                        if (detail.Quantity * sd.Quantity > existGift.Quantity)
                                        {
                                            existGift.Quantity = detail.Quantity * sd.Quantity;
                                        }
                                    }
                                    else
                                    {
                                        //查询赠品是否有限制
                                        var objInventory = db.PromotionProductInventory.Where(p => p.PromotionId == productPromotion.PromotionID && p.MallSapCode == tradeDto.Order.MallSapCode && p.SKU == sd.SKU).FirstOrDefault();
                                        if (objInventory != null)
                                        {
                                            if (objInventory.CurrentInventory > 0)
                                            {
                                                //判断是否够送
                                                int _giftQuantity = detail.Quantity * sd.Quantity;
                                                if (_giftQuantity > objInventory.CurrentInventory)
                                                    _giftQuantity = objInventory.CurrentInventory;
                                                tradeDto.OrderGifts.Add(new OrderGift()
                                                {
                                                    OrderNo = tradeDto.Order.OrderNo,
                                                    SubOrderNo = _GiftSubOrderNo,
                                                    GiftNo = OrderService.CreateGiftSubOrderNO(detail.SubOrderNo, sd.SKU),
                                                    Sku = sd.SKU,
                                                    ProductName = sd.ProductName,
                                                    MallProductId = string.Empty,
                                                    Price = sd.Price,
                                                    Quantity = _giftQuantity,
                                                    IsSystemGift = true,
                                                    AddDate = DateTime.Now
                                                });
                                                //减少店铺赠品库存
                                                db.Database.ExecuteSqlCommand("update PromotionProductInventory set CurrentInventory=CurrentInventory-{1} where Id={0}", objInventory.Id, _giftQuantity);
                                            }
                                        }
                                        else
                                        {
                                            tradeDto.OrderGifts.Add(new OrderGift()
                                            {
                                                OrderNo = tradeDto.Order.OrderNo,
                                                SubOrderNo = _GiftSubOrderNo,
                                                GiftNo = OrderService.CreateGiftSubOrderNO(detail.SubOrderNo, sd.SKU),
                                                Sku = sd.SKU,
                                                ProductName = sd.ProductName,
                                                MallProductId = string.Empty,
                                                Price = sd.Price,
                                                Quantity = detail.Quantity * sd.Quantity,
                                                IsSystemGift = true,
                                                AddDate = DateTime.Now
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                    //满送
                    else if (productPromotion.RuleType == 2)
                    {
                        if (tradeDto.Order.PaymentAmount >= productPromotion.TotalAmount)
                        {
                            //满送赠品只附加到第一个子订单上(子订单号末尾2位是_1)
                            var tmpDetail = tradeDto.OrderDetails.Where(p => p.SubOrderNo.Substring(p.SubOrderNo.Length - 2) == "_1").FirstOrDefault();
                            if (tmpDetail != null)
                            {
                                //赠品附加的子订单号,如果没有传递,则默认为当前子订单号
                                string _GiftSubOrderNo = (string.IsNullOrEmpty(GiftSubOrderNo)) ? tmpDetail.SubOrderNo : GiftSubOrderNo;
                                //附加赠品
                                foreach (var sd in productPromotion.PromotionGifts)
                                {
                                    var existGift = tradeDto.OrderGifts.Where(p => p.Sku == sd.SKU && p.IsSystemGift).FirstOrDefault();
                                    if (existGift != null)
                                    {
                                        //取较大数量
                                        if (sd.Quantity > existGift.Quantity)
                                        {
                                            existGift.Quantity = sd.Quantity;
                                        }
                                    }
                                    else
                                    {
                                        //查询赠品是否有限制
                                        var objInventory = db.PromotionProductInventory.Where(p => p.PromotionId == productPromotion.PromotionID && p.MallSapCode == tradeDto.Order.MallSapCode && p.SKU == sd.SKU).FirstOrDefault();
                                        if (objInventory != null)
                                        {
                                            if (objInventory.CurrentInventory > 0)
                                            {
                                                //判断是否够送
                                                int _giftQuantity = sd.Quantity;
                                                if (_giftQuantity > objInventory.CurrentInventory)
                                                    _giftQuantity = objInventory.CurrentInventory;
                                                tradeDto.OrderGifts.Add(new OrderGift()
                                                {
                                                    OrderNo = tradeDto.Order.OrderNo,
                                                    SubOrderNo = _GiftSubOrderNo,
                                                    GiftNo = OrderService.CreateGiftSubOrderNO(tmpDetail.SubOrderNo, sd.SKU),
                                                    Sku = sd.SKU,
                                                    ProductName = sd.ProductName,
                                                    MallProductId = string.Empty,
                                                    Price = sd.Price,
                                                    Quantity = _giftQuantity,
                                                    IsSystemGift = true,
                                                    AddDate = DateTime.Now
                                                });
                                                //减赠品库存
                                                db.Database.ExecuteSqlCommand("update PromotionProductInventory set CurrentInventory=CurrentInventory-{1} where Id={0}", objInventory.Id, _giftQuantity);
                                            }
                                        }
                                        else
                                        {
                                            tradeDto.OrderGifts.Add(new OrderGift()
                                            {
                                                OrderNo = tradeDto.Order.OrderNo,
                                                SubOrderNo = _GiftSubOrderNo,
                                                GiftNo = OrderService.CreateGiftSubOrderNO(tmpDetail.SubOrderNo, sd.SKU),
                                                Sku = sd.SKU,
                                                ProductName = sd.ProductName,
                                                MallProductId = string.Empty,
                                                Price = sd.Price,
                                                Quantity = sd.Quantity,
                                                IsSystemGift = true,
                                                AddDate = DateTime.Now
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                    return tradeDto;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}