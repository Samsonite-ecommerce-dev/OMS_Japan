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
    
    public partial class ECommercePushRecord
    {
        public long Id { get; set; }
        public int PushType { get; set; }
        public string RelatedTableName { get; set; }
        public long RelatedId { get; set; }
        public string PushMessage { get; set; }
        public bool PushResult { get; set; }
        public string PushResultMessage { get; set; }
        public int PushCount { get; set; }
        public bool IsDelete { get; set; }
        public System.DateTime AddTime { get; set; }
        public System.DateTime EditTime { get; set; }
    }
}
