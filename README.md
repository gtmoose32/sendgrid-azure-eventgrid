# Moosesoft.SendGrid.Azure.EventGrid
[![Build status](https://dev.azure.com/gtmoose/Mathis%20Home/_apis/build/status/SendGrid.Azure.EventGrid%20-%20CICD)](https://dev.azure.com/gtmoose/Mathis%20Home/_build/latest?definitionId=9) 
[![nuget](https://img.shields.io/nuget/v/Moosesoft.SendGrid.Azure.EventGrid.svg)](https://www.nuget.org/packages/Moosesoft.SendGrid.Azure.EventGrid/)
![Nuget](https://img.shields.io/nuget/dt/Moosesoft.SendGrid.Azure.EventGrid)

## What is it?
A library for .NET that converts [Twilio SendGrid delivery and engagment events](https://sendgrid.com/docs/for-developers/tracking-events/event/) into [Azure Event Grid](https://azure.microsoft.com/en-us/services/event-grid/) events, which are published to a topic.

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
        CancellationToken cancellationToken)
    {
        await _eventGridEventPublisher.PublishEventsAsync(request.Body, cancellationToken).ConfigureAwait(false);

        return new OkResult();
    }
}
```
