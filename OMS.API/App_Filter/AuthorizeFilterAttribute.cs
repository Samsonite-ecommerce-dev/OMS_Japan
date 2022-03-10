using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Filters;
using System.Net.Http;
using System.Text;
using System.Web.Http.Controllers;
using Samsonite.Utility.Common;

using OMS.API.Models;
using OMS.API.Utils;

using Samsonite.OMS.Database;
using Samsonite.OMS.Service;



public class AuthorizeFilterAttribute : ActionFilterAttribute
{
    private List<WebApiAccount> objWebApiAccounts = new List<WebApiAccount>();
    public AuthorizeFilterAttribute()
    {
        //获取有效的访问配置
        objWebApiAccounts = APIHelper.GetWebApiAccountList();
    }

    public override void OnActionExecuting(HttpActionContext actionContext)
    {
        var actionName = actionContext.ActionDescriptor.ActionName;
        var controllerName = actionContext.ControllerContext.ControllerDescriptor.ControllerName;
        //访问者IP
        string _ip = HttpContextHelper.GetIP();
        //访问地址
        string _url = actionContext.Request.RequestUri.PathAndQuery;
        //添加日志
        WebApiAccessLog objWebApiAccessLog = new WebApiAccessLog();
        objWebApiAccessLog.Ip = _ip;
        objWebApiAccessLog.Url = _url;
        objWebApiAccessLog.State = true;
        objWebApiAccessLog.CreateTime = DateTime.Now;
        try
        {

            //基础参数
            IDictionary<string, string> paramDict = HttpContextHelper.GetRequestParams(actionContext.Request.RequestUri.Query);
            string _userID = (paramDict.ContainsKey("userid")) ? VariableHelper.SaferequestStr(paramDict["userid"]) : "";
            string _version = (paramDict.ContainsKey("version")) ? VariableHelper.SaferequestStr(paramDict["version"]) : "";
            string _format = (paramDict.ContainsKey("format")) ? VariableHelper.SaferequestStr(paramDict["format"]) : "";
            string _timestamp = (paramDict.ContainsKey("timestamp")) ? VariableHelper.SaferequestStr(paramDict["timestamp"]) : "";
            string _sign = (paramDict.ContainsKey("sign")) ? VariableHelper.SaferequestStr(paramDict["sign"]) : "";

            objWebApiAccessLog.UserID = _userID;

            //账号
            if (string.IsNullOrEmpty(_userID))
            {
                throw new Exception("Userid is mandatory!");
            }
            //版本
            if (string.IsNullOrEmpty(_version))
            {
                throw new Exception("Version is mandatory!");
            }
            //格式
            if (!string.IsNullOrEmpty(_format))
            {
                if (_format.ToLower() != "json") throw new Exception("Invalid Request Format");
            }
            else
            {
                throw new Exception("Format is mandatory!");
            }
            ////时间戳
            //if (!string.IsNullOrEmpty(_timestamp))
            //{
            //    try
            //    {
            //        TimeHelper.UnixTimestampToDateTime(VariableHelper.SaferequestInt64(_timestamp));
            //    }
            //    catch
            //    {
            //        throw new Exception("Invalid Timestamp format");
            //    }

            //    //时间差为正负10分钟
            //    DateTime _ts = TimeHelper.UnixTimestampToDateTime(VariableHelper.SaferequestInt64(_timestamp));
            //    if (DateTime.Compare(_ts, DateTime.Now.AddMinutes(-15)) < 0 || DateTime.Compare(_ts, DateTime.Now.AddMinutes(15)) > 0)
            //    {
            //        throw new Exception("Timestamp has expired!");
            //    }
            //}
            //else
            //{
            //    throw new Exception("Timestamp is mandatory!");
            //}
            ////签名
            //if (string.IsNullOrEmpty(_sign))
            //{
            //    throw new Exception("Sign is mandatory!");
            //}

            //账号是否存在
            var objWebApiAccount = objWebApiAccounts.Where(p => p.AppID == _userID).SingleOrDefault();
            if (objWebApiAccount != null)
            {
                string[] ip_areas = _ip.Split('.');
                //ip段限制,比如192.168.*.*
                string ip_area = string.Empty;
                for (int t = 0; t < ip_areas.Length; t++)
                {
                    if (t == 0)
                    {
                        ip_area += ip_areas[0];
                    }
                    else if (t == 1)
                    {
                        ip_area += "." + ip_areas[1];
                    }
                }
                ip_area = ip_area + ".*.*";
                ////是否允许IP
                //if (objWebApiAccount.Ips.Contains(_ip) || objWebApiAccount.Ips.Contains(ip_area))
                //{
                //    //去除签名字段
                //    paramDict.Remove("sign");
                //    //验证签名
                //    string _appSign = UtilsHelper.CreateSign(paramDict, objWebApiAccount.Token);
                //    if (_appSign == _sign)
                //    {
                //        //日志
                //        objWebApiAccessLog.State = true;
                //    }
                //    else
                //    {
                //        throw new Exception("Incorrect Signature!");
                //    }
                //}
                //else
                //{
                //    throw new Exception("Access Denied!");
                //}
            }
            else
            {
                throw new Exception("User ID does not exist!");
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
            FileLogHelper.WriteLog(new string[] { $"Client Ip:{objWebApiAccessLog.Ip}", $"RequestUri:{objWebApiAccessLog.Url}" }, DateTime.Now.ToString("HH"), $"{controllerName}/{actionName}");
        }
        //跳转继续执行
        base.OnActionExecuting(actionContext);
    }
}

