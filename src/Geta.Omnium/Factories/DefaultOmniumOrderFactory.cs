using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Internal;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using Geta.Omnium.Culture;
using Geta.Omnium.Extensions;
using Geta.Omnium.Models;
using Geta.Omnium.Taxes;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;
using Omnium.EPiServer.Commerce.Mappings;
using Omnium.Public.Discounts.Models;
using Omnium.Public.Orders.Models;
using Omnium.Public.Payments.Models;
using Omnium.Public.Shipments.Models;

namespace Geta.Omnium.Factories
{
    [ServiceConfiguration(typeof(IOmniumOrderFactory))]
    public class DefaultOmniumOrderFactory : IOmniumOrderFactory
    {
        public const string NewOrderStatus = "New";
        public const string InProgressOrderStatus = "InProgress";
        public const string CancelledOrderStatus = "Cancelled";
        public const string PartiallyShippedOrderStatus = "PartiallyShipped";
        public const string CompletedOrderStatus = "Completed";

        private readonly IShippingCalculator _shippingCalculator;
        private readonly IMarketService _marketService;
        private readonly CultureResolver _cultureResolver;
        private readonly ITaxUtility _taxUtility;
        private readonly ReferenceConverter _referenceConverter;
        private readonly IContentRepository _contentRepository;
        private readonly IPaymentManagerFacade _paymentManagerFacade;
        private readonly ILineItemCalculator _lineItemCalculator;
        private readonly IOrderFormCalculator _orderFormCalculator;
        private readonly IOrderGroupCalculator _orderGroupCalculator;
        private readonly IShipmentManagerFacade _shipmentManagerFacade;
        private readonly IPromotionEngine _promotionEngine;

        public DefaultOmniumOrderFactory(
            IShippingCalculator shippingCalculator,
            IMarketService marketService,
            CultureResolver cultureResolver,
            ITaxUtility taxUtility,
            ReferenceConverter referenceConverter,
            IContentRepository contentRepository,
            IPaymentManagerFacade paymentManagerFacade,
            ILineItemCalculator lineItemCalculator,
            IOrderFormCalculator orderFormCalculator,
            IOrderGroupCalculator orderGroupCalculator,
            IShipmentManagerFacade shipmentManagerFacade,
            IPromotionEngine promotionEngine)
        {
            _shippingCalculator = shippingCalculator;
            _marketService = marketService;
            _cultureResolver = cultureResolver;
            _taxUtility = taxUtility;
            _referenceConverter = referenceConverter;
            _contentRepository = contentRepository;
            _paymentManagerFacade = paymentManagerFacade;
            _lineItemCalculator = lineItemCalculator;
            _orderFormCalculator = orderFormCalculator;
            _orderGroupCalculator = orderGroupCalculator;
            _shipmentManagerFacade = shipmentManagerFacade;
            _promotionEngine = promotionEngine;
        }

        public virtual OmniumOrder MapOrder(IPurchaseOrder purchaseOrder)
        {
            // use default Omnium order mapper  
            //var mappedOrder = OrderMappings.ConvertToOmniumOrder((OrderGroup)purchaseOrder);

            var orderForm = purchaseOrder.GetFirstForm();
            return MapOrder(purchaseOrder, orderForm, orderForm.Shipments.ToArray());
        }

        public virtual OmniumOrder MapOrder(
            IPurchaseOrder purchaseOrder, IOrderForm orderForm, IShipment[] shipments)
        {
            var market = _marketService.GetMarket(purchaseOrder.MarketId);
            var currency = purchaseOrder.Currency;
            var billingAddress = GetBillingAddress(purchaseOrder);
            var customerName = GetCustomerName(purchaseOrder, billingAddress);
            var orderType = purchaseOrder.ToOmniumOrderType().ToString();

            var omniumOrderForm = MapOrderForm(purchaseOrder, orderForm, shipments);

            var firstShipment = shipments.FirstOrDefault();
            return new OmniumOrder
            {
                OrderNumber = purchaseOrder.OrderNumber,
                OrderType = orderType,
                OrderOrigin = "Webshop",
                CustomerName = customerName,
                MarketId = purchaseOrder.MarketId.Value,
                Created = purchaseOrder.Created,
                Modified = purchaseOrder.Modified ?? DateTime.MinValue,
                Status = GetOrderStatus(purchaseOrder.OrderStatus),
                StoreId = "",
                BillingAddress = billingAddress,
                PublicOrderNotes = MapComments(purchaseOrder),
                BillingCurrency = currency,
                OrderForm = omniumOrderForm,
                Properties = purchaseOrder.ToPropertyList(),
                CustomerId = purchaseOrder.CustomerId.ToString(),
                CustomerComment = "",
                CustomerReference = "",
                CustomerEmail = firstShipment?.ShippingAddress?.Email,
                CustomerPhone = firstShipment?.ShippingAddress?.DaytimePhoneNumber                
            };
        }

