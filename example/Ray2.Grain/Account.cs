using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Ray2.Grain.Events;
using Ray2.Grain.States;

namespace Ray2.Grain
{
    [EventSourcing("postgresql", "rabbitmq")]
    public class Account : RayGrain<AccountState, long>, IAccount
    {
        public Account(ILogger<Account> logger) : base(logger)
        {

        }

        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();
        }
        protected override long StateId => this.GetPrimaryKeyLong();

        public Task<bool> AddAmount(decimal amount, string uniqueId = null)
        {
            var evt = new AccountAddBalanceEvent(amount);
            evt.RelationEvent = uniqueId;
            return this.ConcurrentWriteAsync(evt, MQ.MQPublishType.Asynchronous);
        }

        public Task<decimal> GetBalance()
        {
            return Task.FromResult(State.Balance);
        }

        public Task Transfer(long toAccountId, decimal amount)
        {
            var evt = new AccountTransferEvent(toAccountId, amount, State.Balance - amount);
            return base.WriteAsync(evt);
        }
    }
}
