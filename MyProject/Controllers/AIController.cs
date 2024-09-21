using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SolutionSharingApp.Controllers
{
    [Route("AI")]
    public class AIController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AIController> _logger;

        public AIController(HttpClient httpClient, IConfiguration configuration, ILogger<AIController> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("GetAIResponse")]
        public async Task<IActionResult> GetAIResponse([FromBody] AIRequest request)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("OpenAI API key is missing.");
                return BadRequest("API key is missing.");
            }

            var apiUrl = "https://api.openai.com/v1/completions";
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                prompt = request.Prompt,
                max_tokens = 150
            };

            var content = new StringContent(
                Newtonsoft.Json.JsonConvert.SerializeObject(requestBody),
                Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(apiUrl, content);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"OpenAI API error: {response.StatusCode}, Content: {await response.Content.ReadAsStringAsync()}");
                return StatusCode((int)response.StatusCode, "OpenAI API error");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return Content(responseContent, "application/json");
        }
    }

    public class AIRequest
    {
        public string? Prompt { get; set; }
    }
}
