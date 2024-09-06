using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ComprehensiveStorageApp.ComprehensiveStorageApp;
using Microsoft.Extensions.Options;

namespace ComprehensiveStorageApp.Services
{
    public class BlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobStorageService(IOptions<AppSettings> appSettings)
        {
            _blobServiceClient = new BlobServiceClient(appSettings.Value.StorageConnectionString);
        }

        public async Task<string> UploadBlobAsync(string containerName, string blobName, Stream content, string contentType)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(content, new BlobHttpHeaders { ContentType = contentType });
            return blobClient.Uri.ToString();
        }

        public async Task DownloadBlobAsync(string containerName, string blobName, Stream destinationStream)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.DownloadToAsync(destinationStream);
        }

        public async Task DeleteBlobAsync(string containerName, string blobName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }
    }
}
