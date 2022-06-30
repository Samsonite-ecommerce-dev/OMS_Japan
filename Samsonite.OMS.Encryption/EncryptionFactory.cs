using System;
using System.Linq;
using System.Collections.Generic;

using Samsonite.OMS.Database;
using Samsonite.OMS.Encryption.Field;
using Samsonite.OMS.Encryption.Interface;

namespace Samsonite.OMS.Encryption
{
    public class EncryptionFactory
    {
        private static IDictionary<Type, Type> IEncryptionMapping()
        {
            IDictionary<Type, Type> _result = new Dictionary<Type, Type>();
            _result.Add(typeof(Customer), typeof(CustomerEncryption));
            _result.Add(typeof(OrderReceive), typeof(OrderReceiveEncryption));
            _result.Add(typeof(OrderModify), typeof(OrderModifyEncryption));
            _result.Add(typeof(OrderReturn), typeof(OrderReturnEncryption));
            _result.Add(typeof(OrderExchange), typeof(OrderExchangeEncryption));
            _result.Add(typeof(OrderBilling), typeof(OrderBillingEncryption));
            _result.Add(typeof(UserEmployee), typeof(UserEmployeeEncryption));
            _result.Add(typeof(View_OrderModify), typeof(View_OrderModifyEncryption));
            _result.Add(typeof(View_OrderReturn), typeof(View_OrderReturnEncryption));
            _result.Add(typeof(View_OrderExchange), typeof(View_OrderExchangeEncryption));
            _result.Add(typeof(View_UserEmployee), typeof(View_UserEmployeeEncryption));
            return _result;
        }


        /// <summary>
        ///  创建实体类对象字段加密类
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEncryptionField Create<T>(T obj)
        {
            IEncryptionField _result = null;

            var o = IEncryptionMapping().Where(p => p.Key == obj.GetType()).SingleOrDefault();
            if (o.Key != null)
            {
                _result = (IEncryptionField)Activator.CreateInstance(o.Value, obj);
            }
            return _result;
        }

        /// <summary>
        /// 创建动态对象字段加密类
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="objFields"></param>
        /// <returns></returns>
        public static IEncryptionField Create(object obj, string[] objFields)
        {
            return new DynamicEncryption(obj, objFields);
        }
    }
}
