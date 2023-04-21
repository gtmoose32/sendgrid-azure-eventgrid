namespace Moosesoft.SendGrid.Azure.EventGrid;

/// <summary>
/// Defines an event publisher that reads raw json SendGrid events and publishes them as Azure EventGrid events.
/// </summary>
public interface IEventGridEventPublisher
{
    /// <summary>
    /// Reads json <see cref="string"/> of SendGrid events and publishes them as Azure EventGrid events to a specified topic endpoint.
    /// </summary>
    /// <param name="sendGridEventsJson">Raw json events received from SendGrid.</param>
    /// <param name="cancellationToken">Cancellation token used to cancel the operation.</param>
    /// <returns>The <see cref="Task"/> returned which can be awaited.</returns>
    Task PublishEventsAsync(string sendGridEventsJson, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads json <see cref="Stream"/> of SendGrid events and publishes them as Azure EventGrid events to a specified topic endpoint.
    /// </summary>
    /// <param name="sendGridEventsStream"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task PublishEventsAsync(Stream sendGridEventsStream, CancellationToken cancellationToken = default);
}