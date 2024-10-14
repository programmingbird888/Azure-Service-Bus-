using Azure.Messaging.ServiceBus;
using DwollaProcessor.DwollaFunction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Configuration;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography.Xml;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace DwollaProcessor.Functions
{
    public class DwollaProcessorFunction
    {
        private readonly ILogger<DwollaProcessorFunction> _logger;
        //private readonly ServiceBusClient _serviceBusClient;
        //private readonly string _queueName;
        //private readonly string _expectedApiKey;
        private readonly HelperClass _helperClass;

        public DwollaProcessorFunction(ILogger<DwollaProcessorFunction> logger, HelperClass helperClass)
        {
            _logger = logger;
            //_serviceBusClient = serviceBusClient;
            //_queueName = configuration.GetValue<string>("ServiceBusQueueName") ?? "ServiceBusQueueName";
            //_expectedApiKey = configuration.GetValue<string>("ExpectedApiKey") ?? "ExpectedApiKey";
            _helperClass = helperClass;
        }

        [Function("DwollaEventProcessor")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("HTTP trigger function processed a request.");
            try
            {
                if (!_helperClass.IsApiKeyValid(req))
                {
                    return await _helperClass.GetUnauthorisedResponse(req, "Unauthorized Access.");
                }
                string requestBody = await _helperClass.ReadRequestBody(req);
                if (!_helperClass.IsValidBody(requestBody))
                {
                    return await _helperClass.GetInternalServerErrorResponse(req, "Invalid request body.");
                }
                return await _helperClass.ProcessAndSendMessage(req, requestBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Message failed.");
                return await _helperClass.GetInternalServerErrorResponse(req, "Message failed to sent.");
            }
            //return await _helperClass.GetOkResponse(req, "Message Sent.");

            //_logger.LogInformation("HTTP trigger function processed a request.");
            //_logger.LogInformation("Authenticating the API key.");

            //if (!req.Headers.TryGetValues(_expectedApiKey, out var apiKey) || apiKey.Equals(_expectedApiKey))
            //{
            //    _logger.LogWarning("Unauthorized access attempt.");
            //    _logger.LogInformation("Authentication failed.");
            //    return await HelperClass.GetUnauthorisedResponse(req, "Authentication failed.");
            //}

            //_logger.LogInformation("Authentication successful.");
            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //_logger.LogInformation("Validation of request body is processed.");

            //if (!HelperClass.IsValidBody(requestBody))
            //{
            //    _logger.LogError("Invalid message format received or message body is null.");
            //    _logger.LogInformation("Validation of request body failed.");
            //    return await HelperClass.GetInternalErrorResponse(req, "Message not found in expected format.");
            //}

            //_logger.LogInformation("Message received in JSON format and processed.");

            //try
            //{
            //    DwollaWebHookEventClass dwollaEvent = new DwollaWebHookEventClass();
            //    JsonDocument doc = JsonDocument.Parse(requestBody);
            //    var root = doc.RootElement;
            //    dwollaEvent.EventType = root.GetProperty("eventType").ToString();
            //    dwollaEvent.Payload = root.GetProperty("payload");

            //    ServiceBusSender sender = _serviceBusClient.CreateSender(_queueName);
            //    var message = new ServiceBusMessage(requestBody)
            //    {
            //        MessageId = Guid.NewGuid().ToString(),
            //        SessionId = Guid.NewGuid().ToString(),
            //    };
            //    message.ApplicationProperties.Add("EventType", dwollaEvent.EventType);
            //    await sender.SendMessageAsync(message);
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "Internal Server Error.");
            //    return await HelperClass.GetInternalErrorResponse(req, "Failed to send message.");
            //}

            //_logger.LogInformation("Message Received.");
            //return await HelperClass.GetOkResponse(req, "Message sent successfully.");
        }
    }
}
