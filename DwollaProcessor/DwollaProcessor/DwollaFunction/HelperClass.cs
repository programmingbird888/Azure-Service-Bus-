using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DwollaProcessor.DwollaFunction
{
    public class HelperClass
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ILogger<HelperClass> _logger;
        private readonly string _queueName;
        private readonly string _expectedApiKey;

        public HelperClass(ServiceBusClient serviceBusClient, IConfiguration configuration, ILogger<HelperClass> logger)
        {
            _serviceBusClient = serviceBusClient;
            _queueName = configuration.GetValue<string>("ServiceBusQueueName") ?? "ServiceBusQueueName";
            _expectedApiKey = configuration.GetValue<string>("ExpectedApiKey") ?? "ExpectedApiKey";
            _logger = logger;
        }
        public bool IsApiKeyValid(HttpRequestData req)
        {
            _logger.LogInformation("Authenticating the API key.");
            if (!req.Headers.TryGetValues(_expectedApiKey, out var apiKeyValues) || apiKeyValues.Equals(_expectedApiKey))
            {
                _logger.LogWarning("Unauthorized access attempt.");
                return false;
            }

            _logger.LogInformation("Authentication successful.");
            return true;
        }
        public async Task<string> ReadRequestBody(HttpRequestData req)
        {
            _logger.LogInformation("Reading request body.");
            return await new StreamReader(req.Body).ReadToEndAsync();
        }
        public async Task<HttpResponseData> ProcessAndSendMessage(HttpRequestData req, string requestBody)
        {
            try
            {
                DwollaWebHookEventClass dwollaEvent = MapRequestToDwollaEventClass(requestBody);
                await SendMessageToServiceBus(requestBody, dwollaEvent);
                return await GetOkResponse(req, "Message sent successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error");
                return await GetInternalServerErrorResponse(req, "Failed to send message.");
            }

        }
        public DwollaWebHookEventClass MapRequestToDwollaEventClass(string requestBody)
        {
            JsonDocument doc = JsonDocument.Parse(requestBody);
            var root = doc.RootElement;
            return new DwollaWebHookEventClass
            {
                EventType = root.GetProperty("eventType").ToString(),
                Payload = root.GetProperty("payload")
            };
        }
        public async Task SendMessageToServiceBus(string requestBody, DwollaWebHookEventClass dwollaEvent)
        {
            ServiceBusSender sender = _serviceBusClient.CreateSender(_queueName);
            var message = new ServiceBusMessage(requestBody)
            {
                ContentType = "application/json",
                MessageId = Guid.NewGuid().ToString()
            };
            message.ApplicationProperties.Add("eventType", dwollaEvent.EventType);
            await sender.SendMessageAsync(message);
        }
        public bool IsValidBody(string json)
        {
            try
            {
                JsonDocument.Parse(json);
                return true;
            }
            catch(JsonException)
            {
                return false;
            }
        }
        public async Task<HttpResponseData> GetUnauthorisedResponse(HttpRequestData req, string errorResponse)
        {
            var response = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
            await response.WriteStringAsync(errorResponse);
            return response;
        }

        public async Task<HttpResponseData> GetInternalServerErrorResponse(HttpRequestData req, string errorResponse)
        {
            var response = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
            await response.WriteStringAsync(errorResponse);
            return response;
        }

        public async Task<HttpResponseData> GetOkResponse(HttpRequestData req, string okResponse)
        {
            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteStringAsync(okResponse);
            return response;
        }
    }
}