        public virtual string GetOrderStatus(OrderStatus orderStatus)
        {
            if (orderStatus.Equals(OrderStatus.InProgress))
                return NewOrderStatus;
            if (orderStatus.Equals(OrderStatus.Cancelled))
                return CancelledOrderStatus;
            if (orderStatus.Equals(OrderStatus.PartiallyShipped))
                return PartiallyShippedOrderStatus;
            if (orderStatus.Equals(OrderStatus.Completed))
                return CompletedOrderStatus;

            return NewOrderStatus;
        }

        public virtual OmniumOrderForm MapOrderForm(IPurchaseOrder orderGroup, IOrderForm orderForm)
        {
            return MapOrderForm(orderGroup, orderForm, orderForm.Shipments.ToArray());
        }

        public virtual OmniumOrderForm MapOrderForm(IPurchaseOrder orderGroup, IOrderForm orderForm, IShipment[] shipments)
        {
            var market = _marketService.GetMarket(orderGroup.MarketId);
            var currency = orderGroup.Currency;
            var shipment = shipments.First();

            var appliedDiscounts = orderGroup.ApplyDiscounts(_promotionEngine, new PromotionEngineSettings())?.ToList();
            var shippingDiscounts = appliedDiscounts?.Where(x => x.IsOfType(DiscountType.Shipping));
            var discounts = appliedDiscounts?.Where(x => !x.IsOfType(DiscountType.Shipping));

            var omniumDiscounts = MapDiscounts(discounts, orderGroup.Currency, market, shipment.ShippingAddress);
            var omniumShipments = shipments
                .Select(x => MapShipment(x, market, currency, shippingDiscounts))
                .ToList();

            var totals = GetOrderFormTotals(orderGroup, market, currency, omniumShipments);

            return new OmniumOrderForm
            {
                Discounts = omniumDiscounts,
                Shipments = omniumShipments,
                Payments = orderForm.Payments.Select(x => MapPayment(x, totals.Total)).ToList(),
                LineItems = omniumShipments.SelectMany(x => x.LineItems).ToList(),
                Properties = orderForm.ToPropertyList(),
                HandlingTotal = totals.Handling,
                SubTotal = totals.SubTotal,
                SubTotalExclTax = totals.SubTotalExclTax,
                ShippingSubTotal = totals.Shipping,
                DiscountAmount = totals.OrderDiscounts + totals.ShippingDiscounts,
                Total = totals.Total,
                TaxTotal = totals.TaxTotal,
                TotalExclTax = totals.TotalExclTax,
                AuthorizedPaymentTotal = Math.Min(orderForm.AuthorizedPaymentTotal, totals.Total),
                CapturedPaymentTotal = Math.Min(orderForm.CapturedPaymentTotal, totals.Total),
                ShippingDiscountTotal = totals.ShippingDiscounts
            };
        }

        private List<OmniumDiscount> MapDiscounts(
            IEnumerable<RewardDescription> rewardDescriptions, Currency currency, IMarket market, IOrderAddress address)
        {
            return rewardDescriptions != null
                ? rewardDescriptions.Select(reward => MapDiscount(reward, currency, market, address)).ToList()
                : new List<OmniumDiscount>();
        }

