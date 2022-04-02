using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.Utility.Common;
using Samsonite.OMS.Service.AppConfig;

using OMS.API.Models.Platform;
using OMS.API.Interface.Platform;


namespace OMS.API.Implments.Platform
{
    public class QueryService : IQueryService
    {
        private EntityRepository _entityRepository;
        public QueryService()
        {
            _entityRepository = new EntityRepository();
        }

        #region store
        /// <summary>
        /// 订单线下店铺列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public GetStoresResponse GetStores(GetStoresRequest request)
        {
            GetStoresResponse _result = new GetStoresResponse();
            using (var db = new ebEntities())
            {
                var _list = db.View_MallDetail.AsQueryable();

                if (!string.IsNullOrEmpty(request.StoreSapCode))
                {
                    _list = _list.Where(p => p.RelatedBrandStore.Contains(request.StoreSapCode));
                }

                //只给线下店铺
                _list = _list.Where(p => p.MallType == (int)MallType.OffLine && p.IsUsed);
                //分页查询
                var _pageView = _entityRepository.GetPage(request.PageIndex, request.PageSize, _list.AsNoTracking(), p => p.SortID, true);
                //返回数据
                _result.Stores = _pageView.Items.Select(p => new GetStoresResponse.Store()
                {
                    StoreID = p.SapCode,
                    StoreName = p.MallName,
                    City = p.City,
                    District = p.District,
                    Address = p.Address,
                    ZipCode = p.ZipCode,
                    Contacts = p.ContactReceiver,
                    Phone = p.ContactPhone,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    StoreType = p.StoreType
                }).ToList();
                _result.totalRecord = _pageView.TotalItems;
                _result.totalPage = PagerHelper.CountTotalPage((int)_result.totalRecord, request.PageSize);
            }
            return _result;
        }
        #endregion

        #region order
        /// <summary>
        /// 获取订单详情信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public GetOrdersDetailResponse GetOrdersDetail(GetOrdersDetailRequest request)
        {
            GetOrdersDetailResponse _result = new GetOrdersDetailResponse();
            using (var db = new ebEntities())
            {

            }
            return _result;
        }
        #endregion

        #region inventory
        /// <summary>
        /// 获取库存信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public GetInventorysResponse GetInventorys(GetInventorysRequest request)
        {
            try
            {
                GetInventorysResponse _result = new GetInventorysResponse();
                using (var db = new ebEntities())
                {
                    var mall = db.Mall.Where(p => p.SapCode == request.MallSapCode && p.PlatformCode == (int)PlatformType.TUMI_Japan).SingleOrDefault();
                    if (mall != null)
                    {
                        //库存警告数量
                        int _WarningInventory = ConfigService.GetWarningInventoryNumTumiConfig();

                        var _list = db.View_MallProductInventory.AsQueryable();

                        //只推送上架中和有效的产品,不推送赠品
                        _list = _list.Where(p => p.MallSapCode == request.MallSapCode && p.ProductType != (int)ProductType.Gift && p.IsOnSale && p.IsUsed);

                        if(!string.IsNullOrEmpty(request.ProductIds))
                        {
                            var _idArrays = request.ProductIds.Split(',').ToList();
                            _list = _list.Where(p => request.ProductIds.Contains(p.MallProductId));
                        }

                        //分页查询
                        var _pageView = _entityRepository.GetPage(request.PageIndex, request.PageSize, _list.AsNoTracking(), p => p.ID, true);
                        //返回数据
                        _result.ListId = "JP-inventory";
                        _result.DefaultInstock = false;
                        _result.Description = "JP-inventory Inventory ( 5640 )";
                        _result.UseBundleInventoryOnly = false;
                        _result.Records = _pageView.Items.Select(p => new GetInventorysResponse.Inventory()
                        {
                            ProductId = p.MallProductId,
                            Allocation = (p.Quantity <= _WarningInventory) ? 0 : p.Quantity,
                            Timestamp = VariableHelper.SaferequestUTCTime(DateTime.Now)
                        }).ToList();
                        _result.totalRecord = _pageView.TotalItems;
                        _result.totalPage = PagerHelper.CountTotalPage((int)_result.totalRecord, request.PageSize);
                    }
                    else
                    {
                        throw new Exception("The mall dose not exists!");
                    }
                }
                return _result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}