using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Samsonite.Utility.Common;

using OMS.API.Interface.ClickCollect;
using OMS.API.Models;
using OMS.API.Models.ClickCollect;
using OMS.API.Utils;

namespace OMS.API.Controllers
{
    [RoutePrefix("api/click_collect")]
    public class ClickCollectController : BaseApiController
    {
        private IQueryService _queryService;
        private IPostService _postService;
        public ClickCollectController(IQueryService queryService, IPostService postService)
        {
            this._queryService = queryService;
            this._postService = postService;
        }

        #region order api
        /// <summary>
        /// 获取订单数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("orders/get")]
        public ApiPageResponse GetOrders([FromUri]GetOrdersRequest request)
        {
            ApiPageResponse result = new ApiPageResponse();

            //过滤参数
            VariableHelper.Validate(request);
            request.PageSize = UtilsHelper.ValidatePageSize(request.PageSize);
            request.PageIndex = UtilsHelper.ValidatePageIndex(request.PageIndex);

            try
            {
                var _res = _queryService.GetOrders(request);
                //返回信息
                result.Code = (int)ApiResultCode.Success;
                result.PageTotal = _res.totalPage;
                result.PageSize = request.PageSize;
                result.PageIndex = request.PageIndex;
                result.Total = _res.totalRecord;
                result.SuccessfulData = _res.Trades;
            }
            catch (Exception ex)
            {
                //返回信息
                result.Code = (int)ApiResultCode.Fail;
                result.Message = ex.Message;
                result.PageTotal = 0;
                result.PageSize = request.PageSize;
                result.PageIndex = request.PageIndex;
                result.Total = 0;
            }
            return result;
        }

        /// <summary>
        /// 获取单条订单数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("order/items/get")]
        public ApiResponse GetOrderItems([FromUri]GetOrderItemsRequest request)
        {
            ApiResponse result = new ApiResponse();

            //过滤参数
            VariableHelper.Validate(request);

            try
            {
                var _res = _queryService.GetOrderItems(request);
                //返回信息
                result.Code = (int)ApiResultCode.Success;
                result.SuccessfulData = _res.TradeInfo;
            }
            catch (Exception ex)
            {
                //返回信息
                result.Code = (int)ApiResultCode.Fail;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 更新状态(仓库到货)
        /// 使用FromBody获取数据时,Post端请求数据格式需要为ContentType = "application/json"
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("order/arrived")]
        public ApiResponse SetStatusToShopArrived([FromBody]object request)
        {
            ApiResponse result = new ApiResponse();

            //过滤参数
            string data = VariableHelper.SaferequestNull(request);

            if (GlobalConfig.IsApiDebugLog)
            {
                FileLogHelper.WriteLog($"PostJson: {data}", DateTime.Now.ToString("HH"), this.ControllerContext.ControllerDescriptor.ControllerName);
            }

            try
            {
                var _result = _postService.SetStatusToShopArrived(data);
                //返回结果
                result = ApiResponse.Success(_result.Where(p => p.Result), _result.Where(p => !p.Result));
            }
            catch (Exception ex)
            {
                //返回信息
                result.Code = (int)ApiResultCode.Fail;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 更新状态(取货完成)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("order/delivered")]
        public ApiResponse SetStatusToDelivered([FromBody]object request)
        {
            ApiResponse result = new ApiResponse();

            //过滤参数
            string data = VariableHelper.SaferequestNull(request);

            if (GlobalConfig.IsApiDebugLog)
            {
                FileLogHelper.WriteLog($"PostJson: {data}", DateTime.Now.ToString("HH"), this.ControllerContext.ControllerDescriptor.ControllerName);
            }

            try
            {
                var _result = _postService.SetStatusToDelivered(data);
                //返回结果
                result = ApiResponse.Success(_result.Where(p => p.Result), _result.Where(p => !p.Result));
            }
            catch (Exception ex)
            {
                //返回信息
                result.Code = (int)ApiResultCode.Fail;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 门店处理已经取消的到店产品
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("order/cancel_handel")]
        public ApiResponse CanceledItemHandle([FromBody]object request)
        {
            ApiResponse result = new ApiResponse();

            //过滤参数
            string data = VariableHelper.SaferequestNull(request);

            if (GlobalConfig.IsApiDebugLog)
            {
                FileLogHelper.WriteLog($"PostJson: {data}", DateTime.Now.ToString("HH"), this.ControllerContext.ControllerDescriptor.ControllerName);
            }

            try
            {
                var _result = _postService.CanceledItemHandle(data);
                //返回结果
                result = ApiResponse.Success(_result.Where(p => p.Result), _result.Where(p => !p.Result));
            }
            catch (Exception ex)
            {
                //返回信息
                result.Code = (int)ApiResultCode.Fail;
                result.Message = ex.Message;
            }
            return result;
        }
        #endregion
    }
}
