using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Samsonite.Utility.Common;

using SagawaSdk.Domain;


namespace OMS.App.Controllers
{
    public class ExternalInterfaceController : InterfaceController
    {
        #region KerryExpress状态接收
        [HttpPost]
        public JsonResult SagawaGoBack()
        {
            JsonResult _result = new JsonResult();
            try
            {
                var _token = VariableHelper.SaferequestNull(Request.Headers["X-API-Key"]);
                var _body = string.Empty;
                using (StreamReader sr = new StreamReader(Request.InputStream))
                {
                    _body = sr.ReadToEnd();
                }

                //解析参数
                var datas = JsonHelper.JsonDeserialize<ExplanationInfo>(_body);

                //返回信息
                _result.Data = new
                {
                    resultCd = "0"
                };
            }
            catch
            {
                //返回信息
                _result.Data = new
                {
                    resultCd = "0"
                };
            }
            return _result;
        }
        #endregion

        #region Lazada AuthorizationCode
        public ActionResult GetLazadaAuthorizationCode()
        {
            ViewBag.AuthorizationCode = VariableHelper.SaferequestNull(Request.QueryString["Code"]);

            return View();
        }
        #endregion 

        #region Shopee AuthorizationCode
        public ActionResult GetShopeeAuthorizationCode()
        {
            ViewBag.ShopID = VariableHelper.SaferequestNull(Request.QueryString["shop_id"]);

            return View();
        }
        #endregion 
    }
}