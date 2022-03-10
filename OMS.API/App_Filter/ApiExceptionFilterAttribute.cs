using System;
using System.Net.Http;
using System.Web.Http.Filters;

using OMS.API.Models;
using OMS.API.Utils;

public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
{
    public override void OnException(HttpActionExecutedContext context)
    {
        var result = new ApiResponse()
        {
            Code = (int)ApiResultCode.Fail,
            Message = context.Exception.Message
        };
        context.Response = new HttpResponseMessage()
        {
            Content = UtilsHelper.ContextResponse(result)
        };
    }
}