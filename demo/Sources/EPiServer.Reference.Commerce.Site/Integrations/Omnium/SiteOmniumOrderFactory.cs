using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;
using Geta.Omnium;
using Geta.Omnium.Culture;
using Geta.Omnium.Factories;
using Geta.Omnium.Taxes;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Markets;
using Omnium.Public.Orders.Models;

namespace EPiServer.Reference.Commerce.Site.Integrations.Omnium
{
    public class SiteOmniumOrderFactory : DefaultOmniumOrderFactory
    {
        public SiteOmniumOrderFactory(
            IShippingCalculator shippingCalculator, 
            IMarketService marketService, 
            CultureResolver cultureResolver, 
            ITaxUtility taxUtility, 
            ReferenceConverter referenceConverter, 
            IContentRepository contentRepository, 
            IPaymentManagerFacade paymentManagerFacade, 
            ILineItemCalculator lineItemCalculator,
            IOrderFormCalculator orderFormCalculator,
            IOrderGroupCalculator orderGroupCalculator,
            IShipmentManagerFacade shipmentManagerFacade,
            IPromotionEngine promotionEngine) 
            : base(shippingCalculator, marketService, cultureResolver, taxUtility, referenceConverter, contentRepository, paymentManagerFacade, lineItemCalculator, orderFormCalculator, orderGroupCalculator, shipmentManagerFacade, promotionEngine)
        {
        }

        public override OmniumOrder MapOrder(IPurchaseOrder purchaseOrder, IOrderForm orderForm, IShipment[] shipments)
        {
            var omniumOrder = base.MapOrder(purchaseOrder, orderForm, shipments);
            omniumOrder.StoreId = "123";
            omniumOrder.MarketId = "NOR";

            return omniumOrder;
        }
    }
}