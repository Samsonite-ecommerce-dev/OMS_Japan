using System;
using System.Collections.Generic;
using System.Linq;

using Samsonite.Utility.Encryption;

namespace Samsonite.OMS.Encryption
{
    public class EncryptionBase
    {
        #region 加解密字段
        /// <summary>
        /// 加密相关字段信息
        /// </summary>
        /// <param name="value"></param>
        public static string EncryptString(string value)
        {
            string _result = string.Empty;
            if (!string.IsNullOrEmpty(value))
            {
                _result = value.AesEncrypt(EncryptionConfig.Key, EncryptionConfig.ivParameter);
            }
            return _result;
        }


        /// <summary>
        /// 解密相关字段信息
        /// </summary>
        /// <param name="value"></param>
        public static string DecryptString(string value)
        {
            string _result = string.Empty;
            if (!string.IsNullOrEmpty(value))
            {
                _result = value.AesDecrypt(EncryptionConfig.Key, EncryptionConfig.ivParameter);
            }
            return _result;
        }
        #endregion

        #region 加解密对象
        /// <summary>
        /// 加解密相关对象
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="objFields"></param>
        /// <param name="isEncrypt">是否加密</param>
        protected void Encrypt_DecryptField(object obj, string[] objFields, bool isEncrypt)
        {
            if (obj != null)
            {
                var type = obj.GetType();
                //判断是不是动态对象.动态对象需要特殊处理
                if (type.FullName == "System.Dynamic.ExpandoObject")
                {
                    var dict = (IDictionary<string, object>)obj;
                    foreach (var field in objFields)
                    {
                        if (dict.ContainsKey(field))
                        {
                            var value = dict[field];
                            //判断是否是string类型
                            if (value is string)
                            {
                                string v = (string)value;
                                if (!string.IsNullOrEmpty(v))
                                {
                                    //加密前先去空格
                                    dict[field] = (isEncrypt) ? v.Trim().AesEncrypt(EncryptionConfig.Key, EncryptionConfig.ivParameter) : v.AesDecrypt(EncryptionConfig.Key, EncryptionConfig.ivParameter);
                                }
                            }
                        }
                    }
                }
                else
                {
                    var props = type.GetProperties().ToList();
                    foreach (var field in objFields)
                    {
                        //通过反射来进行加解密
                        var prop = props.Where(t => t.Name.ToLower() == field.ToLower()).FirstOrDefault();
                        if (prop != null)
                        {
                            if (prop.PropertyType == typeof(string))
                            {
                                string v = (string)prop.GetValue(obj);
                                if (!string.IsNullOrEmpty(v))
                                {
                                    if (isEncrypt)
                                    {
                                        //加密前先去空格
                                        prop.SetValue(obj, v.Trim().AesEncrypt(EncryptionConfig.Key, EncryptionConfig.ivParameter));
                                    }
                                    else
                                    {
                                        prop.SetValue(obj, v.AesDecrypt(EncryptionConfig.Key, EncryptionConfig.ivParameter));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region 脱敏字段
        /// <summary>
        /// 脱敏相关对象
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="hideFields"></param>
        /// <param name="isDecrypt">是否需要先解密</param>
        protected void HideSensitiveField(object obj, HideField[] hideFields, bool isDecrypt)
        {
            if (obj != null)
            {
                var type = obj.GetType();
                //判断是不是动态对象.动态对象需要特殊处理
                if (type.FullName == "System.Dynamic.ExpandoObject")
                {
                    var dict = (IDictionary<string, object>)obj;
                    foreach (var field in hideFields)
                    {
                        if (dict.ContainsKey(field.FieldName))
                        {
                            var value = dict[field.FieldName];
                            if (value is string) //判断是否是string类型
                            {
                                var v = (string)value;
                                var encryptValue = isDecrypt ? v.AesDecrypt(EncryptionConfig.Key, EncryptionConfig.ivParameter) : v;
                                encryptValue = encryptValue.HideSensitiveInfo(field.Sublen, field.BasedOnLeft);
                                dict[field.FieldName] = encryptValue;
                            }
                        }
                    }
                }
                else
                {
                    var props = type.GetProperties().ToList();
                    foreach (var field in hideFields)
                    {
                        //通过反射来进行加解密
                        var prop = props.FirstOrDefault(t => t.Name.ToLower() == field.FieldName.ToLower());
                        if (prop != null)
                        {
                            if (prop.PropertyType == typeof(string))
                            {
                                string v = (string)prop.GetValue(obj);
                                var encryptValue = isDecrypt ? v.AesDecrypt(EncryptionConfig.Key, EncryptionConfig.ivParameter) : v;
                                encryptValue = encryptValue.HideSensitiveInfo(field.Sublen, field.BasedOnLeft);
                                prop.SetValue(obj, encryptValue);
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }

    public class HideField
    {
        /// <summary>
        /// 字段名称
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// <param name="sublen">信息总长与左子串（或右子串）的比例</param>
        /// </summary>
        public int Sublen { get; set; } = 5;

        /// <summary>
        /// <param name="basedOnLeft">当长度异常时,是否显示左边,默认true,默认显示左边</param>
        /// </summary>
        public bool BasedOnLeft { get; set; } = true;

        public HideField(string fieldName) : base()
        {
            FieldName = fieldName;
        }
    }
}
