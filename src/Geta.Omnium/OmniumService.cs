using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Geta.Omnium.Extensions;
using Geta.Omnium.Factories;
using Omnium.Models;

namespace Geta.Omnium
{
    [ServiceConfiguration(typeof(IOmniumService))]
    public class OmniumService : IOmniumService
    {
        private readonly ILogger _logger = LogManager.GetLogger(typeof(OmniumService));
        private readonly IExtendedOrderClient _orderClient;
        private readonly IOmniumOrderFactory _omniumOrderFactory;

        public OmniumService(IExtendedOrderClient orderClient, IOmniumOrderFactory omniumOrderFactory)
        {
            _orderClient = orderClient;
            _omniumOrderFactory = omniumOrderFactory;
        }

        public virtual async Task<Response> TransferOrderToOmnium(IPurchaseOrder purchaseOrder)
        {
            var omniumOrder = _omniumOrderFactory.MapOrder(purchaseOrder);

            _logger.Information($"Sending order with id {omniumOrder.OrderNumber} to Omnium");

            var orderResponse = await _orderClient.AddOrderAsync(omniumOrder);

            if (orderResponse.IsSuccess())
            {
                _logger.Information($"Order with id {omniumOrder.OrderNumber} successfully sent to Omnium");
            }
            else
            {
                _logger.Warning($"Failed sending order {omniumOrder.OrderNumber} to Omnium, status {orderResponse.HttpStatusCode}");

                var exception = orderResponse.OriginalException;
                if (exception != null)
                {
                    _logger.Error(exception.Message, exception);
                }
            }
            return orderResponse;
        }
    }
}
