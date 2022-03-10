using System;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.Database;

namespace Samsonite.OMS.Service
{
    public class FunctionService
    {
        /// <summary>
        /// 返回栏目集合
        /// </summary>
        /// <returns></returns>
        public static List<SysFunctionGroup> GetFunctionGroupObject()
        {
            using (var db = new ebEntities())
            {
                return db.SysFunctionGroup.Where(p => p.Parentid == 0).OrderBy(p => p.Rootid).ToList();
            }
        }

        /// <summary>
        /// 返回功能集合
        /// </summary>
        /// <returns></returns>
        public static List<SysFunction> GetFunctionObject()
        {
            using (var db = new ebEntities())
            {
                return db.SysFunction.OrderBy(p => p.Groupid).OrderBy(p => p.SeqNumber).ToList();
            }
        }

        /// <summary>
        /// 返回角色组集合
        /// </summary>
        /// <returns></returns>
        public static List<SysRole> GetRoleObject()
        {
            using (var db = new ebEntities())
            {
                return db.SysRole.OrderBy(p => p.SeqNumber).ToList();
            }
        }
    }
}