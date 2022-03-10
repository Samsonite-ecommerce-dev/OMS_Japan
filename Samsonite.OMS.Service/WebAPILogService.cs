using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;

namespace Samsonite.OMS.Service
{
    /// <summary>
    /// Web APi 相关日志 
    /// </summary>
    public class WebAPILogService
    {
        /// <summary>
        /// API访问日志
        /// </summary>
        /// <param name="log"></param>
        public static void WriteAccessLog(WebApiAccessLog log)
        {
            using (var db = new logEntities())
            {
                db.WebApiAccessLog.Add(log);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 电商平台下载日志
        /// </summary>
        /// <param name="log"></param>
        public static void WriteECommercePlatformLog(ECommercePlatformApiLog log)
        {
            using (var db = new logEntities())
            {
                db.ECommercePlatformApiLog.Add(log);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// API错误日志
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="level"></param>
        public static void WriteLog(string msg, LogLevel level = LogLevel.Error)
        {
            using (var db = new logEntities())
            {
                db.WebApiLog.Add(new WebApiLog
                {
                    AddDate = DateTime.Now,
                    LogLevel = level.ToString(),
                    Msg = msg
                });
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 服务运行日志
        /// </summary>
        /// <param name="log"></param>
        public static void WriteServiceLog(ServiceLog log)
        {
            using (var db = new logEntities())
            {
                db.ServiceLog.Add(log);
                db.SaveChanges();
            }
        }
    }
}
