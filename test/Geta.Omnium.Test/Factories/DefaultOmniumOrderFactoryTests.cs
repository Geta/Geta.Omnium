using System.Linq;
using EPiServer.Commerce.Order;
using Geta.Omnium.Test.TestSupport.Fakes;
using Mediachase.Commerce;
using Omnium.Public.Orders.Models;
using Xunit;

namespace Geta.Omnium.Test.Factories
{
    public class DefaultOmniumOrderFactoryTests : BaseOrderFactoryTests
    {
        [Fact]
        public void Can_Create_Omnium_Order()
        {
            var purchaseOrder = FakePurchaseOrder.CreatePurchaseOrder();

            var omniumOrder = DefaultOmniumOrderFactorySubject.MapOrder(purchaseOrder);

            var orderStatus = DefaultOmniumOrderFactorySubject.GetOrderStatus(purchaseOrder.OrderStatus);

            var result =
                purchaseOrder.Currency == omniumOrder.BillingCurrency &&
                purchaseOrder.OrderNumber.Equals(omniumOrder.OrderNumber) &&
                purchaseOrder.CustomerId.ToString().Equals(omniumOrder.CustomerId) &&
                purchaseOrder.MarketId.Value.Equals(omniumOrder.MarketId) &&
                orderStatus.Equals(omniumOrder.Status);

            Assert.True(result);
        }

        [Fact]
        public void Can_Create_Omnium_OrderForm()
        {
            var purchaseOrder = FakePurchaseOrder.CreatePurchaseOrder();
            var orderForm = purchaseOrder.Forms.FirstOrDefault();

            var omniumOrderForm = DefaultOmniumOrderFactorySubject.MapOrderForm(purchaseOrder, orderForm);

            var result =
                orderForm.Shipments.Count == omniumOrderForm.Shipments.Count &&
                orderForm.Payments.Count == omniumOrderForm.Payments.Count;

            Assert.True(result);
        }

        [Fact]
        public void Can_Create_Omnium_Payment()
        {
            var payment = FakePayment.CreatePayment();

            var omniumPayment = DefaultOmniumOrderFactorySubject.MapPayment(payment, new Money(payment.Amount, Currency));

            var result =
                payment.Status.Equals(omniumPayment.Status) &&
                payment.Amount.Equals(omniumPayment.Amount) &&
                payment.PaymentMethodId == omniumPayment.PaymentMethodId &&
                payment.TransactionType.Equals(omniumPayment.TransactionType) &&
                payment.ImplementationClass.Equals(omniumPayment.ImplementationClass) &&
                payment.AuthorizationCode.Equals(omniumPayment.AuthorizationCode) &&
                payment.TransactionID.Equals(omniumPayment.TransactionID) &&
                payment.ValidationCode.Equals(omniumPayment.ValidationCode);

            Assert.True(result);
        }

        [Fact]
        public void Can_Create_Omnium_Shipment()
        {
            var shipment = FakeShipment.CreateShipment();

            var omniumShipment = DefaultOmniumOrderFactorySubject.MapShipment(shipment, Market, Currency);

            var result =
                shipment.ShipmentId.ToString().Equals(omniumShipment.ShipmentId) &&
                shipment.ShippingMethodId.ToString().Equals(omniumShipment.ShippingMethodId) && 
                shipment.OrderShipmentStatus.ToString().Equals(omniumShipment.Status) &&
                shipment.ShipmentTrackingNumber.Equals(omniumShipment.ShipmentTrackingNumber) &&
                shipment.LineItems.Count == omniumShipment.LineItems.Count &&
                CompareOrderAddress(shipment.ShippingAddress, omniumShipment.Address);

            Assert.True(result);
        }

        [Fact]
        public void Can_Create_Omnium_OrderAddress()
        {
            var orderAddress = FakeOrderAddress.CreateOrderAddress();

            var omniumOrderAddress = DefaultOmniumOrderFactorySubject.MapOrderAddress(orderAddress);

            var result = CompareOrderAddress(orderAddress, omniumOrderAddress);

            Assert.True(result);
        }

        [Fact]
        public void Can_Create_Omnium_Order_Line()
        {
            var lineItem = FakeLineItem.CreateLineItem();
            var orderAddress = FakeOrderAddress.CreateOrderAddress();

            var omniumOrderLine = DefaultOmniumOrderFactorySubject.MapOrderLine(lineItem, Market, Currency, orderAddress);

            var result =
                lineItem.LineItemId.ToString().Equals(omniumOrderLine.LineItemId) &&
                lineItem.Code.Equals(omniumOrderLine.Code) &&
                lineItem.DisplayName.Equals(omniumOrderLine.DisplayName) &&
                lineItem.Quantity == omniumOrderLine.Quantity;

            Assert.True(result);
        }

        private bool CompareOrderAddress(IOrderAddress orderAddress, OmniumOrderAddress omniumOrderAddress)
        {
            var twoLetterCountryCode = CultureResolver.GetTwoLetterCountryCode(orderAddress.CountryCode);

            return
                $"{orderAddress.FirstName} {orderAddress.LastName}".Equals(omniumOrderAddress.Name) &&
                orderAddress.FirstName.Equals(omniumOrderAddress.FirstName) &&
                orderAddress.LastName.Equals(omniumOrderAddress.LastName) &&
                orderAddress.Line1.Equals(omniumOrderAddress.Line1) &&
                orderAddress.Line2.Equals(omniumOrderAddress.Line2) &&
                orderAddress.PostalCode.Equals(omniumOrderAddress.PostalCode) &&
                orderAddress.City.Equals(omniumOrderAddress.City) &&
                orderAddress.RegionCode.Equals(omniumOrderAddress.RegionCode) &&
                orderAddress.RegionName.Equals(omniumOrderAddress.RegionName) &&
                twoLetterCountryCode.Equals(omniumOrderAddress.CountryCode) &&
                orderAddress.CountryName.Equals(omniumOrderAddress.CountryName) &&
                orderAddress.DaytimePhoneNumber.Equals(omniumOrderAddress.DaytimePhoneNumber) &&
                orderAddress.EveningPhoneNumber.Equals(omniumOrderAddress.EveningPhoneNumber) &&
                orderAddress.Email.Equals(omniumOrderAddress.Email) &&
                orderAddress.Organization.Equals(omniumOrderAddress.Organization);
        }
    }
}
