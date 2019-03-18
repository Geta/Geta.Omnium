using System;
using Mediachase.Commerce.Orders.Dto;

namespace Geta.Omnium
{
    public interface IShipmentManagerFacade
    {
        ShippingMethodDto GetShippingMethod(Guid shippingMethodId, bool returnInactive);
    }
}