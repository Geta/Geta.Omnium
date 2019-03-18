using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Geta.Omnium.Culture;
using Geta.Omnium.Models;
using Mediachase.Commerce;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders;
using Omnium.Public.Models;
using Omnium.Public.Orders.Models;
using Omnium.Public.Payments.Models;
using Omnium.Public.Shipments.Models;

namespace Geta.Omnium.Factories
{
    [ServiceConfiguration(typeof(IEpiOrderFactory))]
    public class DefaultEpiOrderFactory : IEpiOrderFactory
    {
        private readonly IOrderGroupFactory _orderGroupFactory;
        private readonly CultureResolver _cultureResolver;
        private readonly CustomerContext _customerContext;

        public DefaultEpiOrderFactory(IOrderGroupFactory orderGroupFactory, CultureResolver cultureResolver, CustomerContext customerContext)
        {
            _orderGroupFactory = orderGroupFactory;
            _cultureResolver = cultureResolver;
            _customerContext = customerContext;
        }

        /// <summary>
        /// Check if new line items, shipment or payments have been created.
        /// If so, need to sync back to Omnium in order to work with the same id's
        /// </summary>
        /// <returns></returns>
        public virtual bool ShouldSyncOrderBackToOmnium(IPurchaseOrder purchaseOrder, OmniumOrder omniumOrder)
        {
            var orderForm = purchaseOrder.GetFirstForm();
            var omniumOrderForm = omniumOrder.OrderForm;

            // TODO why is ShipmentId and LineItemId a string and a PaymentId an integer?
            if (!orderForm.Shipments.All(s => omniumOrderForm.Shipments.Exists(x => x.ShipmentId.Equals(s.ShipmentId.ToString()))))
            {
                return true;
            }
            if (!orderForm.Payments.All(p => omniumOrderForm.Payments.Exists(x => x.PaymentId.Equals(p.PaymentId))))
            {
                return true;
            }
            if (!orderForm.GetAllLineItems().All(l => omniumOrderForm.LineItems.Exists(x => x.LineItemId.Equals(l.LineItemId.ToString()))))
            {
                return true;
            }
            return false;
        }

        public virtual IPurchaseOrder MapOrder(IPurchaseOrder purchaseOrder, OmniumOrder omniumOrder)
        {
            MapOrderProperties(purchaseOrder, omniumOrder);

            MapOrderForm(purchaseOrder, purchaseOrder.GetFirstForm(), omniumOrder.OrderForm);

            MapBillingAddress(purchaseOrder, omniumOrder.BillingAddress);

            if (purchaseOrder.OrderLink == null || purchaseOrder.OrderLink.OrderGroupId <= 0) // new order
            {
                purchaseOrder.Properties[OrderConstants.MetaFieldOmniumSynchronized] = true;
                purchaseOrder.Properties[OrderConstants.MetaFieldOmniumSynchronizedDate] = DateTime.UtcNow;
            }
            return purchaseOrder;
        }

        public virtual void MapOrderProperties(IPurchaseOrder purchaseOrder, OmniumOrder omniumOrder)
        {
            if (Guid.TryParse(omniumOrder.CustomerId, out var customerGuid) && _customerContext.GetContactById(customerGuid) != null)
            {
                purchaseOrder.CustomerId = customerGuid;
            }

            purchaseOrder.Name = string.IsNullOrEmpty(purchaseOrder.Name) ? "Default" : purchaseOrder.Name;
            purchaseOrder.MarketId = new MarketId(omniumOrder.MarketId);
            purchaseOrder.OrderStatus = MapOrderStatus(omniumOrder.Status);
            purchaseOrder.Currency = omniumOrder.BillingCurrency;
            // TODO purchaseOrder.Notes

            MapProperties(purchaseOrder.Properties, omniumOrder.Properties);
        }

        public virtual void MapBillingAddress(IPurchaseOrder purchaseOrder, OmniumOrderAddress omniumOrderBillingAddress)
        {
            var paymentsWithBillingAddress = purchaseOrder.Forms.SelectMany(x => x.Payments);
            foreach (var payment in paymentsWithBillingAddress)
            {
                var address = MapAddress(purchaseOrder, payment.BillingAddress, omniumOrderBillingAddress);
                payment.BillingAddress = address;
            }
        }

