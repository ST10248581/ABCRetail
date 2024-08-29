using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;

namespace ABCRetail.Services
{


    public class BlobStorageService
    {
        private readonly string _storageConnectionString;
        private readonly BlobServiceClient _blobServiceClient;

        public BlobStorageService()
        {
            _storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=abcretailadminstorage;AccountKey=Tlm4u1G7FsIkqB/wGEHkSVdDxwywfd7IefskUv/o07FEMUS81MeuzQHsh0xHzBoeF7BKeUly5rDB+AStkG+7eg==;EndpointSuffix=core.windows.net";
            _blobServiceClient = new BlobServiceClient(_storageConnectionString);
        }

        public async Task<string> UploadImageAsync(IFormFile imageFile, string containerName, string imageFileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(imageFileName);

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = imageFile.ContentType 
            };

            using (var stream = imageFile.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders
                });
            }

            return blobClient.Uri.ToString();
        }

        public string GetImageUrl(string containerName, string photoName, bool generateSasToken = true)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(photoName);

            if (generateSasToken)
            {
                // Generate a SAS token for secure access if needed
                BlobSasBuilder sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = containerName,
                    BlobName = photoName,
                    Resource = "b",
                    ExpiresOn = DateTimeOffset.UtcNow.AddHours(1) // Set your expiration time
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                // Return the full URL with the SAS token
                Uri sasUri = blobClient.GenerateSasUri(sasBuilder);
                return sasUri.ToString();
            }

            // If public access is enabled, just return the blob's URL
            return blobClient.Uri.ToString();
        }
    }

}
