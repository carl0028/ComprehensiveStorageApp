using ComprehensiveStorageApp.Models;
using ComprehensiveStorageApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ComprehensiveStorageApp.Controllers
{
    public class OrdersController : Controller
    {
        private readonly QueueStorageService _queueStorageService;

        public OrdersController(QueueStorageService queueStorageService)
        {
            _queueStorageService = queueStorageService;
        }

        // Action to process an order (triggered by user interaction)
        public async Task<IActionResult> ProcessOrder(string orderData) // Adjust parameters as needed
        {
            try
            {
                await _queueStorageService.EnqueueMessageAsync("order-processing", orderData);
                TempData["SuccessMessage"] = "Order placed successfully. It will be processed soon.";
                return RedirectToAction("Index", "Home"); // Or redirect to an appropriate view
            }
            catch (Exception ex)
            {
                // Log the exception or display an error message to the user
                TempData["ErrorMessage"] = "An error occurred while placing the order. Please try again later.";
                return RedirectToAction("Index", "Home"); // Or redirect to an appropriate view
            }
        }
    }
}