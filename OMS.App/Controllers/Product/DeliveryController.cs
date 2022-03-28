using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using Newtonsoft.Json;
using Samsonite.Utility.Common;
using Samsonite.OMS.Service;

namespace OMS.App.Controllers
{
    /// <summary>
    /// 更新快递信息
    /// </summary>
    public class DeliveryController : BaseController
    {
        // GET: /Delivery/
        [UserPowerAuthorize]
        public ActionResult Index()
        {
            //获取语言包
            ViewBag.LanguagePack = this.GetLanguagePack;
            //菜单栏
            ViewBag.MenuBar = MenuBar();
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
            string _filePath = VariableHelper.SaferequestNull(Request.Form["filepath"]);

            try
            {
                if (!string.IsNullOrEmpty(_filePath))
                {
                    List<DeliveryDto> list = new List<DeliveryDto>();
                    if (System.IO.File.Exists(Server.MapPath(_filePath)))
                    {
                        int perPage = VariableHelper.SaferequestInt(Request.Form["rows"]);
                        int page = VariableHelper.SaferequestInt(Request.Form["page"]);
                        list = DeliveryService.ConvertToDeliverys(Server.MapPath(_filePath));
                        _result.Data = new
                        {
                            total = list.Count,
                            rows = list.Skip((page - 1) * perPage).Take(perPage).ToList()
                        };
                    }
                    else
                    {
                        throw new Exception(_LanguagePack["common_uploadfile_file_not_exist"]);
                    }
                }
                else
                {
                    throw new Exception(_LanguagePack["common_uploadfile_no_file"]);
                }
            }
            catch
            {
                //返回信息
                _result.Data = new
                {
                    total = 0,
                    rows = new List<DeliveryDto>()
                };
            }
            return _result;
        }

        [HttpPost]
        public ActionResult Save()
        {
            //加载语言包
            var _LanguagePack = GetLanguagePack;

            JsonResult _result = new JsonResult();

            //错误信息
            List<DeliveryDto> _errorList = new List<DeliveryDto>();
            //文件路径
            string _filePath = VariableHelper.SaferequestNull(Request.Form["filepath"]);

            try
            {
                if (System.IO.File.Exists(Server.MapPath(_filePath)))
                {
                    List<DeliveryDto> deliveries = DeliveryService.ConvertToDeliverys(Server.MapPath(_filePath));
                    foreach (var item in deliveries)
                    {
                        Deliverys delivery = new Deliverys
                        {
                            OrderNo = VariableHelper.SaferequestStr(item.OrderNo),
                            SubOrderNo = VariableHelper.SaferequestStr(item.SubOrderNo),
                            MallSapCode = VariableHelper.SaferequestStr(item.MallSapCode),
                            InvoiceNo = VariableHelper.SaferequestStr(item.DeliveryInvoice),
                            DeliveryDate = (!string.IsNullOrEmpty(item.DeliveryDate)) ? VariableHelper.SaferequestStr(item.DeliveryDate) : DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            ExpressName = VariableHelper.SaferequestStr(item.DeliveryName),
                            Packages = 1
                        };
                        DeliveryDto itemResult = DeliveryService.SaveDeliverys(delivery, item.DeliveryCode, "Push the Delivery Invoice manually");
                        if (!itemResult.Result)
                        {
                            //写入错误
                            item.SubOrderNo = $"<span class=\"color_danger\">{itemResult.SubOrderNo}</span>";
                            item.Result = false;
                            item.ResultMsg = $"<span class=\"color_danger\">{itemResult.ResultMsg}</span>";
                            _errorList.Add(item);
                        }
                    }

                    //返回信息
                    if (_errorList.Count == 0)
                    {
                        _result.Data = new
                        {
                            result = true,
                            msg = _LanguagePack["common_data_save_success"],
                            rows = _errorList
                        };
                    }
                    else
                    {
                        _result.Data = new
                        {
                            result = true,
                            msg = _LanguagePack["common_partial_data_save_success"],
                            rows = _errorList
                        };
                    }
                }
                else
                {
                    throw new Exception(_LanguagePack["common_uploadfile_file_not_exist"]);
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
}
