using System;
using System.Collections;
using System.Linq;
using EPiServer.Commerce.Order;
using Geta.Omnium.Test.TestSupport.Fakes;
using Omnium.Public.Orders.Models;
using Xunit;

namespace Geta.Omnium.Test.Factories
{
    public class DefaultEpiOrderFactoryTests : BaseOrderFactoryTests
    {
        [Fact]
        public void Can_Create_Epi_Order()
        {
            var purchaseOrder = FakePurchaseOrder.CreateEmptyPurchaseOrder();
            var omniumOrder = OmniumFakesBuilder.CreateOmniumOrder();

            DefaultEpiOrderFactorySubject.MapOrder(purchaseOrder, omniumOrder);

            var orderStatus = DefaultEpiOrderFactorySubject.MapOrderStatus(omniumOrder.Status);

            var result =
                CompareCustomerId(omniumOrder, purchaseOrder) &&
                omniumOrder.BillingCurrency.Equals(purchaseOrder.Currency) &&
                omniumOrder.MarketId.Equals(purchaseOrder.MarketId.Value) &&
                orderStatus.Equals(purchaseOrder.OrderStatus);

            Assert.True(result);
        }

        [Fact]
        public void Can_Create_Epi_Order_With_Two_Shipments()
        {
            var purchaseOrder = FakePurchaseOrder.CreateEmptyPurchaseOrder();
            var omniumOrder = OmniumFakesBuilder.CreateOmniumOrder();

            var omniumShipment = OmniumFakesBuilder.CreateOmniumShipment();
            omniumShipment.LineItems.Add(OmniumFakesBuilder.CreateOmniumOrderLine());

            omniumOrder.OrderForm.Shipments.Add(omniumShipment);

            DefaultEpiOrderFactorySubject.MapOrder(purchaseOrder, omniumOrder);

            var result =
                purchaseOrder.GetFirstForm().Shipments.Count == omniumOrder.OrderForm.Shipments.Count &&
                purchaseOrder.GetAllLineItems().Count() == omniumOrder.OrderForm.Shipments.Sum(x => x.LineItems.Count);

            Assert.True(result);
        }

        [Fact]
        public void Can_Create_Epi_OrderForm()
        {
            var purchaseOrder = new FakePurchaseOrder();
            var orderForm = new FakeOrderForm();

            var omniumOrder = OmniumFakesBuilder.CreateOmniumOrder();

            DefaultEpiOrderFactorySubject.MapOrderForm(purchaseOrder, orderForm, omniumOrder.OrderForm);

            var result =
                omniumOrder.OrderForm.Shipments.Count == orderForm.Shipments.Count &&
                omniumOrder.OrderForm.Payments.Count == orderForm.Payments.Count;

            Assert.True(result);
        }

        [Fact]
        public void Can_Create_Epi_Shipment()
        {
            var purchaseOrder = new FakePurchaseOrder();
            var shipment = new FakeShipment();
            shipment.ShippingAddress = new FakeOrderAddress();
            var omniumOrder = OmniumFakesBuilder.CreateOmniumOrder();
            var omniumShipment = omniumOrder.OrderForm.Shipments.FirstOrDefault();

            DefaultEpiOrderFactorySubject.MapShipment(purchaseOrder, shipment, omniumShipment);

            var result =
                omniumShipment.ShippingMethodId.Equals(shipment.ShippingMethodId.ToString()) &&
                omniumShipment.ShipmentTrackingNumber.Equals(shipment.ShipmentTrackingNumber) && 
                CompareAddress(omniumShipment.Address, shipment.ShippingAddress) ;
                omniumShipment.WarehouseCode.Equals(shipment.WarehouseCode);

            Assert.True(result);
        }

