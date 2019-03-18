using System;
using EPiServer.Commerce.Order;

namespace Geta.Omnium.Test.TestSupport.Fakes
{
    public class FakePaymentMethod : IPaymentMethod
    {
        public FakePaymentMethod(string name)
        {
            PaymentMethodId = Guid.NewGuid();
            SystemKeyword = Name = name;
        }

        public Guid PaymentMethodId { get; }

        public string SystemKeyword { get; }

        public string Name { get; }

        public string Description => String.Empty;

        public virtual IPayment CreatePayment(decimal amount, IOrderGroup orderGroup)
        {
            throw new NotImplementedException();
        }

        public bool ValidateData()
        {
            return true;
        }
    }
}