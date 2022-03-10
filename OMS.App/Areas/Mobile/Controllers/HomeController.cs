using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.Utility.Common;

namespace OMS.App.Areas.Mobile.Controllers
{
    public class HomeController : BaseController
    {
        // GET: Mobile/Home

        [UserLoginAuthorize]
        public ActionResult Index()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;
            //下拉菜单栏
            ViewBag.MenuBar = this.MenuBar();

            int _ID = VariableHelper.SaferequestInt(Request.QueryString["ID"]);

            //权限分组
            int[] _GroupIDs = new int[] { 17 };
            //默认17
            if (_ID == 0) _ID = _GroupIDs.FirstOrDefault();
            using (var db = new ebEntities())
            {
                SysFunctionGroup objSysFunctionGroup = db.SysFunctionGroup.Where(p => p.Groupid == _ID).SingleOrDefault();
                if (objSysFunctionGroup != null)
                {
                    //分组管理
                    ViewData["group_list"] = db.SysFunctionGroup.Where(p => _GroupIDs.Contains(p.Groupid)).ToList();
                    //登录信息
                    UserSessionInfo _UserSessionInfo = this.CurrentLoginUser;
                    //权限功能列表
                    List<int> _powers = new List<int>();
                    if (_UserSessionInfo != null)
                    {
                        _powers = _UserSessionInfo.UserPowers.Select(p => p.FunctionID).ToList();
                    }
                    ViewBag.UserName = _UserSessionInfo.UserName;
                    //读取菜单
                    ViewData["function_list"] = db.SysFunction.Where(p => p.Groupid == objSysFunctionGroup.Groupid && _powers.Contains(p.Funcid)).OrderBy(p => p.SeqNumber).ToList();
                }
                else
                {
                    return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.NoMessage });
                }
            }

            return View();
        }
    }
}