# Moosesoft.SendGrid.Azure.EventGrid
[![Build status](https://dev.azure.com/gtmoose/Mathis%20Home/_apis/build/status/SendGrid.Azure.EventGrid%20-%20CICD)](https://dev.azure.com/gtmoose/Mathis%20Home/_build/latest?definitionId=9) [![nuget](https://img.shields.io/nuget/v/Moosesoft.SendGrid.Azure.EventGrid.svg)](https://www.nuget.org/packages/Moosesoft.SendGrid.Azure.EventGrid/)

## What is it?
A library for .NET that converts Twilio [SendGrid web hook events](https://sendgrid.com/docs/for-developers/tracking-events/event/) into [EventGrid](https://azure.microsoft.com/en-us/services/event-grid/) events.

## Installing Moosesoft.SendGrid.Azure.EventGrid
```
dotnet add package Moosesoft.SendGrid.Azure.EventGrid
```

## Azure Function Sample
The following sample would require the use of [Http Triggers](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-http-webhook?tabs=csharp) and [Azure Functions Depedency Injection](https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection) to work properly.

```C#
public class SendGridEventHandler
{
    private readonly IEventGridEventPublisher _eventPublisher;

    public SendGridEventHandler(IEventGridEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    [FunctionName("HandleSendGridEvents")]
    public async Task<IActionResult> HandleSendGridHttpRequestAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
        HttpRequest request,
        ILogger log)
    {
        string json;
        using (var reader = new StreamReader(request.Body))
        {
            json = await reader.ReadToEndAsync().ConfigureAwait(false); 
        }

        log.LogInformation(json);

        await _eventPublisher.PublishEventsAsync(json).ConfigureAwait(false);

        return new OkResult();
    }
}
```
