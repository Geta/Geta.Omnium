using EPiServer.Commerce.Order;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;

namespace Geta.Omnium.Taxes
{
    public interface ITaxUtility
    {
        PriceTax GetShippingPriceTax(ILineItem lineItem, IMarket market, Currency currency, IOrderAddress address,
            decimal price);
        PriceTax GetPriceTax(ILineItem lineItem, IMarket market, Currency currency, IOrderAddress address,
            decimal price);
        Money GetPriceWithTax(Money price, double taxPercentage);
        Money GetPriceWithTax(Money price, MarketId marketId, IOrderAddress orderAddress, TaxType taxType, int? taxCategoryId);
        Money GetPriceWithoutTax(Money price, double taxPercentage);
        decimal GetPriceWithoutTax(decimal price, double taxPercentage);
        Money GetTax(Money price, MarketId marketId, IOrderAddress orderAddress, TaxType taxType, int? taxCategoryId);
        Money GetTax(Money price, double taxPercentage);
        decimal GetTax(decimal price, double taxPercentage);
        double GetTaxValue(MarketId marketId, IOrderAddress address, TaxType taxType, int? taxCategoryId);
        double GetTaxValue(MarketId marketId, string countryCode, string regionCode, string postalCode, string city, TaxType taxType, int? taxCategoryId);
        int GetDefaultTaxCategoryId();
        Money GetSalesTax(ILineItem lineItem, IMarket market, Currency currency, IOrderAddress address);
    }
}