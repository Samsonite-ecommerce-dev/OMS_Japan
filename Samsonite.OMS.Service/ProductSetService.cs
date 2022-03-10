using System;
using System.Linq;
using System.Collections.Generic;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service.AppConfig;
using Samsonite.Utility.Common;

namespace Samsonite.OMS.Service
{
    public class ProductSetService
    {
        /// <summary>
        /// 获取套装信息
        /// </summary>
        /// <returns></returns>
        public static List<ProductSetDto> GetProductBundles()
        {
            //读取有效的套装信息
            List<ProductSetDto> _result = new List<ProductSetDto>();
            using (var db = new ebEntities())
            {
                List<ProductSet> objProductSet_List = db.ProductSet.Where(p => p.IsApproval && !p.IsDelete).ToList();
                List<long> SetIDList = objProductSet_List.Select(p => p.Id).ToList();
                string SetIDs = string.Join(",", SetIDList);
                if (string.IsNullOrEmpty(SetIDs)) SetIDs = "0";
                List<ProductSetDto.SetDetail> objProductSetDetail_List = db.Database.SqlQuery<ProductSetDto.SetDetail>("select psd.ProductSetId,psd.SKU,psd.Quantity,psd.Price,psd.IsPrimary,psd.Parent,isnull(p.MarketPrice,0)as MarketPrice,isnull(p.[Description],0)as ProductName,isnull(p.ImageUrl,0)as ProductImage,isnull(p.ProductID,0)as ProductID from ProductSetDetail as psd left join Product as p on psd.sku = p.sku where psd.ProductSetId in (" + SetIDs + ")").ToList();
                List<ProductSetMall> objProductSetMall_List = db.ProductSetMall.Where(p => SetIDList.Contains(p.ProductSetId)).ToList();
                foreach (ProductSet objProductSet in objProductSet_List)
                {
                    _result.Add(new ProductSetDto()
                    {
                        SetID = objProductSet.Id,
                        SetName = objProductSet.SetName,
                        SetCode = objProductSet.SetCode,
                        StartDate = objProductSet.StartDate,
                        EndDate = objProductSet.EndDate,
                        Description = objProductSet.Description,
                        SetDetails = objProductSetDetail_List.Where(p => p.ProductSetId == objProductSet.Id).ToList(),
                        Malls = objProductSetMall_List.Where(p => p.ProductSetId == objProductSet.Id).Select(p => p.MallSapCode).ToList()
                    });
                }
            }
            return _result;
        }

