using System;

namespace OMS.API.Models
{
    /// <summary>
    /// WebApi的返回结果
    /// </summary>
    public class ApiResponse
    {
        /// <summary>
        /// 请求处理单号
        /// </summary>
        public string RequestID { get; set; }

        /// <summary>
        /// 结果代码（100表示成功，其它表示失败）
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// 结果消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 返回成功数据
        /// </summary>
        public object SuccessfulData { get; set; }

        /// <summary>
        /// 返回失败数据
        /// </summary>
        public object FailData { get; set; }

        /// <summary>
        /// 数据获取成功
        /// </summary>
        /// <returns></returns>
        public static ApiResponse Success()
        {
            return new ApiResponse()
            {
                Code = (int)ApiResultCode.Success
            };
        }

        /// <summary>
        /// 数据获取成功
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ApiResponse Success(object data)
        {
            return new ApiResponse()
            {
                Code = (int)ApiResultCode.Success,
                SuccessfulData = data
            };
        }

        /// <summary>
        /// 数据获取成功
        /// </summary>
        /// <param name="successfuldata"></param>
        /// <param name="faildata"></param>
        /// <returns></returns>
        public static ApiResponse Success(object successfuldata, object faildata)
        {
            return new ApiResponse()
            {
                Code = (int)ApiResultCode.Success,
                SuccessfulData = successfuldata,
                FailData = faildata
            };
        }

        /// <summary>
        /// 数据获取失败
        /// </summary>
        /// <param name="message">异常消息</param>
        /// <returns></returns>
        public static ApiResponse Fail(string message)
        {
            return new ApiResponse()
            {
                Code = (int)ApiResultCode.Fail,
                Message = message
            };
        }
    }

    /// <summary>
    /// API 分页返回格式
    /// </summary>
    public class ApiPageResponse : ApiResponse
    {
        /// <summary>
        /// 记录总数
        /// </summary>
        public long Total { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int PageTotal { get; set; }

        /// <summary>
        /// 当前页面
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 每页数据大小
        /// </summary>
        public int PageSize { get; set; }
    }
}