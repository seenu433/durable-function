using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace filevalidation
{
    static class Helpers
    {
        public static CustomerBlobAttributes ParseEventGridPayload(dynamic eventGridItem, ILogger log)
        {
            if (eventGridItem.eventType == @"Microsoft.Storage.BlobCreated"
                && eventGridItem.data.api == @"PutBlob"
                && eventGridItem.data.contentType == @"application/vnd.ms-excel")
            {
                try
                {
                    var retVal = CustomerBlobAttributes.Parse((string)eventGridItem.data.url);
                    return retVal;
                }
                catch (Exception ex)
                {
                    log.LogError(@"Error parsing Event Grid payload", ex);
                }
            }

            return null;
        }

        public static async Task<bool> CreateShippingLabel(string prefix, ILogger logger = null)
        {
            logger?.LogTrace(@"CreateShippingLabel run.");
            if (!CloudStorageAccount.TryParse(Environment.GetEnvironmentVariable(@"OrderBlobStorage"), out var storageAccount))
            {
                throw new Exception(@"Can't create a storage account accessor from app setting connection string, sorry!");
            }

            logger?.LogTrace($@"prefix: {prefix}");

            var blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference("shipping");
            await container.CreateIfNotExistsAsync();

            CloudBlockBlob blob = container.GetBlockBlobReference($@"{prefix}.txt");
            await blob.UploadTextAsync("prefix");

            return true;
        }
    }

    static class HttpExtensions
    {
        public static HttpResponseMessage CreateCompatibleResponse(this HttpRequestMessage _, HttpStatusCode code) => new HttpResponseMessage(code);

        public static HttpResponseMessage CreateCompatibleResponse(this HttpRequestMessage _, HttpStatusCode code, string stringContent) => new HttpResponseMessage(code) { Content = new StringContent(stringContent) };
    }
}