        [Fact]
        public void Can_Create_Epi_Payment()
        {
            var purchaseOrder = new FakePurchaseOrder();
            var payment = new FakePayment();
            var omniumOrder = OmniumFakesBuilder.CreateOmniumOrder();
            var omniumPayment = omniumOrder.OrderForm.Payments.FirstOrDefault();

            DefaultEpiOrderFactorySubject.MapPayment(purchaseOrder, payment, omniumPayment);

            var paymentType = DefaultEpiOrderFactorySubject.MapPaymentType(payment.PaymentType, omniumPayment.PaymentType);
            var transationType = DefaultEpiOrderFactorySubject.MapPaymentTransactionType(omniumPayment.TransactionType);

            var result =
                omniumPayment.PaymentMethodName.Equals(payment.PaymentMethodName) &&
                omniumPayment.CustomerName.Equals(payment.CustomerName) &&
                omniumPayment.Amount.Equals(payment.Amount) &&
                omniumPayment.AuthorizationCode.Equals(payment.AuthorizationCode) &&
                payment.PaymentMethodId.Equals(omniumPayment.PaymentMethodId) &&
                payment.PaymentType == paymentType &&
                payment.Status.Equals(omniumPayment.Status) &&
                payment.TransactionID.Equals(omniumPayment.TransactionId) &&
                payment.TransactionType == transationType.ToString() &&
                payment.ValidationCode.Equals(omniumPayment.ValidationCode);

            Assert.True(result);
        }

        [Fact]
        public void Can_Create_Epi_OrderAddress()
        {
            var purchaseOrder = new FakePurchaseOrder();
            var orderAddress = new FakeOrderAddress();
            var omniumOrder = OmniumFakesBuilder.CreateOmniumOrder();
            var omniumOrderAddress = omniumOrder.OrderForm.Shipments.FirstOrDefault().Address;

            DefaultEpiOrderFactorySubject.MapAddress(purchaseOrder, orderAddress, omniumOrderAddress);

            var result = CompareAddress(omniumOrderAddress, orderAddress);

            Assert.True(result);
        }

        [Fact]
        public void Can_Create_Properties()
        {
            var properties = new Hashtable();
            var omniumProperties = OmniumFakesBuilder.CreateOmniumProperties();
            DefaultEpiOrderFactorySubject.MapProperties(properties, omniumProperties);

            Assert.True(omniumProperties.All(shouldItem => properties.ContainsKey(shouldItem.Key)));
            Assert.True(omniumProperties.All(shouldItem => properties.ContainsValue(shouldItem.Value)));
        }

        [Fact]
        public void Can_Add_New_LineItem()
        {
            var purchaseOrder = FakePurchaseOrder.CreateEmptyPurchaseOrder();
            var omniumOrder = OmniumFakesBuilder.CreateOmniumOrder();

            DefaultEpiOrderFactorySubject.MapOrder(purchaseOrder, omniumOrder);

            // Map it again so the ID's are updated
            omniumOrder = DefaultOmniumOrderFactorySubject.MapOrder(purchaseOrder);

            var shipment = omniumOrder.OrderForm.Shipments.FirstOrDefault();
            shipment.LineItems.Add(OmniumFakesBuilder.CreateOmniumOrderLine());

            DefaultEpiOrderFactorySubject.MapOrder(purchaseOrder, omniumOrder);

            var result = purchaseOrder.GetAllLineItems().Count() == shipment.LineItems.Count;

            Assert.True(result);
        }

        [Fact]
        public void Can_Change_LineItem_Quantity()
        {
            var purchaseOrder = FakePurchaseOrder.CreateEmptyPurchaseOrder();
            var omniumOrder = OmniumFakesBuilder.CreateOmniumOrder();

            DefaultEpiOrderFactorySubject.MapOrder(purchaseOrder, omniumOrder);

            // Map it again so the ID's are updated
            omniumOrder = DefaultOmniumOrderFactorySubject.MapOrder(purchaseOrder);

            var shipment = omniumOrder.OrderForm.Shipments.FirstOrDefault();
            shipment.LineItems.FirstOrDefault().Quantity += 1;

            DefaultEpiOrderFactorySubject.MapOrder(purchaseOrder, omniumOrder);

            var result = purchaseOrder.GetAllLineItems().Sum(x => x.Quantity) == shipment.LineItems.Sum(x => x.Quantity);

            Assert.True(result);
        }

