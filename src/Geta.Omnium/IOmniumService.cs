using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using Omnium.Models;
using Omnium.Public.Orders.Models;

namespace Geta.Omnium
{
    public interface IOmniumService
    {
        Task<Response> TransferOrderToOmnium(IPurchaseOrder purchaseOrder);
        Task<Response> TransferOrderToOmnium(OmniumOrder omniumOrder);
    }
}