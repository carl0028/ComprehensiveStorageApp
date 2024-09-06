using Azure;
using Azure.Data.Tables;
using ComprehensiveStorageApp.ComprehensiveStorageApp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel;

namespace ComprehensiveStorageApp.Services
{
    public class TableStorageService
    {
        private readonly TableServiceClient _tableServiceClient;
        private readonly ILogger<TableStorageService> _logger;

        public TableStorageService(IOptions<AppSettings> appSettings, ILogger<TableStorageService> logger)
        {
            _tableServiceClient = new TableServiceClient(appSettings.Value.StorageConnectionString);
            _logger = logger;
        }

        public async Task CreateTablesIfNotExistsAsync()
        {
            await CreateTableIfNotExistsAsync("Customers");
            await CreateTableIfNotExistsAsync("Products");
        }

        private async Task CreateTableIfNotExistsAsync(string tableName)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient(tableName);
                await tableClient.CreateIfNotExistsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while creating table '{tableName}'.");
                throw;
            }
        }

        // Customer operations
        public async Task<CustomerEntity> GetCustomerAsync(string partitionKey, string rowKey)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient("Customers");
                return await tableClient.GetEntityAsync<CustomerEntity>(partitionKey, rowKey);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogWarning($"Customer not found. PartitionKey: {partitionKey}, RowKey: {rowKey}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving a customer.");
                throw;
            }
        }

        public async Task AddCustomerAsync(CustomerEntity customer)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient("Customers");
                await tableClient.AddEntityAsync(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a customer.");
                throw;
            }
        }

        public async Task UpdateCustomerAsync(CustomerEntity customer)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient("Customers");
                await tableClient.UpdateEntityAsync(customer, ETag.All, TableUpdateMode.Replace);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating a customer.");
                throw;
            }
        }

        public async Task DeleteCustomerAsync(string partitionKey, string rowKey)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient("Customers");
                await tableClient.DeleteEntityAsync(partitionKey, rowKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting a customer.");
                throw;
            }
        }

        public async Task<List<CustomerEntity>> GetAllCustomersAsync()
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient("Customers");
                var queryResultsFilter = tableClient.QueryAsync<CustomerEntity>(filter: "");
                var customers = new List<CustomerEntity>();

                await foreach (var customer in queryResultsFilter)
                {
                    customers.Add(customer);
                }

                return customers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all customers.");
                throw;
            }
        }

        // Product operations with error handling
        public async Task<ProductEntity> GetProductAsync(string partitionKey, string rowKey)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient("Products");
                return await tableClient.GetEntityAsync<ProductEntity>(partitionKey, rowKey);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogWarning($"Product not found. PartitionKey: {partitionKey}, RowKey: {rowKey}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving a product.");
                throw;
            }
        }

        public async Task AddProductAsync(ProductEntity product)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient("Products");
                await tableClient.AddEntityAsync(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a product.");
                throw;
            }
        }

        public async Task UpdateProductAsync(ProductEntity product)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient("Products");
                await tableClient.UpdateEntityAsync(product, ETag.All, TableUpdateMode.Replace);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating a product.");
                throw;
            }
        }

        public async Task DeleteProductAsync(string partitionKey, string rowKey)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient("Products");
                await tableClient.DeleteEntityAsync(partitionKey, rowKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting a product.");
                throw;
            }
        }

        public async Task<List<ProductEntity>> GetAllProductsAsync()
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient("Products");
                var queryResultsFilter = tableClient.QueryAsync<ProductEntity>(filter: "");
                var products = new List<ProductEntity>();

                await foreach (var product in queryResultsFilter)
                {
                    products.Add(product);
                }

                return products;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all products.");
                throw;
            }
        }

        // Customer and Product entity classes
        public class CustomerEntity : ITableEntity
        {
            public string PartitionKey { get; set; }
            public string RowKey { get; set; }
            public DateTimeOffset? Timestamp { get; set; }
            public ETag ETag { get; set; }

            public string Name { get; set; }
            public string Email { get; set; }
            public string Address { get; set; }
            public string PhoneNumber { get; set; }
            public string ProfilePictureUrl { get; set; }
            public string UploadPDFUrl { get; set; }
        }

        public class ProductEntity : ITableEntity
        {
            public string PartitionKey { get; set; }
            public string RowKey { get; set; }
            public DateTimeOffset? Timestamp { get; set; }
            public ETag ETag { get; set; }

            public string Name { get; set; }
            public string Description { get; set; }
            public string ImageUrl { get; set; }
            public decimal Price { get; set; }
            public int QuantityInStock { get; set; }
        }

    }
}