using System;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.Database;
using Samsonite.Utility.Common;
using Samsonite.OMS.Service;

using OMS.API.Models;
using System.Web.Http.Controllers;

namespace OMS.API.Utils
{
    public class AuthorizeHelper
    {
        /// <summary>
        /// 账号权限列表
        /// </summary>
        /// <returns></returns>
        public static List<AuthorizeUser> GetApiAccounts()
        {
            List<AuthorizeUser> _result = new List<AuthorizeUser>();
            using (var db = new ebEntities())
            {
                //账号列表
                var objWebApiAccounts = db.WebApiAccount.Where(p => p.IsUsed).ToList();
                //关联权限
                var objWebApiRoleses = db.WebApiRoles.ToList();

                foreach (var item in objWebApiAccounts)
                {
                    _result.Add(new AuthorizeUser()
                    {
                        Id = item.Id,
                        AppID = item.AppID,
                        Token = item.Token,
                        Ips = item.Ips,
                        Roles = objWebApiRoleses.Where(p => p.AccountID == item.Id).Select(o => o.InterfaceID).ToList()
                    });
                }
            }
            return _result;
        }

        /// <summary>
        /// 登入账号权限判断
        /// </summary>
        /// <param name="objActionContext"></param>
        /// <param name="objAuthorizeUsers"></param>
        /// <returns></returns>
        public static AuthorizeResult VisitValid(HttpActionContext objActionContext, List<AuthorizeUser> objAuthorizeUsers)
        {
            AuthorizeResult _result = new AuthorizeResult();
            AuthorizeParam _paramsRequest = new AuthorizeParam();
            try
            {
                //控制器
                var _controllerName = objActionContext.ControllerContext.ControllerDescriptor.ControllerName;
                var _actionName = objActionContext.ActionDescriptor.ActionName;
                //由于有设置[FromBody],需要重新调整内容指针,否则Request.Content.ReadAsStringAsync()无法读取到信息
                objActionContext.Request.Content.ReadAsStreamAsync().Result.Seek(0, System.IO.SeekOrigin.Begin);
                //访问地址
                _paramsRequest.Url = objActionContext.Request.RequestUri.PathAndQuery;
                //post的body中的内容
                _paramsRequest.PostBody = objActionContext.Request.Content.ReadAsStringAsync().Result;
                //访问者IP
                _paramsRequest.Ip = HttpContextHelper.GetIP();
                //基础参数
                IDictionary<string, string> _params = HttpContextHelper.GetRequestParams(objActionContext.Request.RequestUri.Query);
                _paramsRequest.Userid = (_params.ContainsKey("userid")) ? VariableHelper.SaferequestStr(_params["userid"]) : "";
                _paramsRequest.Version = (_params.ContainsKey("version")) ? VariableHelper.SaferequestStr(_params["version"]) : "";
                _paramsRequest.Format = (_params.ContainsKey("format")) ? VariableHelper.SaferequestStr(_params["format"]) : "";
                //默认MD5方式加密
                _paramsRequest.Method = (_params.ContainsKey("method")) ? VariableHelper.SaferequestStr(_params["method"]) : GlobalConfig.SIGN_METHOD_MD5;
                _paramsRequest.Timestamp = (_params.ContainsKey("timestamp")) ? VariableHelper.SaferequestStr(_params["timestamp"]) : "";
                _paramsRequest.Sign = (_params.ContainsKey("sign")) ? VariableHelper.SaferequestStr(_params["sign"]) : "";

                //账号
                if (string.IsNullOrEmpty(_paramsRequest.Userid))
                {
                    throw new Exception("Userid is mandatory!");
                }
                //版本
                if (string.IsNullOrEmpty(_paramsRequest.Version))
                {
                    throw new Exception("Version is mandatory!");
                }
                //格式
                if (!string.IsNullOrEmpty(_paramsRequest.Format))
                {
                    if (_paramsRequest.Format.ToLower() != "json") throw new Exception("Invalid Request Format");
                }
                else
                {
                    throw new Exception("Format is mandatory!");
                }
                //时间戳
                if (!string.IsNullOrEmpty(_paramsRequest.Timestamp))
                {
                    try
                    {
                        TimeHelper.UnixTimestampToDateTime(VariableHelper.SaferequestInt64(_paramsRequest.Timestamp));
                    }
                    catch
                    {
                        throw new Exception("Invalid Timestamp format");
                    }

                    //时间差为正负10分钟
                    long diffTimes = 10 * 60;
                    long _ts = VariableHelper.SaferequestInt64(_paramsRequest.Timestamp);
                    long _now = TimeHelper.DateTimeToUnixTimestamp(DateTime.Now);
                    if ((_ts > _now + diffTimes) || _ts < _now - diffTimes)
                    {
                        throw new Exception("Timestamp has expired!");
                    }
                }
                else
                {
                    throw new Exception("Timestamp is mandatory!");
                }
                //签名
                if (string.IsNullOrEmpty(_paramsRequest.Sign))
                {
                    throw new Exception("Sign is mandatory!");
                }

                var objAuthorizeUser = objAuthorizeUsers.Where(p => p.AppID == _paramsRequest.Userid).SingleOrDefault();
                if (objAuthorizeUser != null)
                {
                    string[] ip_areas = _paramsRequest.Ip.Split('.');
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
                    //是否允许IP
                    if (objAuthorizeUser.Ips.Contains(_paramsRequest.Ip) || objAuthorizeUser.Ips.Contains(ip_area))
                    {
                        //去除签名字段
                        _params.Remove("sign");
                        //验证签名
                        string _appSign = UtilsHelper.CreateSign(_params, objAuthorizeUser.Token, _paramsRequest.Method);
                        if (_appSign == _paramsRequest.Sign)
                        {
                            //权限列表
                            var objInterfaces = ApiService.InterfaceOptions();
                            //查询当前action的ID
                            var objGroup = objInterfaces.Where(p => p.ControllerName.ToUpper() == _controllerName.ToUpper()).SingleOrDefault();
                            if (objGroup != null)
                            {
                                var objInterface = objGroup.Interfaces.Where(p => p.ActionName.ToUpper() == _actionName.ToUpper()).SingleOrDefault();
                                if (objInterface != null)
                                {
                                    if (objAuthorizeUser.Roles.Contains(objInterface.ID))
                                    {
                                        //返回信息
                                        _result = new AuthorizeResult()
                                        {
                                            Result = true,
                                            Message = string.Empty,
                                            Params = _paramsRequest
                                        };
                                    }
                                    else
                                    {
                                        throw new Exception("Sorry,You have no access to perform this action!");
                                    }
                                }
                                else
                                {
                                    throw new Exception("Sorry,You have no access to perform this action!");
                                }
                            }
                            else
                            {
                                throw new Exception("Sorry,You have no access to perform this action!");
                            }
                        }
                        else
                        {
                            throw new Exception("Incorrect Signature!");
                        }
                    }
                    else
                    {
                        throw new Exception("Access Denied!");
                    }
                }
                else
                {
                    throw new Exception("User ID does not exist!");
                }
            }
            catch (Exception ex)
            {
                //返回信息
                _result = new AuthorizeResult()
                {
                    Result = false,
                    Message = ex.Message,
                    Params = _paramsRequest
                };
            }
            return _result;
        }
    }
}