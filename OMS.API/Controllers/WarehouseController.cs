using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

using Samsonite.OMS.Database;
using Samsonite.OMS.Service;
using Samsonite.Utility.Common;

using OMS.API.Models;
using OMS.API.Models.Warehouse;
using OMS.API.Interface.Warehouse;
using OMS.API.Utils;

namespace OMS.API.Controllers
{
    [RoutePrefix("api/warehouse")]
    public class WarehouseController : ApiController
    {
        private IQueryService _queryService;
        private IPostService _postService;
        public WarehouseController(IQueryService queryService, IPostService postService)
        {
            this._queryService = queryService;
            this._postService = postService;
        }

        /// <summary>
        /// 获取所有新订单数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getOrders")]
        public ApiPageResponse GetOrders([FromUri]GetOrdersRequest request)
        {
            ApiPageResponse result = new ApiPageResponse();

            //过滤参数
            VariableHelper.Validate(request);
            request.PageSize = UtilsHelper.ValidatePageSize(request.PageSize);
            request.PageIndex = UtilsHelper.ValidatePageIndex(request.PageIndex);

            try
            {
                if (string.IsNullOrEmpty(request.StartDate))
                {
                    throw new Exception("Please input a Start Date!");
                }

                if (string.IsNullOrEmpty(request.EndDate))
                {
                    throw new Exception("Please input a End Date!");
                }

                var _res = _queryService.GetOrders(request);
                //返回信息
                result.Code = (int)ApiResultCode.Success;
                result.PageTotal = _res.totalPage;
                result.PageSize = request.PageSize;
                result.PageIndex = request.PageIndex;
                result.Total = _res.totalRecord;
                result.SuccessfulData = _res.Data;
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
                result.SuccessfulData = null;
            }
            return result;
        }

        /// <summary>
        /// 获取变更的订单信息 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getChangedOrders")]
        public ApiPageResponse GetChangedOrders([FromUri]GetChangedOrdersRequest request)
        {
            ApiPageResponse result = new ApiPageResponse();

            //过滤参数
            VariableHelper.Validate(request);
            request.PageSize = UtilsHelper.ValidatePageSize(request.PageSize);
            request.PageIndex = UtilsHelper.ValidatePageIndex(request.PageIndex);

            try
            {
                if (string.IsNullOrEmpty(request.StartDate))
                {
                    throw new Exception("Please input a Start Date!");
                }

                if (string.IsNullOrEmpty(request.EndDate))
                {
                    throw new Exception("Please input a End Date!");
                }

                var _res = _queryService.GetChangedOrders(request);
                //返回信息
                result.Code = (int)ApiResultCode.Success;
                result.PageTotal = _res.totalPage;
                result.PageSize = request.PageSize;
                result.PageIndex = request.PageIndex;
                result.Total = _res.totalRecord;
                result.SuccessfulData = _res.Data;
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
                result.SuccessfulData = null;
            }
            return result;
        }

        /// <summary>
        /// 回复操作状态
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("postReply")]
        public ApiResponse PostReply()
        {
            ApiResponse result = new ApiResponse();
            using (var db = new ebEntities())
            {
                try
                {
                    var _jsonData = Request.Content.ReadAsStringAsync().Result;

                    if (GlobalConfig.IsApiDebugLog)
                    {
                        FileLogHelper.WriteLog($"PostJson: {_jsonData}", DateTime.Now.ToString("HH"), this.ControllerContext.ControllerDescriptor.ControllerName);
                    }

                    var _models = JsonHelper.JsonDeserialize<List<PostReplyRequest>>(_jsonData);
                    if (_models != null)
                    {
                        List<PostReplyResponse> _result = _postService.SavePostReplys(_models);
                        //返回结果
                        result = ApiResponse.Success(_result.Where(p => p.Result), _result.Where(p => !p.Result));
                    }
                    else
                    {
                        throw new Exception("Please input a request data!");
                    }

                }
                catch (Exception ex)
                {
                    //返回结果
                    result = ApiResponse.Fail(ex.Message);
                }
            }
            return result;
        }

