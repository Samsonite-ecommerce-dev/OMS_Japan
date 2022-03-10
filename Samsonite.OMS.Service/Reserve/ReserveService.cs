using System;
using System.Collections.Generic;
using System.Linq;

using Samsonite.OMS.Database;
using Samsonite.OMS.DTO;

namespace Samsonite.OMS.Service.Reserve
{
    /// <summary>
    /// Reserve相关
    /// </summary>
    public class ReserveService
    {
        /// <summary>
        /// 查询预售订单是否需要发送给WMS
        /// </summary>
        /// <returns></returns>
        public static CommonResult SetReserveOrder()
        {
            CommonResult _result = new CommonResult();
            using (var db = new ebEntities())
            {
                List<int> _statusList = new List<int>() { (int)ProductStatus.Pending, (int)ProductStatus.Received };
                var objOrderDetail_List = db.OrderDetail.Where(p => p.IsReservation && p.IsStop && _statusList.Contains(p.Status) && p.ReservationDate <= DateTime.Today).ToList();
                foreach (var item in objOrderDetail_List)
                {
                    _result.TotalRecord++;
                    try
                    {
                        item.IsStop = false;
                        db.SaveChanges();

                        _result.SuccessRecord++;
                    }
                    catch
                    {
                        _result.FailRecord++;
                    }
                }
            }
            return _result;
        }
    }
}
