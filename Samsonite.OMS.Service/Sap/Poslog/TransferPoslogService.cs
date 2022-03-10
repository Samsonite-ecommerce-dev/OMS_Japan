using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using Samsonite.OMS.DTO.Sap;
using Samsonite.Utility.FTP;
using Samsonite.Utility.Common;

namespace Samsonite.OMS.Service.Sap.Poslog
{
    /// <summary>
    /// Poslog数据下载
    /// </summary>
    public class TransferPoslogService
    {
        //#region 生成Poslog(ZKA/ZKB)
        ///// <summary>
        ///// 获取ZKA记录
        ///// </summary>
        ///// <param name="mallSapCode"></param>
        ///// <returns></returns>
        //public static List<TransferPosLog> GetZKAPoslogs(string mallSapCode)
        //{
        //    using (ebEntities db = new ebEntities())
        //    {
        //        //读取待发表
        //        var orderQuery = from od in db.View_OrderDetail
        //                         join sulw in db.SapUploadLogWait.Where(p => p.MallSapCode == mallSapCode && p.LogType == (int)SapLogType.ZKA && p.Status == 0) on new { od.OrderNo, od.SubOrderNo } equals new { sulw.OrderNo, sulw.SubOrderNo }
        //                         select new TransferPosLogItem()
        //                         {
        //                             LogType = SapLogType.ZKA,
        //                             StoreCode = od.MallSapCode,
        //                             OrderId = od.OrderId,
        //                             OrderNo = od.OrderNo,
        //                             SubOrderNo = od.SubOrderNo,
        //                             OrderType = od.OrderType,
        //                             WaitID = sulw.Id,
        //                             TransferStoreID = sulw.StoreId,
        //                             TransferFrom = sulw.TransferFrom,
        //                             TransferTo = sulw.TransferTo,
        //                             SKU = od.SKU,
        //                             Quantity = od.Quantity,
        //                             TranferDate = sulw.BusinessDate
        //                         };


        //        List<TransferPosLogItem> itemDtos = orderQuery.AsNoTracking().ToList();

        //        var tranferLogs = ConvertToLog(itemDtos, SapLogType.ZKA);
        //        return tranferLogs;
        //    }
        //}

        ///// <summary>
        ///// 获取ZKB记录
        ///// </summary>
        ///// <param name="mallSapCode"></param>
        ///// <returns></returns>
        //public static List<TransferPosLog> GetZKBPoslogs(string mallSapCode)
        //{
        //    using (ebEntities db = new ebEntities())
        //    {
        //        //读取待发表
        //        var orderQuery = from od in db.View_OrderDetail
        //                         join sulw in db.SapUploadLogWait.Where(p => p.MallSapCode == mallSapCode && p.LogType == (int)SapLogType.ZKB && p.Status == 0) on new { od.OrderNo, od.SubOrderNo } equals new { sulw.OrderNo, sulw.SubOrderNo }
        //                         select new TransferPosLogItem()
        //                         {
        //                             LogType = SapLogType.ZKB,
        //                             StoreCode = od.MallSapCode,
        //                             OrderId = od.OrderId,
        //                             OrderNo = od.OrderNo,
        //                             SubOrderNo = od.SubOrderNo,
        //                             OrderType = od.OrderType,
        //                             WaitID = sulw.Id,
        //                             TransferStoreID = sulw.StoreId,
        //                             TransferFrom = sulw.TransferFrom,
        //                             TransferTo = sulw.TransferTo,
        //                             SKU = od.SKU,
        //                             Quantity = od.Quantity,
        //                             TranferDate = sulw.BusinessDate
        //                         };


        //        List<TransferPosLogItem> itemDtos = orderQuery.AsNoTracking().ToList();

        //        var tranferLogs = ConvertToLog(itemDtos, SapLogType.ZKB);
        //        return tranferLogs;
        //    }
        //}

        ///// <summary>
        ///// 转换对象
        ///// </summary>
        ///// <param name="itemDtos"></param>
        ///// <param name="sapLogType"></param>
        ///// <returns></returns>
        //private static List<TransferPosLog> ConvertToLog(List<TransferPosLogItem> itemDtos, SapLogType sapLogType)
        //{
        //    List<TransferPosLog> logs = new List<TransferPosLog>();

        //    List<string> skus = itemDtos.GroupBy(p => p.SKU).Select(o => o.Key).ToList();