        private OmniumDiscount MapDiscount(
            RewardDescription reward, Currency currency, IMarket market, IOrderAddress address)
        {
            var discountValue = market.PricesIncludeTax ? reward.SavedAmount : reward.SavedAmount; //reward.GetSavedAmountIncludingTax(market.MarketId, currency, address);

            return new OmniumDiscount
            {
                DiscountAmount = discountValue,
                DiscountCode = reward.AppliedCoupon,
                DiscountName = reward.Promotion.Name,
                DiscountValue = discountValue,
                RewardType = reward.RewardType.ToString(),
                DiscountType = reward.Promotion.DiscountType.ToString()
            };
        }

        public OmniumShipment MapShipment(IShipment shipment, IMarket market, Currency currency)
        {
            return MapShipment(shipment, market, currency, null);
        }

        public virtual OmniumShipment MapShipment(
            IShipment shipment, IMarket market, Currency currency, IEnumerable<RewardDescription> shippingDiscounts)
        {
            var totals = _shippingCalculator.GetShippingTotals(shipment, market, currency);

            var shipmentId = shipment.ShipmentId.ToString();

            var omniumDiscounts = MapDiscounts(shippingDiscounts, currency, market, shipment.ShippingAddress);
            var shippingMethodName = GetShipmentName(shipment) ?? shipment.ShippingMethodName;
            var lineItems = shipment.LineItems.Select(x => MapOrderLine(x, market, currency, shipment.ShippingAddress))
                                              .ToList();

            var shipmentItemsTotalInclTax = lineItems.Sum(x => x.ExtendedPrice);
            var shipmentItemsTax = lineItems.Sum(x => x.TaxTotal);

            var shippingCostsInclTax = market.PricesIncludeTax ? totals.ShippingCost : totals.ShippingCost + totals.ShippingTax;

            var shippingDiscountPrice = shipment.GetShipmentDiscount();

            var result = new OmniumShipment
            {
                Status = shipment.OrderShipmentStatus.ToString(),
                ShipmentId = shipmentId,
                ShippingMethodId = shipment.ShippingMethodId.ToString(),
                ShippingMethodName = shippingMethodName,
                WarehouseCode = shipment.WarehouseCode,
                ShipmentTrackingNumber = shipment.ShipmentTrackingNumber,
                Address = MapOrderAddress(shipment.ShippingAddress),
                LineItems = lineItems,
                Properties = shipment.ToPropertyList(),
                Discounts = omniumDiscounts,
                // Shipping discount 
                ShippingDiscountAmount = shippingDiscountPrice,
                // shipping tax
                ShippingTax = totals.ShippingTax,
                // shipping costs + shipping tax
                ShippingSubTotal = shippingCostsInclTax,
                // sum of all line item (extended) prices
                SubTotal = shipmentItemsTotalInclTax,
                // sum of shipping price + all line item prices
                Total = shippingCostsInclTax + shipmentItemsTotalInclTax - shippingDiscountPrice,
                // sum of shipping tax + all line items tax
                TaxTotal = totals.ShippingTax + shipmentItemsTax,
            };
            return result;
        }

        public virtual OmniumPayment MapPayment(IPayment payment, Money orderFormTotal)
        {
            return new OmniumPayment
            {
                PaymentMethodName = GetPaymentSystemCode(payment),
                CustomerName = payment.CustomerName,
                Amount = Math.Min(payment.Amount, orderFormTotal.Amount),
                AuthorizationCode = payment.AuthorizationCode,
                PaymentId = payment.PaymentId,
                PaymentMethodId = payment.PaymentMethodId,
                PaymentType = payment.PaymentType.ToString(),
                Status = payment.Status,
                TransactionID = payment.TransactionID,
                ImplementationClass = payment.ImplementationClass,
                TransactionType = payment.TransactionType,
                ValidationCode = payment.ValidationCode,
                Properties = payment.ToPropertyList()
            };
        }

