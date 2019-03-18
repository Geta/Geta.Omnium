using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using Omnium.Models;

namespace Geta.Omnium
{
    public interface IOmniumService
    {
        Task<Response> TransferOrderToOmnium(IPurchaseOrder purchaseOrder);
    }
}