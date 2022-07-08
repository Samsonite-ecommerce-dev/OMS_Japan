using System;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.Database;
using Samsonite.OMS.Service.Sap.Poslog.Models;

namespace Samsonite.OMS.Service.Sap.Poslog
{
    /// <summary>
    /// Poslog数据下载
    /// </summary>
    public class PoslogService
    {
        /// <summary>
        ///  上传Poslog到Sap(KE/KR/ZKA/ZKB)
        /// </summary>
        /// <param name="begin">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <param name="mallSapCode">店铺集合</param>
        /// <returns></returns>
        public static List<PoslogUploadResult> UploadPosLogs(DateTime begin, DateTime end, string mallSapCode)
        {
            List<PoslogUploadResult> result = new List<PoslogUploadResult>();
            List<TransactionPosLog> logs = new List<TransactionPosLog>();

            var rq1 = DateTime.Parse(begin.ToString("yyyy-MM-dd 00:00:00"));
            var rq2 = DateTime.Parse(end.ToString("yyyy-MM-dd 23:59:59"));

            if (PoslogConfig.IsUploadPoslog)
            {
                ////---------------------------Transfer(ZKA/ZKB)---------------------------
                //var resultTransfers = UploadTransferPosLogs(mallSapCode);
                //result.AddRange(resultTransfers);
                //---------------------------Transaction(KE/KR)---------------------------
                var resultTransactions = UploadTransactionPosLogs(begin, end, mallSapCode);
                result.AddRange(resultTransactions);
            }
            return result;
        }

        /// <summary>
        /// KE/KR
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="mallSapCode"></param>
        /// <returns></returns>
        private static List<PoslogUploadResult> UploadTransactionPosLogs(DateTime begin, DateTime end, string mallSapCode)
        {
            List<PoslogUploadResult> result = new List<PoslogUploadResult>();
            List<TransactionPosLog> logs = new List<TransactionPosLog>();

            var rq1 = DateTime.Parse(begin.ToString("yyyy-MM-dd 00:00:00"));
            var rq2 = DateTime.Parse(end.ToString("yyyy-MM-dd 23:59:59"));

            if (PoslogConfig.IsUploadPoslog)
            {
                //获取销售(KE)的poslog记录
                var kePoglogs = TransactionPoslogService.GetKePoslogs(mallSapCode, rq1, rq2);
                logs.AddRange(kePoglogs);
                //获取退货(KR)的poslog记录
                var krPoslogs = TransactionPoslogService.GetKrPoslog(mallSapCode, rq1, rq2);
                logs.AddRange(krPoslogs);
                //上传KE/KR
                result = TransactionPoslogService.UploadTransactionPoslog(logs);
            }
            return result;
        }

        ///// <summary>
        ///// ZKA/ZKB
        ///// </summary>
        ///// <param name="mallSapCode"></param>
        ///// <returns></returns>
        //private static List<PoslogUploadResult> UploadTransferPosLogs(string mallSapCode)
        //{
        //    List<PoslogUploadResult> result = new List<PoslogUploadResult>();
        //    List<TransferPosLog> logs = new List<TransferPosLog>();

        //    if (PoslogConfig.IsUploadPoslog)
        //    {
        //        //获取库存转移(ZKA)的poslog记录
        //        var zkaPoglogs = TransferPoslogService.GetZKAPoslogs(mallSapCode);
        //        logs.AddRange(zkaPoglogs);
        //        //获取库存转移(ZKB)的poslog记录
        //        var zkbPoslogs = TransferPoslogService.GetZKBPoslogs(mallSapCode);
        //        logs.AddRange(zkbPoslogs);
        //        //上传KE/KR
        //        result = TransferPoslogService.UploadTransferPoslog(logs);
        //    }
        //    return result;
        //}

        #region 插入Poslog待发表
        /// <summary>
        /// 插入KE/KR
        /// </summary>
        /// <param name="objSapUploadLogWait"></param>
        public static void AddTransactionPoslog(SapUploadLogWait objSapUploadLogWait)
        {
            using (var db = new ebEntities())
            {
                var o = db.SapUploadLogWait.Where(p => p.LogType == objSapUploadLogWait.LogType && p.OrderNo == objSapUploadLogWait.OrderNo && p.SubOrderNo == objSapUploadLogWait.SubOrderNo && objSapUploadLogWait.MallSapCode == objSapUploadLogWait.MallSapCode && p.StoreId == objSapUploadLogWait.StoreId).FirstOrDefault();
                if (o == null)
                {
                    db.SapUploadLogWait.Add(objSapUploadLogWait);
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// 插入ZKA/ZKB
        /// </summary>
        /// <param name="objSapUploadLogWait"></param>
        public static void AddTransferPoslog(SapUploadLogWait objSapUploadLogWait)
        {
            using (var db = new ebEntities())
            {
                var o = db.SapUploadLogWait.Where(p => p.LogType == objSapUploadLogWait.LogType && p.OrderNo == objSapUploadLogWait.OrderNo && p.SubOrderNo == objSapUploadLogWait.SubOrderNo && objSapUploadLogWait.MallSapCode == objSapUploadLogWait.MallSapCode && p.StoreId == objSapUploadLogWait.StoreId).FirstOrDefault();
                if (o == null)
                {
                    db.SapUploadLogWait.Add(objSapUploadLogWait);
                    db.SaveChanges();
                }
            }
        }
        #endregion

        #region 写日志
        /// <summary>
        /// 写日志
        /// </summary>
        public static void WriteSapLog(List<SapLogDto> logs)
        {
            using (ebEntities db = new ebEntities())
            {
                var trans = db.Database.BeginTransaction();
                foreach (var log in logs)
                {
                    db.SapUploadLog.Add(log.SapUploadLog);
                    db.SaveChanges();
                    foreach (var detail in log.Details)
                    {
                        detail.LogId = log.SapUploadLog.Id;
                    }
                    db.SapUploadLogDetail.AddRange(log.Details);
                }
                db.SaveChanges();
                trans.Commit();
            }
        }
        #endregion
    }
}