using Ray2.EventSource;
using Ray2.Internal;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ray2.Storage
{
    public interface IEventStorage: IStorage
    {
        Task<List<EventModel>> GetListAsync(string tableName,  EventQueryModel queryModel);
        Task<EventModel> GetAsync(string tableName, object stateId, long version);
        Task SaveAsync(List<IDataflowBufferWrap<EventStorageModel>> wrapList);
        Task<bool> SaveAsync(EventCollectionStorageModel eventList);
    }
}
