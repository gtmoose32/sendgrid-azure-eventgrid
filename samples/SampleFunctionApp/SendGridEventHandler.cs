using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Moosesoft.SendGrid.Azure.EventGrid;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace SampleFunctionApp;

[ExcludeFromCodeCoverage]
public class SendGridEventHandler
{
    private readonly IEventGridEventPublisher _eventGridEventPublisher;

    public SendGridEventHandler(IEventGridEventPublisher eventGridEventPublisher)
    {
        _eventGridEventPublisher = eventGridEventPublisher ?? throw new ArgumentNullException(nameof(eventGridEventPublisher));
    }

    [FunctionName(nameof(HandleSendGridEventsAsync))]
    public async Task<IActionResult> HandleSendGridEventsAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest request,
        ILogger log)
    {
        string json;
        using (var reader = new StreamReader(request.Body))
        {
            json = await reader.ReadToEndAsync().ConfigureAwait(false);
        }

        log.LogInformation(json);

        await _eventGridEventPublisher.PublishEventsAsync(json).ConfigureAwait(false);

        return new OkResult();
    }
}