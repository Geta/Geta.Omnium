using System.Collections.Generic;
using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;
using Mediachase.Commerce;
using Omnium.Public.Orders.Models;
using Omnium.Public.Payments.Models;
using Omnium.Public.Shipments.Models;

namespace Geta.Omnium.Factories
{
    public interface IOmniumOrderFactory
    {
        OmniumOrder MapOrder(IPurchaseOrder purchaseOrder);
        OmniumOrder MapOrder(IPurchaseOrder purchaseOrder, IOrderForm orderForm, IShipment[] shipment);
        OmniumOrderForm MapOrderForm(IPurchaseOrder orderGroup, IOrderForm orderForm);
        OmniumOrderForm MapOrderForm(IPurchaseOrder orderGroup, IOrderForm orderForm, IShipment[] shipments);
        OmniumShipment MapShipment(IShipment shipment, IMarket market, Currency currency);
        OmniumShipment MapShipment(IShipment shipment, IMarket market, Currency currency, IEnumerable<RewardDescription> shippingDiscounts);
        OmniumPayment MapPayment(IPayment payment, Money orderFormTotal);
        OmniumOrderLine MapOrderLine(ILineItem lineItem, IMarket market, Currency currency, IOrderAddress address);
        OmniumOrderAddress MapOrderAddress(IOrderAddress orderAddress);
    }
}
