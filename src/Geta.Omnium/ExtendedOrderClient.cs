using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using EPiServer.ServiceLocation;
using Omnium.BaseClients;
using Omnium.Models;
using Omnium.Orders.Clients;
using Omnium.Public.Orders.Models;
using Omnium.Public.Search;


namespace Geta.Omnium
{
    [ServiceConfiguration(typeof(IExtendedOrderClient))]
    public class ExtendedOrderClient : OrderClient, IExtendedOrderClient
    {
        private IOmniumClient _client;

        public ExtendedOrderClient(IOmniumClient client) : base(client)
        {
            _client = client;
        }

        public async Task<Response<int>> GetOrdersCountAsync(string[] storeId, string[] status, DateTime changedSince)
        {
            var response = new Response<int>();

            var async = await _client.GetAsync("/api/Orders/GetOrders/Count?" + GetQueryString(storeId, status, changedSince));
            response.HttpStatusCode = async.StatusCode;
            if (async.IsSuccessStatusCode)
            {
                response.Value = await async.Content.ReadAsAsync<int>();
            }
            return response;
        }

        public async Task<Response<List<OmniumOrder>>> GetOrdersAsync(string[] storeId, string[] status, DateTime changedSince, int pageSize, int page)
        {
            var response = new Response<List<OmniumOrder>>();

            var async = await _client.GetAsync("/api/Orders/GetOrders?" + GetQueryString(storeId, status, changedSince, pageSize, page));
            response.HttpStatusCode = async.StatusCode;
            if (async.IsSuccessStatusCode)
            {
                response.Value = (await async.Content.ReadAsAsync<OmniumSearchResult<OmniumOrder>>()).Result;
            }
            return response;
        }

        private string GetQueryString(string[] storeId, string[] status, DateTime changedSince, int? pageSize = null, int? page = null)
        {
            var nvc = new NameValueCollection();
            foreach (var id in storeId)
            {
                nvc.Add(nameof(storeId), id);
            }
            foreach (var item in status)
            {
                nvc.Add(nameof(status), item);
            }
            nvc.Add(nameof(changedSince), changedSince.ToUniversalTime().ToString("o"));
            if (pageSize.HasValue)
            {
                nvc.Add(nameof(pageSize), pageSize.ToString());
            }
            if (page.HasValue)
            {
                nvc.Add(nameof(page), page.ToString());
            }
            return string.Join("&",
                    nvc.AllKeys.Where(key => !string.IsNullOrWhiteSpace(nvc[key]))
                        .Select(
                            key => string.Join("&", nvc.GetValues(key).Select(val =>
                                $"{HttpUtility.UrlEncode(key)}={HttpUtility.UrlEncode(val)}"))));
        }
    }
}
