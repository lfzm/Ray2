using Orleans;
using System.Threading.Tasks;

namespace Ray2.EventProcess
{
    /// <summary>
    /// This is the Event process interface
    /// </summary>
    public interface IEventProcessor : IGrainWithGuidKey, IGrainWithIntegerKey, IGrainWithStringKey
    {
        Task Tell(IEvent @event);
    }
}

