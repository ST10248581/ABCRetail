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
        private readonly AzureQueueService _azureQueueService;
        private readonly AzureFileService _azureFileService;


        public CustomerProfileController()
        {
            _tableService = new AzureTableService("CustomerProfiles");
            _blobStorageService = new BlobStorageService();
            _azureQueueService = new AzureQueueService("customerprofilequeue");
            _azureFileService = new AzureFileService("systemlogs");
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
                await _azureQueueService.SendMessageAsync("Creating Customer Profile...");

                // Handle file upload and save the profile information to the database
                if (request.ProfilePhoto != null)
                {
                    string containerName = "customerprofileimages"; 
                    var imageUrl = await _blobStorageService.UploadImageAsync(request.ProfilePhoto, containerName, customerId.ToString());
                    await _azureQueueService.SendMessageAsync("Uploading Profile Photo...");                 
                }

                _azureFileService.AppendLogAsync("SystemProcessLogs", $"Customer Profile ({request.FirstName} {request.LastName})  Created");

                return RedirectToAction("Loading");
            }
            catch(Exception ex) 
            {
                _azureFileService.AppendLogAsync("ErrorLogs", $"Error: {ex.Message}");
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Error", "Home");
                
            }

        }

        [HttpGet]
        public async Task<JsonResult> GetQueueMessage()
        {
            string message = await _azureQueueService.ReceiveMessageAsync();
            return new JsonResult(new { message });
        }

        public async Task<ActionResult> Loading()
        {
            return View("Loading");
        }

        public IActionResult ProfileSuccess()
        {
            return View("CustomerProfileCaptureSuccessPage");
        }

        public async Task<ActionResult> ShowAllCustomers()
        {
            try
            {
                var entities = await _tableService.GetAllEntitiesAsync<CustomerProfile>();

                var result = new CustomerProfileListResultModel()
                {
                    Customers = new List<CustomerProfileResultModel>()
                };

                foreach (var entity in entities)
                {
                    var profile = new CustomerProfileResultModel()
                    {
                        FirstName = entity.FirstName,
                        LastName = entity.LastName,
                        Email = entity.Email,
                        Phone = entity.Phone,
                        Address = entity.Address
                    };

                    string imageUrl = _blobStorageService.GetImageUrl("customerprofileimages", entity.PartitionKey);
                    profile.ProfilePhotoURL = imageUrl;

                    result.Customers.Add(profile);
                }

                return View("ViewAllCustomers", result);
            }
            catch (Exception ex)
            {
                _azureFileService.AppendLogAsync("ErrorLogs", $"Error: {ex.Message}");
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Error", "Home");

            }

        }
    }
}
