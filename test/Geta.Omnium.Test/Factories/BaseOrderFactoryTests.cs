using System;
using System.Collections.Generic;
using EPiServer;
using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;
using EPiServer.Framework.Cache;
using Geta.Omnium.Culture;
using Geta.Omnium.Factories;
using Geta.Omnium.Taxes;
using Geta.Omnium.Test.TestSupport.Fakes;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Markets;
using Moq;

namespace Geta.Omnium.Test.Factories
{
    public abstract class BaseOrderFactoryTests
    {
        protected DefaultOmniumOrderFactory DefaultOmniumOrderFactorySubject;
        protected DefaultEpiOrderFactory DefaultEpiOrderFactorySubject;
        protected readonly Currency Currency;
        protected readonly MarketImpl Market;
        protected readonly OmniumFakesBuilder OmniumFakesBuilder;
        protected readonly CultureResolver CultureResolver;

        protected BaseOrderFactoryTests()
        {
            OmniumFakesBuilder = new OmniumFakesBuilder();
            Market = new MarketImpl(MarketId.Default);
            Currency = new Currency(Currency.USD);
            CultureResolver = new CultureResolver();

            InitializeDefaultOmniumOrderFactorySubject();
            InitializeDefaultEpiOrderFactorySubject();
        }

        private void InitializeDefaultOmniumOrderFactorySubject()
        {
            var shippingCalculatorMock = new Mock<IShippingCalculator>();
            var marketServiceMock = new Mock<IMarketService>();
            marketServiceMock.Setup(x => x.GetMarket(It.IsAny<MarketId>())).Returns(Market);

            var taxUtilityMock = new Mock<ITaxUtility>();
            var synchronizedObjectInstanceCacheMock = new Mock<ISynchronizedObjectInstanceCache>();
            var referenceConverterMock = new Mock<ReferenceConverter>(new EntryIdentityResolver(synchronizedObjectInstanceCacheMock.Object), new NodeIdentityResolver(synchronizedObjectInstanceCacheMock.Object));
            var contentRepositoryMock = new Mock<IContentRepository>();
            var paymentManagerFacadeMock = new Mock<IPaymentManagerFacade>();
            var shipmentManagerFacadeMock = new Mock<IShipmentManagerFacade>();
            var lineItemCalculatorMock = new Mock<ILineItemCalculator>();
            var orderFormCalculator = new Mock<IOrderFormCalculator>();
            var orderGroupCalculator = new Mock<IOrderGroupCalculator>();
            var promoteEngine = new Mock<IPromotionEngine>();
            var contenLoader = new Mock<IContentLoader>();

            shippingCalculatorMock.SetReturnsDefault(new Money(0, Currency));
            shippingCalculatorMock.Setup(x => x.GetShippingTotals(It.IsAny<IShipment>(), It.IsAny<IMarket>(), It.IsAny<Currency>()))
                .Returns(new ShippingTotals(new Money(0, Currency), new Money(0, Currency), new Money(0, Currency), null));

            lineItemCalculatorMock.SetReturnsDefault(new Money(0, Currency));
            orderFormCalculator.SetReturnsDefault(new Money(0, Currency));
            orderGroupCalculator.SetReturnsDefault(new Money(0, Currency));
            orderGroupCalculator.Setup(x => x.GetOrderGroupTotals(It.IsAny<IPurchaseOrder>()))
                .Returns(new OrderGroupTotals(new Money(0, Currency), new Money(0, Currency), new Money(0, Currency),
                    new Money(0, Currency), new Money(0, Currency), new Dictionary<IOrderForm, OrderFormTotals>()));
            taxUtilityMock.SetReturnsDefault(new Money(0, Currency));
            taxUtilityMock.Setup(x => x.GetSalesTax(It.IsAny<ILineItem>(), It.IsAny<IMarket>(), It.IsAny<Currency>(),
                It.IsAny<IOrderAddress>())).Returns(new Money(0, Currency));
            taxUtilityMock.Setup(x => x.GetPriceTax(It.IsAny<ILineItem>(), It.IsAny<IMarket>(), It.IsAny<Currency>(),
                It.IsAny<IOrderAddress>(), It.IsAny<decimal>())).Returns(new PriceTax());

            DefaultOmniumOrderFactorySubject = new DefaultOmniumOrderFactory(
                shippingCalculatorMock.Object,
                marketServiceMock.Object,
                CultureResolver,
                taxUtilityMock.Object,
                referenceConverterMock.Object,
                contentRepositoryMock.Object,
                paymentManagerFacadeMock.Object,
                lineItemCalculatorMock.Object,
                orderGroupCalculator.Object,
                shipmentManagerFacadeMock.Object,
                promoteEngine.Object,
                contenLoader.Object
                );
        }

        private void InitializeDefaultEpiOrderFactorySubject()
        {
            var orderGroupFactory = new Mock<IOrderGroupFactory>();

            var customerContext = new Mock<CustomerContext>();
            customerContext.Setup(x => x.GetContactById(It.IsAny<Guid>())).Returns(CustomerContact.CreateInstance());

            DefaultEpiOrderFactorySubject = new DefaultEpiOrderFactory(new TestOrderGroupFactory(orderGroupFactory.Object), CultureResolver, customerContext.Object);
        }
    }
}
