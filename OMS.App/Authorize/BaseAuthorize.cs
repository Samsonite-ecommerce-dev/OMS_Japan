using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.Utility.Common;

public class BaseAuthorize : AuthorizeAttribute
{
    /// <summary>
    /// 权限类型
    /// </summary>
    public enum ResultType
    {
        /// <summary>
        /// 页面跳转
        /// </summary>
        View = 1,
        /// <summary>
        /// 返回json格式
        /// </summary>
        Json = 2,
        /// <summary>
        /// 返回内容格式
        /// </summary>
        Content = 3
    }

    /// <summary>
    /// 跳转到登入页面
    /// </summary>
    /// <param name="objType"></param>
    /// <param name="objFilterContext"></param>
    /// <param name="objMsg"></param>
    protected void GoLogin(ResultType objType, AuthorizationContext objFilterContext, string objMsg)
    {
        if (objType == ResultType.Json)
        {
            objFilterContext.Result = new JsonResult { Data = new { result = false, msg = objMsg } };
        }
        else if (objType == ResultType.Content)
        {
            objFilterContext.Result = new ContentResult { ContentEncoding = System.Text.Encoding.UTF8, Content = "" };
        }
        else
        {
            objFilterContext.Result = new RedirectResult("~/Login/Index");
        }
    }

    /// <summary>
    /// 跳转到修改密码页面
    /// </summary>
    /// <param name="objType"></param>
    /// <param name="objFilterContext"></param>
    /// <param name="objMsg"></param>
    protected void GoEditPassword(ResultType objType, AuthorizationContext objFilterContext, string objMsg)
    {
        if (objType == ResultType.Json)
        {
            objFilterContext.Result = new JsonResult { Data = new { result = false, msg = objMsg } };
        }
        else if (objType == ResultType.Content)
        {
            objFilterContext.Result = new ContentResult { ContentEncoding = System.Text.Encoding.UTF8, Content = "" };
        }
        else
        {
            objFilterContext.Result = new RedirectResult("~/Home/EditPassword");
        }
    }

    /// <summary>
    /// 跳转到错误页面
    /// </summary>
    /// <param name="objType"></param>
    /// <param name="objFilterContext"></param>
    /// <param name="objMsg"></param>
    protected void GoError(ResultType objType, AuthorizationContext objFilterContext, string objMsg)
    {
        if (objType == ResultType.Json)
        {
            objFilterContext.Result = new JsonResult { Data = new { result = false, msg = objMsg } };
        }
        else if (objType == ResultType.Content)
        {
            objFilterContext.Result = new ContentResult { ContentEncoding = System.Text.Encoding.UTF8, Content = "" };
        }
        else
        {
            objFilterContext.Result = new RedirectResult("~/Error/Index?type=" + (int)ErrorType.NoPower + "&err=" + System.Web.HttpContext.Current.Server.UrlEncode(objMsg));
        }
    }

    protected void VerificationToken(Dictionary<string, string> objLanguagePack)
    {
        try
        {
            var _antiForgeryCookie = HttpContext.Current.Request.Cookies[AntiForgeryConfig.CookieName];
            string _antiCookieValue = (_antiForgeryCookie != null) ? _antiForgeryCookie.Value : null;
            string _antiTokenValue = VariableHelper.SaferequestNull(HttpContext.Current.Request.QueryString["__RequestVerificationToken"]);
            if (string.IsNullOrEmpty(_antiTokenValue))
            {
                _antiTokenValue = VariableHelper.SaferequestNull(HttpContext.Current.Request.Form["__RequestVerificationToken"]);
            }
            AntiForgery.Validate(_antiCookieValue, _antiTokenValue);
        }
        catch
        {
            throw new Exception(objLanguagePack["common_alert_no_law"]);
        }
    }
}

