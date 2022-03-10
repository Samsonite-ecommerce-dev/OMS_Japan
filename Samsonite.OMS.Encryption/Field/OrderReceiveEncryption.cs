using System;

using Samsonite.OMS.Encryption.Interface;

namespace Samsonite.OMS.Encryption.Field
{
    public class OrderReceiveEncryption : EncryptionBase, IEncryptionField
    {
        private object objMessage = null;
        public OrderReceiveEncryption(object obj)
        {
            objMessage = obj;
        }

        /// <summary>
        /// 加密字段
        /// </summary>
        private readonly string[] _fields = { "Receive", "ReceiveEmail", "ReceiveTel", "ReceiveCel", "ReceiveAddr", "Address1", "Address2" };

        /// <summary>
        /// 脱敏字段
        /// </summary>
        private readonly HideField[] _hideFields = { new HideField("Receive"), new HideField("ReceiveEmail"), new HideField("ReceiveTel"), new HideField("ReceiveCel"), new HideField("ReceiveAddr"), new HideField("Address1"), new HideField("Address2") };

        //// <summary>
        /// 加密相关字段信息
        /// </summary>
        public void Encrypt()
        {
            Encrypt_DecryptField(objMessage, _fields, true);
        }

        /// <summary>
        /// 解密相关字段信息
        /// </summary>
        public void Decrypt()
        {
            Encrypt_DecryptField(objMessage, _fields, false);
        }

        /// <summary>
        /// 脱敏相关字段信息
        /// </summary>
        /// <param name="isDecryption">是否需要先解密</param>
        public void HideSensitive(bool isDecryption = true)
        {
            HideSensitiveField(objMessage, _hideFields, isDecryption);
        }
    }
}
