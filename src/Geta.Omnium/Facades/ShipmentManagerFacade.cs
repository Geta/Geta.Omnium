using System;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;

namespace Geta.Omnium.Facades
{
    [ServiceConfiguration(typeof(IShipmentManagerFacade))]
    public class ShipmentManagerFacade : IShipmentManagerFacade
    {
        public ShippingMethodDto GetShippingMethod(Guid shippingMethodId, bool returnInactive)
        {
            return ShippingManager.GetShippingMethod(shippingMethodId, returnInactive);
        }
    }
}
