using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Omnium.Models;
using Omnium.Orders.Interfaces;
using Omnium.Public.Orders.Models;

namespace Geta.Omnium
{
    public interface IExtendedOrderClient : IOrderClient
    {
        Task<Response<List<OmniumOrder>>> GetOrdersAsync(string[] storeId, string[] status, DateTime changedSince,
            int pageSize, int page);

        Task<Response<int>> GetOrdersCountAsync(string[] storeId, string[] status, DateTime changedSince);
    }
}
