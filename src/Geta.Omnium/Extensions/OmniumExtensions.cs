using System.Collections.Generic;
using EPiServer.Commerce.Storage;
using Omnium.Public.Models;

namespace Geta.Omnium.Extensions
{
    public static class OmniumExtensions
    {
        public static List<OmniumPropertyItem> ToPropertyList(
            this IExtendedProperties extendedProperties)
        {
            var omniumPropertyItemList = new List<OmniumPropertyItem>();
            foreach (var key in extendedProperties.Properties.Keys)
            {
                var str1 = key.ToString();
                var str2 = extendedProperties.Properties[key]?.ToString();
                var omniumPropertyItem = new OmniumPropertyItem()
                {
                    Key = str1,
                    Value = str2
                };
                omniumPropertyItemList.Add(omniumPropertyItem);
            }
            return omniumPropertyItemList;
        }
    }
}
