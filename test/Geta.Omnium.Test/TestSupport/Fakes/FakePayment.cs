using System;
using System.Collections;
using EPiServer.Commerce.Order;
using Mediachase.Commerce.Orders;

namespace Geta.Omnium.Test.TestSupport.Fakes
{
    public class FakePayment : IPayment
    {
        private static int _counter;

        public FakePayment()
        {
            Properties = new Hashtable();
        }

        public decimal Amount { get; set; }

        public string AuthorizationCode { get; set; }

        public IOrderAddress BillingAddress { get; set; }

        public string CustomerName { get; set; }

        public string ImplementationClass { get; set; }

        public int PaymentId { get; set; }

        public Guid PaymentMethodId { get; set; }

        public string PaymentMethodName { get; set; }

        public PaymentType PaymentType { get; set; }

        public Hashtable Properties { get; private set; }

        public string ProviderTransactionID { get; set; }

        public string Status { get; set; }

        public string TransactionID { get; set; }

        public string TransactionType { get; set; }

        public string ValidationCode { get; set; }

        public static FakePayment CreatePayment()
        {
            var payment = new FakePayment
            {
                PaymentId = ++_counter,
                PaymentMethodId = Guid.Parse("ef6a5e4a-de87-4749-bd26-38c41badee6a"),
                PaymentMethodName = "GenericCreditCard",
                TransactionType = "Authorization",
                Amount = 50,
                CustomerName = "Cruz Graham",
                ImplementationClass = "Mediachase.Commerce.Orders.CreditCardPayment,Mediachase.Commerce",
                Status = "Processed",
                BillingAddress = FakeOrderAddress.CreateOrderAddress(),
                AuthorizationCode = "12345",
                TransactionID = "54321",
                ValidationCode = "09876543"
            };
            return payment;
        }
        public static FakePayment CreateEmptyPayment()
        {
            var payment = new FakePayment
            {
                PaymentId = ++_counter,
            };
            return payment;
        }
    }
}
