using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ComprehensiveStorageApp.Models
{
    public class CustomerViewModel
    {
        // PartitionKey and RowKey are not required in the view model
        // They are set in the controller or service layer

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }

        [Display(Name = "Full Name")]
        public string Name { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string Address { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        public IFormFile ProfilePicture { get; set; }

        public string ProfilePictureUrl { get; set; } // This is optional and should not affect validation

        public IFormFile UploadPDF { get; set; }
        public string UploadPDFUrl { get; set; } // This is optional and should not affect validation
    }
}
