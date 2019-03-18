using EPiServer.Framework.Cache;
using Geta.Omnium.Facades;
using Geta.Omnium.Taxes;
using Mediachase.Commerce;
using Moq;
using Xunit;

namespace Geta.Omnium.Test.Taxes
{
    public class TaxUtilityTests
    {
        private readonly ITaxUtility _taxUtility;
        private readonly Currency _currency;

        public TaxUtilityTests()
        {
            var synchronizedObjectInstanceCacheMock = new Mock<ISynchronizedObjectInstanceCache>();
            var catalogTaxManagerFacade = new Mock<ICatalogTaxManagerFacade>();

            _taxUtility = new TaxUtility(synchronizedObjectInstanceCacheMock.Object, catalogTaxManagerFacade.Object);
            _currency = Currency.USD;
        }

        [Theory]
        [InlineData(100, 25, 125)]
        public void Can_Get_Price_With_Tax(decimal price, double taxRate, decimal expectedResult)
        {
            var result = _taxUtility.GetPriceWithTax(new Money(price, _currency), taxRate);

            Assert.Equal(result, expectedResult);
        }

        [Theory]
        [InlineData(100, 25, 80)]
        public void Can_Get_Price_Without_Tax(decimal price, double taxRate, decimal expectedResult)
        {
            var result = _taxUtility.GetPriceWithoutTax(new Money(price, _currency), taxRate);

            Assert.Equal(result, expectedResult);
        }
    }
}
