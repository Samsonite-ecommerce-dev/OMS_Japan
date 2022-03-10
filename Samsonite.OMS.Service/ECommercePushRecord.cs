using System;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;

namespace Samsonite.OMS.Service
{
    public class ECommercePushRecordService
    {
        /// <summary>
        /// 保存获取快递日志
        /// </summary>
        /// <param name="objECommercePushRecord"></param>
        /// <param name="objDB"></param>
        public static void SaveRequireDeliveryLog(ECommercePushRecord objECommercePushRecord, ebEntities objDB = null)
        {
            if (objDB == null) objDB = new ebEntities();
            try
            {
                objECommercePushRecord.PushType = (int)ECommercePushType.RequireTrackingCode;
                ECommercePushRecord objData = objDB.ECommercePushRecord.Where(p => p.PushType == (int)ECommercePushType.RequireTrackingCode && p.RelatedId == objECommercePushRecord.RelatedId).FirstOrDefault();
                if (objData != null)
                {
                    objData.PushType = objECommercePushRecord.PushType;
                    objData.RelatedTableName = objECommercePushRecord.RelatedTableName;
                    objData.RelatedId = objECommercePushRecord.RelatedId;
                    objData.PushMessage = objECommercePushRecord.PushMessage;
                    objData.PushResult = objECommercePushRecord.PushResult;
                    objData.PushResultMessage = objECommercePushRecord.PushResultMessage;
                    objData.PushCount = objData.PushCount + 1;
                    objData.IsDelete = objECommercePushRecord.IsDelete;
                    objData.EditTime = DateTime.Now;
                }
                else
                {
                    objDB.ECommercePushRecord.Add(objECommercePushRecord);
                }
                objDB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 保存推送快递日志
        /// </summary>
        /// <param name="objECommercePushRecord"></param>
        /// <param name="objDB"></param>
        public static void SavePushDeliveryLog(ECommercePushRecord objECommercePushRecord, ebEntities objDB = null)
        {
            if (objDB == null) objDB = new ebEntities();
            try
            {
                objECommercePushRecord.PushType = (int)ECommercePushType.PushTrackingCode;
                ECommercePushRecord objData = objDB.ECommercePushRecord.Where(p => p.PushType == (int)ECommercePushType.PushTrackingCode && p.RelatedId == objECommercePushRecord.RelatedId).FirstOrDefault();
                if (objData != null)
                {
                    objData.PushType = objECommercePushRecord.PushType;
                    objData.RelatedTableName = objECommercePushRecord.RelatedTableName;
                    objData.RelatedId = objECommercePushRecord.RelatedId;
                    objData.PushMessage = objECommercePushRecord.PushMessage;
                    objData.PushResult = objECommercePushRecord.PushResult;
                    objData.PushResultMessage = objECommercePushRecord.PushResultMessage;
                    objData.PushCount = objData.PushCount + 1;
                    objData.IsDelete = objECommercePushRecord.IsDelete;
                    objData.EditTime = DateTime.Now;
                }
                else
                {
                    objDB.ECommercePushRecord.Add(objECommercePushRecord);
                }
                objDB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 保存推送库存日志
        /// </summary>
        /// <param name="objECommercePushInventoryRecord"></param>
        /// <param name="objDB"></param>
        public static void SavePushInventoryLog(ECommercePushInventoryRecord objECommercePushInventoryRecord, ebEntities objDB = null)
        {
            if (objDB == null) objDB = new ebEntities();
            try
            {
                objECommercePushInventoryRecord.PushType = (int)ECommercePushType.PushInventory;
                objDB.ECommercePushInventoryRecord.Add(objECommercePushInventoryRecord);
                objDB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 批量保存推送库存日志
        /// </summary>
        /// <param name="objECommercePushInventoryRecordList"></param>
        /// <param name="objDB"></param>
        public static void SavePushInventoryLogs(List<ECommercePushInventoryRecord> objECommercePushInventoryRecordList, ebEntities objDB = null)
        {
            if (objDB == null) objDB = new ebEntities();
            try
            {
                foreach (var item in objECommercePushInventoryRecordList)
                {
                    item.PushType = (int)ECommercePushType.PushInventory;
                }
                objDB.ECommercePushInventoryRecord.AddRange(objECommercePushInventoryRecordList);
                objDB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 保存推送警告库存日志
        /// </summary>
        /// <param name="objECommercePushInventoryRecord"></param>
        /// <param name="objDB"></param>
        public static void SavePushWarningInventoryLog(ECommercePushInventoryRecord objECommercePushInventoryRecord, ebEntities objDB = null)
        {
            if (objDB == null) objDB = new ebEntities();
            try
            {
                objECommercePushInventoryRecord.PushType = (int)ECommercePushType.PushWarningInventory;
                objDB.ECommercePushInventoryRecord.Add(objECommercePushInventoryRecord);
                objDB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 批量保存推送警告库存日志
        /// </summary>
        /// <param name="objECommercePushInventoryRecordList"></param>
        /// <param name="objDB"></param>
        public static void SavePushWarningInventoryLogs(List<ECommercePushInventoryRecord> objECommercePushInventoryRecordList, ebEntities objDB = null)
        {
            if (objDB == null) objDB = new ebEntities();
            try
            {
                foreach (var item in objECommercePushInventoryRecordList)
                {
                    item.PushType = (int)ECommercePushType.PushWarningInventory;
                }
                objDB.ECommercePushInventoryRecord.AddRange(objECommercePushInventoryRecordList);
                objDB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 保存推送价格日志
        /// </summary>
        /// <param name="objECommercePushPriceRecord"></param>
        /// <param name="objDB"></param>
        public static void SavePushPriceLog(ECommercePushPriceRecord objECommercePushPriceRecord, ebEntities objDB = null)
        {
            if (objDB == null) objDB = new ebEntities();
            try
            {
                objECommercePushPriceRecord.PushType = (int)ECommercePushType.PushPrice;
                objDB.ECommercePushPriceRecord.Add(objECommercePushPriceRecord);
                objDB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 批量保存推送警告库存日志
        /// </summary>
        /// <param name="objECommercePushPriceRecordList"></param>
        /// <param name="objDB"></param>
        public static void SavePushPriceLogs(List<ECommercePushPriceRecord> objECommercePushPriceRecordList, ebEntities objDB = null)
        {
            if (objDB == null) objDB = new ebEntities();
            try
            {
                foreach (var item in objECommercePushPriceRecordList)
                {
                    item.PushType = (int)ECommercePushType.PushPrice;
                }
                objDB.ECommercePushPriceRecord.AddRange(objECommercePushPriceRecordList);
                objDB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
