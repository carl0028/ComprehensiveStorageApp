using ComprehensiveStorageApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ComprehensiveStorageApp.Controllers
{
    public class DocumentsController : Controller
    {
        private readonly FileStorageService _fileStorageService;

        public DocumentsController(FileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        [HttpPost]
        public async Task<IActionResult> UploadContract(IFormFile contractFile)
        {
            if (contractFile != null && contractFile.Length > 0)
            {
                try
                {
                    using (var stream = contractFile.OpenReadStream())
                    {
                        await _fileStorageService.UploadFileAsync("contracts", "customer-contracts", contractFile.FileName, stream);
                    }
                    TempData["SuccessMessage"] = "Contract uploaded successfully.";
                }
                catch (Exception ex)
                {
                    // Log the exception
                    TempData["ErrorMessage"] = "An error occurred while uploading the contract. Please try again later.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Please select a contract file to upload.";
            }

            return RedirectToAction("Index", "Home"); // Or redirect to an appropriate view
        }

        public async Task<IActionResult> DownloadContract(string fileName)
        {
            var memoryStream = new MemoryStream();
            try
            {
                await _fileStorageService.DownloadFileAsync("contracts", "customer-contracts", fileName, memoryStream);
                memoryStream.Position = 0;
                return File(memoryStream, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                // Log the exception 
                TempData["ErrorMessage"] = "An error occurred while downloading the contract. Please try again later.";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> DeleteContract(string fileName)
        {
            try
            {
                await _fileStorageService.DeleteFileAsync("contracts", "customer-contracts", fileName);
                TempData["SuccessMessage"] = "Contract deleted successfully.";
            }
            catch (Exception ex)
            {
                // Log the exception
                TempData["ErrorMessage"] = "An error occurred while deleting the contract. Please try again later.";
            }

            return RedirectToAction("Index", "Home");
        }
    }
}