        public virtual OmniumOrderLine MapOrderLine(
            ILineItem lineItem, IMarket market, Currency currency, IOrderAddress address)
        {
            var marketId = market.MarketId;

            var taxTotal = _lineItemCalculator.GetSalesTax(lineItem, market, currency, address);
            var taxRate = _taxUtility.GetTaxValue(marketId, address, TaxType.SalesTax, lineItem.TaxCategoryId);

            var placedPrice = _taxUtility.GetPriceTax(lineItem, market, currency, address, lineItem.PlacedPrice);
            var discountedAmount = _taxUtility.GetPriceTax(lineItem, market, currency, address, lineItem.GetDiscountTotal(currency)); //all discounts (line item + coupon code)
            var extendedPrice = _taxUtility.GetPriceTax(lineItem, market, currency, address, _lineItemCalculator.GetExtendedPrice(lineItem, currency));
            var discountedPrice = _taxUtility.GetPriceTax(lineItem, market, currency, address, _lineItemCalculator.GetDiscountedPrice(lineItem, currency));

            var omniumOrderLine2 = new OmniumOrderLine
            {
                Code = lineItem.Code,
                ProductId = GetProductCode(lineItem.Code),
                DisplayName = lineItem.DisplayName,
                PlacedPrice = placedPrice.PriceInclTax,
                PlacedPriceExclTax = placedPrice.PriceExclTax,
                ExtendedPrice = extendedPrice.PriceInclTax,
                ExtendedPriceExclTax = extendedPrice.PriceExclTax,
                DiscountedPrice = discountedPrice.PriceInclTax,
                DiscountedPriceExclTax = discountedPrice.PriceExclTax,
                Discounted = discountedAmount.PriceInclTax,
                DiscountedExclTax = discountedAmount.PriceExclTax,
                TaxTotal = taxTotal,
                TaxRate = (decimal)taxRate,
                LineItemId = lineItem.LineItemId.ToString(),
                Quantity = lineItem.Quantity,
                Properties = lineItem.ToPropertyList()
            };
            return omniumOrderLine2;
        }

        public virtual OmniumOrderAddress MapOrderAddress(IOrderAddress orderAddress)
        {
            if (orderAddress == null)
                return new OmniumOrderAddress();

            return new OmniumOrderAddress
            {
                Name = $"{orderAddress.FirstName} {orderAddress.LastName}",
                FirstName = orderAddress.FirstName,
                LastName = orderAddress.LastName,
                DaytimePhoneNumber = orderAddress.DaytimePhoneNumber,
                EveningPhoneNumber = orderAddress.EveningPhoneNumber,
                Email = orderAddress.Email,
                Line1 = orderAddress.Line1,
                Line2 = orderAddress.Line2,
                PostalCode = orderAddress.PostalCode,
                City = orderAddress.City,
                CountryCode = GetFormattedCountryCode(orderAddress.CountryCode),
                CountryName = orderAddress.CountryName,
                Organization = orderAddress.Organization,
                RegionCode = orderAddress.RegionCode,
                RegionName = orderAddress.RegionName
            };
        }

        public virtual List<OmniumComment> MapComments(IPurchaseOrder purchaseOrder)
        {
            if (purchaseOrder.Notes == null)
            {
                return null;
            }

            return purchaseOrder.Notes.Select(x => new OmniumComment
            {
                Created = x.Created,
                CustomerId = x.CustomerId.ToString(),
                Detail = x.Detail,
                Title = x.Title,
                Type = x.Type,
                LineItemId = x.LineItemId?.ToString(),
                Id = x.OrderNoteId.ToString(),
            }).ToList();
        }



        protected virtual OmniumOrderTotals GetOrderFormTotals(
            IPurchaseOrder purchaseOrder, IMarket market, Currency currency, IEnumerable<OmniumShipment> shipments)
        {
            var orderGroupTotals = _orderGroupCalculator.GetOrderGroupTotals(purchaseOrder);

            var totalShipping = new Money(shipments.Sum(s => s.ShippingSubTotal), currency);
            var totalShippingTax = new Money(shipments.Sum(x => x.ShippingTax), currency);
            var totalLineItems = new Money(shipments.Sum(s => s.LineItems.Sum(l => l.ExtendedPrice)), currency);
            var totalLineItemsExclTax = new Money(shipments.Sum(s => s.LineItems.Sum(l => l.ExtendedPriceExclTax)), currency);
            return new OmniumOrderTotals(currency)
            {
                ShippingDiscounts = new Money(shipments.Sum(x => x.ShippingDiscountAmount), currency),
                OrderDiscounts = _orderGroupCalculator.GetOrderDiscountTotal(purchaseOrder),
                Handling = orderGroupTotals.HandlingTotal,
                // total shipping costs
                Shipping = totalShipping,
                // total shipping costs excl tax
                ShippingExclTax = totalShipping - totalShippingTax,
                // total line item prices (extended price)
                SubTotal = totalLineItems,
                // total line items prices exl tax (ExtendedPriceExclTax)
                SubTotalExclTax = totalLineItemsExclTax,
                // total taxes
                TaxTotal = orderGroupTotals.TaxTotal,
                // total incl taxes
                Total = orderGroupTotals.Total,
                // total excl taxes
                TotalExclTax = orderGroupTotals.Total - orderGroupTotals.TaxTotal // new Money(orderGroupTotals.Total - (totalShipmentTax + totalLineItemTax), currency) // total - (shipment tax + lineItems tax)
            };
        }


