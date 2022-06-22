using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.OMS.Encryption;
using Samsonite.OMS.Service;
using Samsonite.Utility.Common;
using OMS.App.Helper;

namespace OMS.App.Controllers
{
    public class CustomerController : BaseController
    {
        //
        // GET: /Customer/

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

            return View();
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult Index_Message()
        {
            //加载语言包
            var _LanguagePack = this.GetLanguagePack;

            JsonResult _result = new JsonResult();
            string _name = VariableHelper.SaferequestStr(Request.Form["name"]);
            string _tel = VariableHelper.SaferequestStr(Request.Form["tel"]);
            using (var db = new ebEntities())
            {
                var _lambda = db.Customer.AsQueryable();

                //搜索条件
                if (!string.IsNullOrEmpty(_name))
                {
                    var _nameEncrypt = EncryptionBase.EncryptString(_name);
                    _lambda = _lambda.Where(p => p.Name == _nameEncrypt);
                }
                if (!string.IsNullOrEmpty(_tel))
                {
                    var _telEncrypt = EncryptionBase.EncryptString(_tel);
                    _lambda = _lambda.Where(p => p.Tel == _telEncrypt || p.Mobile == _telEncrypt);
                }
                //查询
                var _list = this.BaseEntityRepository.GetPage(VariableHelper.SaferequestInt(Request.Form["page"]), VariableHelper.SaferequestInt(Request.Form["rows"]), _lambda.AsNoTracking(), p => p.ID, false);
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
                               ck = dy.ID,
                               s1 = string.Format("<a href=\"javascript:void(0);\" onclick=\"artDialogExtend.Dialog.Open('" + Url.Action("Detail", "Customer") + "?ID={0}',{{title:'{1}-{0}',width:'100%',height:'100%'}});\">{0}</a>", dy.CustomerNo, _LanguagePack["customer_index_detail"]),
                               s2 = dy.PlatformUserNo,
                               s3 = dy.PlatformUserName,
                               s4 = dy.Name,
                               s5 = dy.Tel,
                               s6 = dy.Mobile,
                               s7 = dy.Email,
                               s8 = dy.Zipcode,
                               s9 = AreaService.JoinAreaMessage(new AreaDto() { Province = dy.Province, City = dy.City, District = dy.District }),
                               s10 = dy.Addr

                           }
                };
                return _result;
            }
        }
        #endregion

        #region 编辑
        [UserPowerAuthorize]
        public ActionResult Edit()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;

            Int64 _ID = VariableHelper.SaferequestInt64(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                Customer objCustomer = db.Customer.Where(p => p.ID == _ID).SingleOrDefault();
                if (objCustomer != null)
                {
                    //数据解密
                    EncryptionFactory.Create(objCustomer).Decrypt();
                    //匹配区域信息
                    ViewData["AreaCodes"] = AreaService.GetAreaCode(new AreaDto() { Province = objCustomer.Province, City = objCustomer.City, District = objCustomer.District });

                    return View(objCustomer);
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
            string _PlatformUserNo = VariableHelper.SaferequestStr(Request.Form["PlatformUserNo"]);
            string _PlatformUserName = VariableHelper.SaferequestStr(Request.Form["PlatformUserName"]);
            string _CustomerName = VariableHelper.SaferequestStr(Request.Form["CustomerName"]);
            string _Tel = VariableHelper.SaferequestStr(Request.Form["Tel"]);
            string _Mobile = VariableHelper.SaferequestStr(Request.Form["Mobile"]);
            string _Email = VariableHelper.SaferequestStr(Request.Form["Email"]);
            string _Zipcode = VariableHelper.SaferequestStr(Request.Form["Zipcode"]);
            string _Province = AreaService.GetAreaName(VariableHelper.SaferequestStr(Request.Form["Province"]));
            string _City = AreaService.GetAreaName(VariableHelper.SaferequestStr(Request.Form["City"]));
            string _District = AreaService.GetAreaName(VariableHelper.SaferequestStr(Request.Form["District"]));
            string _Address = VariableHelper.SaferequestStr(Request.Form["Address"]);

            using (var db = new ebEntities())
            {
                try
                {
                    if (string.IsNullOrEmpty(_CustomerName))
                    {
                        throw new Exception(_LanguagePack["customer_edit_message_no_customer_name"]);
                    }

                    Customer objData = db.Customer.Where(p => p.ID == _ID).SingleOrDefault();
                    if (objData != null)
                    {
                        objData.PlatformUserNo = _PlatformUserNo;
                        objData.PlatformUserName = _PlatformUserName;
                        objData.Name = _CustomerName;
                        objData.Tel = _Tel;
                        objData.Mobile = _Mobile;
                        objData.Email = _Email;
                        objData.Zipcode = _Zipcode;
                        objData.Province = _Province;
                        objData.City = _City;
                        objData.District = _District;
                        objData.Addr = _Address;
                        //数据加密
                        EncryptionFactory.Create(objData).Encrypt();
                        db.SaveChanges();
                        //添加日志
                        AppLogService.UpdateLog<Customer>(objData, objData.ID.ToString());
                        //返回信息
                        _result.Data = new
                        {
                            result = true,
                            msg = _LanguagePack["common_data_save_success"]
                        };
                    }
                    else
                    {
                        throw new Exception(_LanguagePack["common_data_load_false"]);
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

                        Customer objCustomer = new Customer();
                        foreach (string _str in _IDs.Split(','))
                        {
                            int _ID = VariableHelper.SaferequestInt(_str);
                            objCustomer = db.Customer.Where(p => p.CustomerNo == _str).SingleOrDefault();
                            if (objCustomer != null)
                            {
                                db.Customer.Remove(objCustomer);
                            }
                            else
                            {
                                throw new Exception(string.Format("{0}:{1}", _str, _LanguagePack["common_data_no_exsit"]));
                            }
                        }

                        //添加日志
                        AppLogService.DeleteLog("Customer", _IDs);
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
                }
                return _result;
            }
        }
        #endregion

        #region 客户详情
        [UserPowerAuthorize]
        public ActionResult Detail()
        {
            //加载语言包
            ViewBag.LanguagePack = this.GetLanguagePack;

            string _CustomerNo = VariableHelper.SaferequestStr(Request.QueryString["ID"]);
            using (var db = new ebEntities())
            {
                Customer objCustomer = db.Customer.Where(p => p.CustomerNo == _CustomerNo).SingleOrDefault();
                if (objCustomer != null)
                {
                    //数据解密并脱敏
                    EncryptionFactory.Create(objCustomer).HideSensitive();

                    return View(objCustomer);
                }
                else
                {
                    return RedirectToAction("Index", "Error", new { Type = (int)ErrorType.NoMessage });
                }
            }
        }

        [UserPowerAuthorize(Type = UserPowerAuthorize.ResultType.Json)]
        public JsonResult Detail_Message()
        {
            //加载语言包
            var _LanguagePack = GetLanguagePack;

            JsonResult _result = new JsonResult();
            List<DynamicRepository.SQLCondition> _SqlWhere = new List<DynamicRepository.SQLCondition>();

            string _CustomerNo = VariableHelper.SaferequestStr(Request.Form["customerno"]);
            string _Type = VariableHelper.SaferequestStr(Request.Form["type"]);

            using (var db = new ebEntities())
            {
                var _lambda = from o in db.Order.Where(p => p.CustomerNo == _CustomerNo)
                              select new OrderQuery
                              {
                                  Id = o.Id,
                                  OrderNo = o.OrderNo,
                                  MallName = o.MallName,
                                  OrderType = o.OrderType,
                                  OrderAmount = o.OrderAmount,
                                  PaymentAmount = o.PaymentAmount,
                                  Status = o.Status,
                                  OrderSource = o.OrderSource,
                                  CreateDate = o.CreateDate
                              };

                //查询
                var _list = this.BaseEntityRepository.GetPage(VariableHelper.SaferequestInt(Request.Form["page"]), VariableHelper.SaferequestInt(Request.Form["rows"]), _lambda.AsNoTracking(), p => p.CreateDate, false);

                List<string> _orders = _list.Items.Select(p => p.OrderNo).ToList();
                List<OrderReceive> objOrderReceive_List = new List<OrderReceive>();
                List<OrderModify> objOrderModify_List = new List<OrderModify>();
                if (_orders.Count > 0)
                {
                    //获取收货信息
                    objOrderReceive_List = db.OrderReceive.Where(p => _orders.Contains(p.OrderNo)).ToList();
                    //获取更新的地址信息
                    objOrderModify_List = db.OrderModify.Where(p => _orders.Contains(p.OrderNo) && p.Status == (int)ProcessStatus.ModifyComplete).ToList();
                }
                foreach (var dy in _list.Items)
                {
                    OrderReceive objOrderReceive = objOrderReceive_List.Where(p => p.OrderNo == dy.OrderNo).FirstOrDefault();
                    if (objOrderReceive != null)
                    {
                        dy.Receiver = objOrderReceive.Receive;
                        dy.ReceiveTel = objOrderReceive.ReceiveTel;
                        dy.ReceiveAddr = objOrderReceive.ReceiveAddr;
                    }

                    OrderModify objOrderModify = objOrderModify_List.Where(p => p.OrderNo == dy.OrderNo).OrderByDescending(p => p.Id).FirstOrDefault();
                    if (objOrderModify != null)
                    {

                        dy.Receiver = objOrderModify.CustomerName;
                        dy.ReceiveTel = objOrderModify.Tel;
                        dy.ReceiveAddr = objOrderModify.Addr;
                    }
                    //数据解密并脱敏
                    EncryptionFactory.Create(dy, new string[] { "UserName", "Receiver", "ReceiveTel", "ReceiveCel", "ReceiveAddr" }).HideSensitive();
                }
                //返回信息
                _result.Data = new
                {
                    total = _list.TotalItems,
                    rows = from dy in _list.Items
                           select new
                           {
                               ck = dy.Id,
                               s1 = string.Format("<a href=\"javascript:void(0);\" onclick=\"artDialogExtend.Dialog.Open('" + Url.Action("Detail", "OrderQuery") + "?ID={0}',{{title:'{1}-{0}',width:'100%',height:'100%'}});\">{0}</a>", dy.OrderNo, _LanguagePack["orderquery_detail_title"]),
                               s2 = dy.MallName,
                               s3 = OrderHelper.GetOrderTypeDisplay(dy.OrderType, true),
                               s4 = VariableHelper.FormateMoney(dy.OrderAmount),
                               s5 = VariableHelper.FormateMoney(dy.PaymentAmount),
                               s6 = dy.Receiver,
                               s7 = dy.ReceiveTel,
                               s8 = dy.ReceiveAddr,
                               s9 = OrderHelper.GetOrderStatusDisplay(dy.Status, true),
                               s10 = OrderHelper.GetOrderSourceDisplay(dy.OrderSource),
                               s11 = VariableHelper.FormateTime(dy.CreateDate, "yyyy-MM-dd HH:mm:ss")
                           }
                };
            }
            return _result;
        }
        #endregion
    }
}
