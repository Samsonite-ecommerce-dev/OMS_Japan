using System;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using Samsonite.OMS.Service.WebHook.Models;

namespace Samsonite.OMS.Service.WebHook
{
    public class WebHookPushOrderService
    {
        /// <summary>
        /// 推送新订单
        /// </summary>
        /// <param name="trades"></param>
        /// <param name="pushTarget"></param>
        public void PushNewOrder(List<TradeDto> trades, int pushTarget)
        {
            using (var db = new ebEntities())
            {
                var orderNos = trades.Select(p => p.Order.OrderNo).ToList();
                if (orderNos.Any())
                {
                    List<WebHookOrderPush> webHookOrderPushs = db.WebHookOrderPush.Where(p => orderNos.Contains(p.OrderNo) && p.PushTarget == pushTarget).ToList();
                    foreach (var item in trades)
                    {
                        var webHookOrderPushTmp = webHookOrderPushs.Where(p => p.OrderNo == item.Order.OrderNo).SingleOrDefault();
                        if (webHookOrderPushTmp == null)
                        {
                            db.WebHookOrderPush.Add(new WebHookOrderPush()
                            {
                                MallSapCode = item.Order.MallSapCode,
                                OrderNo = item.Order.OrderNo,
                                SubOrderNo = string.Empty,
                                PushTarget = pushTarget,
                                PushType = (int)WebHookPushType.New,
                                PushStatus = 0,
                                PushCount = 0,
                                PushMessage = string.Empty,
                                CreateTime = DateTime.Now
                            });
                        }
                    }
                    db.SaveChanges();
                }
            }
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="orderDetails"></param>
        ///// <param name="pushTarget"></param>
        //public void PushOrderStatus(List<View_OrderDetail> orderDetails, int pushTarget)
        //{
        //    using (var db = new ebEntities())
        //    {
        //        var subOrderNos = orderDetails.Select(p => p.SubOrderNo).ToList();
        //        if (subOrderNos.Any())
        //        {
        //            List<WebHookOrderPush> webHookOrderPushs = db.WebHookOrderPush.Where(p => orderNos.Contains(p.OrderNo) && p.PushTarget == pushTarget).ToList();
        //            foreach (var item in tradeDtos)
        //            {
        //                var webHookOrderPushTmp = webHookOrderPushs.Where(p => p.OrderNo == item.Order.OrderNo).SingleOrDefault();
        //                if (webHookOrderPushTmp == null)
        //                {
        //                    db.WebHookOrderPush.Add(new WebHookOrderPush()
        //                    {
        //                        MallSapCode = item.Order.MallSapCode,
        //                        OrderNo = item.Order.OrderNo,
        //                        SubOrderNo = string.Empty,
        //                        PushTarget = pushTarget,
        //                        PushType = (int)PushType.New,
        //                        PushStatus = 0,
        //                        PushCount = 0,
        //                        PushMessage = string.Empty,
        //                        CreateTime = DateTime.Now
        //                    });
        //                }
        //            }
        //            db.SaveChanges();
        //        }
        //    }
        //}
    }
}
