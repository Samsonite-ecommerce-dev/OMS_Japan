using System;
using System.Collections.Generic;
using System.Linq;
using Samsonite.OMS.ECommerce.Interface;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;

namespace Samsonite.OMS.ECommerce
{
    public class ECommerceUtil
    {
        /// <summary>
        /// 平台实例集合
        /// </summary>
        /// <returns></returns>
        private static List<StoreInstance> PlatformInstanceReflect()
        {
            List<StoreInstance> _result = new List<StoreInstance>();
            //日本
            _result.Add(new StoreInstance() { Country = "Japan", PlatformCode = (int)PlatformType.TUMI_Japan, CommerceAPI = new Japan.Tumi.TumiControl() });
            //_result.Add(new StoreInstance() { Country = "Japan", PlatformCode = (int)PlatformType.Micros_Japan, CommerceAPI = new Japan.Micros.MicrosControl() });
            return _result;
        }

        /// <summary>
        /// 获取API 实例
        /// </summary>
        /// <param name="objPlatformCode"></param>
        /// <returns></returns>
        private static IECommerceAPI GetApiInstance(int objPlatformCode)
        {
            IECommerceAPI instance = null;
            var _O = PlatformInstanceReflect().Where(p => p.PlatformCode == objPlatformCode).SingleOrDefault();
            if (_O != null)
            {
                instance = _O.CommerceAPI;
            }
            return instance;
        }

        /// <summary>
        /// 获取所有店铺接口
        /// 1.线上虚拟店/线下实体店
        /// 2.有效的
        /// 3.开启数据下载的
        /// </summary>
        /// <returns></returns>
        public static List<IECommerceAPI> GetAPIs()
        {
            var sdks = new List<IECommerceAPI>();

            using (var db = new ebEntities())
            {
                //读取所有开启数据下载的有效店铺
                List<View_Mall_Platform> objMalls = db.View_Mall_Platform.Where(p => p.IsUsed && p.IsOpenService).OrderBy(p => p.SortID).ToList();
                foreach (var _m in objMalls)
                {
                    //调用对象
                    var apiInstance = GetApiInstance(_m.PlatformCode);
                    if (apiInstance != null)
                    {
                        //设置参数
                        apiInstance.InitPara(_m);
                        sdks.Add(apiInstance);
                    }
                }
            }
            return sdks;
        }

        /// <summary>
        /// 获取所有店铺接口
        /// 1.线上虚拟店/线下实体店
        /// </summary>
        /// <returns></returns>
        public static List<IECommerceAPI> GetWholeAPIs()
        {
            var sdks = new List<IECommerceAPI>();

            using (var db = new ebEntities())
            {
                List<View_Mall_Platform> objMalls = db.View_Mall_Platform.OrderBy(p => p.SortID).ToList();
                foreach (var _m in objMalls)
                {
                    //调用对象
                    var apiInstance = GetApiInstance(_m.PlatformCode);
                    if (apiInstance != null)
                    {
                        //设置参数
                        apiInstance.InitPara(_m);
                        sdks.Add(apiInstance);
                    }
                }
            }
            return sdks;
        }

        /// <summary>
        /// 生成子订单号
        /// </summary>
        /// <param name="objPlatformCode"></param>
        /// <param name="objOrderNo"></param>
        /// <param name="objSubOrderNo"></param>
        /// <param name="objNum"></param>
        /// <returns></returns>
        public static string CreateSubOrderNo(int objPlatformCode, string objOrderNo, string objSubOrderNo, int objNum)
        {
            string _prefix = string.Empty;
            //如果该平台有子订单号,则在子订单号加上前缀,不然则后面增加数字
            if (!string.IsNullOrEmpty(objSubOrderNo))
            {
                return $"{objSubOrderNo}";
            }
            else
            {
                return $"{objOrderNo}_{objNum}";
            }
        }

        public class StoreInstance
        {
            /// <summary>
            /// 所属国家
            /// </summary>
            public string Country { get; set; }

            /// <summary>
            /// 平台编号
            /// </summary>
            public int PlatformCode { get; set; }

            /// <summary>
            /// 对象实例
            /// </summary>
            public IECommerceAPI CommerceAPI { get; set; }
        }
    }
}