        //    using (ebEntities db = new ebEntities())
        //    {
        //        List<Product> products = db.Product.Where(p => skus.Contains(p.SKU)).ToList();
        //        //根据订单和转移店铺ID进行分组
        //        var storeGroups = itemDtos.GroupBy(t => new { t.OrderNo, t.TransferStoreID });
        //        foreach (var detailGroup in storeGroups)
        //        {
        //            var items = detailGroup.ToList();
        //            //如果没有相关订单就继续循环
        //            if (items.Count > 0)
        //            {
        //                var first = items.First();

        //                //子产品列表
        //                List<TransferPosLog.TransferItem> transferItems = (from item in items
        //                                                                   join p in products on item.SKU equals p.SKU
        //                                                                   select new TransferPosLog.TransferItem()
        //                                                                   {
        //                                                                       WaitID = item.WaitID,
        //                                                                       Material = p.Material,
        //                                                                       Grid = p.GdVal,
        //                                                                       Ean = p.EAN,
        //                                                                       SKU = item.SKU,
        //                                                                       StockCategory = PoslogConfig.STOCK_CATEGROY,
        //                                                                       Qty = item.Quantity,
        //                                                                       Field1 = CreateReferenceNumber(first.LogType, first.WaitID, first.OrderNo)
        //                                                                   }).ToList();

        //                TransferPosLog log = new TransferPosLog()
        //                {
        //                    LogType = first.LogType,
        //                    OrderNo = first.OrderNo,
        //                    SubOrderNo = first.SubOrderNo,
        //                    StoreId = first.StoreCode,
        //                    TransferStoreID = first.TransferStoreID,
        //                    TransferFrom = first.TransferFrom,
        //                    TransferTo = first.TransferTo,
        //                    //关联编号
        //                    ReferenceNumber = CreateReferenceNumber(first.LogType, first.WaitID, first.OrderNo),
        //                    BusinessDate = first.TranferDate,
        //                    TransferItems = transferItems
        //                };
        //                logs.Add(log);
        //            }
        //        }
        //    }
        //    return logs;
        //}
        //#endregion

        //#region 生成文件(ZKA/ZKB)
        ///// <summary>
        ///// 生成到Sap 
        ///// </summary>
        ///// <param name="logs"></param>
        //public static List<PoslogUploadResult> UploadTransferPoslog(List<TransferPosLog> logs)
        //{
        //    List<PoslogUploadResult> result = new List<PoslogUploadResult>();
        //    //ftp配置
        //    var ftpConfig = PoslogConfig.TransferFtpConfig;
        //    var savePath = AppDomain.CurrentDomain.BaseDirectory + ftpConfig.LocalSavePath + "/" + DateTime.Now.ToString("yyyy-MM") + "/" + DateTime.Now.ToString("yyyyMMdd");
        //    if (!Directory.Exists(savePath))
        //    {
        //        Directory.CreateDirectory(savePath);
        //    }

        //    List<SapLogDto> saplogs = new List<SapLogDto>();

