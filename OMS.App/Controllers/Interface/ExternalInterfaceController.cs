using System;
using System.Web.Mvc;

using Samsonite.Utility.Common;


namespace OMS.App.Controllers
{
    public class ExternalInterfaceController : InterfaceController
    {
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