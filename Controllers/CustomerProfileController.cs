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

        public CustomerProfileController()
        {
            _tableService = new AzureTableService("CustomerProfiles");
        }

        [HttpGet]
        public IActionResult CreateProfile()
        {
            return View("CustomerProfileCapturePage");
        }

        [HttpPost]
        public async Task<ActionResult> CreateProfile(CustomerProfileRequest request)
        {

            CustomerProfile customer = new CustomerProfile(Guid.NewGuid().ToString(), request.LastName)
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Address = request.Address,  
                Phone = request.Phone
            };

            await _tableService.InsertOrMergeEntityAsync(customer);

            // Handle file upload and save the profile information to the database
            if(request.ProfilePhoto!=null) 
            {
                var fileName = Path.GetFileName(request.ProfilePhoto.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    request.ProfilePhoto.CopyTo(stream);
                }
            }
            
            return RedirectToAction("ProfileSuccess"); // Redirect to a success page or action

        }

        public IActionResult ProfileSuccess()
        {
            return View("CustomerProfileCaptureSuccessPage");
        }
    }
}
