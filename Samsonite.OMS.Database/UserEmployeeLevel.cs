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
    
    public partial class UserEmployeeLevel
    {
        public int LevelID { get; set; }
        public string LevelName { get; set; }
        public string LevelKey { get; set; }
        public bool IsAmountLimit { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsQuantityLimit { get; set; }
        public int TotalQuantity { get; set; }
        public bool IsDefault { get; set; }
        public string Remark { get; set; }
        public System.DateTime AddTime { get; set; }
    }
}
