using System;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.DTO;

namespace OMS.App.Helper
{
    public class CompanyHelper
    {
        #region 公司分类
        /// <summary>
        /// 公司类型集合
        /// </summary>
        /// <returns></returns>
        private static List<DefineEnum> CompanyTypeReflect()
        {
            List<DefineEnum> _result = new List<DefineEnum>();
            _result.Add(new DefineEnum() { ID = (int)CompanyType.SAM, Display = "Samsonite" });
            _result.Add(new DefineEnum() { ID = (int)CompanyType.TUMI, Display = "Tumi" });
            return _result;
        }

        /// <summary>
        /// 公司类型列表
        /// </summary>
        /// <returns></returns>
        public static List<object[]> CompanyTypeObject()
        {
            List<object[]> _result = new List<object[]>();
            foreach (var _o in CompanyTypeReflect())
            {
                _result.Add(new object[] { _o.ID, _o.Display });
            }
            return _result;
        }

        /// <summary>
        /// 公司类型显示值
        /// </summary>
        /// <param name="objStatus"></param>
        /// <param name="objCss"></param>
        /// <returns></returns>
        public static string GetCompanyTypeDisplay(int objStatus, bool objCss = false)
        {
            string _result = string.Empty;
            DefineEnum _O = CompanyTypeReflect().Where(p => p.ID == objStatus).SingleOrDefault();
            if (_O != null)
            {
                if (objCss)
                {
                    _result = string.Format("<label class=\"{0}\">{1}</label>", _O.Css, _O.Display);
                }
                else
                {
                    _result = _O.Display;
                }
            }
            return _result;
        }
        #endregion
    }
}