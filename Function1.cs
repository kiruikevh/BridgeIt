using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FunctionApp
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;
        private readonly HttpClient _httpClient;

        public Function1(ILogger<Function1> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        [Function("Function1")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
        {
            string method = req.Method.ToUpper();
            _logger.LogInformation($"HTTP {method} trigger invoked.");

            try
            {
                if (method == "GET")
                {
                    // Optional: get post id from query string (?id=1)
                    string postId = req.Query["id"];
                    postId = string.IsNullOrEmpty(postId) ? "1" : postId;

                    string getUrl = $"https://jsonplaceholder.typicode.com/posts/{postId}";
                    var response = await _httpClient.GetAsync(getUrl);
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    var json = JsonDocument.Parse(content);
                    return new OkObjectResult(json.RootElement);
                }
                else if (method == "POST")
                {
                    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                    var jsonContent = new StringContent(requestBody, Encoding.UTF8, "application/json");

                    string postUrl = "https://jsonplaceholder.typicode.com/posts";
                    var response = await _httpClient.PostAsync(postUrl, jsonContent);
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    var json = JsonDocument.Parse(content);
                    return new OkObjectResult(json.RootElement);
                }
                else
                {
                    return new BadRequestObjectResult("Unsupported HTTP method.");
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP request error: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }
    }



    public class MyRequestModel
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

}
