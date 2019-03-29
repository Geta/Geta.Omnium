using System.Configuration;
using System.Net;
using EPiServer.Commerce.Order;
using Omnium.Models;
using Omnium.Orders.Interfaces;
using Omnium.Public.Orders.Models;

namespace Geta.Omnium.Extensions
{
    public static class OmniumOrderExtensions
    {
        public static OmniumOrder Anonymize(this OmniumOrder order)
        {
            if (order?.OrderForm == null)
                return order;

            order.CustomerName = order.CustomerName.ToAnonymized();
            order.BillingAddress = Anonymize(order.BillingAddress);

            foreach (var shipment in order.OrderForm.Shipments)
            {
                shipment.Address = Anonymize(shipment.Address);
            }

            return order;
        }

        public static OmniumOrderAddress Anonymize(this OmniumOrderAddress address)
        {
            return new OmniumOrderAddress
            {
                Name = address.Name.ToAnonymized(),
                FirstName = address.FirstName.ToAnonymized(),
                LastName = address.LastName.ToAnonymized(),
                Organization = address.Organization,
                Email = address.Email.ToAnonymized(),
                DaytimePhoneNumber = address.DaytimePhoneNumber.ToAnonymized(),
                EveningPhoneNumber = address.EveningPhoneNumber.ToAnonymized(),
                Line1 = address.Line1.ToAnonymized(),
                Line2 = address.Line2.ToAnonymized(),
                PostalCode = address.PostalCode,
                State = address.State,
                RegionCode = address.RegionCode,
                RegionName = address.RegionName,
                City = address.City,
                CountryCode = address.CountryCode,
                CountryName = address.CountryName
            };
        }

        public static string ToAnonymized(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            var buffer = input.ToCharArray();

            for (var i = 0; i < buffer.Length; i++)
            {
                var c = buffer[i];

                if (!char.IsLetterOrDigit(c))
                    continue;

                buffer[i] = '*';
            }

            return new string(buffer);
        }

        public static OmniumOrderType ToOmniumOrderType(this IPurchaseOrder order)
        {
            //TODO: Determine OrderType from shipping method
            return OmniumOrderType.Online;
        }

        public static bool IsConfigured(this IOrderClient client)
        {
            return IsOmniumConfigured();
        }
        
        private static bool IsOmniumConfigured()
        {
            if (ConfigurationManager.AppSettings["omnium:endpointUrl"] == null)
                return false;
            if (ConfigurationManager.AppSettings["omnium:username"] == null)
                return false;
            if (ConfigurationManager.AppSettings["omnium:token"] == null)
                return false;

            return true;
        }

        public static bool IsSuccess(this Response response)
        {
            if (response == null)
                return false;
            return IsSuccessStatusCode(response.HttpStatusCode);
        }

        public static string ExceptionMessage(this Response response)
        {
            return response?.OriginalException?.Message;
        }

        private static bool IsSuccessStatusCode(HttpStatusCode statusCode)
        {
            return (int)statusCode >= 200 && (int)statusCode <= 299;
        }
    }
}
