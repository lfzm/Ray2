using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ray2.PostgreSQL
{
    public interface IPostgreSqlTableStorage
    {
        Task CreateEventTable(string name);
        Task CreateStateTable(string name);
      
    }
}
