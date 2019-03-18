using System;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using EPiServer.PlugIn;
using EPiServer.Scheduler;
using EPiServer.ServiceLocation;
using Geta.Omnium.Extensions;
using Geta.Omnium.Factories;
using Geta.Omnium.Models;
using Mediachase.Commerce.Markets;

namespace Geta.Omnium.Jobs
{
    [ScheduledPlugIn(
        GUID = "A53AD548-B8BA-4E91-B034-7E876529FBEC",
        DisplayName = "Omnium - Synchronize orders to Epi",
        SortIndex = 280)]
    public class SynchronizeOrdersFromOmnium : ScheduledJobBase
    {
        protected static readonly ILogger Logger = LogManager.GetLogger(typeof(SynchronizeOrdersToOmnium));
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IExtendedOrderClient _orderClient;
        private readonly IMarketService _marketService;
        private readonly IEpiOrderFactory _defaultOrderToOmniumFactory;
        private readonly IOmniumService _omniumService;
        private readonly int _batchSize;
        private readonly IOmniumImportSettings _omniumImportSettings;

        private int _errors;
        private int _found;
        private int _processed;
        private int _existingOrders;
        private int _newOrders;

        // don't create new orders in Episerver
        private bool _mergeNewOrders = true; 

        public SynchronizeOrdersFromOmnium()
        {
            _purchaseOrderRepository = ServiceLocator.Current.GetInstance<IPurchaseOrderRepository>();
            _batchSize = 30;
            _orderRepository = ServiceLocator.Current.GetInstance<IOrderRepository>();
            _orderClient = ServiceLocator.Current.GetInstance<IExtendedOrderClient>();
            _marketService = ServiceLocator.Current.GetInstance<IMarketService>();
            _defaultOrderToOmniumFactory = ServiceLocator.Current.GetInstance<IEpiOrderFactory>();
            _omniumImportSettings = ServiceLocator.Current.GetInstance<IOmniumImportSettings>();
            _omniumService = ServiceLocator.Current.GetInstance<IOmniumService>();
        }

        public override string Execute()
        {
            _errors = _found = _processed = _existingOrders = _newOrders = 0;

            if (!_orderClient.IsConfigured())
            {
                throw new Exception("Omnium client not configured, cannot execute job.");
            }

            var lastExecutionDate = _omniumImportSettings.GetLastSyncFromOmniumDate() ?? DateTime.Now;
            var updateStartDate = DateTime.UtcNow;

            Process(lastExecutionDate).GetAwaiter().GetResult();

            _omniumImportSettings.LogSyncFromOmniumDate(updateStartDate);

            return $"Synchronization complete, found {_found}, processed {_processed} (new {_newOrders}, existing {_existingOrders}), errors {_errors}";
        }

        private async Task Process(DateTime lastExecutionDate)
        {
            var selectedMarkets = new[] { "swix_us" };
            var storeId = _marketService.GetAllMarkets().Where(x => selectedMarkets.Any(m => m.Equals(x.MarketId.Value))).Select(x => x.MarketId.Value).ToArray();
            var status =
                new[]
                {
                    "New",
                    "InTransit",
                    "InProgress",
                    "ReadyForPickup",
                    "Completed",
                    "OrderCanceled",
                    "PartiallyShipped",
                    "Returned",
                    "PartiallyReturned"
                };

            var page = 1;
            var process = true;

            _found = (await _orderClient.GetOrdersCountAsync(storeId, status, lastExecutionDate)).Value;
            while (process)
            {
                process = await ProcessPage(storeId, status, lastExecutionDate, page++);
            }
        }

        private async Task<bool> ProcessPage(string[] storeId, string[] status, DateTime lastExecutionDate, int page = 1)
        {
            var batchSize = _batchSize;

            var orders = (await _orderClient.GetOrdersAsync(storeId, status, lastExecutionDate, batchSize, page)).Value;

            foreach (var omniumOrder in orders)
            {
                try
                {
                    bool isNewOrder = false;
                    var purchaseOrder = _purchaseOrderRepository.Load(omniumOrder.OrderNumber);
                    if (purchaseOrder == null)
                    {
                        if (!_mergeNewOrders)
                        {
                            _processed++;
                            continue;
                        }
                        var newGuid = Guid.NewGuid();
                        purchaseOrder = _orderRepository.Create<IPurchaseOrder>(newGuid, string.Empty);
                        purchaseOrder.OrderNumber = omniumOrder.OrderNumber;
                        isNewOrder = true;
                    }

                    _defaultOrderToOmniumFactory.MapOrder(purchaseOrder, omniumOrder);
                    _orderRepository.Save(purchaseOrder);

                    _processed++;
                    if (isNewOrder)
                    {
                        _newOrders++;
                    }
                    else
                    {
                        _existingOrders++;
                    }

                    if (_defaultOrderToOmniumFactory.ShouldSyncOrderBackToOmnium(purchaseOrder, omniumOrder))
                    {
                        // sync back to Omnium to update ID's on their site (newly created shipment, payment, lineitem etc)
                        await _omniumService.TransferOrderToOmnium(purchaseOrder);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message, ex);
                    _errors++;
                }
            }
            var itemsToProcess = _found - (_processed + _errors);
            return itemsToProcess > 0;
        }
    }
}
