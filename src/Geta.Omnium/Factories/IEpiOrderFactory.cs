using System.Collections;
using System.Collections.Generic;
using EPiServer.Commerce.Order;
using Omnium.Public.Models;
using Omnium.Public.Orders.Models;
using Omnium.Public.Payments.Models;
using Omnium.Public.Shipments.Models;

namespace Geta.Omnium.Factories
{
    public interface IEpiOrderFactory
    {
        bool ShouldSyncOrderBackToOmnium(IPurchaseOrder purchaseOrder, OmniumOrder omniumOrder);
        IPurchaseOrder MapOrder(IPurchaseOrder purchaseOrder, OmniumOrder omniumOrder);
        void MapOrderProperties(IPurchaseOrder purchaseOrder, OmniumOrder omniumOrder);
        void MapBillingAddress(IPurchaseOrder purchaseOrder, OmniumOrderAddress omniumOrderBillingAddress);
        void MapOrderForm(IPurchaseOrder purchaseOrder, IOrderForm orderForm, OmniumOrderForm omniumOrderForm);
        IOrderAddress MapAddress(IPurchaseOrder purchaseOrder, IOrderAddress address, OmniumOrderAddress omniumAddress);
        IPayment MapPayment(IPurchaseOrder purchaseOrder, IPayment payment, OmniumPayment omniumPayment);
        IShipment MapShipment(IPurchaseOrder purchaseOrder, IShipment shipment, OmniumShipment omniumShipment);
        ILineItem MapLineitem(ILineItem lineItem, OmniumOrderLine omniumOrderLine);
        void MapProperties(Hashtable properties, IEnumerable<OmniumPropertyItem> omniumPropertyItem);
    }
}
