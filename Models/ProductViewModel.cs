using Microsoft.AspNetCore.Http;

namespace ComprehensiveStorageApp.Models
{
    public class ProductViewModel
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int QuantityInStock { get; set; }
        public string ImageUrl { get; set; }
        public IFormFile imageFile { get; set; } // Add this property
        // ... other product properties as needed
    }
}