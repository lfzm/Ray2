using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ray2.Configuration.Validator
{
    public class InternalConfigurationVaildator : IInternalConfigurationValidator
    {
        public Task<ConfigurationValidationResult> IsValid(IInternalConfiguration configuration)
        {
            throw new NotImplementedException();
        }
    }
}
