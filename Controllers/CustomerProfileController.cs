using System.Net.Http;
using System.Reflection;
using System.Text;
using ABCRetail.Entities;

using ABCRetail.Models;
using ABCRetail.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ABCRetail.Controllers
{
    public class CustomerProfileController : Controller
    {
        private readonly AzureQueueService _azureQueueService;

        public CustomerProfileController()
        {
            _azureQueueService = new AzureQueueService("customerprofilequeue");
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

                var data = new
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Address = request.Address,
                    Phone = request.Phone,
                    ProfilePhoto = request.ProfilePhoto != null ? Convert.ToBase64String(GetBytesFromFile(request.ProfilePhoto)) : ""
                };

                string jsonData = JsonConvert.SerializeObject(data);

                using (HttpClient client = new HttpClient())
                {
                    var functionUrl = "https://abcretailstoragefunctions.azurewebsites.net/api/CreateProfile?code=cHqA3YmrT1knrrmw0UXsjD4mtKin6dH08sVkhUWB_d6KAzFu-H93Cg%3D%3D";
                    HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(functionUrl, content);

                    if (!response.IsSuccessStatusCode)
                    {
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        throw new Exception($"Error: {response.StatusCode}, Details: {errorMessage}");
                    }
                }

                return RedirectToAction("Loading");
            }
            catch(Exception ex) 
            {
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
                using(var httpClient = new HttpClient())
                {
                    string functionUrl = "https://abcretailstoragefunctions.azurewebsites.net/api/GetAllCustomers?code=cHqA3YmrT1knrrmw0UXsjD4mtKin6dH08sVkhUWB_d6KAzFu-H93Cg%3D%3D";

                    
                    HttpResponseMessage response = await httpClient.GetAsync(functionUrl);

                    if (!response.IsSuccessStatusCode) throw new Exception("Failed to retrieve customer profiles.");
                      
                    
                    var result = await response.Content.ReadFromJsonAsync<CustomerProfileListResultModel>();
                    return View("ViewAllCustomers", result);
                }              

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Error", "Home");

            }

        }

        private byte[] GetBytesFromFile(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
