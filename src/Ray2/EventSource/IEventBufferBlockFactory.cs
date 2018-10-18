using Ray2.Storage;

namespace Ray2.EventSource
{
    public interface IEventBufferBlockFactory
    {
        IEventBufferBlock Create(string storageProviderName, string eventSourceName, IEventStorage eventStorage);
    }
}
