using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ray2
{
    public interface IRayBuilder
    {
        IServiceCollection Services { get; }
        IConfiguration Configuration { get;  }
        void Build();
    }
}
