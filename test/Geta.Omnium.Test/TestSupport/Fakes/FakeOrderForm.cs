using System.Collections;
using System.Collections.Generic;
using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;

namespace Geta.Omnium.Test.TestSupport.Fakes
{
    public class FakeOrderForm : IOrderForm
    {
        public FakeOrderForm()
        {
            Payments = new List<IPayment>();
            Shipments = new List<IShipment>();
            Promotions = new List<PromotionInformation>();
            Properties = new Hashtable();
        }
        public Hashtable Properties { get; }
        public int OrderFormId { get; }
        public decimal AuthorizedPaymentTotal { get; set; }
        public decimal CapturedPaymentTotal { get; set; }
        public decimal HandlingTotal { get; set; }
        public string Name { get; set; }
        public ICollection<IShipment> Shipments { get; }
        public IList<PromotionInformation> Promotions { get; }
        public ICollection<string> CouponCodes { get; }
        public ICollection<IPayment> Payments { get; }
        public bool PricesIncludeTax { get; }

        public IOrderGroup ParentOrderGroup { get; private set; }

        public static FakeOrderForm CreateOrderForm()
        {
            var orderForm = new FakeOrderForm();

            orderForm.Shipments.Add(FakeShipment.CreateShipment());
            orderForm.Payments.Add(FakePayment.CreatePayment());

            return orderForm;
        }

        public static FakeOrderForm CreateEmptyOrderForm()
        {
            return new FakeOrderForm();
        }
    }
}
