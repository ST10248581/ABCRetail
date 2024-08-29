using System.Security.Cryptography;
using ABCRetail.Models;
using ABCRetail.Services;
using Azure.Core;
using Azure.Storage.Files.Shares;
using Microsoft.AspNetCore.Mvc;

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
                if (file.Length > 0)
                {
                    using (var stream = file.OpenReadStream())
                    {
                        await _azureContractFileService.UploadFileAsync(file.FileName, stream);
                    }
                }

                await _azureQueueService.SendMessageAsync("Uploading File...");

                _azureFileService.AppendLogAsync("SystemProcessLogs", $"File Upload: {file.FileName}");

                return RedirectToAction("Loading");
            }
            catch (Exception ex)
            {
                _azureFileService.AppendLogAsync("ErrorLogs", $"Error: {ex.Message}");
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
                var stream = await _azureContractFileService.DownloadFileAsync(fileName);

                _azureFileService.AppendLogAsync("SystemProcessLogs", $"File Download: {fileName}");

                return File(stream, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                _azureFileService.AppendLogAsync("ErrorLogs", $"Error: {ex.Message}");
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Error", "Home");
            }

        }

        private async Task<List<string>> GetFileNamesAsync()
        {
            var fileList = new List<string>();

            var shareClient = new ShareClient(_azureContractFileService.GetConnection(), _azureContractFileService.GetFileShareName());
            var directoryClient = shareClient.GetRootDirectoryClient();


            await foreach (var fileItem in directoryClient.GetFilesAndDirectoriesAsync())
            {
                fileList.Add(fileItem.Name);
            }



            return fileList;
        }

        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }
    }
}
