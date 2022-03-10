using System;
using System.Collections.Generic;
using System.Linq;

namespace OMS.App.Helper
{
    public class DeliveryNoteHelper
    {
        #region DN属性
        public class DeliveryNoteNature
        {
            /// <summary>
            /// 是否紧急DN
            /// </summary>
            private bool _isUrgent = false;
            public bool IsUrgent
            {
                get { return _isUrgent; }
                set { _isUrgent = value; }
            }

            /// <summary>
            /// 是否合包
            /// </summary>
            private bool _isMergePack = false;
            public bool IsMergePack
            {
                get { return _isMergePack; }
                set { _isMergePack = value; }
            }
        }

        /// <summary>
        /// DN类型(紧急\合包)
        /// </summary>
        /// <param name="objOrderNatureLabel"></param>
        /// <returns></returns>
        public static string GetDeliveryNoteNatureLabel(DeliveryNoteNature objDeliveryNoteNatureLabel)
        {
            string _result = string.Empty;
            if (objDeliveryNoteNatureLabel.IsUrgent)
            {
                _result += "<i class=\"fa fa-flag color_danger\"></i>";
            }
            if (objDeliveryNoteNatureLabel.IsMergePack)
            {
                _result += "<i class=\"fa fa-archive color_success\"></i>";
            }
            return _result;
        }
        #endregion

    }
}