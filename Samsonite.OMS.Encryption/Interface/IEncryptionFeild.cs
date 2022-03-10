using System;

namespace Samsonite.OMS.Encryption.Interface
{
    /// <summary>
    /// 实体对象加解密接口
    /// </summary>
    public interface IEncryptionField
    {
        /// <summary>
        /// 加密相关字段信息
        /// </summary>
        void Encrypt();

        /// <summary>
        /// 解密相关字段信息
        /// </summary>
        void Decrypt();

        /// <summary>
        /// 脱敏相关字段信息
        /// </summary>
        /// <param name="isDecryption">是否需要先解密</param>
        void HideSensitive(bool isDecryption = true);
    }
}
