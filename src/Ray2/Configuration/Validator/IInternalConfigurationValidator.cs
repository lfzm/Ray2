using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ray2.Configuration.Validator
{
    public interface IInternalConfigurationValidator
    {
        Task IsValid(InternalConfiguration configuration);
    }
}
