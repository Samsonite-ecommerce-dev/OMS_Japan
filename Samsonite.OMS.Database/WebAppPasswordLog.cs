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
    
    public partial class WebAppPasswordLog
    {
        public long LogID { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
        public int UserID { get; set; }
        public string IP { get; set; }
        public string Remark { get; set; }
        public System.DateTime AddTime { get; set; }
    }
}
