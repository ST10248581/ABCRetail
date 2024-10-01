using System.Security.Cryptography;
using System.Text;
using ABCRetail.Models;
using ABCRetail.Services;
using Azure.Core;
using Azure.Storage.Files.Shares;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ABCRetail.Controllers
{
    public class BusinessContractController : Controller
    {
        private readonly AzureFileService _azureContractFileService;
        private readonly AzureFileService _azureFileService;
        private readonly AzureQueueService _azureQueueService;

        public BusinessContractController()
        {
            _azureContractFileService = new AzureFileService("businesscontracts");
            _azureQueueService = new AzureQueueService("fileupload");
            _azureFileService = new AzureFileService("systemlogs");
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("UploadBusinessContract");
        }

        [HttpPost]
        public async Task<ActionResult> Upload(IFormFile file)
        {
            try
            {
                if (file == null) throw new Exception("File cannot be empty please upload a file.");

                using (var content = new MultipartFormDataContent())
                {
                    var streamContent = new StreamContent(file.OpenReadStream());
                    content.Add(streamContent, "file", file.FileName);

                    using (var client = new HttpClient())
                    {
                        var response = await client.PostAsync("https://abcretailstoragefunctions.azurewebsites.net/api/UploadFile?code=cHqA3YmrT1knrrmw0UXsjD4mtKin6dH08sVkhUWB_d6KAzFu-H93Cg%3D%3D", content);
                        response.EnsureSuccessStatusCode();
                    }
                }

                return RedirectToAction("Loading");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Error", "Home");
            }

        }

        public async Task<ActionResult> Loading()
        {
            return View("Loading");
        }


        [HttpGet]
        public async Task<JsonResult> GetQueueMessage()
        {
            string message = await _azureQueueService.ReceiveMessageAsync();
            return new JsonResult(new { message });
        }


        [HttpGet]
        public async Task<IActionResult> Download()
        {
            try
            {
                // Assuming you have a method to list files
                var fileNames = await GetFileNamesAsync();

                return View("DownloadBusinessContract", new FileNameListModel { FileNames = fileNames });
            }
            catch (Exception ex)
            {
                _azureFileService.AppendLogAsync("ErrorLogs", $"Error: {ex.Message}");
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Error", "Home");
            }

        }

        [HttpGet]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            try
            {
                using (var client = new HttpClient())
                {

                    var data = new
                    {
                        FileName = fileName
                    };

                    string jsonData = JsonConvert.SerializeObject(data);

                    HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("https://abcretailstoragefunctions.azurewebsites.net/api/downloadfile?code=cHqA3YmrT1knrrmw0UXsjD4mtKin6dH08sVkhUWB_d6KAzFu-H93Cg%3D%3D", content);
                    response.EnsureSuccessStatusCode();

                    var stream = await response.Content.ReadAsStreamAsync();

                    return File(stream, "application/octet-stream", fileName);
                }  
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Error", "Home");
            }

        }

        private async Task<List<string>> GetFileNamesAsync()
        {
            using(var httpClient = new HttpClient())
            {

                var functionUrl = "https://abcretailstoragefunctions.azurewebsites.net/api/GetAllFiles?code=cHqA3YmrT1knrrmw0UXsjD4mtKin6dH08sVkhUWB_d6KAzFu-H93Cg%3D%3D";

                HttpResponseMessage response = await httpClient.GetAsync(functionUrl);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                var fileNames = JsonConvert.DeserializeObject<List<string>>(responseBody);

                return fileNames;
            }
        }

        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }
    }
}
