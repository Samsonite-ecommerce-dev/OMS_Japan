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
    
    public partial class View_DeliverysNote
    {
        public long NoteID { get; set; }
        public string DeliveryNo { get; set; }
        public string MallSapCode { get; set; }
        public string NoteMessage { get; set; }
        public bool IsDeal { get; set; }
        public System.DateTime CreateTime { get; set; }
        public int OperUserID { get; set; }
        public Nullable<System.DateTime> OperTime { get; set; }
        public bool IsUsed { get; set; }
        public string MallName { get; set; }
        public string OperUserName { get; set; }
    }
}
