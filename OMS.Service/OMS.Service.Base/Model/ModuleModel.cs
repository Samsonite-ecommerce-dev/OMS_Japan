using System;
using System.Collections.Generic;

namespace OMS.Service.Base.Model
{
    /// <summary>
    /// 服务对象
    /// </summary>
    public class ModuleModel
    {
        public int ModuleID { get; set; }

        public IModule ModuleInstance { get; set; }
    }

    public class InitializationResponse
    {
        public bool IsInit { get; set; }

        public List<ModuleModel> Modules { get; set; }
    }
}
