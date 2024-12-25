using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TestFunction1.Models;

namespace TestFunction1
{
    public class ServiceBusProcessor
    {
        private readonly ILogger<ServiceBusProcessor> _logger;
        private static readonly HttpClient _httpClient = new HttpClient();

        public ServiceBusProcessor(ILogger<ServiceBusProcessor> logger)
        {
            _logger = logger;
        }

        [Function(nameof(ServiceBusProcessor))]
        public async Task Run(
            [ServiceBusTrigger("incoming", Connection = "SFTPServiceBusConnectionString")] CustomerOrderDTO orderDto)
        {
            try
            {
                // Extract API key and endpoint from settings
                string apiKey = orderDto.Settings.ApiKey;
                string endpoint = "https://api.ameriship.com/api/Orders";

                // Serialize the Order object to JSON
                string jsonBody = JsonSerializer.Serialize(orderDto.Order, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });

                // Create the HTTP POST request
                var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
                {
                    Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
                };

                // Add headers
                request.Headers.Add("ApiKey", apiKey);

                // Send the HTTP request
                HttpResponseMessage response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Successfully processed order with Tracking Number: {orderDto.Order.trackingNumber}");
                }
                else
                {
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to process order. StatusCode: {response.StatusCode}, Error: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing message: {ex.Message}");
            }
        }
    }
}
