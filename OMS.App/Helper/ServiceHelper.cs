using System;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.DTO;

namespace OMS.App.Helper
{
    public class ServiceHelper
    {
        #region 服务状态集合
        /// <summary>
        /// 服务状态集合
        /// </summary>
        /// <returns></returns>
        private static List<DefineEnum> ServiceStatusReflect()
        {
            List<DefineEnum> _result = new List<DefineEnum>();
            _result.Add(new DefineEnum() { ID = (int)ServiceStatus.Stop, Display = "停止中", Css = "color_danger" });
            _result.Add(new DefineEnum() { ID = (int)ServiceStatus.Runing, Display = "运行中", Css = "color_success" });
            _result.Add(new DefineEnum() { ID = (int)ServiceStatus.Pause, Display = "暂停中", Css = "color_warning" });
            return _result;
        }

        /// <summary>
        /// 服务状态列表
        /// </summary>
        /// <returns></returns>
        public static List<object[]> ServiceStatusObject()
        {
            List<object[]> _result = new List<object[]>();
            foreach (var _o in ServiceStatusReflect())
            {
                _result.Add(new object[] { _o.ID, _o.Display });
            }
            return _result;
        }

        /// <summary>
        /// 服务状态显示值
        /// </summary>
        /// <param name="objStatus"></param>
        /// <param name="objCss"></param>
        /// <returns></returns>
        public static string GetServiceStatusDisplay(int objStatus, bool objCss = false)
        {
            string _result = string.Empty;
            DefineEnum _O = ServiceStatusReflect().Where(p => p.ID == objStatus).SingleOrDefault();
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

        #region 操作类型集合
        /// <summary>
        /// 操作类型集合
        /// </summary>
        /// <returns></returns>
        private static List<object[]> JobTypeReflect()
        {
            List<object[]> _result = new List<object[]>();
            _result.Add(new object[] { (int)JobType.Start, "启动" });
            _result.Add(new object[] { (int)JobType.Pause, "暂停" });
            _result.Add(new object[] { (int)JobType.Continue, "继续" });
            return _result;
        }

        /// <summary>
        /// 操作类型列表
        /// </summary>
        /// <returns></returns>
        public static List<object[]> JobTypeObject()
        {
            List<object[]> _result = new List<object[]>();
            foreach (var _o in JobTypeReflect())
            {
                _result.Add(new object[] { _o[0], _o[1] });
            }
            return _result;
        }

        /// <summary>
        /// 操作类型显示值
        /// </summary>
        /// <param name="objStatus"></param>
        /// <returns></returns>
        public static string GetJobTypeDisplay(int objStatus)
        {
            string _result = string.Empty;
            foreach (var _O in JobTypeReflect())
            {
                if ((int)_O[0] == objStatus)
                {
                    _result = _O[1].ToString();
                    break;
                }
            }
            return _result;
        }
        #endregion

        #region 操作状态集合
        /// <summary>
        /// 操作状态集合
        /// </summary>
        /// <returns></returns>
        private static List<DefineEnum> JobStatusReflect()
        {
            List<DefineEnum> _result = new List<DefineEnum>();
            _result.Add(new DefineEnum() { ID = (int)JobStatus.Wait, Display = "等待中", Css = "color_primary" });
            _result.Add(new DefineEnum() { ID = (int)JobStatus.Processing, Display = "处理中", Css = "color_warning" });
            _result.Add(new DefineEnum() { ID = (int)JobStatus.Success, Display = "已完成", Css = "color_success" });
            _result.Add(new DefineEnum() { ID = (int)JobStatus.Fail, Display = "处理失败", Css = "color_danger" });
            return _result;
        }

        /// <summary>
        /// 操作状态列表
        /// </summary>
        /// <returns></returns>
        public static List<object[]> JobStatusObject()
        {
            List<object[]> _result = new List<object[]>();
            foreach (var _o in JobStatusReflect())
            {
                _result.Add(new object[] { _o.ID, _o.Display });
            }
            return _result;
        }

        /// <summary>
        /// 操作状态显示值
        /// </summary>
        /// <param name="objStatus"></param>
        /// <param name="objCss"></param>
        /// <returns></returns>
        public static string GetJobStatusDisplay(int objStatus, bool objCss = false)
        {
            string _result = string.Empty;
            DefineEnum _O = JobStatusReflect().Where(p => p.ID == objStatus).SingleOrDefault();
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