        protected virtual string GetFormattedCountryCode(string countryCode)
        {
            if (string.IsNullOrEmpty(countryCode))
                return countryCode;

            try
            {
                return _cultureResolver.GetTwoLetterCountryCode(countryCode);
            }
            catch (ArgumentException)
            {
                return countryCode;
            }
        }

        private OmniumOrderAddress GetBillingAddress(IOrderGroup order)
        {
            var form = order.GetFirstForm();
            var payment = form?.Payments.FirstOrDefault();

            if (PaymentHasBillingAddress(payment) == false)
            {
                // use shipping address as fallback
                var shipment = order.GetFirstShipment();
                if (shipment?.ShippingAddress != null)
                {
                    return MapOrderAddress(shipment.ShippingAddress);
                }
            }
            else if (payment != null)
            {
                return MapOrderAddress(payment.BillingAddress);
            }

            return new OmniumOrderAddress();
        }

        private bool PaymentHasBillingAddress(IPayment payment)
        {
            if (payment?.BillingAddress == null)
                return false;
            if (string.IsNullOrEmpty(payment.BillingAddress.FirstName))
                return false;
            if (string.IsNullOrEmpty(payment.BillingAddress.LastName))
                return false;
            if (string.IsNullOrEmpty(payment.BillingAddress.Email))
                return false;
            if (string.IsNullOrEmpty(payment.BillingAddress.DaytimePhoneNumber))
                return false;
            return true;
        }

        private string GetProductCode(string lineItemCode)
        {
            if (string.IsNullOrEmpty(lineItemCode))
            {
                return null;
            }
            var variationContentLink = _referenceConverter.GetContentLink(lineItemCode);
            if (!ContentReference.IsNullOrEmpty(variationContentLink) && _contentRepository.TryGet<VariationContent>(variationContentLink, out var variationContent))
            {
                var parentProductContentLink = variationContent.GetParentProducts().FirstOrDefault();
                if (!ContentReference.IsNullOrEmpty(parentProductContentLink) &&
                    _contentRepository.TryGet<ProductContent>(parentProductContentLink, out var productContent))
                {
                    return productContent?.Code;
                }
            }
            return string.Empty;
        }

        private string GetPaymentSystemCode(IPayment payment)
        {
            var paymentMethod = _paymentManagerFacade.GetPaymentMethod(payment.PaymentMethodId)?.PaymentMethod.FirstOrDefault();
            if (paymentMethod == null)
            {
                return string.Empty;
            }
            return paymentMethod.SystemKeyword;
        }

        private string GetShipmentName(IShipment shipment)
        {
            var shipmentMethod = _shipmentManagerFacade.GetShippingMethod(shipment.ShippingMethodId, true)?.ShippingMethod.FirstOrDefault();
            if (shipmentMethod == null)
            {
                return string.Empty;
            }
            return shipmentMethod.Name;
        }

        private string GetCustomerName(IPurchaseOrder purchaseOrder, OmniumOrderAddress billingAddress)
        {
            if (string.IsNullOrEmpty(billingAddress.FirstName) && string.IsNullOrEmpty(billingAddress.LastName))
            {
                return purchaseOrder is OrderGroup ? ((OrderGroup)purchaseOrder).CustomerName : string.Empty;
            }
            return $"{billingAddress.FirstName} {billingAddress.LastName}";
        }
    }
}
