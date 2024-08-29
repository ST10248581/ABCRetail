using ABCRetail.Entities;
using ABCRetail.Models;
using ABCRetail.Services;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetail.Controllers
{
    public class ProductInformationController : Controller
    {
        private readonly AzureTableService _tableService;
        private readonly BlobStorageService _blobStorageService;
        private readonly AzureQueueService _azureQueueService;
        private readonly AzureFileService _azureFileService;

        public ProductInformationController()
        {
            _tableService = new AzureTableService("Products");
            _blobStorageService = new BlobStorageService();
            _azureQueueService = new AzureQueueService("productinformationqueue");
            _azureFileService = new AzureFileService("systemlogs");
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("ProductInformationCapturePage");
        }

        [HttpPost]
        public async Task<ActionResult> Index(ProductInformationRequest request)
        {
            try
            {
                var productId = Guid.NewGuid();

                Product product = new Product(productId.ToString(), request.Name)
                {
                    Name = request.Name,
                    Description = request.Description,
                    Price = request.Price,
                    Stock = request.Stock,
                    Category = request.Category,
                   
                };

                await _tableService.InsertOrMergeEntityAsync(product);
                await _azureQueueService.SendMessageAsync("Creating Product...");

                if (request.ProductPhoto != null)
                {
                    await _azureQueueService.SendMessageAsync("Uploading Product Photo...");
                    string containerName = "productimages";
                    var imageUrl = await _blobStorageService.UploadImageAsync(request.ProductPhoto, containerName, productId.ToString());

                }

                _azureFileService.AppendLogAsync("SystemProcessLogs", $"Product Created: {product.Name}");
                return RedirectToAction("Loading");
            }
            catch (Exception ex)
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

        [HttpGet]
        public IActionResult ProductCaptureSuccess()
        {
            return View("ProductCaptureSuccess");
        }

        public async Task<ActionResult> ShowAllProducts()
        {
            try
            {
                var entities = await _tableService.GetAllEntitiesAsync<Product>();

                var result = new ProductListResultModel()
                {
                    Products = new List<ProductInformationResultModel>()
                };

                foreach (var entity in entities)
                {
                    var profile = new ProductInformationResultModel()
                    {
                        ProductID = entity.PartitionKey,
                        Name = entity.Name,
                        Description = entity.Description,
                        Price = entity.Price,
                        Stock = entity.Stock,
                        Category = entity.Category,
                    };

                    string imageUrl = _blobStorageService.GetImageUrl("productimages", entity.PartitionKey);
                    profile.ProfilePhotoURL = imageUrl;

                    result.Products.Add(profile);
                }

                return View("ViewAllProducts", result);
            }
            catch(Exception ex)
            {
                _azureFileService.AppendLogAsync("ErrorLogs", $"Error: {ex.Message}");
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Error", "Home");

            }
            

        }

        public async Task<ActionResult> ProcessOrder(string productId, string productName)
        {
            try
            {
				var product = await _tableService.RetrieveProductEntityAsync(productId, productName);
				await _azureQueueService.SendMessageAsync("Fetching Product Information...");

				if (product == null) throw new Exception("Product could not be found.");

                if (product.Stock <= 0) throw new Exception("Product out of stock.");

				product.Stock--;

				await _azureQueueService.SendMessageAsync("Updating Inventory...");

				_tableService.InsertOrMergeEntityAsync(product);

				await _azureQueueService.SendMessageAsync("Processing Order...");

                _azureFileService.AppendLogAsync("SystemProcessLogs", $"OrderProcessed: {productName}");

                var url = Url.Action("ProcessOrderLoading");
                return Json(new { redirectUrl = url });
			}
            catch(Exception ex)
            {
                _azureFileService.AppendLogAsync("ErrorLogs", $"Error: {ex.Message}");
                TempData["ErrorMessage"] = ex.Message;
				return RedirectToAction("Error", "Home");
			}
            
		}

        public async Task<ActionResult> ProcessOrderLoading()
        {
            return View("ProcessOrder");
        }
    }
}
