using System;
using System.Linq;
using System.Collections.Generic;

using Samsonite.OMS.Database;

namespace Samsonite.OMS.Service
{
    public class WarehouseService
    {
        /// <summary>
        /// 获取默认仓库信息
        /// </summary>
        /// <returns></returns>
        public static WarehouseInfo GetDefaultWarehouse()
        {
            using (var db = new ebEntities())
            {
                return db.WarehouseInfo.Where(p => p.IsDefault).FirstOrDefault();
            }
        }

        /// <summary>
        /// 获取仓库信息
        /// </summary>
        /// <param name="objID"></param>
        /// <returns></returns>
        public static WarehouseInfo GetWarehouse(int objID)
        {
            using (var db = new ebEntities())
            {
                return db.WarehouseInfo.Where(p => p.ID == objID).FirstOrDefault();
            }
        }
    }
}