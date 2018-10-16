using Orleans;
using System.Threading.Tasks;

namespace Ray2.EventProcessing
{
    /// <summary>
    /// This is the Event process interface
    /// </summary>
    public interface IEventProcessor : IGrainWithGuidKey, IGrainWithIntegerKey, IGrainWithStringKey
    {
        Task Tell(IEvent @event);
    }
}

