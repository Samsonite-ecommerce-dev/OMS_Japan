using System;
using Samsonite.OMS.ECommerce.Models;
using Samsonite.OMS.Service.Sap.Poslog.Models;

namespace OMS.ToolWPF.Models
{
    public class CommonResponse
    {
        public long RowID { get; set; }

        public string SuccessFiles { get; set; }

        public string FailFiles { get; set; }

        public int TotalRecord { get; set; }

        public int SuccessRecord { get; set; }

        public int FailRecord { get; set; }
    }

    public class ProductResponse : ProductResult
    {
        public long RowID { get; set; }

        public bool Result { get; set; }

        public string ResultMessage { get; set; }
    }

    public class OrderResponse : OrderResult
    {
        public long RowID { get; set; }

        public bool Result { get; set; }

        public string ResultMessage { get; set; }
    }

    public class ClaimResponse : ClaimResult
    {
        public long RowID { get; set; }

        public bool Result { get; set; }

        public string ResultMessage { get; set; }
    }

    public class DeliveryResponse : DeliveryResult
    {
        public long RowID { get; set; }

        public bool Result { get; set; }

        public string ResultMessage { get; set; }
    }

    public class InventoryResponse : InventoryResult
    {
        public long RowID { get; set; }

        public bool Result { get; set; }

        public string ResultMessage { get; set; }
    }

    public class PriceResponse : PriceResult
    {
        public long RowID { get; set; }

        public bool Result { get; set; }

        public string ResultMessage { get; set; }
    }

    public class DetailResponse : DetailResult
    {
        public long RowID { get; set; }

        public bool Result { get; set; }

        public string ResultMessage { get; set; }
    }

    public class OutboundDeliveryResponse
    {
        public long RowID { get; set; }

        public string MallSapCode { get; set; }

        public string FileName { get; set; }

        public int TotalRecord { get; set; }

        public int SuccessRecord { get; set; }

        public int FailRecord { get; set; }
    }

    public class PoslogResponse: PoslogUploadResult
    {
        public long RowID { get; set; }

        public bool Result { get; set; }

        public string ResultMessage { get; set; }
    }

    public class EmailResponse
    {
        public long RowID { get; set; }

        public long EmailID { get; set; }

        public string RecvEmail { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public bool Status { get; set; }

        public int SendCount { get; set; }

        public string ResultMessage { get; set; }

        public DateTime CreateTime { get; set; }
    }

    public class SMSResponse
    {
        public long RowID { get; set; }

        public long SMSID { get; set; }

        public string RecvMobile { get; set; }

        public string Sender { get; set; }

        public string Content { get; set; }

        public bool Status { get; set; }

        public int SendCount { get; set; }

        public string ResultMessage { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
