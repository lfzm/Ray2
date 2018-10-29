using FluentValidation;
using Orleans.Runtime;
using Ray2.MQ;
using System;

namespace Ray2.Configuration.Validator
{
    public class EventPublishOptionsFluentVaildator : AbstractValidator<EventPublishOptions>
    {
        private readonly IServiceProvider serviceProvider;
        public EventPublishOptionsFluentVaildator(IServiceProvider provider)
        {
            this.serviceProvider = provider;

            base.RuleFor(x => x.MQProvider)
                .NotEmpty()
                .WithMessage(x=>$"EventPublishAttribute.MQProvider in {x.EventPublishFullName} cannot be empty");

            base.RuleFor(x => x.MQProvider)
                .Must(this.HaveMQProviderRegistered)
                .WithMessage("{PropertyValue} MQ provider is not injected into the Ray");

            base.RuleFor(x => x.Topic)
                .NotEmpty()
                .WithMessage(x=> $"EventPublishAttribute.MQTopic in {x.EventPublishFullName} cannot be empty");
        }

        private bool HaveMQProviderRegistered(string MQProvider)
        {
            var provider = this.serviceProvider.GetServiceByName<IEventPublisher>(MQProvider);
            return provider != null;
        }
    }
}
