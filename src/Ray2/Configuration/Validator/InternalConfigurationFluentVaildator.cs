using FluentValidation;
using System.Linq;
using System.Threading.Tasks;

namespace Ray2.Configuration.Validator
{
    public class InternalConfigurationFluentVaildator : AbstractValidator<InternalConfiguration>, IInternalConfigurationValidator
    {
        public InternalConfigurationFluentVaildator(EventProcessOptionsFluentVaildator eventProcessOptionsFluentVaildator, EventSourceOptionsFluentVaildator eventSourceOptionsFluentVaildator, EventPublishOptionsFluentVaildator eventPublishOptionsFluentVaildator)
        {
            //Determine if the event processor name is duplicated
            base.RuleFor(f => f.EventProcessOptionsList.GroupBy(t => t.ProcessorName).Where(t => t.Count() > 1).FirstOrDefault())
                .Null()
                .WithMessage((optionsList, options) => $"{options.Key} event processor name is repeatedly configured and cannot be repeated");

            //Determine whether the event source name is duplicated
            base.RuleFor(f => f.EventSourceOptions.GroupBy(t => t.EventSourceName).Where(t => t.Count() > 1).FirstOrDefault())
                .Null()
                .WithMessage((optionsList, options) => $"Event source name {options.Key} repeat configuration");

            base.RuleForEach(f => f.EventSourceOptions)
                .SetValidator(eventSourceOptionsFluentVaildator);

            base.RuleForEach(f => f.EventProcessOptionsList)
                .SetValidator(eventProcessOptionsFluentVaildator);

            base.RuleForEach(f => f.EventProcessOptionsList)
                .Must((options, processOptions) => this.ProcessHavaEventSource(options, processOptions))
                .WithMessage((options, processOptions) => $"Configuration in {processOptions.ProcessorFullName} EventProcessorAttribute.EventSourceName Value {processOptions.EventSourceName} Cannot find the event source configuration for ");

            base.RuleForEach(f => f.EventPublishOptionsList)
                .SetValidator(eventPublishOptionsFluentVaildator);
        }
        private bool ProcessHavaEventSource(InternalConfiguration configuration, EventProcessOptions eventProcessOptions)
        {
            if (eventProcessOptions.ProcessorType == ProcessorType.SimpleProcessor)
                return true;

            var es = configuration.GetEventSourceOptions(eventProcessOptions.EventSourceName);
            if (es == null)
                return false;
            else
                return true;
        }

        public void IsValid(InternalConfiguration configuration)
        {
            var validateResult =  this.Validate(configuration);
            if (validateResult.IsValid)
            {
                return;
            }
            var error = validateResult.Errors.FirstOrDefault();
            throw new RayConfigurationException("Ray configuration error : " + error.ErrorMessage);
        }


    }
}
