﻿using System.Net.Http;
using System.Text;
using ABCRetail.Entities;
using ABCRetail.Models;
using ABCRetail.Services;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ABCRetail.Controllers
{
    public class ProductInformationController : Controller
    {
        private readonly AzureQueueService _azureQueueService;

        public ProductInformationController()
        {
            _azureQueueService = new AzureQueueService("productinformationqueue");
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

                var product = new
                {
                    Name = request.Name,
                    Description = request.Description,
                    Price = request.Price,
                    Stock = request.Stock,
                    Category = request.Category,
                    ProductPhoto = request.ProductPhoto != null ? Convert.ToBase64String(GetBytesFromFile(request.ProductPhoto)) : ""
                };

                string jsonData = JsonConvert.SerializeObject(product);


                using (HttpClient client = new HttpClient())
                {
                    var functionUrl = "https://abcretailstoragefunctions.azurewebsites.net/api/CreateProduct?code=cHqA3YmrT1knrrmw0UXsjD4mtKin6dH08sVkhUWB_d6KAzFu-H93Cg%3D%3D";
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
            catch (Exception ex)
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

        [HttpGet]
        public IActionResult ProductCaptureSuccess()
        {
            return View("ProductCaptureSuccess");
        }

        public async Task<ActionResult> ShowAllProducts()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    
                    string functionUrl = "https://abcretailstoragefunctions.azurewebsites.net/api/GetAllProducts?code=cHqA3YmrT1knrrmw0UXsjD4mtKin6dH08sVkhUWB_d6KAzFu-H93Cg%3D%3D";

                    HttpResponseMessage response = await httpClient.GetAsync(functionUrl);

                    if (!response.IsSuccessStatusCode) throw new Exception("Failed to retrieve products.");

                    var result = await response.Content.ReadFromJsonAsync<ProductListResultModel>();

                    return View("ViewAllProducts", result);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Error", "Home");

            }


        }

        public IActionResult ProcessOrder(string aProductId, string category, string description, string name, int stock, double price)
        {
            try
            {
                using(var dm = new DataModel())
                {
                    Models.Product prod = new Models.Product()
                    {
                        ProductCategory = category,
                        ProductDescription = description,
                        ProductName = name,
                        ProductStock = stock,
                        ProductPrice = (decimal)price
                    };

                    dm.Products.Add(prod);
                    dm.SaveChanges();

                    TempData["ProductId"] = prod.ProductId;
                    TempData["ProductPrice"] = prod.ProductPrice;

                    TempData["AzureProductId"] = aProductId;
                    TempData["AzureProducName"] = name;
                }
                             
                var url = Url.Action("SelectCustomer");
                return Json(new { redirectUrl = url });
            }
            catch(Exception error)
            {
                TempData["ErrorMessage"] = error.Message;
                var url = Url.Action("Error", "Home");
                return Json(new { redirectUrl = url });
            }
        }

        public async Task<ActionResult> ProcessCustomerOrder(string fullName, string email, string phoneNumber)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var productIdString = TempData["ProductId"] as string;
                    if (string.IsNullOrEmpty(productIdString)) throw new Exception("Product could not be found.");
                    var productId = int.Parse(productIdString);

                    var productPriceString = TempData["ProductPrice"] as string;
                    if (string.IsNullOrEmpty(productPriceString)) throw new Exception("Product price could not be found.");                   
					var productPrice = decimal.Parse(productPriceString);

					using (var dm = new DataModel())
                    {
						Models.Customer customer = new Models.Customer()
						{
							CustomerFullName = fullName,
							CustomerEmail = email,
							CustomerPhoneNumber = phoneNumber
						};

                        var order = new Order()
                        {
                            OrderDate = DateTime.Now,
                            TotalAmount = productPrice,
                            ShippingAddress = "Unknown",
                            OrderStatus = "Processing",
                            PaymentMethod = "EFT"
                        };

						customer.Orders.Add(order);

						order.OrderItems.Add(new OrderItem()
                        {
                            ProductId = productId,
                            Quantity = 1,
                            UnitPrice = productPrice,
                        });

						
					}
                    
                    var azureProductId = TempData["AzureProductId"] as string;
                    var productName = TempData["AzureProducName"] as string;

                    if (string.IsNullOrEmpty(azureProductId) || string.IsNullOrEmpty(productName)) throw new Exception("Product could not be found");

					string functionUrl = "https://abcretailstoragefunctions.azurewebsites.net/api/ProcessOrder?code=cHqA3YmrT1knrrmw0UXsjD4mtKin6dH08sVkhUWB_d6KAzFu-H93Cg%3D%3D";

                    var data = new
                    {
                        ProductId = productId,
                        ProductName = productName
                    };

                    string jsonData = JsonConvert.SerializeObject(data);

                    HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await httpClient.PostAsync(functionUrl, content);

                    if (!response.IsSuccessStatusCode) throw new Exception("Failed to process product order.");

                    var url = Url.Action("ProcessOrderLoading");
                    return Json(new { redirectUrl = url });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Error", "Home");
            }

        }

        public async Task<ActionResult> ProcessOrderLoading()
        {
            return View("ProcessOrder");
        }

        private byte[] GetBytesFromFile(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public async Task<IActionResult> SelectCustomer()
        {
            using (var httpClient = new HttpClient())
            {
                string functionUrl = "https://abcretailstoragefunctions.azurewebsites.net/api/GetAllCustomers?code=cHqA3YmrT1knrrmw0UXsjD4mtKin6dH08sVkhUWB_d6KAzFu-H93Cg%3D%3D";


                HttpResponseMessage response = await httpClient.GetAsync(functionUrl);

                if (!response.IsSuccessStatusCode) throw new Exception("Failed to retrieve customer profiles.");


                var result = await response.Content.ReadFromJsonAsync<CustomerProfileListResultModel>();

                return View("SelectCustomer", result);
            }
        }

            
    }
}