        //    var storeGroups = logs.GroupBy(o => new { o.LogType, o.TransferStoreID, o.OrderNo });
        //    //文件需要分成,不同店铺,不同日期一个file
        //    foreach (var storeGroup in storeGroups)
        //    {
        //        string dateTime = string.Empty;
        //        //按照BusinessDate分文件生成
        //        var dateGroup = storeGroup.GroupBy(t => t.BusinessDate.ToString("yyyy-MM-dd"));
        //        foreach (var group in dateGroup)
        //        {
        //            //按订单完成时间来划分
        //            if (@group != null)
        //            {
        //                dateTime = @group.Key;
        //            }
        //            var fileLogs = CreateTransfersFile(VariableHelper.SaferequestTime(dateTime), storeGroup.Key.LogType, storeGroup.Key.TransferStoreID, storeGroup.Key.OrderNo, @group.ToArray(), savePath);
        //            saplogs.AddRange(fileLogs);
        //            //休眠,防止生成重复文件名
        //            Thread.Sleep(1000);
        //        }
        //    }
        //    //上传poslog到Sap
        //    var ftpInfo = ftpConfig.Ftp;
        //    SFTPHelper helper = new SFTPHelper(ftpInfo.FtpServerIp, ftpInfo.Port, ftpInfo.UserId, ftpInfo.Password);
        //    List<string> logfiles = new List<string>();
        //    foreach (var log in saplogs)
        //    {
        //        logfiles.Add(log.SapUploadLog.FilePath);
        //    }
        //    string _ftpFilePath = $"{ftpInfo.FtpFilePath}{ftpConfig.RemotePath}";
        //    var putResult = FtpService.SendXMLTosFtp(helper, logfiles, _ftpFilePath);
        //    //保存结果
        //    foreach (var log in saplogs)
        //    {
        //        var r = putResult.FirstOrDefault(p => p.FilePath == log.SapUploadLog.FilePath);
        //        if (r != null)
        //        {
        //            if (r.Result)
        //            {
        //                //如果上传成功
        //                foreach (var detail in log.Details)
        //                {
        //                    detail.Key.Status = (int)SapState.ToSap;
        //                    //删除待发表信息
        //                    DeleteSendedPoslog(detail.Value);
        //                }
        //            }
        //            else
        //            {
        //                //如果上传失败
        //                foreach (var detail in log.Details)
        //                {
        //                    detail.Key.Status = (int)SapState.Error;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            //如果上传失败
        //            foreach (var detail in log.Details)
        //            {
        //                detail.Key.Status = (int)SapState.Error;
        //            }
        //        }
        //        //返回信息
        //        foreach (var detail in log.Details)
        //        {
        //            result.Add(new PoslogUploadResult()
        //            {
        //                OrderNo = detail.Key.OrderNo,
        //                SubOrderNo = detail.Key.SubOrderNo,
        //                MallSapCode = detail.Key.MallStoreCode,
        //                LogType = detail.Key.LogType,
        //                Status = detail.Key.Status
        //            });
        //        }
        //    }
        //    //SAP生成日志
        //    PoslogService.WriteSapLog(saplogs);
        //    return result;
        //}

        ///// <summary>
        ///// 按时间生成Transfers文件
        ///// </summary>
        ///// <param name="date"></param>
        ///// <param name="sapLogType"></param>
        ///// <param name="transferStoreCode"></param>
        ///// <param name="orderNo"></param>
        ///// <param name="groupData"></param>
        ///// <param name="savePath"></param>
        ///// <returns></returns>
        //private static List<SapLogDto> CreateTransfersFile(DateTime date, SapLogType sapLogType, string transferStoreCode, string orderNo, TransferPosLog[] groupData, string savePath)
        //{
        //    List<SapLogDto> sapLogs = new List<SapLogDto>();

        //    //每页大小  
        //    const int pageSize = 100;

        //    //总页数  
        //    int pageCount = (int)Math.Ceiling((decimal)groupData.Length / pageSize);

        //    //注意:每个File 最多只生成 100个Transfers,超过100就另外单独一个File
        //    for (int pageNum = 0; pageNum < pageCount; pageNum++)
        //    {
        //        var pageData = groupData.Skip(pageNum * pageSize).Take(pageSize).ToList();

        //        SapUploadLog uploadLog = new SapUploadLog
        //        {
        //            CreateDate = DateTime.Now,
        //            MallSapCode = transferStoreCode,
        //            FileType = "xml",
        //            TransactionDate = date.ToString("yyyy-MM-dd"),
        //            LogType = (int)sapLogType,
        //            TotalCount = groupData.Length
        //        };

        //        var dateTime = new DateTime(date.Year, date.Month, date.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

        //        IDictionary<SapUploadLogDetail, long> details = new Dictionary<SapUploadLogDetail, long>();
        //        savePath = savePath.EndsWith(@"\") || savePath.EndsWith("//") || savePath.EndsWith("/") ? savePath : savePath + @"\";
        //        string fileName = savePath + $"Transfer_{sapLogType.ToString()}_{transferStoreCode}_{orderNo}_{dateTime:yyyyMMddHHmmss}_{pageNum + 1}.xml";

        //        StringBuilder builder = new StringBuilder();
        //        //builder.AppendLine("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");
        //        builder.AppendLine("<Transfers>");
        //        foreach (var log in pageData)
        //        {
        //            foreach (var item in log.TransferItems)
        //            {
        //                details.Add(new SapUploadLogDetail()
        //                {
        //                    LogType = (int)log.LogType,
        //                    MallStoreCode = log.StoreId,
        //                    OrderNo = log.OrderNo,
        //                    SubOrderNo = log.SubOrderNo,
        //                    UploadNo = log.ReferenceNumber,
        //                    TransferStoreCode = log.TransferStoreID,
        //                    Message = $"Transfer From:{log.TransferFrom},Transfer To:{log.TransferTo}",
        //                    Status = (int)SapState.ToSap,
        //                    CreateDate = DateTime.Now,
        //                    IsAppendShippment = false,
        //                    IsAppendSurcharge = false
        //                },
        //                item.WaitID
        //                ); ;
        //            }
        //            builder.Append(BuildTransferXml(log));
        //        }
        //        builder.AppendLine("</Transfers>");
        //        File.WriteAllText(fileName, builder.ToString());

