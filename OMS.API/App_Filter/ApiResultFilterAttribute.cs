using System;
using System.Web.Http.Filters;
using System.Net.Http;
using Samsonite.Utility.Common;

using OMS.API.Models;
using OMS.API.Utils;

public class ApiResultFilterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
    {
        //保存访问日志
        var actionName = actionExecutedContext.ActionContext.ActionDescriptor.ActionName;
        var controllerName = actionExecutedContext.ActionContext.ControllerContext.ControllerDescriptor.ControllerName;
        if (actionExecutedContext.ActionContext.Response != null)
        {
            //取得由API返回的状态码
            var Status = actionExecutedContext.ActionContext.Response.StatusCode;
            //取得由API返回的信息
            var obj = actionExecutedContext.ActionContext.Response.Content;
            string context = string.Empty;
            if (obj is ObjectContent<ApiResponse>)
            {
                ObjectContent<ApiResponse> result = (ObjectContent<ApiResponse>)obj;
                context = UtilsHelper.JsonSerialize(result.Value);
                //返回信息
                actionExecutedContext.Response = new HttpResponseMessage()
                {
                    Content = UtilsHelper.ContextResponse(result.Value)
                };
            }
            else if (obj is ObjectContent<ApiPageResponse>)
            {
                ObjectContent<ApiPageResponse> result = (ObjectContent<ApiPageResponse>)obj;
                context = UtilsHelper.JsonSerialize(result.Value);
                //返回信息
                actionExecutedContext.Response = new HttpResponseMessage()
                {
                    Content = UtilsHelper.ContextResponse(result.Value)
                };
            }
            //文件日志
            if (GlobalConfig.IsApiDebugLog)
            {
                FileLogHelper.WriteLog(new string[] { $"ApiResult: {context}", "ApiResult End.", "********************************************************************************", "\r" }, DateTime.Now.ToString("HH"), $"{controllerName}/{actionName}");
            }
        }
    }
}

