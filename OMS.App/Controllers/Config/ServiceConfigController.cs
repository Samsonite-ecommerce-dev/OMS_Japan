using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service;
using Samsonite.Utility.Common;
using OMS.App.Helper;

namespace OMS.App.Controllers
{
    public class ServiceConfigController : BaseController
    {
        //
        // GET: /ServiceConfig/

        #region 查询
        [UserPowerAuthorize]
        public ActionResult Index()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;
            //菜单栏
            ViewBag.MenuBar = this.MenuBar();
            //功能权限
            ViewBag.FunctionPower = this.FunctionPowers();
            //服务列表
            using (var db = new ebEntities())
            {
                ViewData["service_list"] = db.ServiceModuleInfo.ToList();
            }
            //执行状态
            ViewData["service_status_list"] = ServiceHelper.ServiceStatusObject();
            //操作类型
            ViewData["job_type_list"] = ServiceHelper.JobTypeObject();
            //操作状态
            ViewData["job_status_list"] = ServiceHelper.JobStatusObject();

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult Index_Message()
        {
            JsonResult _result = new JsonResult();
            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            string _keyword = VariableHelper.SaferequestStr(Request.Form["keyword"]);
            int _status = VariableHelper.SaferequestInt(Request.Form["status"]);
            int _isrun = VariableHelper.SaferequestInt(Request.Form["isrun"]);
            using (var db = new DynamicRepository())
            {
                //搜索条件
                if (!string.IsNullOrEmpty(_keyword))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "ModuleTitle like {0}", Param = "%" + _keyword + "%" });
                }

