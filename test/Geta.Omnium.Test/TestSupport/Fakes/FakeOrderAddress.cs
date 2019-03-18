using System.Collections;
using EPiServer.Commerce.Order;

namespace Geta.Omnium.Test.TestSupport.Fakes
{
    public class FakeOrderAddress : IOrderAddress
    {
        public string Id { get; set; }

        public string City { get; set; }

        public string CountryCode { get; set; }

        public string CountryName { get; set; }

        public string DaytimePhoneNumber { get; set; }

        public string Email { get; set; }

        public string EveningPhoneNumber { get; set; }

        public string FaxNumber { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Line1 { get; set; }

        public string Line2 { get; set; }

        public string Organization { get; set; }

        public string PostalCode { get; set; }

        public string RegionCode { get; set; }

        public string RegionName { get; set; }

        public Hashtable Properties { get; set; }

        public static FakeOrderAddress CreateOrderAddress()
        {
            var orderAddress = new FakeOrderAddress
            {
                Id = "Order address 1",
                FirstName = "Cruz",
                LastName = "Graham",
                Line1 = "Kungsgatan 446	",
                Line2 = "",
                PostalCode = "12345",
                City = "Gävle",
                CountryCode = "SWE",
                CountryName = "Sweden",
                Email = "cruz@example.com",
                DaytimePhoneNumber = "0123456789",
                EveningPhoneNumber = "0987654321",
                RegionCode = "",
                RegionName = "Gästrikland",
                Organization = "Brav"
            };
            return orderAddress;
        }

        public static FakeOrderAddress CreateEmptyOrderAddress()
        {
            var orderAddress = new FakeOrderAddress
            {
            };
            return orderAddress;
        }
    }
}