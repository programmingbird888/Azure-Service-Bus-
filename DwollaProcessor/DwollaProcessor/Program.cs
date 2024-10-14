using Azure.Identity;
using Azure.Messaging.ServiceBus;
using DwollaProcessor.DwollaFunction;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using System.Text.Json.Serialization;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddSingleton<HelperClass>();
        services.AddSingleton<DwollaWebHookEventClass>();
        services.AddLogging();
        services.AddSingleton<ServiceBusClient>(provider =>
        {
            var configuration = provider.GetService<IConfiguration>();
            string connectionString = configuration.GetValue<string>("ServiceBusConnectionString") ?? "ServiceBus initials not found.";

            //return new ServiceBusClient(connectionString, new VisualStudioCredential(), new ServiceBusClientOptions
            //{
            //    TransportType = ServiceBusTransportType.AmqpWebSockets
            //});
            return new ServiceBusClient(connectionString);
        });
    })
    .Build();

host.Run();
