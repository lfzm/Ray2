using Ray2.EventSource;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ray2.Storage
{
    public interface IEventStorage: IStorage
    {
        Task<List<EventModel>> GetListAsync(string tableName,  EventQueryModel queryModel);
        Task SaveAsync(List<EventBufferWrap> wrapList);

    }
}
