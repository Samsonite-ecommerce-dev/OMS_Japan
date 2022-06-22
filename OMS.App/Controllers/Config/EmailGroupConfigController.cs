using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service;
using Samsonite.Utility.Common;

namespace OMS.App.Controllers
{
    public class EmailGroupConfigController : BaseController
    {
        //
        // GET: /EmailGroupConfig/

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

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult Index_Message()
        {
            JsonResult _result = new JsonResult();
            string _keyword = VariableHelper.SaferequestStr(Request.Form["keyword"]);
            using (var db = new ebEntities())
            {
                var _lambda = db.SendMailGroup.AsQueryable();

                //搜索条件
                if (!string.IsNullOrEmpty(_keyword))
                {
                    _lambda = _lambda.Where(p => p.GroupName.Contains(_keyword));
                }

                //查询
                var _list = this.BaseEntityRepository.GetPage(VariableHelper.SaferequestInt(Request.Form["page"]), VariableHelper.SaferequestInt(Request.Form["rows"]), _lambda.AsNoTracking(), p => p.ID, true);
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.ID,
                               s1 = dy.GroupName,
                               s2 = dy.MailAddresses,
                               s3 = dy.Remark
                           }
                };
                return _result;
            }
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
            string _GroupName = VariableHelper.SaferequestStr(Request.Form["GroupName"]);
            string _MailAddresses = VariableHelper.SaferequestStr(Request.Form["MailAddresses"]);
            string _Remark = VariableHelper.SaferequestStr(Request.Form["Remark"]);

            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_GroupName))
                    {
                        throw new Exception("组名不能为空");
                    }
                    else
                    {
                        SendMailGroup objSendMailGroup = db.SendMailGroup.Where(p => p.GroupName == _GroupName).SingleOrDefault();
                        if (objSendMailGroup != null)
                        {
                            throw new Exception("组名已经存在，请勿重复");
                        }
                    }

                    if (string.IsNullOrEmpty(_MailAddresses))
                    {
                        throw new Exception("至少添加一个要发送的邮件地址");
                    }

                    SendMailGroup objData = new SendMailGroup()
                    {
                        GroupName = _GroupName,
                        MailAddresses = _MailAddresses,
                        Remark = _Remark,
                        CreateTime = DateTime.Now
                    };
                    db.SendMailGroup.Add(objData);
                    db.SaveChanges();
                    //添加日志
                    AppLogService.InsertLog<SendMailGroup>(objData, objData.ID.ToString());
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
                SendMailGroup objSendMailGroup = db.SendMailGroup.Where(p => p.ID == _ID).SingleOrDefault();
                if (objSendMailGroup != null)
                {
                    return View(objSendMailGroup);
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
            string _GroupName = VariableHelper.SaferequestStr(Request.Form["GroupName"]);
            string _MailAddresses = VariableHelper.SaferequestStr(Request.Form["MailAddresses"]);
            string _Remark = VariableHelper.SaferequestStr(Request.Form["Remark"]);

            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_GroupName))
                    {
                        throw new Exception("组名不能为空");
                    }
                    else
                    {
                        SendMailGroup objSendMailGroup = db.SendMailGroup.Where(p => p.GroupName == _GroupName && p.ID != _ID).SingleOrDefault();
                        if (objSendMailGroup != null)
                        {
                            throw new Exception("组名已经存在，请勿重复");
                        }
                    }

                    if (string.IsNullOrEmpty(_MailAddresses))
                    {
                        throw new Exception("至少添加一个要发送的邮件地址");
                    }

                    SendMailGroup objData = db.SendMailGroup.Where(p => p.ID == _ID).SingleOrDefault();
                    if (objData != null)
                    {
                        objData.GroupName = _GroupName;
                        objData.MailAddresses = _MailAddresses;
                        objData.Remark = _Remark;
                        db.SaveChanges();
                        //添加日志
                        AppLogService.UpdateLog<SendMailGroup>(objData, objData.ID.ToString());
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

        #region 删除
        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Delete_Message()
        {
            JsonResult _result = new JsonResult();
            string _IDs = VariableHelper.SaferequestStr(Request.Form["ID"]);
            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_IDs))
                        {
                            throw new Exception("请至少选择一条要操作的数据");
                        }

                        SendMailGroup objSendMailGroup = new SendMailGroup();
                        foreach (string _str in _IDs.Split(','))
                        {
                            int _ID = VariableHelper.SaferequestInt(_str);
                            objSendMailGroup = db.SendMailGroup.Where(p => p.ID == _ID).SingleOrDefault();
                            if (objSendMailGroup != null)
                            {
                                db.SendMailGroup.Remove(objSendMailGroup);
                            }
                            else
                            {
                                throw new Exception(string.Format("{0}:{1}", _str, "信息不存在或已被删除"));
                            }
                        }
                        db.SaveChanges();
                        Trans.Commit();
                        //添加日志
                        AppLogService.DeleteLog("SendMailGroup", _IDs);
                        //返回信息
                        _result.Data = new
                        {
                            result = true,
                            msg = "数据删除成功"
                        };
                    }
                    catch (Exception ex)
                    {
                        Trans.Rollback();
                        _result.Data = new
                        {
                            result = false,
                            msg = ex.Message
                        };
                    }
                }
                return _result;
            }
        }
        #endregion
    }
}
