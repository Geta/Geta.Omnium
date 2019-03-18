﻿using System.Collections;
using EPiServer.Commerce.Order;
using Mediachase.Commerce.Inventory;

namespace Geta.Omnium.Test.TestSupport.Fakes
{
    public class FakeLineItem : ILineItem, ILineItemDiscountAmount
    {
        private static int _counter;
        
        public FakeLineItem()
        {
            LineItemId = ++_counter;
            Properties = new Hashtable();
        }

        public int LineItemId { get; set; }

        public string Code { get; set; }

        public decimal Quantity { get; set; }
        public decimal ReturnQuantity { get; set; }
        public InventoryTrackingStatus InventoryTrackingStatus { get; set; }
        public bool IsInventoryAllocated { get; set; }

        public decimal LineItemDiscountAmount { get; set; }

        public decimal PlacedPrice { get; set; }

        public decimal OrderLevelDiscountAmount { get; set; }

        public string DisplayName { get; set; }

        public bool IsGift { get; set; }

        public int? TaxCategoryId { get; set; }

        public Hashtable Properties { get; private set; }

        decimal ILineItemDiscountAmount.EntryAmount
        {
            get
            {
                return LineItemDiscountAmount;
            }
            set
            {
                LineItemDiscountAmount = value;
            }
        }

        /// <summary>
        /// Gets or sets the order level discount amount.
        /// </summary>
        /// <value>The order level discount amount.</value>
        decimal ILineItemDiscountAmount.OrderAmount
        {
            get
            {
                return OrderLevelDiscountAmount;
            }
            set
            {
                OrderLevelDiscountAmount = value;
            }
        }

        public static FakeLineItem CreateLineItem()
        {
            var lineItem = new FakeLineItem
            {
                LineItemId = ++_counter,
                Code = "SKU-38193098",
                DisplayName = "L/S ComfortBlend Tee	",
                Quantity = 1,
                PlacedPrice = new decimal(5.5),
                InventoryTrackingStatus = InventoryTrackingStatus.Disabled,
                IsInventoryAllocated = false,
            };
            return lineItem;
        }

        public static FakeLineItem CreateEmptyLineItem()
        {
            var lineItem = new FakeLineItem
            {
                LineItemId = ++_counter,
            };
            return lineItem;
        }
    }
}
