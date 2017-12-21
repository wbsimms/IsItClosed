using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace IsItClosedWeb.Services
{
    public class BlobInformation
    {
        private string PHOTO_FILE_NAME = "garage.jpg";
        private static string storageKey = "tFkjmym5qTXZSuA9UGHJlFWINjPB4Bcn4j1DVxAbz/7EQGnTz4P0sZZ21Eb49y6upWPZbbUOHlsA/Zxc2t5ciA==";

        public BlobInformation()
        {
        }

        public async Task<string> GetInformation()
        {
            StorageCredentials storageCredentials = new StorageCredentials("devfoundriesdev", storageKey);
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("isitclosed");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(PHOTO_FILE_NAME);

            await blockBlob.FetchAttributesAsync();
            var lastMod = blockBlob.Properties.LastModified.Value;
            var cstDate = lastMod.Subtract(TimeSpan.FromHours(6));
            var toString = cstDate.ToString();
            return toString;
        }
    }
}