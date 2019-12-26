# Moosesoft.SendGrid.Azure.EventGrid
[![nuget](https://img.shields.io/nuget/v/Moosesoft.SendGrid.Azure.EventGrid.svg)](https://www.nuget.org/packages/Moosesoft.SendGrid.Azure.EventGrid/)

## What is it?
A library for .NET that converts Twilio SendGrid web hook events into [EventGrid](https://azure.microsoft.com/en-us/services/event-grid/) events and publishes them to a topic hosted in [Microsoft Azure](https://azure.microsoft.com/).

## Installing Moosesoft.SendGrid.Azure.EventGrid
```
Install-Package Moosesoft.SendGrid.Azure.EventGrid
```

or via the .NET Core command line interface:

```
dotnet add package Moosesoft.SendGrid.Azure.EventGrid
```

## Azure Function Sample
The following sample would require the use of [Http Triggers](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-http-webhook?tabs=csharp) and [Azure Functions Depedency Injection](https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection) to work properly.

```C#
public class SendGridEventHandler : ISendGridEventHandler
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
