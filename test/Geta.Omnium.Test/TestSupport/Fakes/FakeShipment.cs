using System;
using System.Collections;
using System.Collections.Generic;
using EPiServer.Commerce.Order;
using Mediachase.Commerce.Orders;

namespace Geta.Omnium.Test.TestSupport.Fakes
{
    public class FakeShipment : IShipment, IShipmentDiscountAmount
    {
        private static int _counter;

        public FakeShipment()
        {
            LineItems = new List<ILineItem>();
            Properties = new Hashtable();
        }

        public int ShipmentId { get; set; }

        public Guid ShippingMethodId { get; set; }

        public string ShippingMethodName { get; set; }

        public IOrderAddress ShippingAddress { get; set; }

        public string ShipmentTrackingNumber { get; set; }

        public OrderShipmentStatus OrderShipmentStatus { get; set; }
        
        public int? PickListId { get; set; }

        public string WarehouseCode { get; set; }

        public ICollection<ILineItem> LineItems { get; set; }

        public decimal ShipmentDiscount { get; set; }

        public Hashtable Properties { get; private set; }

        public IOrderGroup ParentOrderGroup { get; private set; }

        public static FakeShipment CreateShipment()
        {
            var shipment = new FakeShipment
            {
                ShipmentId = ++_counter,
                ShippingMethodId = Guid.Parse("0b4a4792-10d8-497f-8dc1-379be7f85531"),
                ShippingMethodName = "Express-USD",
                OrderShipmentStatus = OrderShipmentStatus.AwaitingInventory,
                ShipmentTrackingNumber = "12345",
                ShippingAddress = FakeOrderAddress.CreateOrderAddress(),
                LineItems = new[] { FakeLineItem.CreateLineItem() },
            };
            return shipment;
        }

        public static FakeShipment CreateEmptyShipment()
        {
            var shipment = new FakeShipment
            {
                ShipmentId = ++_counter,
            };
            return shipment;
        }

    }
}
