using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Filters;
using System.Net.Http;
using System.Web.Http.Controllers;
using Samsonite.Utility.Common;

using OMS.API.Models;
using OMS.API.Utils;

using Samsonite.OMS.Database;
using Samsonite.OMS.Service;

public class AuthorizeFilterAttribute : ActionFilterAttribute
{
    private List<AuthorizeUser> authorizeUsers;
    private string _requestD = string.Empty;
    public AuthorizeFilterAttribute()
    {
        //初始化账号信息
        authorizeUsers = AuthorizeHelper.GetApiAccounts();
    }

    public override void OnActionExecuting(HttpActionContext actionContext)
    {
        WebApiAccessLog objWebApiAccessLog = new WebApiAccessLog();

        var _controllerName = actionContext.ControllerContext.ControllerDescriptor.ControllerName;
        var _actionName = actionContext.ActionDescriptor.ActionName;
        var _postBody = string.Empty;
        _requestD = UtilsHelper.GreateRequestID();
        try
        {
            var _result = AuthorizeHelper.VisitValid(actionContext, authorizeUsers);
            ////测试用-------------
            //_result.Result = true;
            ////-------------------
            //访问信息
            objWebApiAccessLog.LogType = ApiService.GetAPIType(_controllerName);
            objWebApiAccessLog.Url = _result.Params.Url;
            objWebApiAccessLog.RequestID = _requestD;
            objWebApiAccessLog.UserID = _result.Params.Userid;
            objWebApiAccessLog.Ip = _result.Params.Ip;
            objWebApiAccessLog.CreateTime = DateTime.Now;
            //Body参数
            _postBody = _result.Params.PostBody;
            //返回信息
            if (_result.Result)
            {
                objWebApiAccessLog.State = true;
                objWebApiAccessLog.Remark = _result.Message;
            }
            else
            {
                throw new Exception(_result.Message);
            }
        }
        catch (Exception ex)
        {
            //日志
            objWebApiAccessLog.State = false;
            objWebApiAccessLog.Remark = ex.Message;
            //错误提示
            ApiResponse apiResult = new ApiResponse()
            {
                RequestID = _requestD,
                Code = (int)ApiResultCode.Fail,
                Message = ex.Message
            };
            //返回错误信息
            actionContext.Response = new HttpResponseMessage()
            {
                Content = UtilsHelper.ContextResponse(apiResult)
            };
        }
        //添加日志
        WebAPILogService.WriteAccessLog(objWebApiAccessLog);
        //文件日志
        if (GlobalConfig.IsApiDebugLog)
        {
            string[] _logs = new string[] { $"Client Ip:{objWebApiAccessLog.Ip}", $"Request Uri:{objWebApiAccessLog.Url}", $"Request Json:{_postBody}" };
            UtilsHelper.WriteLogger(_controllerName, _actionName, _requestD, _logs);
        }
        //跳转继续执行
        base.OnActionExecuting(actionContext);
    }

    public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
    {
        //保存访问日志
        var _actionName = actionExecutedContext.ActionContext.ActionDescriptor.ActionName;
        var _controllerName = actionExecutedContext.ActionContext.ControllerContext.ControllerDescriptor.ControllerName;
        if (actionExecutedContext.ActionContext.Response != null)
        {
            //取得由API返回的状态码
            var _status = actionExecutedContext.ActionContext.Response.StatusCode;
            //取得由API返回的信息
            var obj = actionExecutedContext.ActionContext.Response.Content;
            string _contextResult = string.Empty;
            if (obj is ObjectContent<ApiResponse>)
            {
                ObjectContent<ApiResponse> result = (ObjectContent<ApiResponse>)obj;
                //添加requestID
                ((ApiResponse)result.Value).RequestID = _requestD;
                _contextResult = UtilsHelper.JsonSerialize(result.Value);
                //返回信息
                actionExecutedContext.Response = new HttpResponseMessage()
                {
                    Content = UtilsHelper.ContextResponse(result.Value)
                };
            }
            else if (obj is ObjectContent<ApiPageResponse>)
            {
                ObjectContent<ApiPageResponse> result = (ObjectContent<ApiPageResponse>)obj;
                //添加requestID
                ((ApiPageResponse)result.Value).RequestID = _requestD;
                _contextResult = UtilsHelper.JsonSerialize(result.Value);
                //返回信息
                actionExecutedContext.Response = new HttpResponseMessage()
                {
                    Content = UtilsHelper.ContextResponse(result.Value)
                };
            }
            //文件日志
            if (GlobalConfig.IsApiDebugLog)
            {
                UtilsHelper.WriteLogger(_controllerName, _actionName, _requestD, new string[] { $"ApiResult: {_contextResult}", "********************************************************************************", "\r" });
            }
        }
    }
}

