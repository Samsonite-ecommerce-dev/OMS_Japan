using System;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.Database;

namespace Samsonite.OMS.Service
{
    public class ExpressCompanyService
    {
        /// <summary>
        /// 返回集合
        /// </summary>
        /// <returns></returns>
        public static List<ExpressCompany> GetExpressCompanyObject()
        {
            using (var db = new ebEntities())
            {
                return db.ExpressCompany.Where(p => p.IsUsed).OrderBy(p => p.Id).OrderBy(p => p.SortID).ToList();
            }
        }

        /// <summary>
        /// 返回全部集合
        /// </summary>
        /// <returns></returns>
        public static List<ExpressCompany> GetALLExpressCompanyObject()
        {
            using (var db = new ebEntities())
            {
                return db.ExpressCompany.OrderBy(p => p.Id).OrderBy(p => p.SortID).ToList();
            }
        }

        /// <summary>
        /// 获取快递公司信息
        /// </summary>
        /// <param name="objValue"></param>
        /// <returns></returns>
        public static int GetExpressID(string objValue)
        {
            int _result = 0;
            using (var db = new ebEntities())
            {
                var objExpressCompany = db.ExpressCompany.Where(p => p.ExpressName == objValue || p.Code == objValue).FirstOrDefault();
                if (objExpressCompany != null)
                {
                    _result = objExpressCompany.Id;
                }
            }
            return _result;
        }

        /// <summary>
        /// 获取快递公司信息
        /// </summary>
        /// <param name="objID"></param>
        /// <returns></returns>
        public static ExpressCompany GetExpressCompany(int objID)
        {
            using (var db = new ebEntities())
            {
                return db.ExpressCompany.Where(p => p.Id == objID).SingleOrDefault();
            }
        }

        /// <summary>
        /// 获取快递公司信息
        /// </summary>
        /// <param name="objValue"></param>
        /// <returns></returns>
        public static ExpressCompany GetExpressCompany(string objValue)
        {
            using (var db = new ebEntities())
            {
                return db.ExpressCompany.Where(p => p.ExpressName == objValue || p.Code == objValue).FirstOrDefault();
            }
        }

        /// <summary>
        /// 获取快递信息
        /// </summary>
        /// <param name="objCompany"></param>
        /// <param name="objExpressNo"></param>
        /// <returns></returns>
        public static string GetExpressMessage(string objCompany, string objExpressNo)
        {
            string _result = string.Empty;
            if (!string.IsNullOrEmpty(objExpressNo))
            {
                _result = $"<i class=\"fa fa-archive color_info\"></i>{objCompany},{objExpressNo}";
            }
            return _result;
        }
    }
}