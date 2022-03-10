using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.Utility.Common;
using System.IO;

namespace Samsonite.OMS.Service.HistoryRecord
{
    public class HistoryRecordService
    {
        /// <summary>
        /// 根据日期清理快递文档
        /// </summary>
        /// <returns></returns>
        public int ClearDeliveryDocument(DateTime obiTime)
        {
            int _result = 0;
            using (var db = new ebEntities())
            {
                List<Order> objOrder_List = db.Order.Where(p => DbFunctions.DiffDays(p.CreateDate, obiTime) == 0).ToList();
                List<string> objOrderNos = objOrder_List.Select(p => p.OrderNo).ToList();
                if (objOrderNos.Count > 0)
                {
                    //读取文件列表
                    List<DeliverysDocument> objDeliverysDocument_List = db.DeliverysDocument.Where(p => objOrderNos.Contains(p.OrderNo)).ToList();
                    foreach (var _O in objDeliverysDocument_List)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(_O.DocumentFile))
                            {
                                int i = _O.DocumentFile.IndexOf("Document/");
                                if (i > -1)
                                {
                                    string _path = $"{AppGlobalService.SITE_PHYSICAL_PATH}{_O.DocumentFile.Substring(i)}";
                                    if (File.Exists(_path))
                                    {
                                        File.Delete(_path);
                                        _result++;
                                    }
                                }
                                else
                                {
                                    throw new Exception("The path  is incorrect!");
                                }
                            }
                            else
                            {
                                throw new Exception("The path dose not exsit!");
                            }
                        }
                        catch { }
                    }
                }
            }
            return _result;
        }
    }
}
