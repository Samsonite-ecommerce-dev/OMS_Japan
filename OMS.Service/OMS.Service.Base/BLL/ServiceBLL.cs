using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using OMS.Service.Base.Model;
using OMS.Service.Base.Enum;

using Samsonite.OMS.Database;
using Samsonite.Utility.Common;

namespace OMS.Service.Base.BLL
{
    public class ServiceBLL
    {
        /// <summary>
        /// 创建映射对象
        /// </summary>
        /// <param name="objServiceModuleInfo"></param>
        /// <returns></returns>
        public static IModule CreateInstance(ServiceModuleInfo objServiceModuleInfo)
        {
            return (IModule)Assembly.Load(objServiceModuleInfo.ModuleAssembly).CreateInstance($"{objServiceModuleInfo.ModuleAssembly}.{objServiceModuleInfo.ModuleType}");
        }
    }
}
