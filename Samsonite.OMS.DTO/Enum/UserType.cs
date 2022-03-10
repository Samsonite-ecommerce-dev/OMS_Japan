using System;

namespace Samsonite.OMS.DTO
{
    /// <summary>
    /// 账号类型
    /// </summary>
    public enum UserType
    {
        /// <summary>
        /// 内部人员
        /// </summary>
        InternalStaff = 0,
        /// <summary>
        /// 仓库人员
        /// </summary>
        WarehouseStaff = 1
    }

    public enum UserStatus
    {
        /// <summary>
        /// 正常
        /// </summary>
        Normal = 0,
        /// <summary>
        /// 被锁定
        /// </summary>
        Locked = 1,
        /// <summary>
        /// 密码过期
        /// </summary>
        ExpiredPwd = 2
    }
}
