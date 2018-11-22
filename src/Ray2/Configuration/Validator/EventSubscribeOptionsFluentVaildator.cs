using FluentValidation;
using Orleans.Runtime;
using Ray2.MQ;
using System;

namespace Ray2.Configuration.Validator
{
    public class EventSubscribeOptionsFluentVaildator : AbstractValidator<EventSubscribeOptions>
    {
        private readonly IServiceProvider serviceProvider;
        public EventSubscribeOptionsFluentVaildator(IServiceProvider provider)
        {
            this.serviceProvider = provider;

            base.RuleFor(x => x.MQProvider)
                .NotEmpty()
                .WithMessage(x => $"EventSubscribeAttribute.MQProvider in {x.EventSubscribeFullName} cannot be empty");

            base.RuleFor(x => x.MQProvider)
                .Must(this.HaveMQProviderRegistered)
                .WithMessage("{PropertyValue} MQ provider is not injected into the Ray");

            base.RuleFor(x => x.Topic)
                .NotEmpty()
                .WithMessage(x => $"EventSubscribeAttribute.Topic in {x.EventSubscribeFullName} cannot be empty");

        }


        private bool HaveMQProviderRegistered(string MQProvider)
        {
            var provider = this.serviceProvider.GetServiceByName<IEventSubscriber>(MQProvider);
            return provider != null;
        }
    }
}
