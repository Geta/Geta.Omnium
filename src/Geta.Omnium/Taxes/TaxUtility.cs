using System;
using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Internal;
using EPiServer.Framework.Cache;
using EPiServer.ServiceLocation;
using Geta.Omnium.Facades;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;

namespace Geta.Omnium.Taxes
{
    [ServiceConfiguration(typeof(ITaxUtility))]
    public class TaxUtility : ITaxUtility
    {
        private const string DefaultTaxCategoryName = "Default";
        private readonly IObjectInstanceCache _objectInstanceCache;
        private readonly ICatalogTaxManagerFacade _catalogTaxManagerFacade;

        public TaxUtility(IObjectInstanceCache objectInstanceCache, ICatalogTaxManagerFacade catalogTaxManagerFacade)
        {
            _objectInstanceCache = objectInstanceCache;
            _catalogTaxManagerFacade = catalogTaxManagerFacade;
        }

        public virtual PriceTax GetShippingPriceTax(ILineItem lineItem, IMarket market, Currency currency, IOrderAddress address, decimal price)
        {
            var taxCalculator = ServiceLocator.Current.GetInstance<ITaxCalculator>();

            var moneyPrice = new Money(price, currency);
            var moneyPriceTaxAmount = taxCalculator.GetShippingTax(lineItem, market, address, moneyPrice);

            return new PriceTax
            {
                PriceExclTax = market.PricesIncludeTax
                    ? moneyPrice - moneyPriceTaxAmount
                    : moneyPrice,
                PriceInclTax = !market.PricesIncludeTax ? moneyPrice + moneyPriceTaxAmount : moneyPrice
            };
        }

        public virtual PriceTax GetPriceTax(ILineItem lineItem, IMarket market, Currency currency, IOrderAddress address, decimal price)
        {
            var taxCalculator = ServiceLocator.Current.GetInstance<ITaxCalculator>();

            var moneyPrice = new Money(price, currency);
            var moneyPriceTaxAmount = taxCalculator.GetSalesTax(lineItem, market, address, moneyPrice);

            return new PriceTax
            {
                PriceExclTax = market.PricesIncludeTax
                    ? moneyPrice - moneyPriceTaxAmount
                    : moneyPrice,
                PriceInclTax = !market.PricesIncludeTax ? moneyPrice + moneyPriceTaxAmount : moneyPrice
            };
        }

        public virtual Money GetPriceWithTax(Money price, double taxPercentage)
        {
            return price + GetTax(price, taxPercentage);
        }

        public virtual Money GetPriceWithTax(Money price, MarketId marketId, IOrderAddress orderAddress, TaxType taxType, int? taxCategoryId)
        {
            return price + GetTax(price, marketId, orderAddress, taxType, taxCategoryId);
        }

        public Money GetPriceWithoutTax(Money price, double taxPercentage)
        {
            return new Money(GetPriceWithoutTax(price.Amount, taxPercentage), price.Currency);
        }

        public decimal GetPriceWithoutTax(decimal price, double taxPercentage)
        {
            return price / ((decimal)taxPercentage / 100.0m + 1.0m);
        }

        public virtual Money GetTax(Money price, MarketId marketId, IOrderAddress orderAddress, TaxType taxType, int? taxCategoryId)
        {
            var zero = new Money(0m, price.Currency);

            if (!taxCategoryId.HasValue)
                return zero;

            var taxValue = GetTaxValue(marketId, orderAddress, taxType, taxCategoryId.Value);
            if (taxValue <= 0.0)
                return zero;

            return new Money(GetTax(price.Amount, taxValue), price.Currency);
        }

        public virtual Money GetTax(Money price, double taxPercentage)
        {
            return new Money(GetTax(price.Amount, taxPercentage), price.Currency);
        }

        public virtual decimal GetTax(decimal price, double taxPercentage)
        {
            return price * ((decimal)taxPercentage / 100.0m);
        }

        public virtual double GetTaxValue(MarketId marketId, IOrderAddress address, TaxType taxType, int? taxCategoryId)
        {
            // Only return 0 when address is null or when tax category is null and we want to get the tax rate for line items. For Shipping tax, we want to use the default tax category
            if (address == null || (taxType == TaxType.SalesTax && !taxCategoryId.HasValue))
                return 0.0;
            return GetTaxValue(marketId, address.CountryCode, address.RegionCode, address.PostalCode, address.City, taxType, taxCategoryId);
        }

        public virtual double GetTaxValue(MarketId marketId, string countryCode, string regionCode, string postalCode, string city, TaxType taxType, int? taxCategoryId)
        {

            var regionIdentifier = GetCacheRegionIdentifier(postalCode, city, regionCode, countryCode);
            var key = GetCacheKey<TaxValue>(marketId, regionIdentifier, taxType, taxCategoryId);

            if (_objectInstanceCache.Get(key) != null)
                return (double)_objectInstanceCache.Get(key);

            var percentage = 0.0;
            if (taxCategoryId.HasValue)
            {
                var taxCategoryName = _catalogTaxManagerFacade.GetTaxCategoryNameById(taxCategoryId.Value);
                // TODO: refactor tax retrieval
#pragma warning disable 618
                var taxValues = OrderContext.Current.GetTaxes(Guid.Empty, taxCategoryName, marketId.Value, countryCode, regionCode, postalCode, null, null, city);
#pragma warning restore 618
                var taxValue = taxValues.FirstOrDefault(x => x.TaxType == taxType);
                percentage = taxValue?.Percentage ?? 0.0;
            }

            _objectInstanceCache.Insert(key, percentage, new CacheEvictionPolicy(new TimeSpan(1, 0, 0), CacheTimeoutType.Sliding));

            return percentage;
        }

        protected virtual string GetCacheKey<T>(MarketId marketId, string regionIdentifier, TaxType taxType, int? taxCategoryId)
        {
            return $"{typeof(T).Name}-{marketId}-{taxType}-{taxCategoryId}-{regionIdentifier}";
        }

        protected virtual string GetCacheRegionIdentifier(string postalCode, string city, string regionCode, string countryCode)
        {
            if (string.IsNullOrWhiteSpace(postalCode))
            {
                return $"ci-rc-{countryCode}-{regionCode}-{city}";
            }

            return $"ci-pc-{countryCode}-{postalCode}";
        }

        public virtual int GetDefaultTaxCategoryId()
        {
            var taxDto = CatalogTaxManager.GetTaxCategoryByName(DefaultTaxCategoryName);
            var taxCategoryRow = taxDto?.TaxCategory.FirstOrDefault();

            return taxCategoryRow?.TaxCategoryId ?? 1;
        }

        public virtual Money GetSalesTax(ILineItem lineItem, IMarket market, Currency currency, IOrderAddress address)
        {
            return lineItem.GetSalesTax(market, currency, address);
        }
    }
}
