using Ray2.EventSources;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ray2.Storage
{
    public interface IEventStorage: IStorage
    {
        Task<List<EventModel>> GetListAsync(string eventSourceName,  EventQueryModel queryModel);
        Task SaveAsync(List<EventBufferWrap> wrapList);

    }
}