        public virtual void MapOrderForm(IPurchaseOrder purchaseOrder, IOrderForm orderForm, OmniumOrderForm omniumOrderForm)
        {
            foreach (var omniumShipment in omniumOrderForm.Shipments)
            {
                var shipment = CreateOrGetShipment(purchaseOrder, orderForm, omniumShipment);
                MapShipment(purchaseOrder, shipment, omniumShipment);
            }

            foreach (var omniumPayment in omniumOrderForm.Payments)
            {
                var payment = CreateOrGetPayment(purchaseOrder, orderForm, omniumPayment);
                MapPayment(purchaseOrder, payment, omniumPayment);
            }
            MapProperties(orderForm.Properties, omniumOrderForm.Properties);
        }

        public virtual IOrderAddress MapAddress(IPurchaseOrder purchaseOrder, IOrderAddress address, OmniumOrderAddress omniumAddress)
        {
            if (omniumAddress == null)
            {
                return address;
            }
            if (address == null)
            {
                address = _orderGroupFactory.CreateOrderAddress(purchaseOrder);
                address.Id = omniumAddress.Name ?? "default";
            }
            address.FirstName = omniumAddress.FirstName;
            address.LastName = omniumAddress.LastName;
            address.DaytimePhoneNumber = omniumAddress.DaytimePhoneNumber;
            address.EveningPhoneNumber = omniumAddress.EveningPhoneNumber;
            address.Email = omniumAddress.Email;
            address.Line1 = omniumAddress.Line1;
            address.Line2 = omniumAddress.Line2;
            address.PostalCode = omniumAddress.PostalCode;
            address.City = omniumAddress.City;
            address.CountryCode = GetFormattedCountryCode(omniumAddress.CountryCode);
            address.CountryName = omniumAddress.CountryName;
            address.Organization = omniumAddress.Organization;
            address.RegionCode = omniumAddress.RegionCode;
            address.RegionName = omniumAddress.RegionName;

            return address;
        }

        public virtual IPayment MapPayment(IPurchaseOrder purchaseOrder, IPayment payment, OmniumPayment omniumPayment)
        {
            payment.PaymentMethodName = omniumPayment.PaymentMethodName;
            payment.CustomerName = omniumPayment.CustomerName;
            payment.Amount = omniumPayment.Amount;
            payment.AuthorizationCode = omniumPayment.AuthorizationCode;
            payment.PaymentMethodId = omniumPayment.PaymentMethodId;
            payment.PaymentType = MapPaymentType(payment.PaymentType, omniumPayment.PaymentType);
            payment.Status = omniumPayment.Status;
            payment.TransactionID = omniumPayment.TransactionID;
            payment.TransactionType = MapPaymentTransactionType(omniumPayment.TransactionType).ToString();
            payment.ValidationCode = omniumPayment.ValidationCode;

            MapProperties(payment.Properties, omniumPayment.Properties);
            return payment;
        }

        public virtual IShipment MapShipment(IPurchaseOrder purchaseOrder, IShipment shipment, OmniumShipment omniumShipment)
        {
            if (omniumShipment == null)
            {
                return shipment;
            }
            if (Guid.TryParse(omniumShipment.ShippingMethodId, out var shippingMethodId))
                shipment.ShippingMethodId = shippingMethodId;
            shipment.ShipmentTrackingNumber = omniumShipment.ShipmentTrackingNumber;
            shipment.ShippingAddress = MapAddress(purchaseOrder, shipment.ShippingAddress, omniumShipment.Address);
            shipment.WarehouseCode = omniumShipment.WarehouseCode;

            // Check if we need to remove line items
            foreach (var lineItem in shipment.LineItems.Where(l =>
                !omniumShipment.LineItems.Any(x => x.LineItemId.Equals(l.LineItemId.ToString()))).ToList())
            {
                shipment.LineItems.Remove(lineItem);
            }

            foreach (var omniumOrderLine in omniumShipment.LineItems)
            {
                var lineItem = CreateOrGetLineItem(purchaseOrder, shipment, omniumOrderLine);
                lineItem = MapLineitem(lineItem, omniumOrderLine);
            }

            MapProperties(shipment.Properties, omniumShipment.Properties);
            return shipment;
        }

