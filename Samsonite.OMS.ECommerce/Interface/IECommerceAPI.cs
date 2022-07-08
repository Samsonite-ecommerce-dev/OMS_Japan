using System;
using System.Collections.Generic;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.ECommerce.Models;

namespace Samsonite.OMS.ECommerce.Interface
{
    /// <summary>
    /// 电商平台操作 API 接口
    /// </summary>
    public interface IECommerceAPI
    {
        /// <summary>
        /// 初始化参数
        /// </summary>
        void InitPara(View_Mall_Platform objMall);

        /// <summary>
        /// 店铺SapCode
        /// </summary>
        /// <returns></returns>
        string StoreSapCode();

        /// <summary>
        /// 店铺名称
        /// </summary>
        /// <returns></returns>
        string StoreName();

        /// <summary>
        /// 店铺缩写
        /// </summary>
        /// <returns></returns>
        string StorePrefix();

        /// <summary>
        /// 平台编号
        /// </summary>
        /// <returns></returns>
        int ECommercePlatformCode();

        /// <summary>
        /// 订单查询
        /// </summary>
        /// <param name="objStartDate"></param>
        /// <param name="objEndDate"></param>
        /// <param name="objPageSize"></param>
        /// <returns></returns>
        List<TradeDto> GetTrades(DateTime objStartDate, DateTime objEndDate);

        /// <summary>
        /// 订单增量查询
        /// </summary>
        /// <param name="objStartDate"></param>
        /// <param name="objEndDate"></param>
        /// <param name="objPageSize"></param>
        /// <returns></returns>
        List<TradeDto> GetIncrementTrades(DateTime objStartDate, DateTime objEndDate);

        /// <summary>
        /// 取消/退货/换货/拒收
        /// </summary>
        /// <param name="objStartDate"></param>
        /// <param name="objEndDate"></param>
        /// <returns></returns>
        List<ClaimInfoDto> GetTradeClaims();

        /// <summary>
        /// 获取产品信息
        /// </summary>
        /// <returns></returns>
        List<ItemDto> GetItems();

        /// <summary>
        /// 获取快递信息
        /// </summary>
        /// <returns></returns>
        CommonResult<DeliveryResult> RequireDeliverys();

        /// <summary>
        /// 推送快递信息
        /// </summary>
        /// <returns></returns>
        CommonResult<DeliveryResult> SendDeliverys();

        /// <summary>
        /// 推送库存
        /// </summary>
        /// <returns></returns>
        CommonResult<InventoryResult> SendInventorys();

        /// <summary>
        /// 推送库存警告
        /// </summary>
        /// <returns></returns>
        CommonResult<InventoryResult> SendInventorysWarning();

        /// <summary>
        /// 推送价格
        /// </summary>
        /// <returns></returns>
        CommonResult<PriceResult> SendPrices();

        /// <summary>
        /// 推送订单详情
        /// </summary>
        /// <param name="objStartDate"></param>
        /// <param name="objEndDate"></param>
        /// <returns></returns>
        CommonResult<DetailResult> SendOrderDetails(DateTime objStartDate, DateTime objEndDate);

        /// <summary>
        /// 获取快递运单信息
        /// </summary>
        /// <returns></returns>
        CommonResult<ExpressResult> GetExpresses();

        /// <summary>
        /// 推送Poslog
        /// </summary>
        /// <returns></returns>
        CommonResult<PoslogResult> PushPoslog();
    }
}
