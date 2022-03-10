using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Service.AppLanguage;

namespace OMS.App.Areas.Mobile.Controllers
{
    public class ErrorController : BaseController
    {
        // GET: Mobile/Error
        public ActionResult Index(int Type, string Message)
        {
            //加载语言包
            var _LanguagePack = LanguageService.Get();
            switch (Type)
            {
                case (int)ErrorType.NoExsit:
                    Message = _LanguagePack["common_alert_no_page"];
                    break;
                case (int)ErrorType.NoMessage:
                    Message = _LanguagePack["common_alert_no_message"];
                    break;
                case (int)ErrorType.NoPower:
                    Message = _LanguagePack["common_alert_no_permission"];
                    break;
                case (int)ErrorType.Other:
                    break;
                default:
                    break;
            }

            ViewBag.ErrorMessage = Message;

            return View();
        }
    }
}