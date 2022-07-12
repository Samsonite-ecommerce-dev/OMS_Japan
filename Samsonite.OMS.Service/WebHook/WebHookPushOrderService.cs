using System;
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
        /// <param name="pushTarget">推送目标(CRM)</param>
        public void PushNewOrders(WebHookPushTarget pushTarget)
        {
            using (var db = new ebEntities())
            {
                //读取最近90天内的订单信息
                var _time = DateTime.Now.AddDays(WebHookConfig.timeAgo);
                var orderQuery = db.Order.Where(p => p.CreateDate >= _time).AsQueryable();
                //过滤已经发送成功的记录
                var filterQuery = orderQuery.Where(p => !(db.WebHookOrderPushLog.Where(o => o.OrderId == p.Id && o.PushStatus == 2 && o.PushTarget == (int)pushTarget && o.OrderId >= orderQuery.DefaultIfEmpty().Min(t => t.Id))).Any()).ToList();
                if (filterQuery.Count > 0)
                {
                    var orderIds = filterQuery.Select(p => p.Id).ToList();
                    var webHookOrderPushLogs = db.WebHookOrderPushLog.Where(p => orderIds.Contains(p.OrderId) && p.PushTarget == (int)pushTarget).ToList();
                    foreach (var item in filterQuery)
                    {
                        //发送到CRM
                        if (1 == 1)
                        {
                            //添加成功记录
                            var tmpWebHookOrderPushLogs = webHookOrderPushLogs.Where(p => p.OrderId == item.Id).FirstOrDefault();
                            if (tmpWebHookOrderPushLogs != null)
                            {
                                tmpWebHookOrderPushLogs.PushStatus = 2;
                                tmpWebHookOrderPushLogs.CompleteTime = DateTime.Now;
                            }
                            else
                            {
                                db.WebHookOrderPushLog.Add(new WebHookOrderPushLog()
                                {
                                    MallSapCode = item.MallSapCode,
                                    OrderId = item.Id,
                                    OrderNo = item.OrderNo,
                                    PushTarget = (int)pushTarget,
                                    PushStatus = 2,
                                    PushCount = 0,
                                    PushMessage = string.Empty,
                                    CreateTime = DateTime.Now,
                                    CompleteTime = null
                                });
                            }
                        }
                        else
                        {
                            //添加失败记录
                            var tmpWebHookOrderPushLogs = webHookOrderPushLogs.Where(p => p.OrderId == item.Id).FirstOrDefault();
                            if (tmpWebHookOrderPushLogs != null)
                            {
                                tmpWebHookOrderPushLogs.PushCount += 1;
                                tmpWebHookOrderPushLogs.PushMessage = "";
                            }
                            else
                            {
                                db.WebHookOrderPushLog.Add(new WebHookOrderPushLog()
                                {
                                    MallSapCode = item.MallSapCode,
                                    OrderId = item.Id,
                                    OrderNo = item.OrderNo,
                                    PushTarget = (int)pushTarget,
                                    PushStatus = 0,
                                    PushCount = 1,
                                    PushMessage = "",
                                    CreateTime = DateTime.Now,
                                    CompleteTime = null
                                });
                            }
                        }
                        db.SaveChanges();
                    }
                }
            }
        }
    }
}
