using System;
using System.Collections.Generic;
using System.IO;
using AutoFixture;
using Omnium.Public.Models;
using Omnium.Public.Orders.Models;
using Omnium.Public.Shipments.Models;

namespace Geta.Omnium.Test.TestSupport.Fakes
{
    public class OmniumFakesBuilder
    {
        public OmniumOrder CreateOmniumOrder()
        {
            var filePath = AppDomain.CurrentDomain.BaseDirectory + "\\..\\..\\TestSupport\\Data\\order.json";
            
            var json = File.ReadAllText(filePath);

            return Newtonsoft.Json.JsonConvert.DeserializeObject<OmniumOrder>(json);
        }

        public OmniumOrderLine CreateOmniumOrderLine()
        {
            return new OmniumOrderLine
            {
                LineItemId = Guid.NewGuid().ToString(),
                Code = "Product-1",
                DisplayName = "Product 1",
                PlacedPrice = 45,
                Quantity = 20
            };
        }

        public OmniumShipment CreateOmniumShipment()
        {
            return new OmniumShipment
            {
                ShipmentId = Guid.NewGuid().ToString(),
                ShippingMethodName = "Express-USD",
                ShippingMethodId = "a07af904-6f77-4a52-8110-b327dbf479d4",
                LineItems = new List<OmniumOrderLine>()
            };
        }

        public IEnumerable<OmniumPropertyItem> CreateOmniumProperties()
        {
            var fixture = new Fixture();
            return new[]
            {
                new OmniumPropertyItem {Key = fixture.Create<string>(), Value = fixture.Create<string>()},
                new OmniumPropertyItem {Key = fixture.Create<string>(), Value = fixture.Create<string>()},
                new OmniumPropertyItem {Key = fixture.Create<string>(), Value = fixture.Create<string>()},
                new OmniumPropertyItem {Key = fixture.Create<string>(), Value = fixture.Create<string>()},
                new OmniumPropertyItem {Key = fixture.Create<string>(), Value = fixture.Create<string>()}
            };
        }
    }
}
