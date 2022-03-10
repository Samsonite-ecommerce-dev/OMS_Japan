using System;
using System.Linq;
using System.Collections.Generic;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;

namespace Samsonite.OMS.Service
{
    public class AreaService
    {
        /// <summary>
        /// 获取地区信息
        /// </summary>
        /// <param name="objCode"></param>
        /// <returns></returns>
        public static string GetAreaName(string objCode)
        {
            string _result = string.Empty;
            using (var db = new ebEntities())
            {
                BSArea objBSArea = db.BSArea.Where(p => p.Code == objCode).SingleOrDefault();
                if (objBSArea != null)
                {
                    _result = objBSArea.Name;
                }
            }
            return _result;
        }

        /// <summary>
        /// 获取地区编号
        /// </summary>
        /// <param name="objName"></param>
        /// <returns></returns>
        public static string GetAreaCode(string objName)
        {
            string _result = string.Empty;
            using (var db = new ebEntities())
            {
                BSArea objBSArea = db.BSArea.Where(p => p.Name == objName).FirstOrDefault();
                if (objBSArea != null)
                {
                    _result = objBSArea.Code;
                }
            }
            return _result;
        }

        /// <summary>
        /// 获取地区集合编号
        /// </summary>
        /// <param name="objArea"></param>
        /// <returns></returns>
        public static AreaDto GetAreaCode(AreaDto objArea)
        {
            AreaDto _result = new AreaDto();
            using (var db = new ebEntities())
            {
                List<BSArea> objBSArea_List = db.BSArea.ToList();
                BSArea objBSArea = new BSArea();
                //国家
                if (!string.IsNullOrEmpty(objArea.Country))
                {
                    objBSArea = objBSArea_List.Where(p => p.Name.Contains(objArea.Country) && p.AreaType == 1).FirstOrDefault();
                    if (objBSArea != null)
                    {
                        _result.Country = objBSArea.Code;
                    }
                    else
                    {
                        _result.Country = string.Empty;
                    }
                }
                //省
                if (!string.IsNullOrEmpty(objArea.Province))
                {

                    if (!string.IsNullOrEmpty(_result.Country))
                    {
                        objBSArea = objBSArea_List.Where(p => p.Name.Contains(objArea.Province) && p.ParentID == _result.Country && p.AreaType == 2).FirstOrDefault();
                    }
                    else
                    {
                        objBSArea = objBSArea_List.Where(p => p.Name.Contains(objArea.Province) && p.AreaType == 2).FirstOrDefault();
                    }
                    if (objBSArea != null)
                    {
                        _result.Province = objBSArea.Code;
                    }
                    else
                    {
                        _result.Province = string.Empty;
                    }
                }
                //市
                if (!string.IsNullOrEmpty(objArea.City))
                {

                    if (!string.IsNullOrEmpty(_result.Province))
                    {
                        objBSArea = objBSArea_List.Where(p => p.Name.Contains(objArea.City) && p.ParentID == _result.Province && p.AreaType == 3).FirstOrDefault();
                    }
                    else
                    {
                        objBSArea = objBSArea_List.Where(p => p.Name.Contains(objArea.City) && p.AreaType == 3).FirstOrDefault();
                    }
                    if (objBSArea != null)
                    {
                        _result.City = objBSArea.Code;
                    }
                    else
                    {
                        _result.City = string.Empty;
                    }
                }
                //区
                if (!string.IsNullOrEmpty(objArea.District))
                {
                    if (!string.IsNullOrEmpty(_result.City))
                    {
                        objBSArea = objBSArea_List.Where(p => p.Name.Contains(objArea.District) && p.ParentID == _result.City && p.AreaType == 4).FirstOrDefault();
                    }
                    else
                    {
                        objBSArea = objBSArea_List.Where(p => p.Name.Contains(objArea.District) && p.AreaType == 4).FirstOrDefault();
                    }
                    if (objBSArea != null)
                    {
                        _result.District = objBSArea.Code;
                    }
                    else
                    {
                        _result.District = string.Empty;
                    }
                }
            }
            return _result;
        }

        /// <summary>
        /// 获取某等级下的区域列表
        /// </summary>
        /// <param name="objCode"></param>
        /// <returns></returns>
        public static List<BSArea> GetAreaParentOption(string objCode)
        {
            using (var db = new ebEntities())
            {
                return db.BSArea.Where(p => p.ParentID == objCode && !p.IsDelete).ToList();
            }
        }

        /// <summary>
        /// 根据区域编码获取全地区信息
        /// </summary>
        /// <param name="objArea"></param>
        /// <returns></returns>
        public static string GetAreaMessage(AreaDto objArea)
        {
            string _result = string.Empty;
            string _Codes = string.Empty;
            if (!string.IsNullOrEmpty(objArea.Country))
            {
                _Codes += $",'{objArea.Country}'";
            }
            if (!string.IsNullOrEmpty(objArea.Province))
            {
                _Codes += $",'{objArea.Province}'";
            }
            if (!string.IsNullOrEmpty(objArea.City))
            {
                _Codes += $",'{objArea.City}'";
            }
            if (!string.IsNullOrEmpty(objArea.District))
            {
                _Codes += $",'{objArea.District}'";
            }
            //读取区域信息
            if (!string.IsNullOrEmpty(_Codes))
            {
                _Codes = _Codes.Substring(1);
                using (var db = new ebEntities())
                {
                    List<BSArea> objBSArea_List = db.Database.SqlQuery<BSArea>("select * from BSArea where Code in (" + _Codes + ") order by AreaType asc").ToList();
                    foreach (var _o in objBSArea_List)
                    {
                        if (string.IsNullOrEmpty(_result))
                        {
                            _result += _o.Name;
                        }
                        else
                        {
                            _result += $",{_o.Name}";
                        }
                    }
                }
            }
            return _result;
        }

        /// <summary>
        /// 根据区域名称获取全地区信息
        /// </summary>
        /// <param name="objArea"></param>
        /// <returns></returns>
        public static string JoinAreaMessage(AreaDto objArea)
        {
            string _result = string.Empty;
            if (!string.IsNullOrEmpty(objArea.Country))
            {
                _result += $",{objArea.Country}";
            }
            if (!string.IsNullOrEmpty(objArea.Province))
            {
                _result += $",{objArea.Province}";
            }
            if (!string.IsNullOrEmpty(objArea.City))
            {
                _result += $",{objArea.City}";
            }
            if (!string.IsNullOrEmpty(objArea.District))
            {
                _result += $",{objArea.District}";
            }
            if (!string.IsNullOrEmpty(_result))
            {
                _result = _result.Substring(1);
            }
            return _result;
        }
    }
}