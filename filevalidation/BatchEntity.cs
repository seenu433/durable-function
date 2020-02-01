using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace filevalidation
{
    [JsonObject(MemberSerialization.OptIn)]
    public class BatchEntity : IBatchEntity
    {
        private readonly string _id;
        private readonly ILogger _logger;

        public BatchEntity(string id, ILogger logger)
        {
            _id = id;
            _logger = logger;
        }

        [JsonProperty]
        public bool IsOrderReceived { get; set; } = false;

        [JsonProperty]
        public bool IsPaymentReceived { get; set; } = false;

        [FunctionName(nameof(BatchEntity))]
        public static Task Run([EntityTrigger]IDurableEntityContext ctx, ILogger logger) => ctx.DispatchAsync<BatchEntity>(ctx.EntityKey, logger);

        public async Task NewFile(string fileUri)
        {
            var newCustomerFile = CustomerBlobAttributes.Parse(fileUri);
            _logger.LogInformation($@"Got new file via event: {newCustomerFile.TransactionType}");

            if(newCustomerFile.TransactionType == "order")
            {
                this.IsPaymentReceived = true;
            }
            else if(newCustomerFile.TransactionType == "payment")
            {
                this.IsOrderReceived = true;
            }

            _logger.LogTrace($@"Actor '{_id}' got file '{newCustomerFile.TransactionType}'");

            if (this.IsOrderReceived && this.IsPaymentReceived)
            {
                _logger.LogInformation(@"Payment received...");

                // call next step in functions with the prefix so it knows what to go grab
                await Helpers.CreateShippingLabel(newCustomerFile.OrderId, _logger);
            }
            else
            {
                _logger.LogInformation($@"Waiting for Payment.....");
            }
        }
    }

    public interface IBatchEntity
    {
        Task NewFile(string fileUri);
    }

}
