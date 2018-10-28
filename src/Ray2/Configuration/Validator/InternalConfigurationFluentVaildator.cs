using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ray2.Configuration.Validator
{
    public class InternalConfigurationFluentVaildator : AbstractValidator<InternalConfiguration>, IInternalConfigurationValidator
    {
        public InternalConfigurationFluentVaildator( EventProcessOptionsFluentVaildator eventProcessOptionsFluentVaildator, EventSourceOptionsFluentVaildator eventSourceOptionsFluentVaildator, EventPublishOptionsFluentVaildator eventPublishOptionsFluentVaildator)
        {
            //Determine if the event processor name is duplicated
            base.RuleFor(f => f.GetEventProcessOptionsList().GroupBy(t => t.ProcessorName).Where(t => t.Count() > 1).Single())
                .Null()
                .WithMessage((optionsList, options) => $"{options.Key} event processor name is repeatedly configured and cannot be repeated");

            //Determine whether the event source name is duplicated
            base.RuleFor(f => f.GetEventSourceOptions().GroupBy(t => t.EventSourceName).Where(t => t.Count() > 1).Single())
                .Null()
                .WithMessage((optionsList, options) => $"Event source name {options.Key} repeat configuration");

            base.RuleForEach(f => f.GetEventProcessOptionsList())
                .SetValidator(eventProcessOptionsFluentVaildator);

            base.RuleForEach(f => f.GetEventSourceOptions())
                .SetValidator(eventSourceOptionsFluentVaildator);

            base.RuleForEach(f=>f.GetEventPublishOptionsList())
                .SetValidator(eventPublishOptionsFluentVaildator);
        }
        public Task<ConfigurationValidationResult> IsValid(IInternalConfiguration configuration)
        {
            throw new NotImplementedException();
        }

       
    }
}
