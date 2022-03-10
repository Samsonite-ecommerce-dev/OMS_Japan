using System;
using System.Linq;
using System.Collections.Generic;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;
using Samsonite.OMS.Encryption;
using Samsonite.Utility.Common;

namespace Samsonite.OMS.Service
{
    /// <summary>
    /// 收货信息
    /// </summary>
    public class OrderReceiveService
    {
        /// <summary>
        /// 获取最新的收货信息
        /// </summary>
        /// <param name="objOrderNo"></param>
        /// <param name="objSubOrderNo"></param>
        /// <returns></returns>
        public static ReceiveDto GetNewestReceive(string objOrderNo, string objSubOrderNo = "")
        {
            ReceiveDto _result = new ReceiveDto();
            using (var db = new ebEntities())
            {
                OrderReceive objOrderReceive = db.OrderReceive.Where(p => p.OrderNo == objOrderNo && p.SubOrderNo == objSubOrderNo).SingleOrDefault();
                if (objOrderReceive != null)
                {
                    //解密数据
                    EncryptionFactory.Create(objOrderReceive).Decrypt();

                    _result.OrderNo = objOrderReceive.OrderNo;
                    _result.SubOrderNo = objOrderReceive.SubOrderNo;
                    _result.Receiver = objOrderReceive.Receive;
                    _result.ReceiverEmail = objOrderReceive.ReceiveEmail;
                    _result.Tel = objOrderReceive.ReceiveTel;
                    _result.Mobile = objOrderReceive.ReceiveCel;
                    _result.ZipCode = objOrderReceive.ReceiveZipcode;
                    _result.Country = objOrderReceive.Country;
                    _result.City = objOrderReceive.City;
                    _result.Address = objOrderReceive.ReceiveAddr;

                    //查询是否修改过收货信息
                    OrderModify objOrderModify = db.OrderModify.Where(p => p.OrderNo == objOrderNo && p.SubOrderNo == objSubOrderNo && p.Status == (int)ProcessStatus.ModifyComplete).OrderByDescending(p => p.Id).FirstOrDefault();
                    if (objOrderModify != null)
                    {
                        //解密数据
                        EncryptionFactory.Create(objOrderModify).Decrypt();

                        _result.Receiver = objOrderModify.CustomerName;
                        _result.Tel = objOrderModify.Tel;
                        _result.Mobile = objOrderModify.Mobile;
                        _result.ZipCode = objOrderModify.Zipcode;
                        _result.Address = objOrderModify.Addr;
                    }
                }
            }
            return _result;
        }

        /// <summary>
        /// 获取地址信息
        /// </summary>
        /// <param name="objReceiveDto"></param>
        /// <returns></returns>
        public static string GetReceiveMessage(ReceiveDto objReceiveDto)
        {
            string _result = string.Empty;
            if (!string.IsNullOrEmpty(objReceiveDto.Receiver))
            {
                _result += "," + objReceiveDto.Receiver;
            }
            if (!string.IsNullOrEmpty(objReceiveDto.Tel))
            {
                _result += "," + objReceiveDto.Tel;
            }
            if (!string.IsNullOrEmpty(objReceiveDto.Mobile))
            {
                _result += "," + objReceiveDto.Mobile;
            }
            if (!string.IsNullOrEmpty(objReceiveDto.ZipCode))
            {
                _result += "," + objReceiveDto.ZipCode;
            }
            if (!string.IsNullOrEmpty(objReceiveDto.Address))
            {
                _result += "," + objReceiveDto.Address;
            }
            if (_result.Length > 0)
            {
                _result = _result.Substring(1);
            }
            return _result;
        }

        /// <summary>
        /// 分割地址信息
        /// </summary>
        /// <param name="objStr"></param>
        /// <returns></returns>
        public static List<string> SplitAddress(string objStr)
        {
            List<string> _result = new List<string>();
            //每个地址信息最大数量
            int maxLength = 39;
            int _t = 0;
            int _tmp_js = 0;
            string _tmp_str = string.Empty;
            foreach (char str in objStr)
            {
                _t++;
                if (_tmp_js + VariableHelper.ThaiStrLength(str.ToString()) > maxLength)
                {
                    _result.Add(_tmp_str);
                    //新的一排计数开始
                    _tmp_js = 1;
                    _tmp_str = str.ToString();
                }
                else
                {
                    if (_t == objStr.Length)
                    {
                        _tmp_str += str.ToString();
                        _result.Add(_tmp_str);
                    }
                    else
                    {
                        _tmp_js += VariableHelper.ThaiStrLength(str.ToString());
                        _tmp_str += str.ToString();
                    }
                }
            }
            //补满5个地址
            int _c = _result.Count;
            if (_result.Count < 5)
            {
                for (int t = 0; t < 5 - _c; t++)
                {
                    _result.Add("");
                }
            }
            return _result;
        }
    }
}
