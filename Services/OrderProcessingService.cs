using Azure.Storage.Queues.Models;
using ComprehensiveStorageApp.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace ComprehensiveStorageApp.Services
{
    public class OrderProcessingService : IHostedService, IDisposable
    {
        private readonly ILogger<OrderProcessingService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private Timer _timer;

        public OrderProcessingService(ILogger<OrderProcessingService> logger,
                                      IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Order Processing Service is starting.");

            //_timer = new Timer(DoWorkAsync, null, TimeSpan.Zero,
            //    TimeSpan.FromSeconds(30)); // Check the queue every 30 seconds

            _timer = new Timer(DoWorkWrapper, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));

            return Task.CompletedTask;
        }

        // Wrapper method for Timer
        private void DoWorkWrapper(object state)
        {
            // Call the async method, fire-and-forget
            DoWorkAsync(state).ContinueWith(task =>
            {
                // Log if there was an exception
                if (task.Exception != null)
                {
                    _logger.LogError(task.Exception, "Error occurred during DoWorkAsync");
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        //private async void DoWorkAsync(object state)
        private async Task DoWorkAsync(object state)
        {
            _logger.LogInformation("Checking for new orders...");

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var queueStorageService = scope.ServiceProvider.GetRequiredService<QueueStorageService>();
                var tableStorageService = scope.ServiceProvider.GetRequiredService<TableStorageService>();

                var messages = await queueStorageService.DequeueMessagesAsync("order-processing", 1, TimeSpan.FromMinutes(5));

                foreach (var message in messages)
                {
                    try
                    {
                        await ProcessOrderAsync(message, tableStorageService, queueStorageService);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing order: {message.MessageText}");
                        // Consider re-queueing with a delay or moving to a dead-letter queue
                    }
                }
            }
        }

        private async Task ProcessOrderAsync(QueueMessage message, TableStorageService tableStorageService, QueueStorageService queueStorageService)
        {
            var orderData = message.MessageText;
            _logger.LogInformation($"Processing order: {orderData}");

            try
            {
                // 1. Deserialize the order data
                var order = JsonConvert.DeserializeObject<Order>(orderData);

                // 2. Update inventory in Table storage
                foreach (var item in order.Items)
                {
                    var product = await tableStorageService.GetProductAsync("Products", item.ProductId);
                    if (product != null && product.QuantityInStock >= item.Quantity)
                    {
                        product.QuantityInStock -= item.Quantity;
                        await tableStorageService.UpdateProductAsync(product);
                    }
                    else
                    {
                        _logger.LogError($"Insufficient stock or product not found: {item.ProductId}");
                        // Handle insufficient stock or product not found appropriately (e.g., notify user, cancel order)
                    }
                }

                // 3. Delete the processed message
                await queueStorageService.DeleteMessageAsync("order-processing", message);

                _logger.LogInformation($"Order processed successfully: {orderData}");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing order data. Invalid JSON format.");
                // Consider moving the message to a dead-letter queue or handling the error appropriately
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Order Processing Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        // Example Order and OrderItem classes 
        public class Order
        {
            public string OrderId { get; set; }
            public List<OrderItem> Items { get; set; }
        }

        public class OrderItem
        {
            public string ProductId { get; set; }
            public int Quantity { get; set; }
        }
    }
}