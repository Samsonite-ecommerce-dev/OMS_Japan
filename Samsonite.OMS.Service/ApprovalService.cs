using System;
using System.Linq;
using System.Collections.Generic;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service.AppConfig;

namespace Samsonite.OMS.Service
{
    public class ApprovalService
    {
        public static List<ApprovalInfo> ApprovalOption()
        {
            List<ApprovalInfo> _result = new List<ApprovalInfo>();
            using (var db = new ebEntities())
            {
                _result = db.ApprovalInfo.ToList();
            }
            return _result;
        }

        /// <summary>
        /// 是否完成审核
        /// </summary>
        /// <param name="objApprovalType">审核流程类型</param>
        /// <param name="objID">关联表ID</param>
        /// <param name="objConfig">配置信息</param>
        /// <returns></returns>
        public static bool IsApproval(ApprovalType objApprovalType, long objID, List<string> objConfig)
        {
            bool _result = true;
            using (var db = new ebEntities())
            {
                if (objConfig.Count > 0)
                {
                    List<ApprovalRecord> objApprovalRecord_List = db.ApprovalRecord.Where(p => p.ApprovalProjectID == (int)objApprovalType && p.DetailID == objID).ToList();
                    foreach (string _str in objConfig)
                    {
                        if (objApprovalRecord_List.Where(p => p.ApprovalIdentify == _str).SingleOrDefault() == null)
                        {
                            _result = false;
                            break;
                        }
                    }
                }

            }
            return _result;
        }
    }
}