# Moosesoft.SendGrid.Azure.EventGrid
[![Build status](https://dev.azure.com/gtmoose/Mathis%20Home/_apis/build/status/SendGrid.Azure.EventGrid%20-%20CICD)](https://dev.azure.com/gtmoose/Mathis%20Home/_build/latest?definitionId=9) [![nuget](https://img.shields.io/nuget/v/Moosesoft.SendGrid.Azure.EventGrid.svg)](https://www.nuget.org/packages/Moosesoft.SendGrid.Azure.EventGrid/)

## What is it?
A library for .NET that converts [Twilio SendGrid delivery and engagment events](https://sendgrid.com/docs/for-developers/tracking-events/event/) into [EventGrid](https://azure.microsoft.com/en-us/services/event-grid/) events.

## Installing Moosesoft.SendGrid.Azure.EventGrid
```
dotnet add package Moosesoft.SendGrid.Azure.EventGrid
```

## Azure Function Sample
Working version of the sample code below is found [here](https://github.com/gtmoose32/sendgrid-azure-eventgrid/tree/master/samples).  Note:  The Azure Event Grid topic configuration settings in the sample are fakes.  

```C#
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
```
