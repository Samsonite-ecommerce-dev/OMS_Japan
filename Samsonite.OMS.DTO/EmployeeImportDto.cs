using System;

namespace Samsonite.OMS.DTO
{
    /// <summary>
    /// 员工导入模型
    /// </summary>
    public class EmployeeImportDto
    {
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 限制条件
        /// </summary>
        public string LevelKey { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public bool Effect { get; set; }

        public bool Result { get; set; }

        public string ResultMsg { get; set; }
    }
}
