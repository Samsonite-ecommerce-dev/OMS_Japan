using System;
using System.Linq;
using System.Collections.Generic;

using Samsonite.OMS.Database;

namespace Samsonite.OMS.Service
{
    public class StorageService
    {
        /// <summary>
        /// 获取仓库列表
        /// </summary>
        /// <returns></returns>
        public static List<StorageInfo> GetStorageOption()
        {
            using (var db = new ebEntities())
            {
                return db.StorageInfo.OrderBy(p => p.StorageID).ToList();
            }
        }
    }
}