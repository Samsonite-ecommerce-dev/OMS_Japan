using System;
using System.Collections.Generic;

using Samsonite.OMS.DTO;

namespace Samsonite.OMS.Service
{
    public class ApiService
    {
        /// <summary>
        /// 获取api接口用途
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static int GetAPIType(string controller)
        {
            int _result = 0;
            switch (controller)
            {
                case "Warehouse":
                    _result = (int)APIType.Warehouse;
                    break;
                case "ClickCollect":
                    _result = (int)APIType.ClickCollect;
                    break;
                case "Platform":
                    _result = (int)APIType.Platform;
                    break;
            }

            return _result;
        }

        /// <summary>
        /// 功能组列表
        /// </summary>
        /// <returns></returns>
        public static List<InterfaceGroupDto> InterfaceOptions()
        {
            List<InterfaceGroupDto> _result = new List<InterfaceGroupDto>();
            //仓库接口
            _result.Add(new InterfaceGroupDto()
            {
                GroupID = 1,
                GroupName = "Warehouse",
                ControllerName = "Warehouse",
                Interfaces = new List<InterfaceDto>()
                {
                    new InterfaceDto()
                    {
                         ID=100,
                         RouteGroup="",
                         InterfaceName="Get Orders",
                         ActionName="GetOrders",
                         SeqNumber=1
                    },
                    new InterfaceDto()
                    {
                         ID=101,
                         RouteGroup="",
                         InterfaceName="Get Changed Orders",
                         ActionName="GetChangedOrders",
                         SeqNumber=2
                    },new InterfaceDto()
                    {
                         ID=102,
                         RouteGroup="",
                         InterfaceName="Post Reply",
                         ActionName="PostReply",
                         SeqNumber=3
                    },
                    new InterfaceDto()
                    {
                         ID=103,
                         RouteGroup="",
                         InterfaceName="Post Delivery",
                         ActionName="PostDelivery",
                         SeqNumber=4
                    },
                    new InterfaceDto()
                    {
                         ID=104,
                         RouteGroup="",
                         InterfaceName="Post Inventory",
                         ActionName="PostInventory",
                         SeqNumber=5
                    },
                    new InterfaceDto()
                    {
                         ID=105,
                         RouteGroup="",
                         InterfaceName="Update Shipment Status",
                         ActionName="UpdateShipmentStatus",
                         SeqNumber=6
                    },
                    new InterfaceDto()
                    {
                         ID=106,
                         RouteGroup="",
                         InterfaceName="Update WMS Status",
                         ActionName="UpdateWMSStatus",
                         SeqNumber=7
                    }
                },
                RootID = 1
            });
            //C&C接口
            _result.Add(new InterfaceGroupDto()
            {
                GroupID = 2,
                GroupName = "Click & Collect",
                ControllerName = "ClickCollect",
                Interfaces = new List<InterfaceDto>()
                {
                    new InterfaceDto()
                    {
                         ID=201,
                         RouteGroup="order",
                         InterfaceName="Get Orders",
                         ActionName="GetOrders",
                         SeqNumber=1
                    },
                    new InterfaceDto()
                    {
                         ID=202,
                         RouteGroup="order",
                         InterfaceName="Get Order Detail",
                         ActionName="GetOrderItems",
                         SeqNumber=2
                    },new InterfaceDto()
                    {
                         ID=203,
                         RouteGroup="order",
                         InterfaceName="Product ShopArrived",
                         ActionName="SetStatusToShopArrived",
                         SeqNumber=3
                    },
                    new InterfaceDto()
                    {
                         ID=204,
                         RouteGroup="order",
                         InterfaceName="Product Delivered",
                         ActionName="SetStatusToDelivered",
                         SeqNumber=4
                    },
                    new InterfaceDto()
                    {
                         ID=211,
                         RouteGroup="claim",
                         InterfaceName="Order Cancel",
                         ActionName="OrderCancel",
                         SeqNumber=5
                    },
                    new InterfaceDto()
                    {
                         ID=212,
                         RouteGroup="claim",
                         InterfaceName="Order Return",
                         ActionName="OrderCancel",
                         SeqNumber=6
                    },
                    new InterfaceDto()
                    {
                         ID=221,
                         RouteGroup="store",
                         InterfaceName="Get Stores",
                         ActionName="GetStores",
                         SeqNumber=7
                    },
                    new InterfaceDto()
                    {
                         ID=231,
                         RouteGroup="sms",
                         InterfaceName="Send SMS",
                         ActionName="SendSMS",
                         SeqNumber=8
                    }
                },
                RootID = 2
            });

            return _result;
        }
    }
}