        /// <summary>
        /// 提交快递信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("postDelivery")]
        public ApiResponse PostDelivery()
        {
            ApiResponse result = new ApiResponse();
            try
            {
                var _jsonData = Request.Content.ReadAsStringAsync().Result;

                if (GlobalConfig.IsApiDebugLog)
                {
                    FileLogHelper.WriteLog($"PostJson: {_jsonData}", DateTime.Now.ToString("HH"), this.ControllerContext.ControllerDescriptor.ControllerName);
                }

                var _models = JsonHelper.JsonDeserialize<List<PostDeliverysRequest>>(_jsonData);
                if (_models != null)
                {
                    List<PostDeliverysResponse> _result = _postService.SaveDeliverys(_models);
                    //返回结果
                    result = ApiResponse.Success(_result.Where(p => p.Result), _result.Where(p => !p.Result));
                }
                else
                {
                    throw new Exception("Please input a request data!");
                }
            }
            catch (Exception ex)
            {
                //返回结果
                result = ApiResponse.Fail(ex.Message);
            }
            return result;
        }

        /// <summary>
        /// 更新库存
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("postInventory")]
        //使用FromBody获取数据时,Post端请求数据格式需要为ContentType = "application/json"
        //public ApiResult Post([FromBody] object res)
        public ApiResponse PostInventory()
        {
            ApiResponse result = new ApiResponse();
            using (var db = new ebEntities())
            {
                try
                {
                    var res = Request.Content.ReadAsStringAsync().Result;

                    if (GlobalConfig.IsApiDebugLog)
                    {
                        FileLogHelper.WriteLog($"PostJson: {res.ToString()}", DateTime.Now.ToString("HH"), this.ControllerContext.ControllerDescriptor.ControllerName);
                    }

                    var _models = JsonHelper.JsonDeserialize<List<PostInventoryRequest>>(res.ToString());
                    if (_models != null)
                    {
                        //------------每次更新库存前,清空旧库存数据-----------------------------------------------------
                        //注:1.只重置下架中的产品库存
                        //   2.解决WMS不推送0库存的问题
                        //   3.WMS需要一次性推送完所有库存,反向置0的产品不更新时间
                        ProductService.ResetZeroInvenroty();
                        //-----------------------------------------------------------------------------------------------

                        //计算当前WMS还未获取的订单数量
                        var _reduceQuantitys = ProductService.CalculateUnRequireOrder();
                        List<PostInventoryResponse> _result = _postService.SaveInventorys(_models, _reduceQuantitys);
                        //计算套装数量-----------------------------------------------------------------------------------
                        InventoryService.CalculateBundleInventory_OnSale();
                        //-----------------------------------------------------------------------------------------------
                        //返回结果
                        result = ApiResponse.Success(_result.Where(p => p.Result), _result.Where(p => !p.Result));
                    }
                    else
                    {
                        throw new Exception("Please input a request data!");
                    }
                }
                catch (Exception ex)
                {
                    //返回结果
                    result = ApiResponse.Fail(ex.Message);
                }
            }
            return result;
        }

        /// <summary>
        /// 提交订单关联信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("postDetail")]
        public ApiResponse PostDetail()
        {
            ApiResponse result = new ApiResponse();
            try
            {
                var _jsonData = Request.Content.ReadAsStringAsync().Result;

                if (GlobalConfig.IsApiDebugLog)
                {
                    FileLogHelper.WriteLog($"PostJson: {_jsonData}", DateTime.Now.ToString("HH"), this.ControllerContext.ControllerDescriptor.ControllerName);
                }

                var _models = JsonHelper.JsonDeserialize<List<PostDetailRequest>>(_jsonData);
                if (_models != null)
                {
                    List<PostDetailResponse> _result = _postService.SaveReplyDetails(_models);
                    //返回结果
                    result = ApiResponse.Success(_result.Where(p => p.Result), _result.Where(p => !p.Result));
                }
                else
                {
                    throw new Exception("Please input a request data!");
                }
            }
            catch (Exception ex)
            {
                //返回结果
                result = ApiResponse.Fail(ex.Message);
            }
            return result;
        }
    }
}
