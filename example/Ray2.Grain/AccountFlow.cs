using Microsoft.Extensions.Logging;
using Orleans;
using Ray2.Grain.Events;
using Ray2.Grain.States;
using System.Threading.Tasks;

namespace Ray2.Grain
{
    [EventProcessor(typeof(Account), "rabbitmq",  "postgresql",OnceProcessCount =2000)]
    public class AccountFlow : RayProcessorGrain<AccountState, long>
    {
        private readonly ILogger logger;
        public AccountFlow(ILogger<AccountFlow> logger)
        {
            this.logger = logger;
        }
        protected override long StateId => this.GetPrimaryKeyLong();

        public override Task OnEventProcessing(IEvent @event)
        {
            switch (@event)
            {
                case AccountTransferEvent value: return TransferHandle(value);
                case AccountAddBalanceEvent value: return AddBalanceHandle(value);
                default: return Task.CompletedTask;
            }
        }

        private async Task TransferHandle(AccountTransferEvent evt)
        {
            var toActor = GrainFactory.GetGrain<IAccount>(evt.ToAccountId);
            await toActor.AddAmount(evt.Amount, evt.GetRelationKey());
        }

        private  Task AddBalanceHandle(AccountAddBalanceEvent evt)
        {
            //this.logger.LogError($"加款：{evt.Amount}; 余额：{evt.Balance}");
            return Task.CompletedTask;
        }
    }
}
