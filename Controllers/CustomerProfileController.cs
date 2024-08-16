using System.Reflection;
using ABCRetail.Models;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetail.Controllers
{
    public class CustomerProfileController : Controller
    {
        [HttpGet]
        public IActionResult CreateProfile()
        {
            return View("CustomerProfileCapturePage");
        }

        [HttpPost]
        public IActionResult CreateProfile(CustomerProfileRequest request)
        {

            // Handle file upload and save the profile information to the database
            var fileName = Path.GetFileName(request.ProfilePhoto.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                request.ProfilePhoto.CopyTo(stream);
            }


            return RedirectToAction("ProfileSuccess"); // Redirect to a success page or action

        }

        public IActionResult ProfileSuccess()
        {
            return View("CustomerProfileCaptureSuccessPage");
        }
    }
}
