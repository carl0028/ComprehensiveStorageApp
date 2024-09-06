using ComprehensiveStorageApp.Models;
using ComprehensiveStorageApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static ComprehensiveStorageApp.Services.TableStorageService;

namespace ComprehensiveStorageApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly TableStorageService _tableStorageService;
        private readonly BlobStorageService _blobStorageService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(TableStorageService tableStorageService, BlobStorageService blobStorageService, ILogger<ProductsController> logger)
        {
            _tableStorageService = tableStorageService;
            _blobStorageService = blobStorageService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var productEntities = await _tableStorageService.GetAllProductsAsync();
                var productViewModels = productEntities.Select(entity => new ProductViewModel
                {
                    PartitionKey = entity.PartitionKey,
                    RowKey = entity.RowKey,
                    Name = entity.Name,
                    Description = entity.Description,
                    ImageUrl = entity.ImageUrl
                    // ... map other properties as needed
                });

                return View(productViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving products.");
                ModelState.AddModelError(string.Empty, "An error occurred while retrieving products.");
                return View(Enumerable.Empty<ProductViewModel>());
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductViewModel model, IFormFile imageFile)
        {
            _logger.LogInformation("Product creation form submitted.");

            
                _logger.LogInformation("Model state is valid.");

                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        using (var stream = imageFile.OpenReadStream())
                        {
                            model.ImageUrl = await _blobStorageService.UploadBlobAsync("product-images", imageFile.FileName, stream, imageFile.ContentType);
                        }
                    }

                    var productEntity = new ProductEntity
                    {
                        PartitionKey = "Products",
                        RowKey = Guid.NewGuid().ToString(),
                        Name = model.Name,
                        Description = model.Description,
                        ImageUrl = model.ImageUrl
                        // ... other properties
                    };

                    await _tableStorageService.AddProductAsync(productEntity);

                    _logger.LogInformation("Product created successfully. Redirecting to Index.");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while creating the product.");
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the product. Please try again later.");
                }
            

            return View(model);
        }

        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            try
            {
                var product = await _tableStorageService.GetProductAsync(partitionKey, rowKey);
                if (product == null)
                {
                    return NotFound();
                }

                var model = new ProductViewModel
                {
                    PartitionKey = product.PartitionKey,
                    RowKey = product.RowKey,
                    Name = product.Name,
                    Description = product.Description,
                    ImageUrl = product.ImageUrl
                    // ... other properties
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the product for editing.");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ProductViewModel model, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Only upload a new image if one was selected
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        using (var stream = imageFile.OpenReadStream())
                        {
                            model.ImageUrl = await _blobStorageService.UploadBlobAsync("product-images", imageFile.FileName, stream, imageFile.ContentType);
                        }
                    }

                    var productEntity = new ProductEntity
                    {
                        PartitionKey = model.PartitionKey,
                        RowKey = model.RowKey,
                        Name = model.Name,
                        Description = model.Description,
                        ImageUrl = model.ImageUrl
                        // ... other properties
                    };

                    await _tableStorageService.UpdateProductAsync(productEntity);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while updating the product.");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the product. Please try again later.");
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            try
            {
                var product = await _tableStorageService.GetProductAsync(partitionKey, rowKey);
                if (product == null)
                {
                    return NotFound();
                }

                var model = new ProductViewModel
                {
                    PartitionKey = product.PartitionKey,
                    RowKey = product.RowKey,
                    Name = product.Name,
                    Description = product.Description,
                    ImageUrl = product.ImageUrl
                    // ... other properties
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving product details.");
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            try
            {
                var product = await _tableStorageService.GetProductAsync(partitionKey, rowKey);
                if (product == null)
                {
                    return NotFound();
                }

                var model = new ProductViewModel
                {
                    PartitionKey = product.PartitionKey,
                    RowKey = product.RowKey,
                    Name = product.Name,
                    Description = product.Description,
                    ImageUrl = product.ImageUrl
                    // ... other properties
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the product for deletion.");
                return RedirectToAction("Index");
            }
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            try
            {
                await _tableStorageService.DeleteProductAsync(partitionKey, rowKey);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the product.");
                return RedirectToAction("Delete", new { partitionKey, rowKey });
            }
        }
    }
}