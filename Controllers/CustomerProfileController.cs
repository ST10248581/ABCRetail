using System.Reflection;
using ABCRetail.Entities;

using ABCRetail.Models;
using ABCRetail.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetail.Controllers
{
    public class CustomerProfileController : Controller
    {
        private readonly AzureTableService _tableService;
        private readonly BlobStorageService _blobStorageService;

        public CustomerProfileController()
        {
            _tableService = new AzureTableService("CustomerProfiles");
            _blobStorageService = new BlobStorageService();
        }

        [HttpGet]
        public IActionResult CreateProfile()
        {
            return View("CustomerProfileCapturePage");
        }

        [HttpPost]
        public async Task<ActionResult> CreateProfile(CustomerProfileRequest request)
        {
            try
            {
                var customerId = Guid.NewGuid();

                CustomerProfile customer = new CustomerProfile(customerId.ToString(), request.LastName)
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Address = request.Address,
                    Phone = request.Phone
                };

                await _tableService.InsertOrMergeEntityAsync(customer);

                // Handle file upload and save the profile information to the database
                if (request.ProfilePhoto != null)
                {
                    string containerName = "customerprofileimages"; 
                    var imageUrl = await _blobStorageService.UploadImageAsync(request.ProfilePhoto, containerName, customerId.ToString());
                    
                }

                return RedirectToAction("ProfileSuccess");
            }
            catch(Exception ex) 
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Error", "Home");
                
            }

        }

        public IActionResult ProfileSuccess()
        {
            return View("CustomerProfileCaptureSuccessPage");
        }
    }
}
