using System;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using EPiServer.PlugIn;
using EPiServer.Scheduler;
using EPiServer.ServiceLocation;
using Geta.Omnium.Extensions;
using Geta.Omnium.Models;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Search;
using Omnium.Orders;

namespace Geta.Omnium.Jobs
{
    [ScheduledPlugIn(
        GUID = "037a2e76-92ad-4bb9-87a9-541770bd28ae",
        DisplayName = "Omnium - Synchronize orders to Omnium",
        SortIndex = 280)]
    public class SynchronizeOrdersToOmnium : ScheduledJobBase
    {
        protected static readonly ILogger Logger = LogManager.GetLogger(typeof(SynchronizeOrdersToOmnium));
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderClient _orderClient;
        private readonly IOmniumService _omniumService;
        private readonly int _batchSize;

        private int _errors;
        private int _found;
        private int _processed;

        public SynchronizeOrdersToOmnium()
        {
            _batchSize = 1;
            _orderRepository = ServiceLocator.Current.GetInstance<IOrderRepository>();
            _orderClient = ServiceLocator.Current.GetInstance<IOrderClient>();
            _omniumService = ServiceLocator.Current.GetInstance<IOmniumService>();
        }

        public override string Execute()
        {
            _errors = _found = _processed = 0;

            if (!_orderClient.IsConfigured())
            {
                throw new Exception("Omnium client not configured, cannot execute job.");
            }

            var page = 1;
            var process = true;

            while (process && page == 1)
            {
                process = ProcessPage(page++).GetAwaiter().GetResult();
            }

            return $"Synchronization complete, found {_found}, processed {_processed}, errors {_errors}";
        }

        public async Task<bool> ProcessPage(int page = 1)
        {
            var options = GetOrderSearchOptions(page);
            var parameters = GetOrderSearchParameters();
            var orderBatch =  /* new[] {_orderRepository.Load<PurchaseOrder>(1020) }; */ OrderContext.Current.Search<PurchaseOrder>(parameters, options);

            _found += orderBatch.Length;

            await ProcessBatch(orderBatch);

            return orderBatch.Length == _batchSize;
        }

        private async Task ProcessBatch(PurchaseOrder[] orders)
        {
            var tasks = orders.Select(Process).ToList();

            await Task.WhenAll(tasks);
        }

        private async Task Process(PurchaseOrder purchaseOrder)
        {
            try
            {
                var orderResponse = await _orderClient.GetOrderAsync(purchaseOrder.TrackingNumber);
                var order = orderResponse.Value;
                if (order == null)
                {
                    var tasks = _omniumService.TransferOrderToOmnium(purchaseOrder);
                    var responses = await Task.WhenAll(tasks);

                    var failed = responses.FirstOrDefault(x => !x.IsSuccess());
                    if (failed != null)
                    {
                        throw new Exception($"Error while adding '{purchaseOrder.TrackingNumber}' to Omnium", failed.OriginalException);
                    }
                }

                purchaseOrder[OrderConstants.MetaFieldOmniumSynchronized] = true;
                purchaseOrder[OrderConstants.MetaFieldOmniumSynchronizedDate] = DateTime.UtcNow;

                _orderRepository.Save(purchaseOrder);
                _processed++;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                _errors++;
            }
        }

        private OrderSearchParameters GetOrderSearchParameters()
        {
            return new OrderSearchParameters
            {
                SqlMetaWhereClause = $"(META.{OrderConstants.MetaFieldOmniumSynchronized} IS NULL OR META.{OrderConstants.MetaFieldOmniumSynchronized} = 0)"
            };
        }

        private OrderSearchOptions GetOrderSearchOptions(int page = 1)
        {
            if (page < 1)
                throw new ArgumentException("page cannot be less than one", nameof(page));

            var options = new OrderSearchOptions
            {
                CacheResults = false,
                StartingRecord = (page - 1) * _batchSize,
                RecordsToRetrieve = _batchSize
            };

            options.Classes.Add("PurchaseOrder");

            return options;
        }
    }
}
