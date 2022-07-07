using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using Samsonite.Utility.Common;

using OMS.App.Helper;

namespace OMS.App.Controllers
{
    public class DeliveryRequireErrorController : BaseController
    {
        //
        // GET: /DeliveryRequireError/

        #region 查询
        [UserPowerAuthorize]
        public ActionResult Index()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;
            //菜单栏
            ViewBag.MenuBar = this.MenuBar();
            //功能权限
            ViewBag.FunctionPower = this.FunctionPowers();
            //快速时间选项
            ViewData["quicktime_list"] = QuickTimeHelper.QuickTimeOption();

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult Index_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            string _orderid = VariableHelper.SaferequestStr(Request.Form["orderid"]);
            string _storeid = VariableHelper.SaferequestStr(Request.Form["store"]);
            string _sku = VariableHelper.SaferequestStr(Request.Form["sku"]);
            string _time1 = VariableHelper.SaferequestStr(Request.Form["time1"]);
            string _time2 = VariableHelper.SaferequestStr(Request.Form["time2"]);
            string _msg = VariableHelper.SaferequestStr(Request.Form["msg"]);
            int _status = VariableHelper.SaferequestInt(Request.Form["status"]);
            using (var db = new ebEntities())
            {
                var _lambda1 = db.View_OrderDetail.AsQueryable();
                var _lambda2 = db.ECommercePushRecord.AsQueryable();

                //搜索条件
                if (!string.IsNullOrEmpty(_orderid))
                {
                    _lambda1 = _lambda1.Where(p => p.OrderNo.Contains(_orderid) || p.SubOrderNo.Contains(_orderid));
                }

                if (!string.IsNullOrEmpty(_storeid))
                {
                    _lambda1 = _lambda1.Where(p => p.MallSapCode == _storeid);
                }
                else
                {
                    //默认显示当前账号允许看到的店铺订单
                    var _UserMalls = this.CurrentLoginUser.UserMalls;
                    _lambda1 = _lambda1.Where(p => _UserMalls.Contains(p.MallSapCode));
                }

                if (!string.IsNullOrEmpty(_sku))
                {
                    _lambda1 = _lambda1.Where(p => p.SKU.Contains(_sku) || p.ProductId.Contains(_sku));
                }

                if (!string.IsNullOrEmpty(_time1))
                {
                    var _beginTime = VariableHelper.SaferequestTime(_time1);
                    _lambda1 = _lambda1.Where(p => SqlFunctions.DateDiff("day", p.OrderTime, _beginTime) <= 0);
                }

                if (!string.IsNullOrEmpty(_time2))
                {
                    var _endTime = VariableHelper.SaferequestTime(_time2);
                    _lambda1 = _lambda1.Where(p => SqlFunctions.DateDiff("day", p.OrderTime, _endTime) >= 0);
                }

                if (!string.IsNullOrEmpty(_msg))
                {
                    _lambda2 = _lambda2.Where(p => p.PushResultMessage.Contains(_msg));
                }

                if (_status > 0)
                {
                    _lambda2 = _lambda2.Where(p => p.IsDelete);
                }
                else
                {
                    _lambda2 = _lambda2.Where(p => !p.IsDelete);
                }

                //只显示需要推送状态的订单
                _lambda1 = _lambda1.Where(p => p.ProductStatus == (int)ProductStatus.Received);
                //获取快递号
                _lambda2 = _lambda2.Where(p => p.PushType == (int)ECommercePushType.RequireTrackingCode);
                //失败订单
                _lambda2 = _lambda2.Where(p => !p.PushResult);

                var _lambda = from od in _lambda1
                              join epr in _lambda2 on od.DetailID equals epr.RelatedId
                              select new { od, epr };

                //查询
                var _list = this.BaseEntityRepository.GetPage(VariableHelper.SaferequestInt(Request.Form["page"]), VariableHelper.SaferequestInt(Request.Form["rows"]), _lambda.AsNoTracking(), p => p.epr.EditTime, false);
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.epr.Id,
                               s1 = string.Format("<a href=\"javascript:void(0);\" onclick=\"artDialogExtend.Dialog.Open('" + Url.Action("Detail", "OrderQuery") + "?ID={0}',{{title:'{1}-{0}',width:'100%',height:'100%'}});\">{0}</a>", dy.od.OrderNo, _LanguagePack["ordererror_detail_title"]),
                               s2 = dy.od.SubOrderNo,
                               s3 = dy.od.MallName,
                               s4 = dy.od.SKU,
                               s5 = OrderHelper.GetProductStatusDisplay(dy.od.ProductStatus, true),
                               s6 = VariableHelper.FormateTime(dy.od.OrderTime, "yyyy-MM-dd HH:mm:ss"),
                               s7 = dy.epr.PushCount,
                               s8 = dy.epr.PushResultMessage,
                               s9 = dy.epr.EditTime.ToString("yyyy-MM-dd HH:mm:ss")
                           }
                };
            }
            return _result;
        }
        #endregion

        //#region 重新申请快递号
        //[UserPowerAuthorize]
        //public ActionResult ReApply()
        //{
        //    //加载语言包
        //    ViewBag.LanguagePack = this.GetLanguagePack;

        //    Int64 _ID = VariableHelper.SaferequestInt64(Request.QueryString["ID"]);
        //    using (var db = new ebEntities())
        //    {
        //        ECommercePushRecord objECommercePushRecord = db.ECommercePushRecord.Where(p => p.Id == _ID && p.PushType == (int)ECommercePushType.RequireTrackingCode).SingleOrDefault();
        //        if (objECommercePushRecord != null)
        //        {
        //            View_OrderDetail objView_OrderDetail = db.View_OrderDetail.Where(p => p.DetailID == objECommercePushRecord.RelatedId && p.ProductStatus == (int)ProductStatus.Pending).SingleOrDefault();
        //            if (objView_OrderDetail != null)
        //            {
        //                ViewData["order_detail"] = objView_OrderDetail;

        //                return View(objECommercePushRecord);
        //            }
        //            else
        //            {
        //                return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.NoMessage });
        //            }
        //        }
        //        else
        //        {
        //            return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.NoMessage });
        //        }
        //    }
        //}

        //[UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        //public JsonResult ReApply_Message()
        //{
        //    //加载语言包
        //    var _LanguagePack = this.GetLanguagePack;

        //    JsonResult _result = new JsonResult();
        //    Int64 _ID = VariableHelper.SaferequestInt64(Request.Form["id"]);
        //    using (var db = new ebEntities())
        //    {
        //        try
        //        {
        //            ECommercePushRecord objECommercePushRecord = db.ECommercePushRecord.Where(p => p.Id == _ID && p.PushType == (int)ECommercePushType.RequireTrackingCode).SingleOrDefault();
        //            if (objECommercePushRecord != null)
        //            {
        //                View_OrderDetail objView_OrderDetail = db.View_OrderDetail.Where(p => p.DetailID == objECommercePushRecord.RelatedId && p.ProductStatus == (int)ProductStatus.Pending).SingleOrDefault();
        //                if (objView_OrderDetail != null)
        //                {
        //                    //如果是Lazada
        //                    if (objView_OrderDetail.PlatformType == (int)PlatformType.LAZADA_Singapore)
        //                    {
        //                        //读取店铺信息
        //                        View_Mall_Platform objView_Mall_Platform = db.View_Mall_Platform.Where(p => p.SapCode == objView_OrderDetail.MallSapCode).SingleOrDefault();
        //                        if (objView_Mall_Platform != null)
        //                        {
        //                            LazadaAPI api_Lazada = new LazadaAPI()
        //                            {
        //                                MallName = objView_Mall_Platform.MallName,
        //                                MallSapCode = objView_Mall_Platform.SapCode,
        //                                UserID = objView_Mall_Platform.UserID,
        //                                Token = objView_Mall_Platform.Token,
        //                                FtpID = objView_Mall_Platform.FtpID,
        //                                PlatformCode = objView_Mall_Platform.PlatformCode,
        //                                Url = objView_Mall_Platform.Url,
        //                                AppKey = objView_Mall_Platform.AppKey,
        //                                AppSecret = objView_Mall_Platform.AppSecret,
        //                            };

        //                            //获取快递号
        //                            var result = api_Lazada.GetTrackingNumbers(new List<View_OrderDetail>() { objView_OrderDetail });
        //                            if (result.ResultData[0].Result)
        //                            {
        //                                //返回信息
        //                                _result.Data = new
        //                                {
        //                                    result = true,
        //                                    msg = $"{_LanguagePack["common_data_save_success"]}:{result.ResultData[0].Data.InvoiceNo}"
        //                                };
        //                            }
        //                            else
        //                            {
        //                                throw new Exception(result.ResultData[0].ResultMessage);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            throw new Exception(_LanguagePack["common_data_load_false"]);
        //                        }
        //                    }
        //                    //如果是Zalora
        //                    else if (objView_OrderDetail.PlatformType == (int)PlatformType.ZALORA_Singapore)
        //                    {
        //                        //读取店铺信息
        //                        View_Mall_Platform objView_Mall_Platform = db.View_Mall_Platform.Where(p => p.SapCode == objView_OrderDetail.MallSapCode).SingleOrDefault();
        //                        if (objView_Mall_Platform != null)
        //                        {
        //                            ZaloraAPI api_Zalora = new ZaloraAPI()
        //                            {
        //                                MallName = objView_Mall_Platform.MallName,
        //                                MallSapCode = objView_Mall_Platform.SapCode,
        //                                UserID = objView_Mall_Platform.UserID,
        //                                Token = objView_Mall_Platform.Token,
        //                                FtpID = objView_Mall_Platform.FtpID,
        //                                PlatformCode = objView_Mall_Platform.PlatformCode,
        //                                Url = objView_Mall_Platform.Url,
        //                                AppKey = objView_Mall_Platform.AppKey,
        //                                AppSecret = objView_Mall_Platform.AppSecret,
        //                            };

        //                            //获取快递号
        //                            var result = api_Zalora.GetTrackingNumbers(new List<View_OrderDetail>() { objView_OrderDetail });
        //                            if (result.ResultData[0].Result)
        //                            {
        //                                //返回信息
        //                                _result.Data = new
        //                                {
        //                                    result = true,
        //                                    msg = $"{_LanguagePack["common_data_save_success"]}:{result.ResultData[0].Data.InvoiceNo}"
        //                                };
        //                            }
        //                            else
        //                            {
        //                                throw new Exception(result.ResultData[0].ResultMessage);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            throw new Exception(_LanguagePack["common_data_load_false"]);
        //                        }
        //                    }
        //                    //Shopee
        //                    else if (objView_OrderDetail.PlatformType == (int)PlatformType.SHOPEE_Singapore)
        //                    {
        //                        //读取店铺信息
        //                        View_Mall_Platform objView_Mall_Platform = db.View_Mall_Platform.Where(p => p.SapCode == objView_OrderDetail.MallSapCode).SingleOrDefault();
        //                        if (objView_Mall_Platform != null)
        //                        {
        //                            ShopeeAPI api_Shopee = new ShopeeAPI()
        //                            {
        //                                MallName = objView_Mall_Platform.MallName,
        //                                MallSapCode = objView_Mall_Platform.SapCode,
        //                                UserID = objView_Mall_Platform.UserID,
        //                                Token = objView_Mall_Platform.Token,
        //                                FtpID = objView_Mall_Platform.FtpID,
        //                                PlatformCode = objView_Mall_Platform.PlatformCode,
        //                                Url = objView_Mall_Platform.Url,
        //                                AppKey = objView_Mall_Platform.AppKey,
        //                                AppSecret = objView_Mall_Platform.AppSecret,
        //                            };

        //                            //获取快递号
        //                            var result = api_Shopee.GetTrackingNumbers(new List<View_OrderDetail>() { objView_OrderDetail });
        //                            if (result.ResultData[0].Result)
        //                            {
        //                                //返回信息
        //                                _result.Data = new
        //                                {
        //                                    result = true,
        //                                    msg = $"{_LanguagePack["common_data_save_success"]}:{result.ResultData[0].Data.InvoiceNo}"
        //                                };
        //                            }
        //                            else
        //                            {
        //                                throw new Exception(result.ResultData[0].ResultMessage);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            throw new Exception(_LanguagePack["common_data_load_false"]);
        //                        }
        //                    }
        //                    //如果是Demandware
        //                    else if (objView_OrderDetail.PlatformType == (int)PlatformType.DEMANDWARE_Singapore)
        //                    {
        //                        //读取店铺信息
        //                        View_Mall_Platform objView_Mall_Platform = db.View_Mall_Platform.Where(p => p.SapCode == objView_OrderDetail.MallSapCode).SingleOrDefault();
        //                        if (objView_Mall_Platform != null)
        //                        {
        //                            DemandwareAPI api_DemandwareAPI = new DemandwareAPI()
        //                            {
        //                                MallName = objView_Mall_Platform.MallName,
        //                                MallSapCode = objView_Mall_Platform.SapCode,
        //                                UserID = objView_Mall_Platform.UserID,
        //                                Token = objView_Mall_Platform.Token,
        //                                FtpID = objView_Mall_Platform.FtpID,
        //                                PlatformCode = objView_Mall_Platform.PlatformCode,
        //                                Url = objView_Mall_Platform.Url,
        //                                AppKey = objView_Mall_Platform.AppKey,
        //                                AppSecret = objView_Mall_Platform.AppSecret,
        //                            };
        //                            //获取快递号
        //                            var result = api_DemandwareAPI.GetTrackingNumbers(new List<View_OrderDetail>() { objView_OrderDetail });
        //                            if (result.ResultData[0].Result)
        //                            {
        //                                //返回信息
        //                                _result.Data = new
        //                                {
        //                                    result = true,
        //                                    msg = $"{_LanguagePack["common_data_save_success"]}:{result.ResultData[0].Data.InvoiceNo}"
        //                                };
        //                            }
        //                            else
        //                            {
        //                                throw new Exception(result.ResultData[0].ResultMessage);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            throw new Exception(_LanguagePack["common_data_load_false"]);
        //                        }
        //                    }
        //                    //如果是Tumi
        //                    else if (objView_OrderDetail.PlatformType == (int)PlatformType.TUMI_Singapore)
        //                    {
        //                        //读取店铺信息
        //                        View_Mall_Platform objView_Mall_Platform = db.View_Mall_Platform.Where(p => p.SapCode == objView_OrderDetail.MallSapCode).SingleOrDefault();
        //                        if (objView_Mall_Platform != null)
        //                        {
        //                            TumiAPI api_TumiAPI = new TumiAPI()
        //                            {
        //                                MallName = objView_Mall_Platform.MallName,
        //                                MallSapCode = objView_Mall_Platform.SapCode,
        //                                UserID = objView_Mall_Platform.UserID,
        //                                Token = objView_Mall_Platform.Token,
        //                                FtpID = objView_Mall_Platform.FtpID,
        //                                PlatformCode = objView_Mall_Platform.PlatformCode,
        //                                Url = objView_Mall_Platform.Url,
        //                                AppKey = objView_Mall_Platform.AppKey,
        //                                AppSecret = objView_Mall_Platform.AppSecret,
        //                            };
        //                            //获取快递号
        //                            var result = api_TumiAPI.GetTrackingNumbers(new List<View_OrderDetail>() { objView_OrderDetail });
        //                            if (result.ResultData[0].Result)
        //                            {
        //                                //返回信息
        //                                _result.Data = new
        //                                {
        //                                    result = true,
        //                                    msg = $"{_LanguagePack["common_data_save_success"]}:{result.ResultData[0].Data.InvoiceNo}"
        //                                };
        //                            }
        //                            else
        //                            {
        //                                throw new Exception(result.ResultData[0].ResultMessage);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            throw new Exception(_LanguagePack["common_data_load_false"]);
        //                        }
        //                    }
        //                    //如果是Micros
        //                    else if (objView_OrderDetail.PlatformType == (int)PlatformType.Micros_Singapore)
        //                    {
        //                        //读取店铺信息
        //                        View_Mall_Platform objView_Mall_Platform = db.View_Mall_Platform.Where(p => p.SapCode == objView_OrderDetail.MallSapCode).SingleOrDefault();
        //                        if (objView_Mall_Platform != null)
        //                        {
        //                            MicrosAPI api_MicrosAPI = new MicrosAPI()
        //                            {
        //                                MallName = objView_Mall_Platform.MallName,
        //                                MallSapCode = objView_Mall_Platform.SapCode,
        //                                UserID = objView_Mall_Platform.UserID,
        //                                Token = objView_Mall_Platform.Token,
        //                                FtpID = objView_Mall_Platform.FtpID,
        //                                PlatformCode = objView_Mall_Platform.PlatformCode,
        //                                Url = objView_Mall_Platform.Url,
        //                                AppKey = objView_Mall_Platform.AppKey,
        //                                AppSecret = objView_Mall_Platform.AppSecret,
        //                            };
        //                            //获取快递号
        //                            var result = api_MicrosAPI.GetTrackingNumbers(new List<View_OrderDetail>() { objView_OrderDetail });
        //                            if (result.ResultData[0].Result)
        //                            {
        //                                //返回信息
        //                                _result.Data = new
        //                                {
        //                                    result = true,
        //                                    msg = $"{_LanguagePack["common_data_save_success"]}:{result.ResultData[0].Data.InvoiceNo}"
        //                                };
        //                            }
        //                            else
        //                            {
        //                                throw new Exception(result.ResultData[0].ResultMessage);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            throw new Exception(_LanguagePack["common_data_load_false"]);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        throw new Exception(_LanguagePack["common_data_load_false"]);
        //                    }
        //                }
        //                else
        //                {
        //                    throw new Exception(_LanguagePack["common_data_load_false"]);
        //                }
        //            }
        //            else
        //            {
        //                throw new Exception(_LanguagePack["common_data_load_false"]);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            //返回信息
        //            _result.Data = new
        //            {
        //                result = false,
        //                msg = ex.Message
        //            };
        //        }

        //        return _result;
        //    }
        //}
        //#endregion

        #region 删除
        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Delete_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            string _IDs = VariableHelper.SaferequestStr(Request.Form["ID"]);
            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_IDs))
                    {
                        throw new Exception(_LanguagePack["common_data_need_one"]);
                    }

                    ECommercePushRecord objECommercePushRecord = new ECommercePushRecord();
                    foreach (string _str in _IDs.Split(','))
                    {
                        Int64 _ID = VariableHelper.SaferequestInt64(_str);
                        objECommercePushRecord = db.ECommercePushRecord.Where(p => p.Id == _ID && p.PushType == (int)ECommercePushType.RequireTrackingCode).SingleOrDefault();
                        if (objECommercePushRecord != null)
                        {
                            objECommercePushRecord.IsDelete = true;
                        }
                        else
                        {
                            throw new Exception(string.Format("{0}:{1}", _str, _LanguagePack["common_data_no_exsit"]));
                        }
                    }
                    db.SaveChanges();
                    //返回信息
                    _result.Data = new
                    {
                        result = true,
                        msg = _LanguagePack["common_data_delete_success"]
                    };
                }
                catch (Exception ex)
                {
                    _result.Data = new
                    {
                        result = false,
                        msg = ex.Message
                    };
                }
                return _result;
            }
        }
        #endregion

        #region 恢复
        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Restore_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            string _IDs = VariableHelper.SaferequestStr(Request.Form["ID"]);
            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_IDs))
                    {
                        throw new Exception(_LanguagePack["common_data_need_one"]);
                    }

                    ECommercePushRecord objECommercePushRecord = new ECommercePushRecord();
                    foreach (string _str in _IDs.Split(','))
                    {
                        Int64 _ID = VariableHelper.SaferequestInt64(_str);
                        objECommercePushRecord = db.ECommercePushRecord.Where(p => p.Id == _ID && p.PushType == (int)ECommercePushType.RequireTrackingCode).SingleOrDefault();
                        if (objECommercePushRecord != null)
                        {
                            objECommercePushRecord.IsDelete = false;
                        }
                        else
                        {
                            throw new Exception(string.Format("{0}:{1}", _str, _LanguagePack["common_data_no_exsit"]));
                        }
                    }
                    db.SaveChanges();
                    //返回信息
                    _result.Data = new
                    {
                        result = true,
                        msg = _LanguagePack["common_data_recover_success"]
                    };
                }
                catch (Exception ex)
                {
                    _result.Data = new
                    {
                        result = false,
                        msg = ex.Message
                    };
                }
                return _result;
            }
        }
        #endregion
    }
}
