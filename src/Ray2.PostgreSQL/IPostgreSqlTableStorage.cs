using System.Threading.Tasks;

namespace Ray2.PostgreSQL
{
    public interface IPostgreSqlTableStorage
    {
        void CreateEventTable(string name,object stateId);
        void CreateStateTable(string name, object stateId);
      
    }
}
