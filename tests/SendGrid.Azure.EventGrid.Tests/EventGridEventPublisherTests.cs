using System.Reflection;
using System.Text;

// ReSharper disable AssignNullToNotNullAttribute

namespace SendGrid.Azure.EventGrid.Tests;

[ExcludeFromCodeCoverage]
[TestClass]
public class EventGridEventPublisherTests
{
    private readonly EventGridEventPublisherSettings _defaultSettings = EventGridEventPublisherSettings.Default;
    private EventGridPublisherClient _eventGridClient;
    private IEventGridEventPublisher _sut;

    [TestInitialize]
    public void Init()
    {
        _eventGridClient = Substitute.For<EventGridPublisherClient>();
        _sut = new EventGridEventPublisher(_eventGridClient);
    }

    [TestMethod]
    public async Task PublishEventsAsync_Test()
    {
        //Arrange 
        var json = await GetEventJsonFromFileAsync("SendGridEvents.json").ConfigureAwait(false);

        //Act
        await _sut.PublishEventsAsync(json).ConfigureAwait(false);

        //Assert
        await _eventGridClient.Received(1)
            .SendEventsAsync(Arg.Is<IEnumerable<EventGridEvent>>(events => ValidatePublishedEvents(events, _defaultSettings)), Arg.Is(CancellationToken.None))
            .ConfigureAwait(false);
    }

    [TestMethod]
    public async Task PublishEventsAsync_ReceivedNullJson_Test()
    {
        //Act
        Func<Task> act = () => _sut.PublishEventsAsync(null);

        //Assert
        (await act.Should()
            .ThrowExactlyAsync<ArgumentException>())
            .WithMessage("Cannot be null, empty or whitespace. (Parameter 'sendGridEventsJson')");

        await _eventGridClient.DidNotReceiveWithAnyArgs()
            .SendEventsAsync(Arg.Any<IEnumerable<EventGridEvent>>(), Arg.Any<CancellationToken>())
            .ConfigureAwait(false);
    }

    [TestMethod]
    public async Task PublishEventsAsync_ReceivedEmptyJsonArray_Test()
    {
        //Arrange 
        var json = "[]";

        //Act
        await _sut.PublishEventsAsync(json).ConfigureAwait(false);

        //Assert
        await _eventGridClient.DidNotReceiveWithAnyArgs()
            .SendEventsAsync(Arg.Any<IEnumerable<EventGridEvent>>(), Arg.Any<CancellationToken>())
            .ConfigureAwait(false);
    }

    [TestMethod]
    public async Task PublishEventsAsync_WithCustomBuilders_Test()
    {
        //Arrange 
        var json = await GetEventJsonFromFileAsync("SendGridEvents.json").ConfigureAwait(false);

        var settings = new EventGridEventPublisherSettings(
            j => $"/my/custom/subject/{j["sg_event_id"].Value<string>()}",
            j => $"CustomEventType.{j["event"].Value<string>()}");

        var sut = new EventGridEventPublisher(_eventGridClient, settings);

        //Act
        await sut.PublishEventsAsync(json).ConfigureAwait(false);

        //Assert
        await _eventGridClient.Received(1)
            .SendEventsAsync(Arg.Is<IEnumerable<EventGridEvent>>(events => ValidatePublishedEvents(events, settings)), Arg.Is(CancellationToken.None))
            .ConfigureAwait(false);
    }

    private static Task<string> GetEventJsonFromFileAsync(string fileName)
    {
        var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fileName);
        return File.ReadAllTextAsync(path);
    }

    private static bool ValidatePublishedEvents(
        IEnumerable<EventGridEvent> eventGridEvents,
        EventGridEventPublisherSettings settings)
    {
        var events = eventGridEvents.ToArray();
        if (events.Length != 11) return false;

        foreach (var @event in events)
        {
            var json = JObject.Parse(Encoding.UTF8.GetString(@event.Data.ToArray()));
            if (@event.Id.Equals(json["sg_event_id"].Value<string>(), StringComparison.OrdinalIgnoreCase) &&
                @event.Subject.Equals(settings.BuildEventSubject(json), StringComparison.OrdinalIgnoreCase) &&
                @event.EventType.Equals(settings.BuildEventType(json), StringComparison.OrdinalIgnoreCase) &&
                json["custom_arg1"].Value<string>().Equals("test!"))
            {
                continue;
            }

            return false;
        }

        return true;
    }
}