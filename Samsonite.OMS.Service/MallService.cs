using System;
using System.Linq;
using System.Collections.Generic;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;

namespace Samsonite.OMS.Service
{
    public class MallService
    {
        /// <summary>
        /// 获取店铺列表
        /// </summary>
        /// <returns></returns>
        public static List<Mall> GetMallOption()
        {
            using (var db = new ebEntities())
            {
                return db.Mall.Where(p => p.IsUsed).OrderBy(p => p.Id).OrderBy(p => p.SortID).ToList();
            }
        }

        /// <summary>
        /// 获取全部店铺列表
        /// </summary>
        /// <returns></returns>
        public static List<Mall> GetAllMallOption()
        {
            using (var db = new ebEntities())
            {
                return db.Mall.OrderBy(p => p.Id).ThenBy(p => p.SortID).ToList();
            }
        }

        /// <summary>
        /// 获取线上虚拟店铺列表
        /// </summary>
        /// <returns></returns>
        public static List<Mall> GetMallOption_OnLine()
        {
            using (var db = new ebEntities())
            {
                return db.Mall.Where(p => p.MallType == (int)MallType.OnLine && p.IsUsed).OrderBy(p => p.Id).ThenBy(p => p.SortID).ToList();
            }
        }

        /// <summary>
        /// 获取线下实体店铺列表
        /// </summary>
        /// <returns></returns>
        public static List<Mall> GetMallOption_OffLine()
        {
            using (var db = new ebEntities())
            {
                return db.Mall.Where(p => p.MallType == (int)MallType.OffLine && p.IsUsed).OrderBy(p => p.Id).ThenBy(p => p.SortID).ToList();
            }
        }

        /// <summary>
        /// 按照sapcode获取店铺集合
        /// </summary>
        /// <param name="objMallSapCodes"></param>
        /// <returns></returns>
        public static List<Mall> GetMalls(List<string> objMallSapCodes)
        {
            List<Mall> _result = new List<Mall>();
            if (objMallSapCodes.Count > 0)
            {
                using (var db = new ebEntities())
                {
                    _result = db.Mall.Where(p => objMallSapCodes.Contains(p.SapCode)).ToList();
                }
            }
            return _result;
        }

        /// <summary>
        /// 获取平台ID获取店铺集合
        /// </summary>
        /// <param name="objPlatformCode"></param>
        /// <returns></returns>
        public static List<Mall> GetMallsByPlatform(List<int> objPlatformCode)
        {
            List<Mall> _result = new List<Mall>();
            if (objPlatformCode.Count > 0)
            {
                using (var db = new ebEntities())
                {
                    _result = db.Mall.Where(p => objPlatformCode.Contains(p.PlatformCode)).ToList();
                }
            }
            return _result;
        }

        /// <summary>
        /// 获取店铺对象
        /// </summary>
        /// <param name="objID"></param>
        /// <returns></returns>
        public static Mall GetMall(int objID)
        {
            using (var db = new ebEntities())
            {
                return db.Mall.Where(p => p.Id == objID).SingleOrDefault();
            }
        }

        /// <summary>
        /// 获取店铺对象
        /// </summary>
        /// <param name="objSapCode"></param>
        /// <returns></returns>
        public static Mall GetMall(string objSapCode)
        {
            using (var db = new ebEntities())
            {
                return db.Mall.Where(p => p.SapCode == objSapCode).SingleOrDefault();
            }
        }

        /// <summary>
        /// 获取店铺名称
        /// </summary>
        /// <param name="objMallSapCode"></param>
        /// <returns></returns>
        public static string GetMallName(string objMallSapCode)
        {
            string _result = string.Empty;
            using (var db = new ebEntities())
            {
                Mall objMall = db.Mall.Where(p => p.SapCode == objMallSapCode).FirstOrDefault();
                if (objMall != null)
                {
                    _result = objMall.Name;
                }
            }
            return _result;
        }

        /// <summary>
        /// 获取店铺名称
        /// </summary>
        /// <param name="objMalls"></param>
        /// <param name="objMallSapCode"></param>
        /// <returns></returns>
        public static string GetMallName(List<Mall> objMalls, string objMallSapCode)
        {
            string _result = string.Empty;
            Mall objMall = objMalls.Where(p => p.SapCode == objMallSapCode).FirstOrDefault();
            if (objMall != null)
            {
                _result = objMall.Name;
            }
            return _result;
        }

        /// <summary>
        /// 获取虚拟仓库编号
        /// </summary>
        /// <param name="objMallSapCode"></param>
        /// <returns></returns>
        public static string GetMallVirtualWMSCode(string objMallSapCode)
        {
            string _result = string.Empty;
            using (var db = new ebEntities())
            {
                Mall objMall = db.Mall.Where(p => p.SapCode == objMallSapCode).FirstOrDefault();
                if (objMall != null)
                {
                    _result = objMall.VirtualWMSCode;
                }
            }
            return _result;
        }

        /// <summary>
        /// 获取平台列表
        /// </summary>
        /// <returns></returns>
        public static List<ECommercePlatform> GetPlatformOption()
        {
            using (var db = new ebEntities())
            {
                return db.ECommercePlatform.ToList();
            }
        }
    }
}