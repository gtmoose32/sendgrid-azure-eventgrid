using Azure;
using Azure.Messaging.EventGrid;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moosesoft.SendGrid.Azure.EventGrid;
using SampleFunctionApp;
using System;
using System.Diagnostics.CodeAnalysis;

[assembly: FunctionsStartup(typeof(Startup))]
namespace SampleFunctionApp;

[ExcludeFromCodeCoverage]
public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services
            .AddSingleton(provider =>
                new EventGridPublisherClient(
                    new Uri(provider.GetRequiredService<IConfiguration>()["TopicEndpointUri"]),
                    new AzureKeyCredential(provider.GetRequiredService<IConfiguration>()["TopicKey"])))
            .AddSingleton(EventGridEventPublisherSettings.Default)
            .AddSingleton<IEventGridEventPublisher, EventGridEventPublisher>();
    }
}