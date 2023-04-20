namespace Moosesoft.SendGrid.Azure.EventGrid;

/// <summary>
/// Reads SendGrid events json, converts them to <see cref="EventGridEvent"/> and publishes those events to a topic.
/// </summary>
public class EventGridEventPublisher : IEventGridEventPublisher
{
    private readonly EventGridEventPublisherSettings _settings;
    private readonly EventGridPublisherClient _eventGridClient;

    /// <summary>
    /// Initialize a new instance of <see cref="EventGridEventPublisher"/>.
    /// </summary>
    /// <param name="eventGridClient">The client used to publish SendGrid events to an Azure EventGrid topic.</param>
    /// <param name="settings">Settings used to help build events.</param>
    public EventGridEventPublisher(EventGridPublisherClient eventGridClient, EventGridEventPublisherSettings settings = null)
    {
        _eventGridClient = eventGridClient ?? throw new ArgumentNullException(nameof(eventGridClient));
        _settings = settings ?? EventGridEventPublisherSettings.Default;
    }

    /// <inheritdoc />
    public async Task PublishEventsAsync(string sendGridEventsJson, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(sendGridEventsJson))
            throw new ArgumentException("Cannot be null, empty or whitespace.", nameof(sendGridEventsJson));

        var events = JsonConvert.DeserializeObject<IEnumerable<JObject>>(sendGridEventsJson).ToArray();
        if (!events.Any()) return;

        var eventGridEvents = events
            .Select(ToEventGridEvent)
            .ToArray();

        await _eventGridClient.SendEventsAsync(eventGridEvents, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task PublishEventsAsync(Stream sendGridEventsStream, CancellationToken cancellationToken = default)
    {
        if (sendGridEventsStream == null) throw new ArgumentNullException(nameof(sendGridEventsStream));

        using var reader = new StreamReader(sendGridEventsStream, Encoding.UTF8);
        var json = await reader.ReadToEndAsync().ConfigureAwait(false);

        await PublishEventsAsync(json, cancellationToken).ConfigureAwait(false);
    }

    private const string EventIdKey = "sg_event_id";
    private EventGridEvent ToEventGridEvent(JObject eventJson)
    {
        return new EventGridEvent(
            _settings.BuildEventSubject(eventJson),
            _settings.BuildEventType(eventJson),
            "1", 
            new BinaryData(Encoding.UTF8.GetBytes(eventJson.ToString())))
        {
            Id = eventJson.TryGetValue(EventIdKey, StringComparison.OrdinalIgnoreCase, out var token)
                ? token.Value<string>()
                : Guid.NewGuid().ToString()
        };
    }
}