        [Fact]
        public void Can_Add_New_Shipment_With_LineItem()
        {
            var purchaseOrder = FakePurchaseOrder.CreateEmptyPurchaseOrder();
            var omniumOrder = OmniumFakesBuilder.CreateOmniumOrder();

            DefaultEpiOrderFactorySubject.MapOrder(purchaseOrder, omniumOrder);

            // Map it again so the ID's are updated
            omniumOrder = DefaultOmniumOrderFactorySubject.MapOrder(purchaseOrder);

            var shipment = OmniumFakesBuilder.CreateOmniumShipment();
            shipment.LineItems.Add(OmniumFakesBuilder.CreateOmniumOrderLine());

            omniumOrder.OrderForm.Shipments.Add(shipment);

            DefaultEpiOrderFactorySubject.MapOrder(purchaseOrder, omniumOrder);

            var result =
                purchaseOrder.GetFirstForm().Shipments.Count == omniumOrder.OrderForm.Shipments.Count &&
                purchaseOrder.GetAllLineItems().Count() == omniumOrder.OrderForm.Shipments.Sum(x => x.LineItems.Count);

            Assert.True(result);
        }

        [Fact]
        public void Can_Split_Shipment()
        {
            var purchaseOrder = FakePurchaseOrder.CreateEmptyPurchaseOrder();
            var omniumOrder = OmniumFakesBuilder.CreateOmniumOrder();
            omniumOrder.OrderForm.Shipments.FirstOrDefault().LineItems.Add(OmniumFakesBuilder.CreateOmniumOrderLine());

            DefaultEpiOrderFactorySubject.MapOrder(purchaseOrder, omniumOrder);

            // Map it again so the ID's are updated
            omniumOrder = DefaultOmniumOrderFactorySubject.MapOrder(purchaseOrder);

            var omniumShipment = omniumOrder.OrderForm.Shipments.FirstOrDefault();
            var omniumOrderLine = omniumShipment.LineItems.LastOrDefault();
            omniumShipment.LineItems.Remove(omniumOrderLine);

            var newShipment = OmniumFakesBuilder.CreateOmniumShipment();
            newShipment.LineItems.Add(omniumOrderLine);
            omniumOrder.OrderForm.Shipments.Add(newShipment);

            DefaultEpiOrderFactorySubject.MapOrder(purchaseOrder, omniumOrder);
            omniumOrder = DefaultOmniumOrderFactorySubject.MapOrder(purchaseOrder);

            var result =
                purchaseOrder.GetFirstForm().Shipments.Count == omniumOrder.OrderForm.Shipments.Count &&
                purchaseOrder.GetFirstForm().Shipments.All(s =>
                    s.LineItems.Count == omniumOrder.OrderForm.Shipments
                        .FirstOrDefault(x => x.ShipmentId == s.ShipmentId.ToString()).LineItems.Count);

            Assert.True(result);
        }

        private bool CompareAddress(OmniumOrderAddress omniumOrderAddress, IOrderAddress orderAddress)
        {
            var threeLetterCountryCode = CultureResolver.GetThreeLetterCountryCode(omniumOrderAddress.CountryCode);

            return
                omniumOrderAddress.Name.Equals($"{orderAddress.FirstName} {orderAddress.LastName}") &&
                omniumOrderAddress.FirstName.Equals(orderAddress.FirstName) &&
                omniumOrderAddress.LastName.Equals(orderAddress.LastName) &&
                omniumOrderAddress.Line1.Equals(orderAddress.Line1) &&
                omniumOrderAddress.Line2.Equals(orderAddress.Line2) &&
                omniumOrderAddress.PostalCode.Equals(orderAddress.PostalCode) &&
                omniumOrderAddress.City.Equals(orderAddress.City) &&
                omniumOrderAddress.RegionCode.Equals(orderAddress.RegionCode) &&
                omniumOrderAddress.RegionName.Equals(orderAddress.RegionName) &&
                threeLetterCountryCode.Equals(orderAddress.CountryCode) &&
                omniumOrderAddress.CountryName.Equals(orderAddress.CountryName) &&
                omniumOrderAddress.DaytimePhoneNumber.Equals(orderAddress.DaytimePhoneNumber) &&
                omniumOrderAddress.EveningPhoneNumber.Equals(orderAddress.EveningPhoneNumber) &&
                omniumOrderAddress.Email.Equals(orderAddress.Email) &&
                omniumOrderAddress.Organization.Equals(orderAddress.Organization);
        }

        private bool CompareCustomerId(OmniumOrder omniumOrder, IPurchaseOrder purchaseOrder)
        {
            if (Guid.TryParse(omniumOrder.CustomerId, out var omniumCustomerId) &&
                purchaseOrder.CustomerId != Guid.Empty)
            {
                return omniumCustomerId == purchaseOrder.CustomerId;
            }
            return true;
        }
    }
}
