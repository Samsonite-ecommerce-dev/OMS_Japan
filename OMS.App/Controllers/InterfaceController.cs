using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Samsonite.Utility.Common;

namespace OMS.App.Controllers
{
    public class InterfaceController : Controller
    {
        public InterfaceController()
        {

        }

        /// <summary>
        /// IP限制
        /// </summary>
        /// <returns></returns>
        private List<string> LimitIP()
        {
            List<string> _IPs = new List<string>();
            _IPs.Add("127.0.0.1");
            _IPs.Add("10.40.32.199");
            return _IPs;
        }

        /// <summary>
        /// 判断IP是否允许访问
        /// </summary>
        /// <param name="objIP"></param>
        /// <returns></returns>
        public bool IsAllowVisit(string objIP)
        {
            return (LimitIP().Contains(objIP));
        }

        public class ResultMessage
        {
            /// <summary>
            /// 返回结果
            /// </summary>
            public bool Result { get; set; }

            /// <summary>
            /// 返回的错误号
            /// </summary>
            public string ReturnCode { get; set; }

            /// <summary>
            /// 返回信息
            /// </summary>
            public string ReturnMessage { get; set; }
        }
    }
}