                if (_status > 0)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "Status={0}", Param = (_status - 1) });
                }

                if (_isrun > 0)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "IsRun={0}", Param = 1 });
                }

                //查询
                var _list = db.GetPage<ServiceModuleInfo>("select * from ServiceModuleInfo order by SortID asc", _SqlWhere, VariableHelper.SaferequestInt(Request.Form["rows"]), VariableHelper.SaferequestInt(Request.Form["page"]));
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.ModuleID,
                               s1 = dy.ModuleTitle,
                               s2 = dy.ModuleMark,
                               s3 = dy.ModuleType,
                               s4 = GetModuleRunType(dy.FixType, dy.FixTime),
                               s5 = dy.SortID,
                               s6 = ServiceHelper.GetServiceStatusDisplay(dy.Status, true),
                               s7 = GetOperButton(dy.ModuleID, dy.IsRun, dy.Status),
                               s8 = (dy.NextRunTime == null) ? "--" : dy.NextRunTime.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                               s9 = (dy.IsRun) ? "<label class=\"fa fa-check color_primary\"></label>" : "<label class=\"fa fa-close color_danger\"></label>",
                               s10 = (dy.IsNotify) ? "<label class=\"fa fa-check color_primary\"></label>" : "<label class=\"fa fa-close color_danger\"></label>",
                               s11 = dy.Remark
                           }
                };
                return _result;
            }
        }

        private string GetModuleRunType(int objFixType, string objFixTime)
        {
            string _result = string.Empty;
            if (objFixType == 1)
            {
                _result = $"每隔{(VariableHelper.SaferequestDouble(objFixTime) / 60)}分钟执行一次服务";
            }
            else
            {
                _result = $"每天{objFixTime}定时执行一次服务";
            }
            return _result;
        }

        private string GetOperButton(int objModuleID, bool objIsRun, int objStatus)
        {
            string _result = string.Empty;
            if (objIsRun)
            {
                switch (objStatus)
                {
                    case (int)ServiceStatus.Stop:
                        _result = $"<a href=\"javascript:startService({objModuleID})\" class=\"fa fa-play-circle color_primary\">&nbsp;</a>";
                        break;
                    case (int)ServiceStatus.Runing:
                        _result = $"<a href=\"javascript:pauseService({objModuleID})\" class=\"fa fa-pause color_warning\">&nbsp;</a>";
                        break;
                    case (int)ServiceStatus.Pause:
                        _result = $"<a href=\"javascript:continueService({objModuleID})\" class=\"fa fa-play color_success\">&nbsp;</a>";
                        break;
                    default:
                        break;
                }
            }
            return _result;
        }
        #endregion

        #region 添加
        [UserPowerAuthorize]
        public ActionResult Add()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Add_Message()
        {
            JsonResult _result = new JsonResult();
            string _ModuleTitle = VariableHelper.SaferequestStr(Request.Form["ModuleTitle"]);
            string _ModuleWorkflowID = VariableHelper.SaferequestStr(Request.Form["ModuleWorkflowID"]);
            string _ModuleMark = VariableHelper.SaferequestStr(Request.Form["ModuleMark"]);
            string _ModuleAssembly = VariableHelper.SaferequestStr(Request.Form["ModuleAssembly"]);
            string _ModuleType = VariableHelper.SaferequestStr(Request.Form["ModuleType"]);
            string _ModuleBLL = VariableHelper.SaferequestStr(Request.Form["ModuleBLL"]);
            int _FixType = VariableHelper.SaferequestInt(Request.Form["FixType"]);
            string _FixTime = VariableHelper.SaferequestStr(Request.Form["FixTime"]);
            string _Remark = VariableHelper.SaferequestEditor(Request.Form["Remark"]);
            int _IsRun = VariableHelper.SaferequestInt(Request.Form["IsRun"]);
            int _IsNotify = VariableHelper.SaferequestInt(Request.Form["IsNotify"]);
            int _SortID = 0;

            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_ModuleTitle))
                    {
                        throw new Exception("服务名称不能为空");
                    }

                    if (string.IsNullOrEmpty(_ModuleMark))
                    {
                        throw new Exception("服务标识不能为空");
                    }
                    else
                    {
                        ServiceModuleInfo objServiceModuleInfo = db.ServiceModuleInfo.Where(p => p.ModuleMark == _ModuleMark).SingleOrDefault();
                        if (objServiceModuleInfo != null)
                        {
                            throw new Exception("服务标识已经存在，请勿重复");
                        }
                    }

                    if (string.IsNullOrEmpty(_ModuleAssembly))
                    {
                        throw new Exception("Assembly不能为空");
                    }

                    if (string.IsNullOrEmpty(_ModuleType))
                    {
                        throw new Exception("Class不能为空");
                    }

                    if (string.IsNullOrEmpty(_ModuleBLL))
                    {
                        throw new Exception("BLL不能为空");
                    }

                    _SortID = db.Database.SqlQuery<int>("select isnull(Max(SortID),0) As SortID from ServiceModuleInfo").SingleOrDefault() + 1;

                    ServiceModuleInfo objData = new ServiceModuleInfo()
                    {
                        ModuleTitle = _ModuleTitle,
                        ModuleWorkflowID = _ModuleWorkflowID,
                        ModuleMark = _ModuleMark,
                        ModuleAssembly = _ModuleAssembly,
                        ModuleType = _ModuleType,
                        ModuleBLL = _ModuleBLL,
                        FixType = _FixType,
                        FixTime = _FixTime,
                        IsRun = (_IsRun == 1),
                        IsNotify = (_IsNotify == 1),
                        Remark = _Remark,
                        SortID = _SortID,
                        Status = (int)ServiceStatus.Stop,
                        NextRunTime = null,
                        CreateTime = DateTime.Now
                    };
                    db.ServiceModuleInfo.Add(objData);
                    db.SaveChanges();
                    //添加日志
                    AppLogService.InsertLog<ServiceModuleInfo>(objData, objData.ModuleID.ToString());
                    //返回信息
                    _result.Data = new
                    {
                        result = true,
                        msg = "数据保存成功"
                    };
                }
                catch (Exception ex)
                {
                    _result.Data = new
                    {
                        result = false,
                        msg = ex.Message
                    };
                }
                return _result;
            }
        }
        #endregion

        #region 编辑
        [UserPowerAuthorize]
        public ActionResult Edit()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;

            int _ID = VariableHelper.SaferequestInt(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                ServiceModuleInfo objServiceModuleInfo = db.ServiceModuleInfo.Where(p => p.ModuleID == _ID).SingleOrDefault();
                if (objServiceModuleInfo != null)
                {
                    return View(objServiceModuleInfo);
                }
                else
                {
                    return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.NoMessage });
                }
            }
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Edit_Message()
        {
            JsonResult _result = new JsonResult();
            int _ID = VariableHelper.SaferequestInt(Request.Form["ID"]);
            string _ModuleTitle = VariableHelper.SaferequestStr(Request.Form["ModuleTitle"]);
            string _ModuleWorkflowID = VariableHelper.SaferequestStr(Request.Form["ModuleWorkflowID"]);
            string _ModuleMark = VariableHelper.SaferequestStr(Request.Form["ModuleMark"]);
            string _ModuleAssembly = VariableHelper.SaferequestStr(Request.Form["ModuleAssembly"]);
            string _ModuleType = VariableHelper.SaferequestStr(Request.Form["ModuleType"]);
            string _ModuleBLL = VariableHelper.SaferequestStr(Request.Form["ModuleBLL"]);
            int _FixType = VariableHelper.SaferequestInt(Request.Form["FixType"]);
            string _FixTime = VariableHelper.SaferequestStr(Request.Form["FixTime"]);
            string _Remark = VariableHelper.SaferequestEditor(Request.Form["Remark"]);
            int _SortID = VariableHelper.SaferequestInt(Request.Form["Sort"]);
            int _IsRun = VariableHelper.SaferequestInt(Request.Form["IsRun"]);
            int _IsNotify = VariableHelper.SaferequestInt(Request.Form["IsNotify"]);

            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_ModuleTitle))
                    {
                        throw new Exception("服务名称不能为空");
                    }

                    if (string.IsNullOrEmpty(_ModuleMark))
                    {
                        throw new Exception("服务标识不能为空");
                    }
                    else
                    {
                        ServiceModuleInfo objServiceModuleInfo = db.ServiceModuleInfo.Where(p => p.ModuleMark == _ModuleMark && p.ModuleID != _ID).SingleOrDefault();
                        if (objServiceModuleInfo != null)
                        {
                            throw new Exception("服务标识已经存在，请勿重复");
                        }
                    }

                    if (string.IsNullOrEmpty(_ModuleAssembly))
                    {
                        throw new Exception("Assembly不能为空");
                    }

                    if (string.IsNullOrEmpty(_ModuleType))
                    {
                        throw new Exception("Class不能为空");
                    }

                    if (string.IsNullOrEmpty(_ModuleBLL))
                    {
                        throw new Exception("BLL不能为空");
                    }

                    ServiceModuleInfo objData = db.ServiceModuleInfo.Where(p => p.ModuleID == _ID).SingleOrDefault();
                    if (objData != null)
                    {
                        objData.ModuleTitle = _ModuleTitle;
                        objData.ModuleWorkflowID = _ModuleWorkflowID;
                        objData.ModuleMark = _ModuleMark;
                        objData.ModuleAssembly = _ModuleAssembly;
                        objData.ModuleType = _ModuleType;
                        objData.ModuleBLL = _ModuleBLL;
                        objData.FixType = _FixType;
                        objData.FixTime = _FixTime;
                        objData.IsRun = (_IsRun == 1);
                        objData.IsNotify = (_IsNotify == 1);
                        objData.SortID = _SortID;
                        objData.Remark = _Remark;
                        db.SaveChanges();

                        //添加日志
                        AppLogService.UpdateLog<ServiceModuleInfo>(objData, objData.ModuleID.ToString());

                        //返回信息
                        _result.Data = new
                        {
                            result = true,
                            msg = "数据保存成功"
                        };
                    }
                    else
                    {
                        throw new Exception("数据读取失败");
                    }
                }
                catch (Exception ex)
                {
                    _result.Data = new
                    {
                        result = false,
                        msg = ex.Message
                    };
                }
                return _result;
            }
        }
        #endregion

        #region 操作
        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Oper_Message()
        {
            JsonResult _result = new JsonResult();
            Int64 _ModuleID = VariableHelper.SaferequestInt64(Request.Form["id"]);
            int _OperType = VariableHelper.SaferequestInt(Request.Form["type"]);
            using (var db = new ebEntities())
            {
                try
                {
                    //限制每个服务当前只能有插入一条未处理完成的工作流
                    var objServiceModuleJob = db.ServiceModuleJob.Where(p => p.ModuleID == _ModuleID && (new List<int>() { (int)JobStatus.Wait, (int)JobStatus.Processing }).Contains(p.Status)).FirstOrDefault();
                    if (objServiceModuleJob != null)
                    {
                        throw new Exception("当前存在未处理完的工作流,请稍后在试.");
                    }

                    ServiceModuleInfo objServiceModuleInfo = db.ServiceModuleInfo.Where(p => p.ModuleID == _ModuleID).SingleOrDefault();
                    if (objServiceModuleInfo != null)
                    {
                        switch (_OperType)
                        {
                            case (int)JobType.Start:
                                if (objServiceModuleInfo.Status == (int)ServiceStatus.Stop)
                                {
                                    db.ServiceModuleJob.Add(new ServiceModuleJob()
                                    {
                                        ModuleID = objServiceModuleInfo.ModuleID,
                                        OperType = (int)JobType.Start,
                                        Status = (int)JobStatus.Wait,
                                        StatusMessage = string.Empty,
                                        AddTime = DateTime.Now
                                    });
                                    db.SaveChanges();
                                }
                                else
                                {
                                    throw new Exception("只有当服务流程为停止时才能进行启动操作.");
                                }
                                break;
                            case (int)JobType.Pause:
                                if (objServiceModuleInfo.Status == (int)ServiceStatus.Runing)
                                {
                                    db.ServiceModuleJob.Add(new ServiceModuleJob()
                                    {
                                        ModuleID = objServiceModuleInfo.ModuleID,
                                        OperType = (int)JobType.Pause,
                                        Status = (int)JobStatus.Wait,
                                        StatusMessage = string.Empty,
                                        AddTime = DateTime.Now
                                    });
                                    db.SaveChanges();
                                }
                                else
                                {
                                    throw new Exception("只有当服务流程为运行中时才能进行暂停操作");
                                }
                                break;
                            case (int)JobType.Continue:
                                if (objServiceModuleInfo.Status == (int)ServiceStatus.Pause)
                                {
                                    db.ServiceModuleJob.Add(new ServiceModuleJob()
                                    {
                                        ModuleID = objServiceModuleInfo.ModuleID,
                                        OperType = (int)JobType.Continue,
                                        Status = (int)JobStatus.Wait,
                                        StatusMessage = string.Empty,
                                        AddTime = DateTime.Now
                                    });
                                    db.SaveChanges();
                                }
                                else
                                {
                                    throw new Exception("只有当服务流程为暂停中时才能进行继续操作");
                                }
                                break;
                            default:
                                throw new Exception("未知指令.");
                        }

                        //返回信息
                        _result.Data = new
                        {
                            result = true,
                            msg = "指令保存成功,请耐心等待执行..."
                        };
                    }
                    else
                    {
                        throw new Exception("数据读取失败");
                    }
                }
                catch (Exception ex)
                {
                    _result.Data = new
                    {
                        result = false,
                        msg = ex.Message
                    };
                }
                return _result;
            }
        }
        #endregion

        #region 操作记录
        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult Index_OperLog_Message()
        {
            JsonResult _result = new JsonResult();
            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            int _ModuleID = VariableHelper.SaferequestInt(Request.Form["moduleid"]);
            int _JobType = VariableHelper.SaferequestInt(Request.Form["job_type"]);
            int _JobStatus = VariableHelper.SaferequestInt(Request.Form["job_status"]);
            string _time = VariableHelper.SaferequestStr(Request.Form["time"]);

            using (var db = new DynamicRepository())
            {
                //搜索条件
                if (_ModuleID > 0)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "smj.ModuleID={0}", Param = _ModuleID });
                }

                if (_JobType > 0)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "smj.OperType={0}", Param = _JobType });
                }

                if (_JobStatus > 0)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "smj.Status={0}", Param = (_JobStatus - 1) });
                }

                if (!string.IsNullOrEmpty(_time))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,smj.AddTime,{0})=0 ", Param = VariableHelper.SaferequestTime(_time) });
                }

                //查询
                var _list = db.GetPage<dynamic>("select smj.ID,smj.OperType,smj.Status,smj.StatusMessage,smj.AddTime,smi.ModuleTitle from ServiceModuleJob as smj inner join ServiceModuleInfo as smi on smj.ModuleID=smi.ModuleID order by smj.ID desc", _SqlWhere, VariableHelper.SaferequestInt(Request.Form["rows"]), VariableHelper.SaferequestInt(Request.Form["page"]));
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               s1 = dy.ModuleTitle,
                               s2 = ServiceHelper.GetJobTypeDisplay(dy.OperType),
                               s3 = ServiceHelper.GetJobStatusDisplay(dy.Status, true),
                               s4 = dy.StatusMessage,
                               s5 = dy.AddTime.ToString("yyyy-MM-dd HH:mm:ss")
                           }
                };
                return _result;
            }
        }
        #endregion
    }
}
