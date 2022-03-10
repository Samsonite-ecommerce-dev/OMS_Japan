using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Service;
using Samsonite.OMS.Service.AppLanguage;

public class UserLoginAuthorize : BaseAuthorize
{
    //默认页面跳转模式
    public ResultType Type { get; set; }
    //是否开启CSRF防御
    public bool IsAntiForgeryToken { get; set; }
    public override void OnAuthorization(AuthorizationContext filterContext)
    {
        //加载语言包
        var _LanguagePack = LanguageService.Get();

        try
        {
            UserSessionInfo objUserSession = UserLoginService.GetCurrentLoginUser();
            if (objUserSession != null)
            {
                if (IsAntiForgeryToken)
                {
                    //防止CSRF攻击
                    this.VerificationToken(_LanguagePack);
                }
            }
            else
            {
                throw new Exception(_LanguagePack["common_alert_no_login"]);
            }
        }
        catch (Exception ex)
        {
            this.GoLogin(Type, filterContext, ex.Message);
        }
    }
}

