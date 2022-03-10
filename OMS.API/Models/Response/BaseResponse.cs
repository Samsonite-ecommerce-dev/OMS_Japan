using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OMS.API.Models
{
    public class BaseResponse
    {
        /// <summary>
        /// 返回结果
        /// </summary>
        public bool Result { get; set; }
        /// <summary>
        /// 返回信息
        /// </summary>
        public string Message { get; set; }
    }
}