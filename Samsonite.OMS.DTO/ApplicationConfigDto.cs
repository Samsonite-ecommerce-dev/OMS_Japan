using System;
using System.Collections.Generic;

namespace Samsonite.OMS.DTO
{
    public class ApplicationConfigDto
    {
        /// <summary>
        /// 语言版本
        /// </summary>
        public List<int> LanguagePacks { get; set; }

        /// <summary>
        /// 系统内产品ID
        /// </summary>
        public string ProductIDConfig { get; set; }

        /// <summary>
        /// 支付方式
        /// </summary>
        public List<int> PaymentTypeConfig { get; set; }

        /// <summary>
        /// Samsonite库存报警数量
        /// </summary>
        public int WarningInventoryNumConfig { get; set; }

        /// <summary>
        /// Tumi库存报警数量
        /// </summary>
        public int WarningInventoryNumTumiConfig { get; set; }

        /// <summary>
        /// 小数点精确位数
        /// </summary>
        public int AmountAccuracy { get; set; }

        /// <summary>
        /// 套装审核流程
        /// </summary>
        public List<string> BundleApproval { get; set; }

        /// <summary>
        /// 促销活动审核流程
        /// </summary>
        public List<string> PromotionApproval { get; set; }

        /// <summary>
        /// 邮件配置
        /// </summary>
        public EmailDto EmailConfig { get; set; }

        /// <summary>
        /// 是否使用api
        /// </summary>
        public bool IsUseAPI { get; set; }

        /// <summary>
        /// 样式
        /// </summary>
        public string SkinStyle { get; set; }
    }
}
