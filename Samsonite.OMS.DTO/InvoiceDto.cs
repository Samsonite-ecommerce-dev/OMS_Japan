using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Samsonite.OMS.DTO
{
    /// <summary>
    /// 发票信息
    /// </summary>
    [Serializable]
    public class InvoiceDto
    {
        /// <summary>
        /// 发票类型
        /// </summary>
        [JsonProperty("invoiceType")]
        public string InvoiceType { get; set; }

        /// <summary>
        /// 发票号
        /// </summary>
        [JsonProperty("invoiceNo")]
        public string InvoiceNo { get; set; }

        /// <summary>
        /// 发票抬头
        /// </summary>
        [JsonProperty("invoiceTitle")]
        public string InvoiceTitle { get; set; }

        /// <summary>
        /// 发票金额
        /// </summary>
        [JsonProperty("invoiceAmount")]
        public string InvoiceAmount { get; set; }
    }
}
