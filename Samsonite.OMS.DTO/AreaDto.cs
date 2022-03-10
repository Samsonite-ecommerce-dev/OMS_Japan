using System;
using System.Collections.Generic;

namespace Samsonite.OMS.DTO
{
    /// <summary>
    /// 地域信息
    /// </summary>
    public class AreaDto
    {
        /// <summary>
        /// 国家
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// 省/自治区/直辖市
        /// </summary>
        public string Province { get; set; }

        /// <summary>
        /// 地区(省下面的地级市)
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// 县/市(县级市)/区
        /// </summary>
        public string District { get; set; }
    }
}
