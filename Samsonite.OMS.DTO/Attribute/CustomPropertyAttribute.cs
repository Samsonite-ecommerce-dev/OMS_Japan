using System;

namespace Samsonite.OMS.DTO
{
    /// <summary>
    /// 自定义特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class CustomPropertyAttribute : Attribute
    {
        public string CustomName { get; set; }
    }
}
