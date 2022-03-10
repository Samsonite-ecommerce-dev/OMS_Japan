using System;
using System.Collections.Generic;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;

namespace Samsonite.OMS.Service
{
    public class ServiceLogService
    {
        public static void Info(int objServiceType, string objMessage)
        {
            LogBase(objServiceType, LogLevel.Info, objMessage, string.Empty);
        }

        public static void Info(int objServiceType, string objMessage, string objRemark)
        {
            LogBase(objServiceType, LogLevel.Info, objMessage, objRemark);
        }

        public static void WARN(int objServiceType, string objMessage)
        {
            LogBase(objServiceType, LogLevel.Warning, objMessage, string.Empty);
        }

        public static void WARN(int objServiceType, string objMessage, string objRemark)
        {
            LogBase(objServiceType, LogLevel.Warning, objMessage, objRemark);
        }

        public static void ERROR(int objServiceType, string objMessage)
        {
            LogBase(objServiceType, LogLevel.Error, objMessage, string.Empty);
        }

        public static void ERROR(int objServiceType, string objMessage, string objRemark)
        {
            LogBase(objServiceType, LogLevel.Error, objMessage, objRemark);
        }

        public static void DEBUG(int objServiceType, string objMessage)
        {
            LogBase(objServiceType, LogLevel.Debug, objMessage, string.Empty);
        }

        public static void DEBUG(int objServiceType, string objMessage, string objRemark)
        {
            LogBase(objServiceType, LogLevel.Debug, objMessage, objRemark);
        }

        private static void LogBase(int objServiceType, LogLevel objLogLevel, string objMessage, string objRemark)
        {
            using (var db = new logEntities())
            {
                try
                {
                    db.ServiceLog.Add(new ServiceLog()
                    {
                        LogType = objServiceType,
                        LogLevel = (int)objLogLevel,
                        LogMessage = objMessage,
                        LogRemark = objRemark,
                        LogIp = string.Empty,
                        CreateTime = DateTime.Now
                    });
                    db.SaveChanges();
                }
                catch { }
            }
        }
    }
}