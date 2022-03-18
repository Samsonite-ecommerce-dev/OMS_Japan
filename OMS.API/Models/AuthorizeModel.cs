using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OMS.API.Models
{
    public class AuthorizeUser
    {
        /// <summary>
        /// 账号ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 账号名
        /// </summary>
        public string AppID { get; set; }

        /// <summary>
        /// 秘钥
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// ip限制
        /// </summary>
        public string Ips { get; set; }

        /// <summary>
        /// 权限组
        /// </summary>
        public List<int> Roles { get; set; }
    }

    public class AuthorizeParam : ApiRequest
    {
        /// <summary>
        /// 访问地址
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 访问地址
        /// </summary>
        public string PostBody { get; set; }

        /// <summary>
        /// 访问者IP
        /// </summary>
        public string Ip { get; set; }
    }

    public class AuthorizeResult
    {
        /// <summary>
        /// 返回结果
        /// </summary>
        public bool Result { get; set; }

        /// <summary>
        /// 获取的参数
        /// </summary>
        public AuthorizeParam Params { get; set; }

        /// <summary>
        /// 返回信息
        /// </summary>
        public string Message { get; set; }
    }
}