        //        //sap log
        //        uploadLog.FilePath = fileName;
        //        SapLogDto dto = new SapLogDto
        //        {
        //            SapUploadLog = uploadLog,
        //            Details = details
        //        };
        //        sapLogs.Add(dto);
        //    }
        //    return sapLogs;
        //}
        //#endregion

        //#region 转换成Transaction(ZKA/ZKB)对象
        ///// <summary>
        ///// 创建Transfer的XML文件
        ///// </summary>
        ///// <param name="log"></param>
        ///// <returns></returns>
        //private static string BuildTransferXml(TransferPosLog log)
        //{
        //    DateTime businessDate = log.BusinessDate;
        //    StringBuilder xmlBuilder = new StringBuilder();
        //    string itemType = GetTransferItemsType(log.LogType);
        //    xmlBuilder.AppendLine($"<Transfer Type=\"{itemType}\">");
        //    xmlBuilder.AppendLine($"    <ReferenceNumber>{log.ReferenceNumber}</ReferenceNumber>");
        //    xmlBuilder.AppendLine($"    <StoreId>{log.TransferStoreID}</StoreId>");
        //    xmlBuilder.AppendLine($"    <BusinessDate>{businessDate:yyyy-MM-ddTHH:mm:ss}</BusinessDate>");//必须
        //    if (log.TransferItems.Any())
        //    {
        //        xmlBuilder.AppendLine("    <Items>");
        //        foreach (var item in log.TransferItems)
        //        {
        //            xmlBuilder.AppendLine("        <Item>");
        //            xmlBuilder.AppendLine($"           <Material>{item.Material}</Material>");
        //            xmlBuilder.AppendLine($"           <Grid>{item.Grid}</Grid>");
        //            xmlBuilder.AppendLine($"           <Ean>{item.Ean}</Ean>");
        //            xmlBuilder.AppendLine($"           <StockCategory>{item.StockCategory}</StockCategory>");
        //            xmlBuilder.AppendLine($"           <Qty>{item.Qty}</Qty>");
        //            xmlBuilder.AppendLine($"           <Field1>{item.Field1}</Field1>");
        //            xmlBuilder.AppendLine("        </Item>");
        //        }
        //        xmlBuilder.AppendLine("    </Items>");
        //    }
        //    xmlBuilder.AppendLine("</Transfer>");
        //    return xmlBuilder.ToString();
        //}
        //#endregion

        //#region 函数
        //private static string GetTransferItemsType(SapLogType objSapLogType)
        //{
        //    string result = string.Empty;
        //    switch (objSapLogType)
        //    {
        //        case SapLogType.ZKA:
        //            result = "Pickup";
        //            break;
        //        case SapLogType.ZKB:
        //            result = "Fillup";
        //            break;
        //        default:
        //            result = "";
        //            break;
        //    }
        //    return result;
        //}

        ///// <summary>
        ///// 创建唯一关联编号
        ///// </summary>
        ///// <param name="sapLogType"></param>
        ///// <param name="waitID"></param>
        ///// <param name="orderNo"></param>
        ///// <returns></returns>
        //private static string CreateReferenceNumber(SapLogType sapLogType, long waitID, string orderNo)
        //{
        //    string _result = string.Empty;
        //    //ZKA/ZKB+待发表ID+订单号
        //    _result = $"{orderNo}_{sapLogType.ToString()}{waitID}";
        //    return _result;
        //}

        ///// <summary>
        ///// 更新待发表状态
        ///// </summary>
        ///// <param name="waitID"></param>
        //private static void DeleteSendedPoslog(long waitID)
        //{
        //    using (var db = new ebEntities())
        //    {
        //        db.Database.ExecuteSqlCommand("update SapUploadLogWait set status=1 where Id={0} and status=0", waitID);
        //    }
        //}
        //#endregion
    }
}