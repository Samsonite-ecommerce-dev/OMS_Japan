using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Encryption;
using Samsonite.OMS.Service;
using Samsonite.Utility.Common;
using OMS.App.Helper;

namespace OMS.App.Controllers
{
    public class OrderModifyController : BaseController
    {
        //
        // GET: /OrderModify/

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
            //订单号
            ViewBag.OrderID = VariableHelper.SaferequestStr(Request.QueryString["OrderID"]);
            //快速时间选项
            ViewData["quicktime_list"] = QuickTimeHelper.QuickTimeOption();
            //流程状态
            ViewData["proccess_list"] = OrderHelper.ProccessModifyStatusObject();
            //仓库状态
            ViewData["wh_list"] = OrderHelper.WarehouseStatusReflect();

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult Index_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            string _orderid = VariableHelper.SaferequestStr(Request.Form["orderid"]);
            string _time1 = VariableHelper.SaferequestStr(Request.Form["time1"]);
            string _time2 = VariableHelper.SaferequestStr(Request.Form["time2"]);
            string[] _storeid = VariableHelper.SaferequestStringArray(Request.Form["store"]);
            int _process_status = VariableHelper.SaferequestInt(Request.Form["proccess_status"]);
            int _stock_status = VariableHelper.SaferequestInt(Request.Form["stock_status"]);
            using (var db = new DynamicRepository())
            {
                //搜索条件
                if (!string.IsNullOrEmpty(_orderid))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "((OrderNo like {0}) or (SubOrderNo like {0}))", Param = "%" + _orderid + "%" });
                }

                if (!string.IsNullOrEmpty(_time1))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,AddDate,{0})<=0", Param = VariableHelper.SaferequestTime(_time1) });
                }

                if (!string.IsNullOrEmpty(_time2))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,AddDate,{0})>=0", Param = VariableHelper.SaferequestTime(_time2) });
                }

                //默认显示当前账号允许看到的店铺订单
                var _UserMalls = new List<string>();
                if (_storeid != null)
                {
                    _UserMalls = _storeid.Where(p => this.CurrentLoginUser.UserMalls.Contains(p)).ToList();
                }
                else
                {
                    _UserMalls = this.CurrentLoginUser.UserMalls;
                }
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "MallSapCode in (select item from strToIntTable('" + string.Join(",", _UserMalls) + "',','))", Param = null });

                if (_process_status > 0)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "Status={0}", Param = _process_status });
                }

                if (_stock_status > 0)
                {
                    switch (_stock_status)
                    {
                        case 1:
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "ApiIsRead={0}", Param = 0 });
                            break;
                        case 2:
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "ApiIsRead={0}", Param = 1 });
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "ApiStatus={0}", Param = (int)WarehouseStatus.DealSuccessful });
                            break;
                        case 3:
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "ApiIsRead={0}", Param = 1 });
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "ApiStatus={0}", Param = (int)WarehouseStatus.DealFail });
                            break;
                        case 4:
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "ApiIsRead={0}", Param = 1 });
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "ApiStatus={0}", Param = (int)WarehouseStatus.Dealing });
                            break;
                        default:
                            break;
                    }
                }

                //查询
                var _list = db.GetPage<View_OrderModify>("select * from View_OrderModify order by Id desc,ChangeID desc", _SqlWhere, VariableHelper.SaferequestInt(Request.Form["rows"]), VariableHelper.SaferequestInt(Request.Form["page"]));
                //数据解密并脱敏
                foreach (var item in _list.Items)
                {
                    EncryptionFactory.Create(item).HideSensitive();
                }
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.Id,
                               s0 = dy.Status,
                               s1 = string.Format("<a href=\"javascript:void(0);\" onclick=\"artDialogExtend.Dialog.Open('" + Url.Action("Detail", "OrderQuery") + "?ID={0}',{{title:'{0}',width:'100%',height:'100%'}});\">{0}</a>{1}", dy.OrderNo, OrderHelper.GetProcessNatureLabel(new OrderHelper.ProcessNature() { IsSystemModify = dy.IsSystemModify })),
                               s2 = dy.SubOrderNo,
                               s3 = dy.MallName,
                               s4 = dy.CustomerName,
                               s5 = dy.Tel,
                               s6 = dy.Mobile,
                               s7 = dy.Zipcode,
                               s8 = AreaService.JoinAreaMessage(new AreaDto() { Province = dy.Province, City = dy.City, District = dy.District }),
                               s9 = dy.Addr,
                               s10 = OrderHelper.GetProcessStatusDisplay(dy.Status, true),
                               s11 = string.Format("{0}<br/>{1}{2}", dy.AddUserName, dy.AddDate.ToString("yyyy-MM-dd HH:mm:ss"), (!string.IsNullOrEmpty(dy.Remark)) ? "<br/>" + dy.Remark : ""),
                               s12 = (!dy.IsSystemModify) ? OrderHelper.GetWarehouseStatusDisplay(dy.ApiIsRead, dy.ApiStatus, true) + ((dy.ApiReplyDate != null) ? "<br/>" + dy.ApiReplyDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "") + ((dy.ApiReplyMsg != null) ? "<br/>" + dy.ApiReplyMsg : "") : "",
                               s13 = string.Format("{0}", ((dy.AcceptUserDate != null) ? dy.AcceptUserName + "<br/>" + VariableHelper.FormateTime(dy.AcceptUserDate, "yyyy-MM-dd HH:mm:ss") : ""))
                           }
                };
            }
            return _result;
        }
        #endregion

        #region 添加
        [UserPowerAuthorize]
        public ActionResult Add()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;

            Int64 _ID = VariableHelper.SaferequestInt64(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                Order objOrder = db.Order.Where(p => p.Id == _ID).SingleOrDefault();
                if (objOrder != null)
                {
                    ViewData["order"] = objOrder;
                    List<OrderDetail> objOrderDetail_List = db.OrderDetail.Where(p => p.OrderId == objOrder.Id && !p.IsSetOrigin && !p.IsDelete && !p.IsExchangeNew).ToList();
                    //最新收货信息集合
                    List<ReceiveDto> objReceiveDto_List = new List<ReceiveDto>();
                    foreach (var _o in objOrderDetail_List)
                    {
                        objReceiveDto_List.Add(OrderReceiveService.GetNewestReceive(_o.OrderNo, _o.SubOrderNo));
                    }
                    ViewData["receive_list"] = objReceiveDto_List;

                    return View(objOrderDetail_List);
                }
                else
                {
                    return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.NoMessage });
                }
            }
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Add_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            string _OrderID = VariableHelper.SaferequestStr(Request.Form["OrderID"]);
            string _Remark = VariableHelper.SaferequestStr(Request.Form["Remark"]);
            int _TotalSelect = VariableHelper.SaferequestInt(Request.Form["TotalSelect"]);

            string _SelectIDs = Request.Form["SelectID"];
            string _Sorts = Request.Form["Sort"];
            //如果字段中存在逗号,那么使用同一个Name获取数据时候由于逗号分隔原理,无法取到正确数据
            //string _Addreses = Request.Form["Address"];

            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        Order objOrder = db.Order.Where(p => p.OrderNo == _OrderID).SingleOrDefault();
                        if (objOrder != null)
                        {
                            if (string.IsNullOrEmpty(_SelectIDs))
                            {
                                throw new Exception(_LanguagePack["ordermodify_edit_message_at_least_one_product"]);
                            }

                            //要修改的子订单
                            string[] _SelectID_Array = _SelectIDs.Split(',');
                            string[] _Sort_Array = _Sorts.Split(',');
                            int _Sort = 0;
                            string _Receiver = string.Empty;
                            string _Tel = string.Empty;
                            string _Mobile = string.Empty;
                            string _Zipcode = string.Empty;
                            string _Address = string.Empty;

                            //读取子订单
                            OrderDetail objOrderDetail = new OrderDetail();
                            OrderModify objOrderModify = new OrderModify();
                            OrderChangeRecord objOrderChangeRecord = new OrderChangeRecord();
                            OrderWMSReply objOrderWMSReply = new OrderWMSReply();
                            OrderLog objOrderLog = new OrderLog();
                            //排除套装原始订单
                            List<OrderDetail> objOrderDetail_List = db.OrderDetail.Where(p => p.OrderNo == objOrder.OrderNo && !p.IsSetOrigin && !p.IsDelete).ToList();
                            //初始化预售信息
                            int _OrgStatus = 0;
                            //是否内部编辑
                            bool _IsSystemModify = false;
                            for (int t = 0; t < _SelectID_Array.Length; t++)
                            {
                                _Sort = VariableHelper.SaferequestInt(_Sort_Array[t]);
                                _Receiver = VariableHelper.SaferequestStr(Request.Form["Receiver_" + _Sort]);
                                _Tel = VariableHelper.SaferequestStr(Request.Form["Tel_" + _Sort]);
                                _Mobile = VariableHelper.SaferequestStr(Request.Form["Mobile_" + _Sort]);
                                _Zipcode = VariableHelper.SaferequestStr(Request.Form["ZipCode_" + _Sort]);
                                _Address = VariableHelper.SaferequestStr(Request.Form["Address_" + _Sort]);

                                if (string.IsNullOrEmpty(_Receiver))
                                {
                                    throw new Exception(_LanguagePack["ordermodify_edit_message_no_receiver"]);
                                }

                                if (string.IsNullOrEmpty(_Tel) && string.IsNullOrEmpty(_Mobile))
                                {
                                    throw new Exception(_LanguagePack["ordermodify_edit_message_no_tel"]);
                                }

                                if (string.IsNullOrEmpty(_Address))
                                {
                                    throw new Exception(_LanguagePack["ordermodify_edit_message_no_address"]);
                                }

                                Int64 _id = VariableHelper.SaferequestInt64(_SelectID_Array[t]);
                                objOrderDetail = objOrderDetail_List.Where(p => p.Id == _id).SingleOrDefault();
                                if (objOrderDetail != null)
                                {
                                    _OrgStatus = objOrderDetail.Status;
                                    //是否无效信息
                                    if (objOrderDetail.IsDelete || objOrderDetail.IsError)
                                    {
                                        throw new Exception(string.Format("{0}:{1}", objOrderDetail.SubOrderNo, _LanguagePack["common_data_no_avail"]));
                                    }

                                    //判断在发货前和待处理之后才允许编辑信息
                                    List<int> objAllowStatus = new List<int>() { (int)ProductStatus.Received, (int)ProductStatus.Processing };
                                    if (!objAllowStatus.Contains(objOrderDetail.Status))
                                    {
                                        throw new Exception(string.Format("{0}:{1}", objOrderDetail.SubOrderNo, _LanguagePack["ordermodify_edit_message_state_no_allow"]));
                                    }
                                    else
                                    {
                                        //只有Demandware的订单在Pedding状态允许编辑,其它订单都不允许
                                        //注:Tumi/Micros订单需要修改错误的邮编等信息,从而解决因为错误的收货信息而无法在Singpost申请快递号的问题
                                        if (objOrderDetail.Status == (int)ProductStatus.Received)
                                        {
                                            if (objOrder.PlatformType == (int)PlatformType.TUMI_Japan || objOrder.PlatformType == (int)PlatformType.Micros_Japan)
                                            {
                                                //如果是DW的订单在pending状态下需要自动完成
                                                _IsSystemModify = true;
                                            }
                                            else
                                            {
                                                throw new Exception(string.Format("{0}:{1}", objOrderDetail.SubOrderNo, _LanguagePack["ordermodify_edit_message_state_no_allow"]));
                                            }
                                        }
                                    }

                                    //查询是否已经存在未处理完成的修改记录,如果存在则不再重复插入
                                    View_OrderModify objView_OrderModify = db.View_OrderModify.Where(p => p.SubOrderNo == objOrderDetail.SubOrderNo && p.Status < (int)ProcessStatus.ModifyComplete).FirstOrDefault();
                                    if (objView_OrderModify != null)
                                    {
                                        throw new Exception(string.Format("{0}:{1}", objView_OrderModify.SubOrderNo, _LanguagePack["ordermodify_edit_message_exist_process"]));
                                    }
                                    //修改流程表
                                    objOrderModify = new OrderModify()
                                    {
                                        OrderNo = objOrder.OrderNo,
                                        MallSapCode = objOrder.MallSapCode,
                                        CustomerName = _Receiver,
                                        Tel = _Tel,
                                        Mobile = _Mobile,
                                        Zipcode = _Zipcode,
                                        Addr = _Address,
                                        CountryCode = string.Empty,
                                        Province = string.Empty,
                                        City = string.Empty,
                                        District = string.Empty,
                                        ExpressId = 0,
                                        InvoiceNo = string.Empty,
                                        UserId = this.CurrentLoginUser.Userid,
                                        AddDate = DateTime.Now,
                                        Remark = _Remark,
                                        AcceptUserId = (_IsSystemModify) ? this.CurrentLoginUser.Userid : 0,
                                        AcceptUserDate = (_IsSystemModify) ? VariableHelper.SaferequestNullTime(DateTime.Now) : null,
                                        AcceptRemark = string.Empty,
                                        FromApi = false,
                                        SubOrderNo = objOrderDetail.SubOrderNo,
                                        Status = (_IsSystemModify) ? (int)ProcessStatus.ModifyComplete : (int)ProcessStatus.Modify,
                                        IsSystemModify = _IsSystemModify,
                                        ManualUserId = 0,
                                        ManualUserDate = null,
                                        ManualRemark = string.Empty
                                    };
                                    //数据加密
                                    EncryptionFactory.Create(objOrderModify).Encrypt();
                                    db.OrderModify.Add(objOrderModify);
                                    db.SaveChanges();
                                    //插入api表
                                    //如果是系统内部直接编辑
                                    if (_IsSystemModify)
                                    {
                                        objOrderChangeRecord = new OrderChangeRecord()
                                        {
                                            OrderNo = objOrderDetail.OrderNo,
                                            SubOrderNo = objOrderDetail.SubOrderNo,
                                            Type = (int)OrderChangeType.Modify,
                                            DetailTableName = OrderModifyProcessService.TableName,
                                            DetailId = objOrderModify.Id,
                                            UserId = this.CurrentLoginUser.Userid,
                                            Status = 1,
                                            Remarks = string.Empty,
                                            ApiIsRead = false,
                                            ApiReadDate = null,
                                            ApiReplyDate = null,
                                            ApiReplyMsg = string.Empty,
                                            AddDate = DateTime.Now,
                                            IsDelete = true
                                        };
                                    }
                                    else
                                    {
                                        objOrderChangeRecord = new OrderChangeRecord()
                                        {
                                            OrderNo = objOrderDetail.OrderNo,
                                            SubOrderNo = objOrderDetail.SubOrderNo,
                                            Type = (int)OrderChangeType.Modify,
                                            DetailTableName = OrderModifyProcessService.TableName,
                                            DetailId = objOrderModify.Id,
                                            UserId = this.CurrentLoginUser.Userid,
                                            Status = 0,
                                            Remarks = string.Empty,
                                            ApiIsRead = false,
                                            ApiReadDate = null,
                                            ApiReplyDate = null,
                                            ApiReplyMsg = string.Empty,
                                            AddDate = DateTime.Now,
                                            IsDelete = false
                                        };
                                    }
                                    db.OrderChangeRecord.Add(objOrderChangeRecord);
                                    //修改子订单状态
                                    objOrderDetail.Status = (_IsSystemModify) ? _OrgStatus : (int)ProductStatus.Modify;
                                    objOrderDetail.EditDate = DateTime.Now;
                                    //添加子订单log
                                    objOrderLog = new OrderLog()
                                    {
                                        Msg = "Modify Processing",
                                        OrderNo = objOrderDetail.OrderNo,
                                        SubOrderNo = objOrderDetail.SubOrderNo,
                                        UserId = this.CurrentLoginUser.Userid,
                                        OriginStatus = _OrgStatus,
                                        NewStatus = (int)ProductStatus.Modify,
                                        CreateDate = DateTime.Now
                                    };
                                    db.OrderLog.Add(objOrderLog);
                                    //如果是内部编辑
                                    if (_IsSystemModify)
                                    {
                                        //添加子订单log
                                        objOrderLog = new OrderLog()
                                        {
                                            Msg = "Modify complete,then rollback the product status",
                                            OrderNo = objOrderDetail.OrderNo,
                                            SubOrderNo = objOrderDetail.SubOrderNo,
                                            UserId = this.CurrentLoginUser.Userid,
                                            OriginStatus = (int)ProductStatus.Modify,
                                            NewStatus = _OrgStatus,
                                            CreateDate = DateTime.Now
                                        };
                                        db.OrderLog.Add(objOrderLog);
                                    }
                                }
                                else
                                {
                                    throw new Exception(string.Format("{0}:{1}", objOrderDetail.SubOrderNo, _LanguagePack["common_data_no_exsit"]));
                                }
                            }
                            db.SaveChanges();
                            Trans.Commit();
                            //返回信息
                            _result.Data = new
                            {
                                result = true,
                                msg = _LanguagePack["common_data_save_success"]
                            };
                        }
                        else
                        {
                            throw new Exception(_LanguagePack["ordermodify_edit_message_no_order"]);
                        }
                    }
                    catch (Exception ex)
                    {
                        Trans.Rollback();
                        _result.Data = new
                        {
                            result = false,
                            msg = ex.Message
                        };
                    }
                }
                return _result;
            }
        }
        #endregion

        #region 编辑
        [UserPowerAuthorize]
        public ActionResult Edit()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;
            ViewBag.LanguagePack = _LanguagePack;

            Int64 _ID = VariableHelper.SaferequestInt64(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                View_OrderModify objView_OrderModify = db.View_OrderModify.Where(p => p.Id == _ID && p.Type == (int)OrderChangeType.Modify).SingleOrDefault();
                if (objView_OrderModify != null)
                {
                    if (objView_OrderModify.Status != (int)ProcessStatus.Modify)
                    {
                        return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.Other, Message = _LanguagePack["ordermodify_edit_message_no_allow_edit"] });
                    }
                    else
                    {
                        //数据解密
                        EncryptionFactory.Create(objView_OrderModify).Decrypt();

                        return View(objView_OrderModify);
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.NoMessage });
                }
            }
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Edit_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            Int64 _ID = VariableHelper.SaferequestInt64(Request.Form["ID"]);
            string _Receiver = VariableHelper.SaferequestStr(Request.Form["Receiver"]);
            string _Tel = VariableHelper.SaferequestStr(Request.Form["Tel"]);
            string _Mobile = VariableHelper.SaferequestStr(Request.Form["Mobile"]);
            string _Zipcode = VariableHelper.SaferequestStr(Request.Form["ZipCode"]);
            //string _Province = VariableHelper.SaferequestStr(Request.Form["Province"]);
            //string _City = VariableHelper.SaferequestStr(Request.Form["City"]);
            //string _District = VariableHelper.SaferequestStr(Request.Form["District"]);
            string _Address = VariableHelper.SaferequestStr(Request.Form["Address"]);
            string _Remark = VariableHelper.SaferequestStr(Request.Form["Remark"]);

            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_Receiver))
                    {
                        throw new Exception(_LanguagePack["ordermodify_edit_message_no_receiver"]);
                    }

                    if (string.IsNullOrEmpty(_Tel) && string.IsNullOrEmpty(_Mobile))
                    {
                        throw new Exception(_LanguagePack["ordermodify_edit_message_no_tel"]);
                    }

                    if (string.IsNullOrEmpty(_Address))
                    {
                        throw new Exception(_LanguagePack["ordermodify_edit_message_no_address"]);
                    }

                    View_OrderModify objView_OrderModify = db.View_OrderModify.Where(p => p.Id == _ID && p.Type == (int)OrderChangeType.Modify).SingleOrDefault();
                    if (objView_OrderModify != null)
                    {
                        if (objView_OrderModify.Status != (int)ProcessStatus.Modify)
                        {
                            throw new Exception(_LanguagePack["ordermodify_edit_message_no_allow_edit"]);
                        }

                        OrderModify objData = db.OrderModify.Where(p => p.Id == objView_OrderModify.Id).SingleOrDefault();
                        if (objData != null)
                        {
                            objData.CustomerName = _Receiver;
                            objData.Tel = _Tel;
                            objData.Mobile = _Mobile;
                            //objData.Province = _Province;
                            //objData.City = AreaService.GetAreaName(_City);
                            //objData.District = AreaService.GetAreaName(_District);
                            objData.Zipcode = _Zipcode;
                            objData.Addr = _Address;
                            objData.Remark = _Remark;
                            //数据加密
                            EncryptionFactory.Create(objData).Encrypt();
                            db.SaveChanges();
                            //添加日志
                            AppLogService.UpdateLog<OrderModify>(objData, objData.Id.ToString());
                            //返回信息
                            _result.Data = new
                            {
                                result = true,
                                msg = _LanguagePack["common_data_save_success"]
                            };
                        }
                        else
                        {
                            throw new Exception(_LanguagePack["ordermodify_edit_message_no_modify"]);
                        }
                    }
                    else
                    {
                        throw new Exception(_LanguagePack["ordermodify_edit_message_no_modify"]);
                    }
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
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_IDs))
                        {
                            throw new Exception(_LanguagePack["common_data_need_one"]);
                        }

                        var _IdArrays = VariableHelper.SaferequestInt64Array(_IDs);
                        object[] _r = new object[2];
                        foreach (var id in _IdArrays)
                        {
                            _r = OrderModifyProcessService.Delete(id, db);
                            if (!Convert.ToBoolean(_r[0]))
                            {
                                throw new Exception(_r[1].ToString());
                            }
                        }
                        Trans.Commit();
                        //返回信息
                        _result.Data = new
                        {
                            result = true,
                            msg = _LanguagePack["common_data_delete_success"]
                        };
                    }
                    catch (Exception ex)
                    {
                        Trans.Rollback();
                        _result.Data = new
                        {
                            result = false,
                            msg = ex.Message
                        };
                    }
                }
                return _result;
            }
        }
        #endregion

        #region 人工处理
        [UserPowerAuthorize]
        public ActionResult Manual()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;
            ViewBag.LanguagePack = _LanguagePack;

            using (var db = new ebEntities())
            {
                string _IDs = VariableHelper.SaferequestStr(Request.QueryString["ID"]);
                if (string.IsNullOrEmpty(_IDs))
                {
                    return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.Other, Message = _LanguagePack["common_data_need_one"] });
                }
                else
                {
                    var _IdArrays = VariableHelper.SaferequestInt64Array(_IDs);
                    var objView_OrderModify_List = db.View_OrderModify.Where(p => _IdArrays.Contains(p.Id) && p.Type == (int)OrderChangeType.Modify).ToList();
                    if (objView_OrderModify_List.Count > 0)
                    {
                        ViewBag.IDs = string.Join(",", objView_OrderModify_List.Select(p => p.Id).ToList());
                        foreach (View_OrderModify objView_OrderModify in objView_OrderModify_List)
                        {
                            if (objView_OrderModify.Status != (int)ProcessStatus.ModifyFail)
                            {
                                return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.Other, Message = string.Format("{0}:{1}", objView_OrderModify.SubOrderNo, _LanguagePack["ordermodify_edit_message_error_state"]) });
                            }
                        }

                        return View();
                    }
                    else
                    {
                        return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.NoMessage });
                    }
                }
            }
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json, IsAntiForgeryToken = true)]
        public JsonResult Manual_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            string _IDs = VariableHelper.SaferequestStr(Request.Form["ID"]);
            int _Result = VariableHelper.SaferequestInt(Request.Form["Result"]);
            string _Remark = VariableHelper.SaferequestEditor(Request.Form["Remark"]);
            using (var db = new ebEntities())
            {
                using (var Trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(_IDs))
                        {
                            throw new Exception(_LanguagePack["common_data_need_one"]);
                        }

                        var _IdArrays = VariableHelper.SaferequestInt64Array(_IDs);
                        object[] _r = new object[2];
                        foreach (var id in _IdArrays)
                        {
                            _r = OrderModifyProcessService.ManualInterference(id, (_Result == 1), _Remark, db);
                            if (!Convert.ToBoolean(_r[0]))
                            {
                                throw new Exception(_r[1].ToString());
                            }
                        }
                        Trans.Commit();
                        //返回信息
                        _result.Data = new
                        {
                            result = true,
                            msg = _LanguagePack["common_data_save_success"]
                        };
                    }
                    catch (Exception ex)
                    {
                        Trans.Rollback();
                        _result.Data = new
                        {
                            result = false,
                            msg = ex.Message
                        };
                    }
                }
                return _result;
            }
        }
        #endregion

        #region 详情
        public ActionResult Detail()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;

            Int64 _ID = VariableHelper.SaferequestInt64(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                View_OrderModify objView_OrderModify = db.View_OrderModify.Where(p => p.Id == _ID).SingleOrDefault();
                if (objView_OrderModify != null)
                {
                    //数据解密
                    EncryptionFactory.Create(objView_OrderModify).Decrypt();

                    OrderReceive objOrderReceive = db.OrderReceive.Where(p => p.OrderNo == objView_OrderModify.OrderNo && p.SubOrderNo == objView_OrderModify.SubOrderNo).SingleOrDefault();
                    //数据解密
                    EncryptionFactory.Create(objOrderReceive).Decrypt();
                    ViewData["order_receive"] = objOrderReceive;

                    return View(objView_OrderModify);
                }
                else
                {
                    return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.NoMessage });
                }
            }
        }
        #endregion

        #region 生成文档
        [UserPowerAuthorize]
        public FileResult ExportExcel()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();
            string _orderid = VariableHelper.SaferequestStr(Request.Form["OrderNumber"]);
            string _time1 = VariableHelper.SaferequestStr(Request.Form["Time1"]);
            string _time2 = VariableHelper.SaferequestStr(Request.Form["Time2"]);
            string[] _storeid = VariableHelper.SaferequestStringArray(Request.Form["StoreName"]);
            int _process_status = VariableHelper.SaferequestInt(Request.Form["ProcessStatus"]);
            int _stock_status = VariableHelper.SaferequestInt(Request.Form["StockStatus"]);
            using (var db = new DynamicRepository())
            {
                //搜索条件
                if (!string.IsNullOrEmpty(_orderid))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "((OrderNo like {0}) or (SubOrderNo like {0}))", Param = "%" + _orderid + "%" });
                }

                if (!string.IsNullOrEmpty(_time1))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,AddDate,{0})<=0", Param = VariableHelper.SaferequestTime(_time1) });
                }

                if (!string.IsNullOrEmpty(_time2))
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "datediff(day,AddDate,{0})>=0", Param = VariableHelper.SaferequestTime(_time2) });
                }

                //默认显示当前账号允许看到的店铺订单
                var _UserMalls = new List<string>();
                if (_storeid != null)
                {
                    _UserMalls = _storeid.Where(p => this.CurrentLoginUser.UserMalls.Contains(p)).ToList();
                }
                else
                {
                    _UserMalls = this.CurrentLoginUser.UserMalls;
                }
                _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "MallSapCode in (select item from strToIntTable('" + string.Join(",", _UserMalls) + "',','))", Param = null });

                if (_process_status > 0)
                {
                    _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "Status={0}", Param = _process_status });
                }

                if (_stock_status > 0)
                {
                    switch (_stock_status)
                    {
                        case 1:
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "ApiIsRead={0}", Param = 0 });
                            break;
                        case 2:
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "ApiIsRead={0}", Param = 1 });
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "ApiStatus={0}", Param = (int)WarehouseStatus.DealSuccessful });
                            break;
                        case 3:
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "ApiIsRead={0}", Param = 1 });
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "ApiStatus={0}", Param = (int)WarehouseStatus.DealFail });
                            break;
                        case 4:
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "ApiIsRead={0}", Param = 1 });
                            _SqlWhere.Add(new DynamicRepository.SQLCondition() { Condition = "ApiStatus={0}", Param = (int)WarehouseStatus.Dealing });
                            break;
                        default:
                            break;
                    }
                }

                DataTable dt = new DataTable();
                dt.Columns.Add(_LanguagePack["ordermodify_index_order_number"]);
                dt.Columns.Add(_LanguagePack["ordermodify_index_sub_order_number"]);
                dt.Columns.Add(_LanguagePack["ordermodify_index_store"]);
                dt.Columns.Add(_LanguagePack["ordermodify_index_customer"]);
                dt.Columns.Add(_LanguagePack["ordermodify_index_tel"]);
                dt.Columns.Add(_LanguagePack["ordermodify_index_mobile"]);
                dt.Columns.Add(_LanguagePack["ordermodify_index_zipcode"]);
                dt.Columns.Add(_LanguagePack["ordermodify_index_area"]);
                dt.Columns.Add(_LanguagePack["ordermodify_index_address"]);
                dt.Columns.Add(_LanguagePack["ordermodify_index_process"]);
                dt.Columns.Add(_LanguagePack["ordermodify_index_oper_message"]);
                dt.Columns.Add(_LanguagePack["ordermodify_index_stock_reply"]);
                dt.Columns.Add(_LanguagePack["ordermodify_index_confirm_message"]);

                //查询
                DataRow _dr = null;
                var _list = db.Fetch<View_OrderModify>("select * from View_OrderModify order by Id desc,ChangeID desc", _SqlWhere);
                foreach (var dy in _list)
                {
                    //数据解密并脱敏
                    EncryptionFactory.Create(dy).HideSensitive();

                    _dr = dt.NewRow();
                    _dr[0] = dy.OrderNo;
                    _dr[1] = dy.SubOrderNo;
                    _dr[2] = dy.MallName;
                    _dr[3] = dy.CustomerName;
                    _dr[4] = dy.Tel;
                    _dr[5] = dy.Mobile;
                    _dr[6] = dy.Zipcode;
                    _dr[7] = AreaService.JoinAreaMessage(new AreaDto() { Province = dy.Province, City = dy.City, District = dy.District });
                    _dr[8] = dy.Addr;
                    _dr[9] = OrderHelper.GetProcessStatusDisplay(dy.Status, false);
                    _dr[10] = string.Format("{0} {1}{2}", dy.AddUserName, dy.AddDate.ToString("yyyy-MM-dd HH:mm:ss"), (!string.IsNullOrEmpty(dy.Remark)) ? " " + dy.Remark : "");
                    _dr[11] = (!dy.IsSystemModify) ? OrderHelper.GetWarehouseStatusDisplay(dy.ApiIsRead, dy.ApiStatus, false) + ((dy.ApiReplyDate != null) ? " " + dy.ApiReplyDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "") + ((dy.ApiReplyMsg != null) ? " " + dy.ApiReplyMsg : "") : "";
                    _dr[12] = string.Format("{0}", ((dy.AcceptUserDate != null) ? dy.AcceptUserName + " " + VariableHelper.FormateTime(dy.AcceptUserDate, "yyyy-MM-dd HH:mm:ss") : ""));
                    dt.Rows.Add(_dr);
                }

                string _filepath = $"~{AppGlobalService.UPLOAD_CACHE_PATH}/{DateTime.Now.ToString("yyyy-MM-dd")}";
                if (!Directory.Exists(Server.MapPath(_filepath)))
                {
                    Directory.CreateDirectory(Server.MapPath(_filepath));
                }
                string _fileName = string.Format("OrderModify_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss"));
                _fileName = System.Web.HttpUtility.UrlEncode(_fileName, Encoding.GetEncoding("UTF-8"));
                string _path = Server.MapPath(string.Format("{0}/{1}", _filepath, _fileName));
                ExcelHelper objExcelHelper = new ExcelHelper(_path);
                objExcelHelper.DataTableToExcel(dt, "Sheet1", true);
                return File(_path, "application/ms-excel", _fileName);
            }
        }
        #endregion
    }
}
