using Ray2.Storage;

namespace Ray2.EventSourcing
{
    public interface IEventBufferBlockFactory
    {
        IEventBufferBlock Create(string storageProviderName, string eventSourceName, IEventStorage eventStorage);
    }
}
