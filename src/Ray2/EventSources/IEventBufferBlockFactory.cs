using Ray2.Storage;

namespace Ray2.EventSources
{
    public interface IEventBufferBlockFactory
    {
        IEventBufferBlock Create(string storageProviderName, string eventSourceName, IEventStorage eventStorage);
    }
}
