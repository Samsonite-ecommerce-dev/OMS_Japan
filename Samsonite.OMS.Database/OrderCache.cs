//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Samsonite.OMS.Database
{
    using System;
    using System.Collections.Generic;
    
    public partial class OrderCache
    {
        public long ID { get; set; }
        public string MallSapCode { get; set; }
        public string OrderNo { get; set; }
        public string DataString { get; set; }
        public System.DateTime AddDate { get; set; }
        public int Status { get; set; }
        public int ErrorCount { get; set; }
        public string ErrorMessage { get; set; }
    }
}
