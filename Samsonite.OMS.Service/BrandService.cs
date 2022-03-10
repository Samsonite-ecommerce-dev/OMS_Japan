using System;
using System.Linq;
using System.Collections.Generic;

using Samsonite.OMS.Database;
using Samsonite.Utility.Common;

namespace Samsonite.OMS.Service
{
    public class BrandService
    {
        /// <summary>
        /// 获取一级品牌集合
        /// </summary>
        /// <returns></returns>
        public static List<string> GetBrandOption()
        {
            List<string> _result = new List<string>();
            using (var db = new ebEntities())
            {
                _result = db.Database.SqlQuery<string>("select BrandName from Brand where IsParent=0 and IsLock=0").ToList();
            }
            return _result;
        }

        /// <summary>
        /// 获取一级品牌列表
        /// </summary>
        /// <returns></returns>
        public static List<object[]> GetBrands()
        {
            List<object[]> _result = new List<object[]>();
            using (var db = new ebEntities())
            {
                //品牌列表
                var objBrand_List = db.Brand.Where(p => p.ParentID == 0 && !p.IsLock).OrderBy(p => p.RootID).ToList();
                foreach (var _O in objBrand_List)
                {
                    _result.Add(new object[] { _O.ID, _O.BrandName });
                }
            }
            return _result;
        }

        /// <summary>
        /// 获取一级品牌列表
        /// </summary>
        /// <returns></returns>
        public static List<Brand> GetBrandLists()
        {
            List<Brand> _result = new List<Brand>();
            using (var db = new ebEntities())
            {
                _result = db.Brand.Where(p => p.ParentID == 0 && !p.IsLock).OrderBy(p => p.RootID).ToList();
            }
            return _result;
        }

        /// <summary>
        /// 获取父品牌列表
        /// </summary>
        /// <returns></returns>
        public static List<object[]> GetParentBrandOption()
        {
            List<object[]> _result = new List<object[]>();
            using (var db = new ebEntities())
            {
                //品牌列表
                var objBrand_List = db.Brand.Where(p => p.IsParent && p.ParentID == 0).OrderBy(p => p.RootID).ToList();
                foreach (var _O in objBrand_List)
                {
                    _result.Add(new object[] { _O.ID, _O.BrandName });
                }
            }
            return _result;
        }

        /// <summary>
        /// 获取子品牌
        /// </summary>
        /// <param name="objBrandID"></param>
        /// <returns></returns>
        public static List<string> GetSons(int objBrandID)
        {
            List<string> _result = new List<string>();
            using (var db = new ebEntities())
            {
                //如果是合并品牌,则传递过来的是id值,需要获取下级brand集合
                Brand objBrand = db.Brand.Where(p => p.ID == objBrandID).SingleOrDefault();
                if (objBrand != null)
                {
                    //如果是合并品牌
                    if (objBrand.IsParent)
                    {
                        _result = db.Database.SqlQuery<string>("select BrandName from Brand where ParentID={0}", objBrandID).ToList();
                    }
                    else
                    {
                        _result.Add(objBrand.BrandName);
                    }
                }
            }
            return _result;
        }
    }
}