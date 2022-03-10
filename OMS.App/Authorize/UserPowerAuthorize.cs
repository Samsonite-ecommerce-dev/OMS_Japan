using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service;
using Samsonite.OMS.Service.AppLanguage;

public class UserPowerAuthorize : BaseAuthorize
{
    //默认页面跳转模式
    public ResultType Type { get; set; }
    //是否开启CSRF防御
    public bool IsAntiForgeryToken { get; set; }
    public override void OnAuthorization(AuthorizationContext filterContext)
    {
        //加载语言包
        var _LanguagePack = LanguageService.Get();

        using (var db = new ebEntities())
        {
            UserSessionInfo objUserSession = UserLoginService.GetCurrentLoginUser();
            try
            {
                if (objUserSession != null)
                {
                    //1.首次登入没有修改密码
                    //2.90天内没有修改密码
                    if (objUserSession.UserStatus == (int)UserStatus.ExpiredPwd)
                    {
                        this.GoEditPassword(Type, filterContext, _LanguagePack["common_alert_first_editpassword"]);
                    }
                    else
                    {
                        //防止CSRF攻击
                        if (IsAntiForgeryToken)
                        {
                            this.VerificationToken(_LanguagePack);
                        }
                        //当前权限
                        List<UserSessionInfo.UserPower> objUserPower_List = objUserSession.UserPowers;
                        string _controller = filterContext.RouteData.Values["controller"].ToString();
                        string _action = string.Empty;
                        if (Type == ResultType.Json)
                        {
                            //如果是数据处理页面,则取下划线前面的功能标识
                            string[] _actionArray = filterContext.RouteData.Values["action"].ToString().ToLower().Split('_');
                            _action = _actionArray[0];
                        }
                        else
                        {
                            _action = filterContext.RouteData.Values["action"].ToString();
                        }
                        //获取权限id
                        SysFunction objSysFunction = db.SysFunction.Where(p => p.FuncSign.ToLower() == _controller.ToLower()).SingleOrDefault();
                        if (objSysFunction != null)
                        {
                            var _O = objUserPower_List.Where(p => p.FunctionID == objSysFunction.Funcid).FirstOrDefault();
                            if (_O != null)
                            {
                                //比较操作权限
                                if (!_O.FunctionPower.Contains(_action.ToLower()))
                                {
                                    throw new Exception(_LanguagePack["common_alert_no_permission"]);
                                }
                            }
                            else
                            {
                                throw new Exception(_LanguagePack["common_alert_no_permission"]);
                            }
                        }
                        else
                        {
                            throw new Exception(_LanguagePack["common_alert_no_permission"]);
                        }
                    }
                }
                else
                {
                    throw new Exception(_LanguagePack["common_alert_no_login"]);
                }
            }
            catch (Exception ex)
            {
                this.GoError(Type, filterContext, ex.Message);
            }
        }
    }
}

