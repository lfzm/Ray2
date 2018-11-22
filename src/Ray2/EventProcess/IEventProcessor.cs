using Orleans;
using Ray2.EventSource;
using System.Threading.Tasks;

namespace Ray2.EventProcess
{
    /// <summary>
    /// This is the Event process interface
    /// </summary>
    public interface IEventProcessor : IGrainWithGuidKey, IGrainWithIntegerKey, IGrainWithStringKey
    {
        Task<bool> Tell(EventModel model);
    }
}

