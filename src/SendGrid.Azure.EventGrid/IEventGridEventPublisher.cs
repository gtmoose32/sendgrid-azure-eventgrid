using System.Threading;
using System.Threading.Tasks;

namespace SendGrid.Azure.EventGrid
{
    public interface IEventGridEventPublisher
    {
        Task PublishEventsAsync(string sendGridEventsJson, CancellationToken cancellationToken = default);
    }
}
