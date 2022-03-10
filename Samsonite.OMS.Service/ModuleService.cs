using System;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.Database;

namespace Samsonite.OMS.Service
{
    public class ModuleService
    {
        /// <summary>
        /// 返回服务集合
        /// </summary>
        /// <returns></returns>
        public static List<ServiceModuleInfo> GetModuleObject()
        {
            using (var db = new ebEntities())
            {
                return db.ServiceModuleInfo.OrderBy(p => p.SortID).ToList();
            }
        }
    }
}