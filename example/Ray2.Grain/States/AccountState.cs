using Ray2.Grain.Events;
using System.Runtime.Serialization;

namespace Ray2.Grain.States
{
    public class AccountState : State<long>
    {
        protected override void PlayEvent(IEvent @event)
        {
            switch (@event)
            {
                case AccountTransferEvent e:BalanceChangeHandle(e.Balance);break;
                case AccountAddBalanceEvent e: AmountAddEventHandle(e); break;
                default:
                    break;
            }
        }

        private void BalanceChangeHandle(decimal balance)
        {
            this.Balance = balance;
        }

        private void AmountAddEventHandle(AccountAddBalanceEvent evt)
        {
            this.Balance += evt.Amount;
            evt.Balance = this.Balance;
        }

        [DataMember]
        public decimal Balance { get; set; }
    }
}
