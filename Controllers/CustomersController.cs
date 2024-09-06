using ComprehensiveStorageApp.Models;
using ComprehensiveStorageApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;
using static ComprehensiveStorageApp.Services.TableStorageService;

namespace ComprehensiveStorageApp.Controllers
{
    public class CustomersController : Controller
    {
        private readonly TableStorageService _tableStorageService;
        private readonly BlobStorageService _blobStorageService;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(TableStorageService tableStorageService, BlobStorageService blobStorageService, ILogger<CustomersController> logger)
        {
            _tableStorageService = tableStorageService;
            _blobStorageService = blobStorageService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var customerEntities = await _tableStorageService.GetAllCustomersAsync();

                var customerViewModels = customerEntities.Select(entity => new CustomerViewModel
                {
                    PartitionKey = entity.PartitionKey,
                    RowKey = entity.RowKey,
                    Name = entity.Name,
                    Email = entity.Email,
                    Address = entity.Address,
                    PhoneNumber = entity.PhoneNumber,
                    ProfilePictureUrl = entity.ProfilePictureUrl
                });

                return View(customerViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving customers.");
                ModelState.AddModelError(string.Empty, "An error occurred while retrieving customers.");
                return View(Enumerable.Empty<CustomerViewModel>());
            }
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CustomerViewModel model)
        {
            model.PartitionKey = "Customers";
            model.RowKey = Guid.NewGuid().ToString();


            try
            {
                if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                {
                    using (var stream = model.ProfilePicture.OpenReadStream())
                    {
                        model.ProfilePictureUrl = await _blobStorageService.UploadBlobAsync(
                            "profile-pictures",
                            model.ProfilePicture.FileName,
                            stream,
                            model.ProfilePicture.ContentType);
                    }
                }

                if (model.UploadPDF != null && model.UploadPDF.Length > 0)
                {
                    using (var stream = model.UploadPDF.OpenReadStream())
                    {
                        model.UploadPDFUrl = await _blobStorageService.UploadBlobAsync(
                            "upload-pdf",
                            model.UploadPDF.FileName,
                            stream,
                            model.UploadPDF.ContentType);
                    }
                }
                Console.WriteLine("+++++++++++++" + model.UploadPDFUrl);

                var customerEntity = new CustomerEntity
                {
                    PartitionKey = model.PartitionKey,
                    RowKey = model.RowKey,
                    Name = model.Name,
                    Email = model.Email,
                    Address = model.Address,
                    PhoneNumber = model.PhoneNumber,
                    ProfilePictureUrl = model.ProfilePictureUrl,
                    UploadPDFUrl = model.UploadPDFUrl
                };
                Console.WriteLine("--------------" + customerEntity.UploadPDFUrl);

                await _tableStorageService.AddCustomerAsync(customerEntity);
                _logger.LogInformation("Customer created successfully.");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the customer.");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the customer. Please try again later.");
                return View(model);
            }
        }






        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            try
            {
                var customer = await _tableStorageService.GetCustomerAsync(partitionKey, rowKey);
                if (customer == null)
                {
                    return NotFound();
                }

                var model = new CustomerViewModel
                {
                    PartitionKey = customer.PartitionKey,
                    RowKey = customer.RowKey,
                    Name = customer.Name,
                    Email = customer.Email,
                    Address = customer.Address,
                    PhoneNumber = customer.PhoneNumber,
                    ProfilePictureUrl = customer.ProfilePictureUrl
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the customer for editing.");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CustomerViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Handle profile picture upload (if a new one is provided)
                    if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                    {
                        using (var stream = model.ProfilePicture.OpenReadStream())
                        {
                            model.ProfilePictureUrl = await _blobStorageService.UploadBlobAsync("profile-pictures",
                                model.ProfilePicture.FileName, stream, model.ProfilePicture.ContentType);
                        }
                    }

                    var customerEntity = new CustomerEntity
                    {
                        PartitionKey = model.PartitionKey,
                        RowKey = model.RowKey,
                        Name = model.Name,
                        Email = model.Email,
                        Address = model.Address,
                        PhoneNumber = model.PhoneNumber,
                        ProfilePictureUrl = model.ProfilePictureUrl
                    };

                    await _tableStorageService.UpdateCustomerAsync(customerEntity);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while updating the customer.");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the customer. Please try again later.");
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            try
            {
                var customer = await _tableStorageService.GetCustomerAsync(partitionKey, rowKey);
                if (customer == null)
                {
                    return NotFound();
                }

                var model = new CustomerViewModel
                {
                    PartitionKey = customer.PartitionKey,
                    RowKey = customer.RowKey,
                    Name = customer.Name,
                    Email = customer.Email,
                    Address = customer.Address,
                    PhoneNumber = customer.PhoneNumber,
                    ProfilePictureUrl = customer.ProfilePictureUrl
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving customer details.");
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            try
            {
                var customer = await _tableStorageService.GetCustomerAsync(partitionKey, rowKey);
                if (customer == null)
                {
                    return NotFound();
                }

                var model = new CustomerViewModel
                {
                    PartitionKey = customer.PartitionKey,
                    RowKey = customer.RowKey,
                    Name = customer.Name,
                    Email = customer.Email
                    // ... other properties
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the customer for deletion.");
                return RedirectToAction("Index");
            }
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
        {
            try
            {
                await _tableStorageService.DeleteCustomerAsync(partitionKey, rowKey);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the customer.");
                return RedirectToAction("Delete", new { partitionKey, rowKey });
            }
        }
    }
}