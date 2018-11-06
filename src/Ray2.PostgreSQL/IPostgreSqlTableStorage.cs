using System.Threading.Tasks;

namespace Ray2.PostgreSQL
{
    public interface IPostgreSqlTableStorage
    {
        Task CreateEventTable(string name,object stateId);
        Task CreateStateTable(string name, object stateId);
      
    }
}
