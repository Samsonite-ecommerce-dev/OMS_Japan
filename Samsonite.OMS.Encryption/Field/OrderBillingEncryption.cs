﻿using System;

using Samsonite.OMS.Encryption.Interface;

namespace Samsonite.OMS.Encryption.Field
{
    public class OrderBillingEncryption : EncryptionBase, IEncryptionField
    {
        private object objMessage = null;
        public OrderBillingEncryption(object obj)
        {
            objMessage = obj;
        }

        /// <summary>
        /// 加密字段
        /// </summary>
        private readonly string[] _fields = { "FirstName", "LastName", "Phone", "Email", "Address1", "Address2" };

        /// <summary>
        /// 脱敏字段
        /// </summary>
        private readonly HideField[] _hideFields = { new HideField("FirstName"), new HideField("LastName"), new HideField("Phone"), new HideField("Email"), new HideField("Address1"), new HideField("Address2") };

        /// <summary>
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
