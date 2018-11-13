using Ray2.EventSource;
using Ray2.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ray2.PostgreSQL
{
    public interface IPostgreSqlEventStorage
    {
        Task<List<EventModel>> GetListAsync( EventQueryModel queryModel);
        Task<EventModel> GetAsync(object stateId, long version);
        Task SaveAsync(List<EventStorageBufferWrap> wrapList);
        Task<bool> SaveAsync(EventCollectionStorageModel eventList);

    }
}
