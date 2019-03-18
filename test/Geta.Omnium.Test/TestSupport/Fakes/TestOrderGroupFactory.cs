using System;
using EPiServer.Commerce.Order;
using Moq;

namespace Geta.Omnium.Test.TestSupport.Fakes
{
    public class TestOrderGroupFactory : IOrderGroupFactory
    {
        private readonly IOrderGroupFactory _orderGroupFactory;

        public TestOrderGroupFactory(IOrderGroupFactory orderGroupFactory)
        {
            _orderGroupFactory = orderGroupFactory;
        }

        public IOrderGroupBuilder BuilderFor(IOrderGroup orderGroup)
        {
            return _orderGroupFactory.BuilderFor(orderGroup);
        }

        public IOrderForm CreateOrderForm(IOrderGroup orderGroup)
        {
            return FakeOrderForm.CreateEmptyOrderForm();
        }

        public IShipment CreateShipment(IOrderGroup orderGroup)
        {
            return FakeShipment.CreateEmptyShipment();
        }

        public ILineItem CreateLineItem(string code, IOrderGroup orderGroup)
        {
            return FakeLineItem.CreateEmptyLineItem();
        }

        public IOrderAddress CreateOrderAddress(IOrderGroup orderGroup)
        {
            return FakeOrderAddress.CreateEmptyOrderAddress();
        }

        public IOrderNote CreateOrderNote(IOrderGroup orderGroup)
        {
            return _orderGroupFactory.CreateOrderNote(orderGroup);
        }

        public IPayment CreatePayment(IOrderGroup orderGroup)
        {
            return FakePayment.CreateEmptyPayment();
        }

        public IPayment CreatePayment(IOrderGroup orderGroup, Type paymentType)
        {
            return FakePayment.CreateEmptyPayment();
        }

        public ICreditCardPayment CreateCardPayment(IOrderGroup orderGroup)
        {
            return _orderGroupFactory.CreateCardPayment(orderGroup);
        }

        public ITaxValue CreateTaxValue(IOrderGroup orderGroup)
        {
            return _orderGroupFactory.CreateTaxValue(orderGroup);
        }
    }
}
