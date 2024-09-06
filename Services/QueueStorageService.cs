using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using ComprehensiveStorageApp.ComprehensiveStorageApp;
using Microsoft.Extensions.Options;

namespace ComprehensiveStorageApp.Services
{
    public class QueueStorageService
    {
        private readonly QueueServiceClient _queueServiceClient;

        public QueueStorageService(IOptions<AppSettings> appSettings)
        {
            _queueServiceClient = new QueueServiceClient(appSettings.Value.StorageConnectionString);
        }

        public async Task CreateQueueIfNotExistsAsync(string queueName)
        {
            var queueClient = _queueServiceClient.GetQueueClient(queueName);
            await queueClient.CreateIfNotExistsAsync();
        }

        public async Task EnqueueMessageAsync(string queueName, string message)
        {
            var queueClient = _queueServiceClient.GetQueueClient(queueName);
            await queueClient.SendMessageAsync(message);
        }

        public async Task<QueueMessage[]> DequeueMessagesAsync(string queueName, int maxMessages, TimeSpan visibilityTimeout)
        {
            var queueClient = _queueServiceClient.GetQueueClient(queueName);
            return await queueClient.ReceiveMessagesAsync(maxMessages, visibilityTimeout);
        }

        public async Task DeleteMessageAsync(string queueName, QueueMessage message)
        {
            var queueClient = _queueServiceClient.GetQueueClient(queueName);
            await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
        }
    }
}