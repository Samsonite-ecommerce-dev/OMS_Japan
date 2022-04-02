using System;
using System.Web.Http;
using Samsonite.Utility.Common;

using OMS.API.Interface.Platform;
using OMS.API.Models;
using OMS.API.Models.Platform;
using OMS.API.Utils;

namespace OMS.API.Controllers
{
    [RoutePrefix("api/platform")]
    public class PlatformController : BaseApiController
    {
        private IQueryService _queryService;
        public PlatformController(IQueryService queryService)
        {
            this._queryService = queryService;
        }

        #region store
        /// <summary>
        /// 获取线下店铺数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("stores/get")]
        public ApiPageResponse GetStores([FromUri]GetStoresRequest request)
        {
            ApiPageResponse result = new ApiPageResponse();

            //过滤参数
            VariableHelper.Validate(request);
            request.PageSize = UtilsHelper.ValidatePageSize(request.PageSize);
            request.PageIndex = UtilsHelper.ValidatePageIndex(request.PageIndex);

            try
            {
                var _res = _queryService.GetStores(request);
                //返回信息
                result.Code = (int)ApiResultCode.Success;
                result.PageTotal = _res.totalPage;
                result.PageSize = request.PageSize;
                result.PageIndex = request.PageIndex;
                result.Total = _res.totalRecord;
                result.SuccessfulData = _res.Stores;
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
        #endregion

        #region order
        /// <summary>
        /// 获取订单详情信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("order/details")]
        public ApiResponse GetOrdersDetail([FromUri]GetOrdersDetailRequest request)
        {
            ApiResponse result = new ApiResponse();

            //过滤参数
            VariableHelper.Validate(request);

            try
            {
                var _res = _queryService.GetOrdersDetail(request);
                //返回信息
                result.Code = (int)ApiResultCode.Success;
                result.SuccessfulData = _res.Stores;
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

        #region inventory
        /// <summary>
        /// 获取库存信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("inventorys/get")]
        public ApiPageResponse GetInventorys([FromUri]GetInventorysRequest request)
        {
            ApiPageResponse result = new ApiPageResponse();

            //过滤参数
            VariableHelper.Validate(request);
            request.PageSize = UtilsHelper.ValidatePageSize(request.PageSize);
            request.PageIndex = UtilsHelper.ValidatePageIndex(request.PageIndex);

            try
            {
                var _res = _queryService.GetInventorys(request);
                //返回信息
                result.Code = (int)ApiResultCode.Success;
                result.PageTotal = _res.totalPage;
                result.PageSize = request.PageSize;
                result.PageIndex = request.PageIndex;
                result.Total = _res.totalRecord;
                result.SuccessfulData = _res;
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
        #endregion
    }
}
