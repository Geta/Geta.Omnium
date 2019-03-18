using Mediachase.Commerce;

namespace Geta.Omnium.Models
{
    public class OmniumOrderTotals
    {
        public OmniumOrderTotals(Currency currency)
        {
            var zero = new Money(0m, currency);

            Handling = zero;
            Shipping = zero;
            ShippingExclTax = zero;
            SubTotal = zero;
            SubTotalExclTax = zero;
            TaxTotal = zero;
            Total = zero;
            ShippingDiscounts = zero;
            OrderDiscounts = zero;
        }

        public Money Handling { get; set; }
        public Money Shipping { get; set; }
        public Money ShippingExclTax { get; set; }
        public Money SubTotal { get; set; }
        public Money SubTotalExclTax { get; set; }
        public Money TaxTotal { get; set; }
        public Money Total { get; set; }
        public Money TotalExclTax { get; set; }
        public Money ShippingDiscounts { get; set; }
        public Money OrderDiscounts { get; set; }
    }
}
