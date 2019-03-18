# Omnium

## Description
Integration package for Omnium.

## Features
* Scheduled job for syncing orders to Omnium
* Scheduled job for synching orders back to Episerver

## How to get started?
* ``install-package Geta.Omnium``
* Implement ``IOmniumImportSettings``

Create class that implements the ``IOmniumImportSettings`` interface.
```
    public class OmniumImportSettings : IOmniumImportSettings
    {
        private readonly IContentRepository _contentRepository;
        private readonly ISiteDefinitionRepository _siteDefinitionRepository;

        public OmniumImportSettings(
            IContentRepository contentRepository,
            ISiteDefinitionRepository siteDefinitionRepository)
        {
            _contentRepository = contentRepository;
            _siteDefinitionRepository = siteDefinitionRepository;
        }

        public void LogSyncFromOmniumDate(DateTime dateTime)
        {
            throw new NotImplementedException();
        }

        public DateTime? GetLastSyncFromOmniumDate()
        {
            throw new NotImplementedException();
        }
    }
```

Register the implementation.

``` services.AddTransient<IOmniumImportSettings, OmniumImportSettings>(); ```

Usually the last synced data would be stored on a settings page. By doing this the date can be modified in the CMS.

## Override default factories
For synching orders to Omnium the ``IPurchaseOrder`` object is mapped to a ``OmniunOrder``. To override the default functionality create a class and override from the ``DefaultOmniumOrderFactory`` class.

```
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
```

And register the implementation.

``` services.AddTransient<IOmniumOrderFactory, SiteOmniumOrderFactory>(); ```

Same can be done for the ``DefaultEpiOrderFactory`` for customizing the default functionality of mapping an OmniumOrder to a IPurchaseOrder.
	
## Package maintainer
https://github.com/patkleef

## Changelog
[Changelog](CHANGELOG.md)
