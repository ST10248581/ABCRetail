using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
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
    }

}
