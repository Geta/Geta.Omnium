using System;
using System.Collections;
using System.Collections.Generic;
using EPiServer.Commerce.Order;
using Mediachase.Commerce;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;

namespace Geta.Omnium.Test.TestSupport.Fakes
{
    public class FakePurchaseOrder : IPurchaseOrder
    {
        public FakePurchaseOrder()
        {
            Forms = new List<IOrderForm>();
            Properties = new Hashtable();
        }

        public Hashtable Properties { get; }
        public OrderReference OrderLink { get; }
        public ICollection<IOrderForm> Forms { get; }
        public ICollection<IOrderNote> Notes { get; }
        public IMarket Market { get; set; }
        public string Name { get; set; }
        public Guid? Organization { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public Currency Currency { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime Created { get; }
        public DateTime? Modified { get; }
        public MarketId MarketId { get; set; }
        public string MarketName { get; set; }
        public bool PricesIncludeTax { get; set; }
        public string OrderNumber { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public ICollection<IReturnOrderForm> ReturnForms { get; }

        public static FakePurchaseOrder CreatePurchaseOrder()
        {
            var purchaseOrder = new FakePurchaseOrder
            {
                Currency = new Currency(Currency.USD),
                OrderNumber = "PO0813",
                CustomerId = Guid.Parse("8b7d0a30-0e10-46d8-badf-259e9fb1541a"),
                Market = new MarketImpl(MarketId.Default),
                Name = "Default",
                OrderStatus = OrderStatus.InProgress,
            };

            purchaseOrder.Forms.Add(FakeOrderForm.CreateOrderForm());

            return purchaseOrder;
        }

        public static FakePurchaseOrder CreateEmptyPurchaseOrder()
        {
            var purchaseOrder = new FakePurchaseOrder();

            purchaseOrder.Forms.Add(new FakeOrderForm());

            return purchaseOrder;
        }
    }
}
