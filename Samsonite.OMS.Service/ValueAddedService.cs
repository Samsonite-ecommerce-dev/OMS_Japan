using System;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.DTO;
using Samsonite.OMS.Database;
using Samsonite.Utility.Common;

namespace Samsonite.OMS.Service
{
    public class ValueAddedService
    {
        /// <summary>
        /// Patch
        /// </summary>
        public const string MONOGRAM_PATCH = "MONOPATCH";

        /// <summary>
        /// tag
        /// </summary>
        public const string MONOGRAM_TAG = "MONOTAG";

        /// <summary>
        /// able
        /// </summary>
        public const string MONOGRAM_ABLE = "MONOABLE";

        /// <summary>
        /// giftCard
        /// </summary>
        public const string GIFT_CARD = "GIFT_CARD";

        /// <summary>
        /// 解析增值服务信息
        /// </summary>
        /// <param name="objOrderValueAddedServices"></param>
        /// <returns></returns>
        public static ValueAddedServicesDto ParseInfo(List<OrderValueAddedService> objOrderValueAddedServices)
        {
            ValueAddedServicesDto _result = new ValueAddedServicesDto()
            {
                Monograms = new List<MonogramDto>(),
                GiftBoxInfo = null,
                GiftCardInfo = null
            };
            if (objOrderValueAddedServices.Count > 0)
            {
                //Monogram
                var monograms = objOrderValueAddedServices.Where(p => p.Type == (int)ValueAddedServicesType.Monogram).ToList();
                foreach (var item in monograms)
                {
                    var monogramTmp = JsonHelper.JsonDeserialize<MonogramDto>(item.MonoValue);
                    monogramTmp.Location = item.MonoLocation;
                    _result.Monograms.Add(monogramTmp);
                }

                //GiftBox
                var giftBoxInfo = objOrderValueAddedServices.Where(p => p.Type == (int)ValueAddedServicesType.GiftBox).SingleOrDefault();
                if (giftBoxInfo != null)
                {
                    _result.GiftBoxInfo = JsonHelper.JsonDeserialize<GiftBoxDto>(giftBoxInfo.MonoValue);
                }

                //Gift Card
                var giftCardInfo = objOrderValueAddedServices.Where(p => p.Type == (int)ValueAddedServicesType.GiftCard).SingleOrDefault();
                if (giftCardInfo != null)
                {
                    _result.GiftCardInfo = JsonHelper.JsonDeserialize<GiftCardDto>(giftCardInfo.MonoValue);
                }
            }
            return _result;
        }
    }
}