        public virtual ILineItem MapLineitem(ILineItem lineItem, OmniumOrderLine omniumOrderLine)
        {
            if (omniumOrderLine == null)
            {
                return lineItem;
            }
            lineItem.DisplayName = omniumOrderLine.DisplayName;
            lineItem.PlacedPrice = omniumOrderLine.PlacedPrice;
            lineItem.Quantity = omniumOrderLine.Quantity;

            MapProperties(lineItem.Properties, omniumOrderLine.Properties);
            return lineItem;
        }

        public virtual void MapProperties(Hashtable properties, IEnumerable<OmniumPropertyItem> omniumPropertyItem)
        {
            if (omniumPropertyItem != null)
            {
                foreach (var item in omniumPropertyItem)
                {
                    var value = GetValue(item.Value);
                    if (properties.ContainsKey(item.Key))
                    {
                        properties[item.Key] = value;
                    }
                    else
                    {
                        properties.Add(item.Key, value);
                    }
                }
            }
        }

        public virtual PaymentType MapPaymentType(PaymentType paymentType, string omniumPaymentType)
        {
            return Enum.TryParse<PaymentType>(omniumPaymentType, out var newPaymentType) ? newPaymentType : paymentType;
        }

        public virtual TransactionType MapPaymentTransactionType(string omniumPaymentTransactionType)
        {
            return Enum.TryParse<TransactionType>(omniumPaymentTransactionType, out var newTransactionType) ? newTransactionType : TransactionType.Other;
        }
        public virtual OrderStatus MapOrderStatus(string omniumOrderStatus)
        {
            switch (omniumOrderStatus)
            {
                case DefaultOmniumOrderFactory.NewOrderStatus:
                case DefaultOmniumOrderFactory.InProgressOrderStatus:
                    return OrderStatus.InProgress;
                case DefaultOmniumOrderFactory.CancelledOrderStatus:
                    return OrderStatus.Cancelled;
                case DefaultOmniumOrderFactory.PartiallyShippedOrderStatus:
                    return OrderStatus.PartiallyShipped;
                case DefaultOmniumOrderFactory.CompletedOrderStatus:
                    return OrderStatus.Completed;
            }
            return OrderStatus.InProgress;
        }

        protected virtual string GetFormattedCountryCode(string countryCode)
        {
            if (string.IsNullOrEmpty(countryCode))
                return countryCode;

            try
            {
                return _cultureResolver.GetThreeLetterCountryCode(countryCode);
            }
            catch (ArgumentException)
            {
                return countryCode;
            }
        }

        private IPayment CreateOrGetPayment(IPurchaseOrder purchaseOrder, IOrderForm orderForm, OmniumPayment omniumPayment)
        {
            IPayment payment = null;
            if (omniumPayment.PaymentId != 0)
            {
                payment = orderForm.Payments.FirstOrDefault(x => x.PaymentId == omniumPayment.PaymentId || x.PaymentId <= 0);
            }
            if (payment == null)
            {
                payment = _orderGroupFactory.CreatePayment(purchaseOrder);
                orderForm.Payments.Add(payment);
            }
            return payment;
        }

        private IShipment CreateOrGetShipment(IPurchaseOrder purchaseOrder, IOrderForm orderForm, OmniumShipment omniumShipment)
        {
            IShipment shipment = null;
            if (int.TryParse(omniumShipment.ShipmentId, out int shipmentId))
            {
                shipment = orderForm.Shipments.FirstOrDefault(x => x.ShipmentId == shipmentId || x.ShipmentId <= 0);
            }
            if (shipment == null)
            {
                shipment = _orderGroupFactory.CreateShipment(purchaseOrder);
                orderForm.Shipments.Add(shipment);
            }
            return shipment;
        }

        private ILineItem CreateOrGetLineItem(IPurchaseOrder purchaseOrder, IShipment shipment, OmniumOrderLine omniumOrderLine)
        {
            ILineItem lineItem = null;
            if (int.TryParse(omniumOrderLine.LineItemId, out int lineItemId))
            {
                lineItem = shipment.LineItems.FirstOrDefault(x => x.LineItemId == lineItemId);
            }
            if (lineItem == null)
            {
                lineItem = _orderGroupFactory.CreateLineItem(omniumOrderLine.Code, purchaseOrder);
                shipment.LineItems.Add(lineItem);
            }
            return lineItem;
        }

        private object GetValue(string value)
        {
            if (bool.TryParse(value, out var booleanValue))
            {
                return booleanValue;
            }
            return value;
        }
    }
}
