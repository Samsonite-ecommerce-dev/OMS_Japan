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
    
    public partial class MallProductPriceRange
    {
        public long ID { get; set; }
        public long MP_ID { get; set; }
        public string SKU { get; set; }
        public decimal SalesPrice { get; set; }
        public Nullable<System.DateTime> SalesValidBegin { get; set; }
        public Nullable<System.DateTime> SalesValidEnd { get; set; }
        public bool IsDefault { get; set; }
    }
}