using Azure.Storage.Files.Shares;
using ComprehensiveStorageApp.ComprehensiveStorageApp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ComprehensiveStorageApp.Services
{
    public class FileStorageService
    {
        private readonly ShareServiceClient _shareServiceClient;
        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(IOptions<AppSettings> appSettings, ILogger<FileStorageService> logger)
        {
            _shareServiceClient = new ShareServiceClient(appSettings.Value.StorageConnectionString);
            _logger = logger;
        }

        public async Task<ShareDirectoryClient> GetOrCreateDirectoryAsync(string shareName, string directoryPath)
        {
            try
            {
                var shareClient = _shareServiceClient.GetShareClient(shareName);
                await shareClient.CreateIfNotExistsAsync();

                var directoryClient = shareClient.GetDirectoryClient(directoryPath);
                await directoryClient.CreateIfNotExistsAsync();

                return directoryClient;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while getting or creating directory '{directoryPath}' in share '{shareName}'.");
                throw; // Or you can rethrow a custom exception or handle it differently based on your application's needs
            }
        }

        public async Task UploadFileAsync(string shareName, string directoryPath, string fileName, Stream content)
        {
            try
            {
                var directoryClient = await GetOrCreateDirectoryAsync(shareName, directoryPath);
                var fileClient = directoryClient.GetFileClient(fileName);
                await fileClient.CreateAsync(content.Length);
                await fileClient.UploadAsync(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while uploading file '{fileName}' to directory '{directoryPath}' in share '{shareName}'.");
                throw;
            }
        }

        public async Task DownloadFileAsync(string shareName, string directoryPath, string fileName, Stream destinationStream)
        {
            try
            {
                var directoryClient = _shareServiceClient.GetShareClient(shareName).GetDirectoryClient(directoryPath);
                var fileClient = directoryClient.GetFileClient(fileName);
                if (await fileClient.ExistsAsync())
                {
                    var response = await fileClient.DownloadAsync();
                    await response.Value.Content.CopyToAsync(destinationStream);
                }
                else
                {
                    _logger.LogWarning($"File not found for download. Share: {shareName}, Directory: {directoryPath}, FileName: {fileName}");
                    // You might want to throw an exception or handle this case differently in your controller
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while downloading file '{fileName}' from directory '{directoryPath}' in share '{shareName}'.");
                throw;
            }
        }

        public async Task DeleteFileAsync(string shareName, string directoryPath, string fileName)
        {
            try
            {
                var directoryClient = _shareServiceClient.GetShareClient(shareName).GetDirectoryClient(directoryPath);
                var fileClient = directoryClient.GetFileClient(fileName);
                await fileClient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting file '{fileName}' from directory '{directoryPath}' in share '{shareName}'.");
                throw;
            }
        }
    }
}