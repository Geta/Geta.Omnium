using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Geta.Omnium.Taxes;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;

namespace Geta.Omnium.Extensions
{
    public static class RewardDescriptionExtensions
    {
        public static Money GetSavedAmountIncludingTax(this RewardDescription description, ICart cart, TaxUtility taxUtility = null)
        {
            var initialAmount = new Money(description.SavedAmount, cart.Currency);

            if (cart.PricesIncludeTax)
                return initialAmount;

            var shipment = cart.GetFirstShipment();
            var shippingAddress = shipment?.ShippingAddress;

            return GetSavedAmountIncludingTax(description, cart.MarketId, cart.Currency, shippingAddress, taxUtility);
        }

        public static Money GetSavedAmountIncludingTax(this RewardDescription description, MarketId marketId, Currency currency, IOrderAddress address, TaxUtility taxUtility = null)
        {
            var initialAmount = new Money(description.SavedAmount, currency);

            if (taxUtility == null)
                taxUtility = ServiceLocator.Current.GetInstance<TaxUtility>();

            var taxCategoryId = taxUtility.GetDefaultTaxCategoryId();
            return taxUtility.GetPriceWithTax(initialAmount, marketId, address, GetTaxType(description), taxCategoryId);
        }

        public static TaxType GetTaxType(this RewardDescription description)
        {
            switch (description.Promotion.DiscountType)
            {
                case DiscountType.Shipping:
                    return TaxType.ShippingTax;
                default:
                    return TaxType.SalesTax;
            }
        }

        public static bool IsOfType(this RewardDescription description, DiscountType discountType)
        {
            return description.Promotion?.DiscountType == discountType;
        }
    }
}