        /// <summary>
        /// 解析套装订单
        /// </summary>
        /// <param name="objTradeDto"></param>
        /// <param name="objBundles"></param>
        /// <returns></returns>
        public static List<TradeDto> ParseProductBundle(TradeDto objTradeDto, ProductSetDto objBundles)
        {
            try
            {
                List<TradeDto> tradeDtos = new List<TradeDto>();
                //金额精准度
                int _AmountAccuracy = ConfigService.GetAmountAccuracyConfig();
                //原始记录
                objTradeDto.OrderDetail.IsSet = true;
                objTradeDto.OrderDetail.IsSetOrigin = true;
                objTradeDto.OrderDetail.IsReservation = false;
                objTradeDto.OrderDetail.IsDelete = false;
                objTradeDto.OrderDetail.IsError = false;
                objTradeDto.OrderDetail.AddDate = DateTime.Now;
                objTradeDto.OrderDetail.EditDate = null;
                //套装原始订单不保存收货信息和快递信息,不会存在gift信息
                tradeDtos.Add(new TradeDto()
                {
                    Order = objTradeDto.Order,
                    OrderDetail = objTradeDto.OrderDetail,
                    Customer = objTradeDto.Customer,
                    PaymentGifts = objTradeDto.PaymentGifts,
                    Payments = objTradeDto.Payments,
                    Billing = objTradeDto.Billing,
                    //只保存订单级别的优惠信息
                    DetailAdjustments = objTradeDto.DetailAdjustments.Where(p => string.IsNullOrEmpty(p.SubOrderNo)).ToList(),
                    OrderShippingAdjustments = objTradeDto.OrderShippingAdjustments,
                    Employee = objTradeDto.Employee
                });

                //解析套装子订单
                decimal _SetTotalPrice = objBundles.SetDetails.Sum(p => p.Price * p.Quantity);
                //套装销售数量(理论上为1)
                int _SetQuantity = objTradeDto.OrderDetail.Quantity;
                //实际付款金额等于套装原始订单的实际付款金额
                decimal _PaymentAmount = 0;
                decimal _r_PaymentAmount = objTradeDto.OrderDetail.PaymentAmount;
                decimal _ActualPayment = 0;
                decimal _r_ActualPayment = objTradeDto.OrderDetail.ActualPaymentAmount;
                int _k = 0;
                //已匹配套装名称或者套装code为准
                foreach (var sd in objBundles.SetDetails.OrderByDescending(p => p.IsPrimary))
                {
                    //如果套装子产品数量可能大于1
                    for (int i = 0; i < sd.Quantity; i++)
                    {
                        _k++;
                        TradeDto t = new TradeDto()
                        {
                            Order = objTradeDto.Order,
                            Customer = objTradeDto.Customer,
                            Employee = objTradeDto.Employee
                        };
                        //实际付款金额
                        //最后一个子订单在分摊实际付款金额时,使用减法计算
                        if (_k == objBundles.SetDetails.Sum(p => p.Quantity))
                        {
                            _PaymentAmount = _r_PaymentAmount;
                            _ActualPayment = _r_ActualPayment;
                        }
                        else
                        {
                            if (_SetTotalPrice > 0)
                            {
                                _PaymentAmount = Math.Round((sd.Price * objTradeDto.OrderDetail.PaymentAmount / _SetTotalPrice), _AmountAccuracy);
                                _r_PaymentAmount -= _PaymentAmount;
                                _ActualPayment = Math.Round((sd.Price * objTradeDto.OrderDetail.ActualPaymentAmount / _SetTotalPrice), _AmountAccuracy);
                                _r_ActualPayment -= _ActualPayment;
                            }
                            else
                            {
                                _PaymentAmount = 0;
                                _ActualPayment = 0;
                            }
                        }
                        string _SubOrderNo = OrderService.CreateSetSubOrderNO(objTradeDto.OrderDetail.SubOrderNo, sd.SKU, objBundles.SetDetails.Sum(p => p.Quantity), _k);
                        string _ParentSubOrderNo = string.Empty;
                        //如果是次级产品
                        if (!sd.IsPrimary)
                        {
                            var _tmp_Parent = tradeDtos.Where(p => p.OrderDetail.SKU == sd.Parent).FirstOrDefault();
                            if (_tmp_Parent != null)
                            {
                                _ParentSubOrderNo = _tmp_Parent.OrderDetail.SubOrderNo;
                            }
                        }
                        t.OrderDetail = new OrderDetail
                        {
                            OrderNo = objTradeDto.Order.OrderNo,
                            SubOrderNo = _SubOrderNo,
                            ParentSubOrderNo = _ParentSubOrderNo,
                            CreateDate = t.Order.CreateDate,
                            RRPPrice = sd.MarketPrice,
                            SupplyPrice = 0,
                            SellingPrice = sd.Price,
                            PaymentAmount = _PaymentAmount,
                            //单价按照套装中产品的单价,付款金额,按照(订单付款金额/订单总金额)的比例计算
                            ActualPaymentAmount = _ActualPayment,
                            ProductName = sd.ProductName,
                            ProductId = sd.ProductId,
                            ProductPic = sd.ProductImage,
                            SKU = sd.SKU,
                            SkuGrade = string.Empty,
                            SkuProperties = string.Empty,
                            MallProductId = objTradeDto.OrderDetail.MallProductId,
                            MallSkuId = objTradeDto.OrderDetail.MallSkuId,
                            SetCode = objTradeDto.OrderDetail.SetCode,
                            Quantity = 1,
                            Status = objTradeDto.OrderDetail.Status,
                            EBStatus = objTradeDto.OrderDetail.EBStatus,
                            ShippingProvider = objTradeDto.OrderDetail.ShippingProvider,
                            ShippingType = objTradeDto.OrderDetail.ShippingType,
                            ShippingStatus = objTradeDto.OrderDetail.ShippingStatus,
                            DeliveringPlant = objTradeDto.OrderDetail.DeliveringPlant,
                            CancelQuantity = objTradeDto.OrderDetail.CancelQuantity,
                            ReturnQuantity = objTradeDto.OrderDetail.ReturnQuantity,
                            ExchangeQuantity = objTradeDto.OrderDetail.ExchangeQuantity,
                            RejectQuantity = objTradeDto.OrderDetail.RejectQuantity,
                            Tax = objTradeDto.OrderDetail.Tax,
                            TaxRate = objTradeDto.OrderDetail.TaxRate,
                            IsReservation = objTradeDto.OrderDetail.IsReservation,
                            ReservationDate = objTradeDto.OrderDetail.ReservationDate,
                            ReservationRemark = objTradeDto.OrderDetail.ReservationRemark,
                            IsSet = true,
                            IsSetOrigin = false,
                            IsPre = objTradeDto.OrderDetail.IsPre,
                            IsGift = objTradeDto.OrderDetail.IsGift,
                            IsUrgent = objTradeDto.OrderDetail.IsUrgent,
                            IsExchangeNew = objTradeDto.OrderDetail.IsExchangeNew,
                            IsSystemCancel = objTradeDto.OrderDetail.IsSystemCancel,
                            IsEmployee = objTradeDto.OrderDetail.IsEmployee,
                            AddDate = DateTime.Now,
                            EditDate = null,
                            CompleteDate = null,
                            ExtraRequest = string.Empty,
                            IsStop = objTradeDto.OrderDetail.IsStop,
                            IsError = false,
                            ErrorMsg = string.Empty,
                            ErrorRemark = string.Empty,
                            IsDelete = false
                        };
                        //收货信息
                        t.Receive = GenericHelper.TCopyValue<OrderReceive>(objTradeDto.Receive);
                        t.Receive.SubOrderNo = _SubOrderNo;
                        //将套装原始订单的产品级促销信息复制到子订单上并删除
                        foreach (var o in objTradeDto.DetailAdjustments.Where(p => !string.IsNullOrEmpty(p.SubOrderNo)))
                        {
                            var tmp = GenericHelper.TCopyValue<OrderDetailAdjustment>(o);
                            //平摊价格
                            tmp.SubOrderNo = t.OrderDetail.SubOrderNo;
                            tmp.BasePrice = Math.Round((o.BasePrice * sd.Price * sd.Quantity / _SetTotalPrice), _AmountAccuracy);
                            tmp.NetPrice = Math.Round((o.NetPrice * sd.Price * sd.Quantity / _SetTotalPrice), _AmountAccuracy);
                            tmp.Tax = Math.Round((o.Tax * sd.Price * sd.Quantity / _SetTotalPrice), _AmountAccuracy);
                            tmp.GrossPrice = Math.Round((o.GrossPrice * sd.Price * sd.Quantity / _SetTotalPrice), _AmountAccuracy);
                            tmp.TaxBasis = Math.Round((o.TaxBasis * sd.Price * sd.Quantity / _SetTotalPrice), _AmountAccuracy);
                            t.DetailAdjustments.Add(tmp);
                        }
                        if (_k == 1)
                        {
                            //产品级别的优惠信息附加到第一个套装子产品上
                            var _subDetailAdjustments = objTradeDto.DetailAdjustments.Where(p => !string.IsNullOrEmpty(p.SubOrderNo));
                            if (_subDetailAdjustments.Count() > 0)
                            {
                                foreach (var DetailAdjustment in _subDetailAdjustments)
                                {
                                    DetailAdjustment.SubOrderNo = _SubOrderNo;
                                }
                            }
                            //赠品信息附加到第一个套装子产品上
                            if (objTradeDto.OrderGifts.Count > 0)
                            {
                                foreach (var _g in objTradeDto.OrderGifts)
                                {
                                    var _tmp_g = GenericHelper.TCopyValue<OrderGift>(_g);
                                    //重新生成赠品订单号
                                    _tmp_g.GiftNo = OrderService.CreateGiftSubOrderNO(_SubOrderNo, _tmp_g.Sku);
                                    _tmp_g.SubOrderNo = _SubOrderNo;
                                    t.OrderGifts.Add(_tmp_g);
                                }
                            }
                        }
                        tradeDtos.Add(t);
                    }
                }
                return tradeDtos;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// 获取套装名称
        /// </summary>
        /// <param name="objSetCode"></param>
        /// <returns></returns>
        public static string GetBundleName(string objSetCode)
        {
            string _result = string.Empty;
            using (var db = new ebEntities())
            {
                var objProductSet = db.ProductSet.Where(p => p.SetCode == objSetCode).SingleOrDefault();
                if (objProductSet != null)
                {
                    _result = objProductSet.SetName;
                }
            }
            return _result;
        }
    }
}