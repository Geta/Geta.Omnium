using System;
using Geta.Omnium.Culture;
using Xunit;

namespace Geta.Omnium.Test.Culture
{
    public class CultureResolverTests
    {
        private readonly CultureResolver _cultureResolver;

        public CultureResolverTests()
        {
            _cultureResolver = new CultureResolver();
        }

        [Theory]
        [InlineData("NLD", "NL")]
        [InlineData("USA", "US")]
        public void Can_Get_Two_Letter_CountryCode(string threeLetterCountryCode, string expectedResult)
        {
            var result = _cultureResolver.GetTwoLetterCountryCode(threeLetterCountryCode);

            Assert.Equal(result, expectedResult);
        }

        [Theory]
        [InlineData("NL", "NLD")]
        [InlineData("US", "USA")]
        public void Can_Get_Three_Letter_CountryCode(string twoLetterCountryCode, string expectedResult)
        {
            var result = _cultureResolver.GetThreeLetterCountryCode(twoLetterCountryCode);

            Assert.Equal(result, expectedResult);
        }

        [Fact]
        public void Throws_Exception_When_Two_Letter_Country_Code_Parameter_is_Invalid()
        {
            Assert.Throws<ArgumentException>(() => _cultureResolver.GetThreeLetterCountryCode("Netherlands"));
        }

        [Fact]
        public void Throws_Exception_When_Three_Letter_Country_Code_Parameter_is_Invalid()
        {
            Assert.Throws<ArgumentException>(() => _cultureResolver.GetTwoLetterCountryCode("Netherlands"));
        }
    }
}
