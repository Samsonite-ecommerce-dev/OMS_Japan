using System;
using System.Collections.Generic;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using Samsonite.OMS.ECommerce.Interface;
using Samsonite.OMS.ECommerce.Models;

namespace Samsonite.OMS.ECommerce.Japan.Tumi
{
    public class TumiControl : TumiAPI, IECommerceAPI
    {
        #region 基础参数
        /// <summary>
        /// 初始化参数
        /// </summary>
        /// <param name="objMall"></param>
        public void InitPara(View_Mall_Platform objMall)
        {
            this.SetPara(objMall);
        }

        /// <summary>
        /// 店铺SapCode
        /// </summary>
        /// <returns></returns>
        public string StoreSapCode()
        {
            return this.MallSapCode;
        }

        /// <summary>
        /// 店铺名称
        /// </summary>
        /// <returns></returns>
        public string StoreName()
        {
            return this.MallName;
        }

        /// <summary>
        /// 店铺缩写
        /// </summary>
        /// <returns></returns>
        public string StorePrefix()
        {
            return this.MallPrefix;
        }

        /// <summary>
        /// 平台编号
        /// </summary>
        /// <returns></returns>
        public int ECommercePlatformCode()
        {
            return this.PlatformCode;
        }
        #endregion

        #region 交易
        /// <summary>
        /// 订单查询
        /// </summary>
        /// <param name="objStartDate"></param>
        /// <param name="objEndDate"></param>
        /// <returns></returns>
        public List<TradeDto> GetTrades(DateTime objStartDate, DateTime objEndDate)
        {
            if (this.ServicePowers.IsGetTrades)
            {
                return this.GetOrders();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 订单增量查询
        /// </summary>
        /// <param name="objStartDate"></param>
        /// <param name="objEndDate"></param>
        /// <returns></returns>
        public List<TradeDto> GetIncrementTrades(DateTime objStartDate, DateTime objEndDate)
        {
            if (this.ServicePowers.IsGetIncrementTrades)
            {
                return this.GetOrders();
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region 取消/退货/换货/拒收
        /// <summary>
        /// 取消/退货/换货/拒收查询
        /// </summary>
        /// <param name="objStartDate"></param>
        /// <param name="objEndDate"></param>
        /// <returns></returns>
        public List<ClaimInfoDto> GetTradeClaims()
        {
            if (this.ServicePowers.IsGetTradeClaims)
            {
                var objClaimInfoList = this.GetClaims();
                return ECommerceBaseService.JoinClaimList(objClaimInfoList, this.MallSapCode);
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region 商品
        /// <summary>
        /// 获取产品信息
        /// </summary>
        public List<ItemDto> GetItems()
        {
            if (this.ServicePowers.IsGetItems)
            {
                return this.GetOnSaleItems();
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region 订单详情
        /// <summary>
        /// 推送订单详情
        /// </summary>
        /// <param name="objStartDate"></param>
        /// <param name="objEndDate"></param>
        /// <returns></returns>
        public CommonResult<DetailResult> SendOrderDetails(DateTime objStartDate, DateTime objEndDate)
        {
            if (this.ServicePowers.IsSendOrderDetail)
            {
                return this.PushOrderDetails(objStartDate, objEndDate);
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region 快递信息
        /// <summary>
        /// 获取快递信息
        /// </summary>
        /// <returns></returns>
        public CommonResult<DeliveryResult> RequireDeliverys()
        {
            return null;
        }

        /// <summary>
        /// 推送快递信息
        /// </summary>
        /// <returns></returns>
        public CommonResult<DeliveryResult> SendDeliverys()
        {
            if (this.ServicePowers.IsSendDelivery)
            {
                return this.SetReadyToShip();
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region 库存
        /// <summary>
        /// 推送库存
        /// </summary>
        /// <returns></returns>
        public CommonResult<InventoryResult> SendInventorys()
        {
            if (this.ServicePowers.IsSendInventorys)
            {
                return this.PushInventorys();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 推送库存警告
        /// </summary>
        /// <returns></returns>
        public CommonResult<InventoryResult> SendInventorysWarning()
        {
            if (this.ServicePowers.IsSendInventorysWarning)
            {
                return this.PushInventorysWarning();
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region 价格
        /// <summary>
        /// 推送价格
        /// </summary>
        /// <returns></returns>
        public CommonResult<PriceResult> SendPrices()
        {
            if (this.ServicePowers.IsSendPrice)
            {
                return this.PushPrices();
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region 物流信息
        /// <summary>
        /// 从平台获取快递号
        /// </summary>
        /// <returns></returns>
        public CommonResult<ExpressResult> GetExpresses()
        {
            if (this.ServicePowers.IsGetExpress)
            {
                return this.GetExpressFromPlatform();
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region Poslog
        /// <summary>
        /// 推送Poslog到SAP
        /// </summary>
        /// <returns></returns>
        public CommonResult<PoslogResult> PushPoslog()
        {
            if (this.ServicePowers.IsPoslog)
            {
                return ExtendAPI.PushPoslog(this.StoreSapCode());
            }
            else
            {
                return null;
            }
        }
        #endregion
    }
}
