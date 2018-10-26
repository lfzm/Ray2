using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2
{
    public interface IRayBuilder
    {
        IServiceCollection Services { get; }
        IConfiguration Configuration { get;  }
        void Build();
    }
}
