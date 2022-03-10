using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Service.AppLanguage;
using Samsonite.Utility.Common;

namespace Samsonite.OMS.Service
{
    public class UserLoginService
    {
        /// <summary>
        /// 账号登录
        /// </summary>
        /// <param name="objUserName"></param>
        /// <param name="objPassword"></param>
        /// <param name="objIsRemember">默认保存1天，记住密码保存30天</param>
        /// <returns></returns>
        public static object[] UserLogin(string objUserName, string objPassword, bool objIsRemember)
        {
            object[] _result = new object[2];

            using (var db = new ebEntities())
            {
                //加载语言包
                var _LanguagePack = LanguageService.Get();
                try
                {
                    if (string.IsNullOrEmpty(objUserName)) throw new Exception(_LanguagePack["login_index_message_no_username"]);
                    if (string.IsNullOrEmpty(objPassword)) throw new Exception(_LanguagePack["login_index_message_no_password"]);

                    UserInfo objUserInfo = db.UserInfo.SingleOrDefault<UserInfo>(p => p.UserName == objUserName);
                    if (objUserInfo != null)
                    {
                        if (objUserInfo.Pwd.ToLower() == EncryptPassword(objPassword).ToLower())
                        {
                            if (objUserInfo.Status == (int)UserStatus.Locked)
                            {
                                throw new Exception(_LanguagePack["login_index_message_username_lock"]);
                            }

                            //查看密码是否已经过期
                            bool IsExpired = false;
                            if (objUserInfo.LastPwdEditTime == null)
                            {
                                IsExpired = true;
                            }
                            else
                            {
                                if (objUserInfo.LastPwdEditTime.Value.AddDays(AppGlobalService.PWD_VALIDITY_TIME) < DateTime.Now)
                                {
                                    IsExpired = true;
                                }
                            }
                            if (IsExpired)
                            {
                                objUserInfo.Status = (int)UserStatus.ExpiredPwd;
                                db.SaveChanges();
                            }

                            //写入登录信息
                            UserSessionInfo objUserSessionInfo = new UserSessionInfo();
                            objUserSessionInfo.Userid = (int)objUserInfo.UserID;
                            objUserSessionInfo.UserName = objUserInfo.UserName;
                            objUserSessionInfo.Passwd = objUserInfo.Pwd;
                            objUserSessionInfo.UserType = objUserInfo.Type;
                            objUserSessionInfo.UserStatus = objUserInfo.Status;
                            objUserSessionInfo.UserPowers = GetUserFunctions(objUserSessionInfo.Userid);
                            objUserSessionInfo.UserMalls = GetUserMalls(objUserSessionInfo.Userid);
                            objUserSessionInfo.DefaultLanguage = objUserInfo.DefaultLanguage;
                            //上机日志表
                            AppLogService.LoginLog(new WebAppLoginLog()
                            {
                                LoginStatus = true,
                                Account = objUserInfo.UserName,
                                Password = EncryptPassword(objPassword),
                                UserID = objUserInfo.UserID,
                                IP = UrlHelper.GetRequestIP(),
                                Remark = string.Empty,
                                AddTime = DateTime.Now
                            });
                            //写入Session
                            SetUserSession(objUserSessionInfo);
                            //写入Cookie
                            if (objIsRemember)
                            {
                                SetUserCookie(objUserSessionInfo, 1);
                            }
                            //设置默认语言
                            LanguageService.SetLanguage(objUserInfo.DefaultLanguage);
                            //返回信息
                            _result[0] = true;
                            _result[1] = string.Empty;
                        }
                        else
                        {
                            //保存日志
                            AppLogService.LoginLog(new WebAppLoginLog()
                            {
                                LoginStatus = false,
                                Account = objUserName,
                                Password = EncryptPassword(objPassword),
                                UserID = 0,
                                IP = UrlHelper.GetRequestIP(),
                                Remark = _LanguagePack["login_index_message_err"],
                                AddTime = DateTime.Now
                            });

                            //读取今天最近的5次该账号记录,如果全部出错,则直接锁定帐号
                            using (var db1 = new logEntities())
                            {
                                List<WebAppLoginLog> objWebAppLoginLog_List = db1.Database.SqlQuery<WebAppLoginLog>("select top " + AppGlobalService.PWDERROR_LOCK_NUM + " * from WebAppLoginLog where Account={0} and datediff(day,AddTime,{1})=0 order by LogID desc", objUserName, DateTime.Today.ToString("yyyy-MM-dd")).ToList();
                                if (objWebAppLoginLog_List.Where(p => !p.LoginStatus).Count() == AppGlobalService.PWDERROR_LOCK_NUM)
                                {
                                    objUserInfo.Status = (int)UserStatus.Locked;
                                    db.SaveChanges();

                                    //返回信息
                                    _result[0] = false;
                                    _result[1] = _LanguagePack["login_index_message_username_lock"];
                                }
                                else
                                {
                                    //返回信息
                                    _result[0] = false;
                                    _result[1] = _LanguagePack["login_index_message_err"];
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new Exception(_LanguagePack["login_index_message_username_not_exist"]);
                    }
                }
                catch (Exception ex)
                {
                    //保存日志
                    AppLogService.LoginLog(new WebAppLoginLog()
                    {
                        LoginStatus = false,
                        Account = objUserName,
                        Password = EncryptPassword(objPassword),
                        UserID = 0,
                        IP = UrlHelper.GetRequestIP(),
                        Remark = ex.Message,
                        AddTime = DateTime.Now
                    });

                    //返回信息
                    _result[0] = false;
                    _result[1] = ex.Message;
                }
            }
            return _result;
        }

        /// <summary>
        /// 获取当前登录用户，不存在返回null
        /// </summary>
        /// <returns></returns>
        public static UserSessionInfo GetCurrentLoginUser()
        {
            UserSessionInfo objUserSessionInfo = null;
            try
            {
                if (HttpContext.Current.Session[string.Format("{0}_LoginMessage", AppGlobalService.SESSION_KEY)] != null)
                {
                    objUserSessionInfo = (UserSessionInfo)System.Web.HttpContext.Current.Session[string.Format("{0}_LoginMessage", AppGlobalService.SESSION_KEY)];
                }
                else
                {
                    //如果不存在，则去读cookie
                    HttpCookie objCookie = System.Web.HttpContext.Current.Request.Cookies[string.Format("{0}_LoginMessage", AppGlobalService.COOKIE_KEY)];
                    if (objCookie != null)
                    {
                        //Encrypt解密
                        string _UserName = EncryptHelper.DecryptPassWord(objCookie["Uname"].ToString());
                        string _UserPass = EncryptHelper.DecryptPassWord(objCookie["Upass"].ToString());
                        objUserSessionInfo = GetUserMessage(_UserName, _UserPass);
                    }
                }
            }
            catch
            {
                objUserSessionInfo = null;
            }
            return objUserSessionInfo;
        }

        /// <summary>
        /// 用户退出
        /// </summary>
        public static void UserLoginOut()
        {
            //清空session
            System.Web.HttpContext.Current.Session.Remove(string.Format("{0}_LoginMessage", AppGlobalService.SESSION_KEY));
            System.Web.HttpContext.Current.Session.Abandon();
            //清空cookie
            CookieHelper.ClearCookie(string.Format("{0}_LoginMessage", AppGlobalService.COOKIE_KEY));

        }

        /// <summary>
        /// 获取当前用户ID
        /// </summary>
        public static int GetCurrentUserID
        {
            get
            {
                UserSessionInfo objUserSessionInfo = GetCurrentLoginUser();
                return (objUserSessionInfo != null) ? objUserSessionInfo.Userid : 0;
            }
        }

        //根据账户和密码(md5加密后)获取信息
        private static UserSessionInfo GetUserMessage(string objUserName, string objUserPass)
        {
            UserSessionInfo objUserSessionInfo = new UserSessionInfo();
            using (var db = new ebEntities())
            {
                UserInfo objUserInfo = db.UserInfo.SingleOrDefault<UserInfo>(p => p.UserName == objUserName && p.Pwd == objUserPass.ToLower());
                if (objUserInfo != null)
                {
                    objUserSessionInfo.Userid = objUserInfo.UserID;
                    objUserSessionInfo.UserName = objUserInfo.UserName;
                    objUserSessionInfo.Passwd = objUserInfo.Pwd;
                    objUserSessionInfo.UserType = objUserInfo.Type;
                    objUserSessionInfo.UserStatus = objUserInfo.Status;
                    objUserSessionInfo.UserPowers = GetUserFunctions(objUserSessionInfo.Userid);
                    objUserSessionInfo.UserMalls = GetUserMalls(objUserSessionInfo.Userid);
                    if (objUserSessionInfo.UserMalls.Count == 0) objUserSessionInfo.UserMalls.Add("0");
                    objUserSessionInfo.DefaultLanguage = objUserInfo.DefaultLanguage;
                    //写入Session
                    SetUserSession(objUserSessionInfo);
                }
                else
                {
                    objUserSessionInfo = null;
                }
            }
            return objUserSessionInfo;
        }

        /// <summary>
        /// 保存session
        /// </summary>
        /// <param name="objUserSessionInfo"></param>
        private static void SetUserSession(UserSessionInfo objUserSessionInfo)
        {
            System.Web.HttpContext.Current.Session[string.Format("{0}_LoginMessage", AppGlobalService.SESSION_KEY)] = objUserSessionInfo;
        }

        /// <summary>
        /// 保存cookie
        /// </summary>
        /// <param name="objUserSessionInfo"></param>
        /// <param name="objTime"></param>
        private static void SetUserCookie(UserSessionInfo objUserSessionInfo, int objTime)
        {
            Dictionary<string, string> objDictionary = new Dictionary<string, string>();
            //Encrypt加密
            objDictionary.Add("Uname", EncryptHelper.EncryptPassWord(objUserSessionInfo.UserName));
            objDictionary.Add("Upass", EncryptHelper.EncryptPassWord(objUserSessionInfo.Passwd));
            CookieHelper.InsertCookie(string.Format("{0}_LoginMessage", AppGlobalService.COOKIE_KEY), objDictionary, objTime);
        }

        /// <summary>
        /// 获取用户的权限
        /// </summary>
        /// <param name="objUserid"></param>
        private static List<UserSessionInfo.UserPower> GetUserFunctions(int objUserid)
        {
            List<UserSessionInfo.UserPower> _result = new List<UserSessionInfo.UserPower>();
            using (var db = new ebEntities())
            {
                //获取用户权限组
                List<int> objUserRoles_List = db.UserRoles.Where(p => p.Userid == objUserid).Select(p => p.Roleid).ToList();
                if (objUserRoles_List.Count > 0)
                {
                    //获取具体权限
                    var objSysRoleFunction_List = db.SysRoleFunction.Where(p => objUserRoles_List.Contains(p.Roleid));
                    foreach (var _o in objSysRoleFunction_List)
                    {
                        List<string> _OperPowers = new List<string>();
                        //操作权限
                        foreach (string _str in _o.Powers.Split(','))
                        {
                            _OperPowers.Add(_str.ToLower());
                        }
                        var _p = _result.Where(p => p.FunctionID == _o.Funid).FirstOrDefault();
                        if (_p != null)
                        {
                            _p.FunctionPower = MergePowers(_p.FunctionPower, _OperPowers);
                        }
                        else
                        {
                            _result.Add(new UserSessionInfo.UserPower() { FunctionID = _o.Funid, FunctionPower = _OperPowers });
                        }
                    }
                }
            }
            return _result;
        }

        /// <summary>
        /// 合并多个操作权限集合
        /// </summary>
        /// <returns></returns>
        private static List<string> MergePowers(List<string> objOrgPower, List<string> objNewPower)
        {
            foreach (string _str in objNewPower)
            {
                if (!objOrgPower.Contains(_str))
                {
                    objOrgPower.Add(_str.ToLower());
                }
            }
            return objOrgPower;
        }

        /// <summary>
        /// 获取用户店铺权限
        /// </summary>
        /// <param name="objUserid"></param>
        /// <returns></returns>
        private static List<string> GetUserMalls(int objUserid)
        {
            List<string> _result = new List<string>();
            using (var db = new ebEntities())
            {
                _result = db.UserMalls.Where(p => p.Userid == objUserid).Select(p => p.MallSapCode).ToList();
            }
            return _result;
        }

        /// <summary>
        /// 密码加密
        /// </summary>
        /// <param name="objPassword"></param>
        /// <returns></returns>
        public static string EncryptPassword(string objPassword)
        {
            string _result = string.Empty;
            //先Md5加密
            _result = EncryptHelper.Md5_32(objPassword);
            //再AES加密
            _result = _result.AesEncrypt();
            return _result;
        }

        /// <summary>
        /// 创建密钥
        /// </summary>
        /// <param name="objLength"></param>
        /// <returns></returns>
        public static string CreatePrivateKey(int objLength)
        {
            string _result = string.Empty;
            string[] _array = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            Random rnd = new Random();
            for (int i = 0; i < objLength; i++)
            {
                rnd = new Random(int.Parse(DateTime.Now.ToString("HHmmssfff")) + i);
                _result += _array[rnd.Next(0, 61)];
            }
            return _result;
        }
    }
}