using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.Utility.Common;

namespace Samsonite.OMS.Service
{
    public class AppLogService
    {
        #region 系统操作日志
        /// <summary>
        /// 添加日志
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objData"></param>
        /// <param name="objRecord"></param>
        public static void InsertLog<T>(T objData, string objRecord)
        {
            using (var db = new logEntities())
            {
                try
                {
                    db.WebAppOperationLog.Add(new WebAppOperationLog()
                    {
                        OperationType = (int)OperationLogType.Insert,
                        TableName = objData.GetType().Name,
                        UserID = UserLoginService.GetCurrentUserID,
                        UserIP = UrlHelper.GetRequestIP(),
                        RecordID = objRecord,
                        LogMessage = JsonHelper.JsonSerialize(objData),
                        AddTime = DateTime.Now
                    });
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 添加日志
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objData"></param>
        /// <param name="objRecord"></param>
        public static void InsertLog<T>(List<T> objData, string objRecord)
        {
            using (var db = new logEntities())
            {
                try
                {
                    db.WebAppOperationLog.Add(new WebAppOperationLog()
                    {
                        OperationType = (int)OperationLogType.Insert,
                        TableName = objData.GetType().Name,
                        UserID = UserLoginService.GetCurrentUserID,
                        UserIP = UrlHelper.GetRequestIP(),
                        RecordID = objRecord,
                        LogMessage = JsonHelper.JsonSerialize(objData),
                        AddTime = DateTime.Now
                    });
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 更新日志
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objData"></param>
        /// <param name="objRecord"></param>
        public static void UpdateLog<T>(T objData, string objRecord)
        {
            using (var db = new logEntities())
            {
                try
                {
                    db.WebAppOperationLog.Add(new WebAppOperationLog()
                    {
                        OperationType = (int)OperationLogType.Update,
                        TableName = objData.GetType().Name,
                        UserID = UserLoginService.GetCurrentUserID,
                        UserIP = UrlHelper.GetRequestIP(),
                        RecordID = objRecord,
                        LogMessage = JsonHelper.JsonSerialize(objData),
                        AddTime = DateTime.Now
                    });
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 更新日志
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objData"></param>
        /// <param name="objRecord"></param>
        public static void UpdateLog<T>(List<T> objData, string objRecord)
        {
            using (var db = new logEntities())
            {
                try
                {
                    db.WebAppOperationLog.Add(new WebAppOperationLog()
                    {
                        OperationType = (int)OperationLogType.Update,
                        TableName = objData.GetType().Name,
                        UserID = UserLoginService.GetCurrentUserID,
                        UserIP = UrlHelper.GetRequestIP(),
                        RecordID = objRecord,
                        LogMessage = JsonHelper.JsonSerialize(objData),
                        AddTime = DateTime.Now
                    });
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 删除日志
        /// </summary>
        /// <param name="objTableName"></param>
        /// <param name="objRecord"></param>
        /// <param name="objMessage"></param>
        public static void DeleteLog(string objTableName, string objRecord, string objMessage = "")
        {
            using (var db = new logEntities())
            {
                try
                {
                    db.WebAppOperationLog.Add(new WebAppOperationLog()
                    {
                        OperationType = (int)OperationLogType.Delete,
                        TableName = objTableName,
                        UserID = UserLoginService.GetCurrentUserID,
                        UserIP = UrlHelper.GetRequestIP(),
                        RecordID = objRecord,
                        LogMessage = objMessage,
                        AddTime = DateTime.Now
                    });
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 系统日志
        /// </summary>
        /// <param name="objMessage"></param>
        /// <param name="logLevel"></param>
        public static void SystemLog(string objMessage, string logLevel = "")
        {
            using (var db = new logEntities())
            {
                try
                {
                    db.WebAppErrorLog.Add(new WebAppErrorLog()
                    {
                        UserID = UserLoginService.GetCurrentUserID,
                        UserIP = UrlHelper.GetRequestIP(),
                        LogLevel = (logLevel == string.Empty) ? Samsonite.OMS.DTO.ErrorLogType.System.ToString() : logLevel,
                        LogMessage = objMessage,
                        AddTime = DateTime.Now
                    });
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        #endregion

        #region 登入日志
        /// <summary>
        /// 登录日志
        /// </summary>
        /// <param name="objWebAppLoginLog"></param>
        public static void LoginLog(WebAppLoginLog objWebAppLoginLog)
        {
            using (var db = new logEntities())
            {
                try
                {
                    db.WebAppLoginLog.Add(objWebAppLoginLog);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 密码修改日志
        /// </summary>
        /// <param name="objWebAppPasswordLog"></param>
        public static void PasswordLog(WebAppPasswordLog objWebAppPasswordLog)
        {
            using (var db = new logEntities())
            {
                try
                {
                    db.WebAppPasswordLog.Add(objWebAppPasswordLog);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        #endregion

        #region 电商平台日志
        /// <summary>
        /// 电商平台日志
        /// </summary>
        /// <param name="order"></param>
        /// <param name="msg"></param>
        public static void WriteECommercePlatformLog(Order order, string msg)
        {
            WebAPILogService.WriteECommercePlatformLog(new ECommercePlatformApiLog
            {
                CreateDate = DateTime.Now,
                LogType = (int)LogLevel.Error,
                Msg = msg,
                PlatformType = order.PlatformType,
                OrderNo = order.OrderNo,
                MallSapCode = order.MallSapCode
            });
        }
        #endregion

        #region 外部接口日志
        /// <summary>
        /// 外部接口推送日志
        /// </summary>
        /// <param name="objExternalInterfaceLog"></param>
        public static void WriteExternalInterfaceLog(ExternalInterfaceLog objExternalInterfaceLog)
        {
            using (var db = new logEntities())
            {
                try
                {
                    db.ExternalInterfaceLog.Add(objExternalInterfaceLog);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        #endregion
    }

    #region enum
    public enum OperationLogType
    {
        /// <summary>
        /// 添加
        /// </summary>
        Insert = 1,
        /// <summary>
        /// 编辑
        /// </summary>
        Update = 2,
        /// <summary>
        /// 删除
        /// </summary>
        Delete = 3
    }
    #endregion
}