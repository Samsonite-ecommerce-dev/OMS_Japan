using System;
using System.Linq;

using Samsonite.OMS.Encryption.Interface;

namespace Samsonite.OMS.Encryption.Field
{
    public class DynamicEncryption : EncryptionBase, IEncryptionField
    {
        private object objMessage = null;
        private string[] objFields = null;
        private HideField[] objHideFields = null;
        public DynamicEncryption(object obj, string[] fields)
        {
            objMessage = obj;
            objFields = fields;
            objHideFields = new HideField[fields.Count()];
            for (var i = 0; i < objFields.Length; i++)
            {
                objHideFields[i] = new HideField(objFields[i]);
            }
        }

        /// <summary>
        /// 加密相关字段信息
        /// </summary>
        public void Encrypt()
        {
            Encrypt_DecryptField(objMessage, objFields, true);
        }

        /// <summary>
        /// 解密相关字段信息
        /// </summary>
        public void Decrypt()
        {
            Encrypt_DecryptField(objMessage, objFields, false);
        }

        /// <summary>
        /// 脱敏相关字段信息
        /// </summary>
        /// <param name="isDecryption">是否需要先解密</param>
        public void HideSensitive(bool isDecryption = true)
        {
            HideSensitiveField(objMessage, objHideFields, isDecryption);
        }
    }
}
