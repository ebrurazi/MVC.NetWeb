using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SolutionSharingApp.Services
{
    public class NLPService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly ILogger<NLPService> _logger;

        public NLPService(HttpClient httpClient, IConfiguration configuration, ILogger<NLPService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _baseUrl = configuration.GetSection("NLPService")["BaseUrl"] ?? throw new ArgumentNullException(nameof(_baseUrl), "Base URL for NLPService cannot be null or empty.");
        }

        public async Task<(string[] Keywords, float[] Vector)> GetKeywordsAndVectorAsync(string text)
        {
            _logger.LogInformation("Sending request to NLPService with text: {Text}", text);

            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/get_keywords_and_vector", new { text });
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<NLPResponse>();

            if (result == null || result.Keywords == null || result.Vector == null)
            {
                _logger.LogError("NLP API response was null or incomplete.");
                return (Array.Empty<string>(), Array.Empty<float>());
            }

            _logger.LogInformation("Received keywords: {Keywords}, vector: {Vector}", string.Join(", ", result.Keywords), result.Vector);
            return (result.Keywords, result.Vector);
        }

        private class NLPResponse
        {
            public string[]? Keywords { get; set; }
            public float[]? Vector { get; set; }
        }
    }
}
