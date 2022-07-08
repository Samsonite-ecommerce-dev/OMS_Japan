using System;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.Database;
using Samsonite.OMS.Service.WebHook.Models;

namespace Samsonite.OMS.Service.WebHook
{
    public class WebHookPushOrderService
    {
        /// <summary>
        /// 推送新订单
        /// </summary>
        /// <param name="webHookOrders"></param>
        /// <param name="webHookPushTarget"></param>
        public void PushNewOrder(List<WebHookPushOrderRequest> webHookOrders, WebHookPushTarget webHookPushTarget)
        {
            using (var db = new ebEntities())
            {
                var orderNos = webHookOrders.Select(p => p.OrderNo).ToList();
                if (orderNos.Any())
                {
                    List<WebHookOrderPush> webHookOrderPushs = db.WebHookOrderPush.Where(p => orderNos.Contains(p.OrderNo) && p.PushTarget == (int)webHookPushTarget).ToList();
                    foreach (var item in webHookOrders)
                    {
                        var webHookOrderPushTmp = webHookOrderPushs.Where(p => p.OrderNo == item.OrderNo && p.MallSapCode == item.MallSapCode).SingleOrDefault();
                        if (webHookOrderPushTmp == null)
                        {
                            db.WebHookOrderPush.Add(new WebHookOrderPush()
                            {
                                MallSapCode = item.MallSapCode,
                                OrderNo = item.OrderNo,
                                SubOrderNo = string.Empty,
                                PushTarget = (int)webHookPushTarget,
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

        /// <summary>
        /// 推送订单状态
        /// </summary>
        /// <param name="webHookOrderStatus"></param>
        /// <param name="webHookPushTarget"></param>
        public void PushOrderStatus(List<WebHookPushOrderStatusRequest> webHookOrderStatus, WebHookPushTarget webHookPushTarget)
        {
            using (var db = new ebEntities())
            {
                var subOrderNos = webHookOrderStatus.Select(p => p.SubOrderNo).ToList();
                if (subOrderNos.Any())
                {
                    List<WebHookOrderPush> webHookOrderPushs = db.WebHookOrderPush.Where(p => subOrderNos.Contains(p.SubOrderNo) && p.PushTarget == (int)webHookPushTarget && p.PushType != 0).ToList();
                    foreach (var item in webHookOrderStatus)
                    {
                        var webHookOrderPushTmp = webHookOrderPushs.Where(p => p.OrderNo == item.OrderNo && p.SubOrderNo == item.SubOrderNo && p.MallSapCode == item.MallSapCode && p.PushType == item.PushType).SingleOrDefault();
                        if (webHookOrderPushTmp == null)
                        {
                            db.WebHookOrderPush.Add(new WebHookOrderPush()
                            {
                                MallSapCode = item.MallSapCode,
                                OrderNo = item.OrderNo,
                                SubOrderNo = item.SubOrderNo,
                                PushTarget = (int)webHookPushTarget,
                                PushType = item.PushType,
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
    }
}
