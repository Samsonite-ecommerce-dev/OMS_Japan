﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class logEntities : DbContext
    {
        public logEntities()
            : base("name=logEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<ECommercePlatformApiLog> ECommercePlatformApiLog { get; set; }
        public virtual DbSet<ExternalInterfaceLog> ExternalInterfaceLog { get; set; }
        public virtual DbSet<ServiceLog> ServiceLog { get; set; }
        public virtual DbSet<WebApiAccessLog> WebApiAccessLog { get; set; }
        public virtual DbSet<WebApiLog> WebApiLog { get; set; }
        public virtual DbSet<WebAppErrorLog> WebAppErrorLog { get; set; }
        public virtual DbSet<WebAppLoginLog> WebAppLoginLog { get; set; }
        public virtual DbSet<WebAppOperationLog> WebAppOperationLog { get; set; }
        public virtual DbSet<WebAppPasswordLog> WebAppPasswordLog { get; set; }
    }
}