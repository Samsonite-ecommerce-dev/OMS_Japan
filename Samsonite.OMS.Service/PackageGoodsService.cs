using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Samsonite.OMS.Database;
using Samsonite.Utility.Common;

namespace Samsonite.OMS.Service
{
    public class PackageGoodsService
    {
        /// <summary>
        /// 生成EAN
        /// </summary>
        /// <returns></returns>
        public static string CreateUPC()
        {

            using (var db = new ebEntities())
            {
                string _EAN = db.Database.SqlQuery<string>("select Max(EAN) from tj").FirstOrDefault();
                if (_EAN != null)
                {
                    _EAN = (VariableHelper.SaferequestInt64(_EAN) + 1).ToString();
                }
                else
                {
                    _EAN = "88800000000001";
                }
                return "SET_"+_EAN;
            }
        }
    }
}